using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShip : BasicShip
{
    public static bool parry = false;

    public static bool shielded = false;

    [Header("Button Management")]
    public CombatButton abilityButton;

    public float abilityCooldown, ultimateCooldown;

    public PlayerAttributes attr;

    public buttonManager buttonManager;

    [Header("Special effects")]
    public GameObject hasteEffect;

    //public GameObject healEffect;

    public GameObject hpWarn;

    [Header("Other objects")]
    public EnemyShip mainEnemy;

    public List<PlayerShip> otherShips;

    public AudioClip SFX_explode;

    public AudioClip SFX_lowHealth;

    public List<Turret> turrets;

    public GameObject warpIn;

    public GameObject warpWindow;

    public float weaponCooldown;

    [Header("Audio")]
    protected AudioSource SFX;

    private bool autoAttack;
    private float hasteMod = 1.0f;

    [Header("UX for HP")]
    private bool lowHP = false;

    #region Setup

    public virtual void shipSetup()
    {
        alive = true;
        if (!healthBar.gameObject.activeSelf) { healthBar.gameObject.SetActive(true); }
        healthBar.Refresh(maxHealth, health);
        affect = gameManager.gameObject.GetComponent<AffectManager>();
        StartCoroutine(healthUpdate()); //Begins constant health evaluation
    }

    #endregion Setup

    public void activateUltimate(float _time)
    {
        if (!alive)
        {
            alive = true;
            StartCoroutine(respawnWarp());
        }
        hasteEffect.SetActive(true);
        hasteMod = 2.0f;
        ultimate = true;
        StartCoroutine(hasteTimer(_time));
    }

    public void beginAutoAttack()
    {
        if (!autoAttack)
        {
            autoAttack = true;
            fireWeapons(TargetManager.target, "Player");
            StartCoroutine(autoFire());
        }
    }

    public void delayFirstFire()
    {
        StartCoroutine(firstFire());
    }

    public void endCombat()
    {
        autoAttack = false;
    }

    public override void fireWeapons(BasicShip _target, string tag)
    {
        base.fireWeapons(_target, "Player");
    }

    public void fullHeal()
    {
        health = maxHealth;
        healthBar.Refresh(maxHealth, health);
    }

    public void playWarp()
    {
        warpIn.GetComponent<ParticleSystem>().Play();
    }

    //public override void receiveHealing(float percentage)
    //{
    //    base.receiveHealing(percentage);
    //    var healingEffect = Instantiate(healEffect, transform.position, Quaternion.identity, this.transform);
    //    var missingDisp = maxHealth - health;
    //    var toDisplay = missingDisp * percentage;
    //    damageText.receiveHealing((int)toDisplay, percentage);
    //}

    public void updatePlayers(EnemyShip _mainShip, List<Turret> _turrets)
    {
        mainEnemy = _mainShip;
        turrets = new List<Turret>(_turrets);
    }

    public virtual void upgradeShip()
    {
        upgrade = true;
    }

    protected override void die()
    {
        SFX.PlayOneShot(SFX_explode);
        base.die();
        affect.loseShip(this);
        tellGM();
    }

    protected override void doneDeath()
    {
        base.doneDeath();
        hpWarn.SetActive(false);
        GetComponent<MeshRenderer>().enabled = false;
        autoAttack = false;
        StartCoroutine(playerShipRespawn());
    }

    protected void globalCooldowns()
    {
        buttonManager.globalCooldown();
    }

    protected override void healthEval()
    {
        base.healthEval();
        if (!lowHP)
        {
            if (health < (0.25f * maxHealth))
            {
                lowHP = true;
                hpWarn.SetActive(true);
                SFX.PlayOneShot(SFX_lowHealth);
            }
        }
        if (lowHP)
        {
            if (health > (0.25 * maxHealth))
            {
                lowHP = false;
                hpWarn.SetActive(false);
            }
        }
    }

    protected void lookAtTarget()
    {
        if (TargetManager.target != null)
        {
            transform.LookAt(TargetManager.target.transform.position);
        }
    }

    protected override void passDamageToAffect(float damage)
    {
        base.passDamageToAffect(damage);
        affect.playerHit(damage);
    }

    protected virtual void tellGM()
    {
    }

    private IEnumerator autoFire()
    {
        var timer = weaponCooldown;
        while (autoAttack)
        {
            lookAtTarget();
            if (timer > 0.0f)
            //If there is still time left on our timer, reduce it and inform the slider
            {
                timer -= Time.deltaTime * hasteMod;
            }
            else //Otherwise, fire wepaons and reset timer and slider
            {
                fireWeapons(TargetManager.target, "Player");
                timer = weaponCooldown;
            }
            yield return null;
        }
    }

    private IEnumerator firstFire()
    {
        yield return new WaitForSeconds(Random.Range(2.5f, 5.0f));
        beginAutoAttack();
    }

    private IEnumerator hasteTimer(float _time)
    {
        yield return new WaitForSeconds(_time);
        hasteEffect.SetActive(false);
        ultimate = false;
        hasteMod = 1.0f;
    }

    private IEnumerator playerShipRespawn()
    {
        var windowOpen = false;
        var respawn = attr.respawnTimer;
        abilityButton.startCooldown(respawn);

        while (respawn > 0.0f && !alive)
        {
            respawn -= Time.deltaTime;

            if (respawn <= 2.5f && !windowOpen)
            {
                var window = Instantiate(warpWindow, new Vector3(0, 0, -7.5f), Quaternion.Euler(90, 0, 0), this.transform);
                window.transform.localPosition = new Vector3(0, 0, -7.5f);
                window.GetComponent<portalAppear>().warp(0.75f);
                windowOpen = true;
            }
            yield return null;
        }
        StartCoroutine(respawnWarp());
    }

    private IEnumerator respawnWarp()
    {
        GetComponent<MeshRenderer>().enabled = true;
        var targetPos = transform.position;
        var prewarpPos = transform.position;
        prewarpPos.z -= 150;
        transform.localScale = new Vector3(0.25f, 0.25f, 2.5f);
        transform.position = prewarpPos;
        while (Vector3.Distance(transform.position, targetPos) > 75f)
        {
            var newPosition = Vector3.MoveTowards(transform.position, targetPos, 250.0f * Time.deltaTime);
            transform.position = newPosition;
            yield return null;
        }
        while (transform.localScale.z < 1.0f)
        {
            var newPosition = Vector3.MoveTowards(transform.position, targetPos, 10.0f * Time.deltaTime);
            transform.position = newPosition;
            var z = transform.localScale.z;
            z -= 40.0f * Time.deltaTime;
            transform.localScale = new Vector3(1, 1, z);
            yield return null;
        }
        transform.position = targetPos;
        transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

        alive = true;
        healthBar.gameObject.SetActive(true);
        fullHeal();
        affect.respawn(this);
        StartCoroutine(healthUpdate());

        beginAutoAttack();
    }

    private void Start()
    {
        SFX = GetComponent<AudioSource>();
        abilityButton.myCD = abilityCooldown;
        //ultimateButton.myCD = ultimateCooldown;
    }

    #region buttons

    //public void autoAttackActivate()
    //{
    //    if (attackButton.canActivate())
    //    {
    //        attackButton.targetsUp();
    //    }
    //}

    public virtual void standardAbility()
    {
        if (abilityButton.canActivate())
        {
            abilityButton.sendToButton(abilityCooldown);
        }
    }

    #endregion buttons
}