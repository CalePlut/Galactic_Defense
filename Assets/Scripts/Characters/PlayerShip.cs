using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackStance { aggressive, defensive, regenerative, holdFire }

public class PlayerShip : BasicShip
{
    #region Balance

    [Header("Balance")]
    public PlayerAttributes attr;

    #endregion Balance

    #region Mechanics

    [Header("Button Management")]
    public buttonManager buttonManager;

    public basicButton abilityButton;

    [Header("Ability attributes")]
    public float abilityCooldown, ultimateCooldown;

    public static bool parry = false;
    public static bool shielded = false;

    //Auto Attack
    public float weaponCooldown;

    private bool autoAttack;
    private float hasteMod = 1.0f;
    protected static AttackStance stance;

    #endregion Mechanics

    #region Object references

    [Header("Other objects")]
    public EnemyShip mainEnemy;

    public List<PlayerShip> otherShips;
    public List<Turret> turrets;
    public TargetManager targetManager;

    #endregion Object references

    #region UI and SFX

    [Header("UI and SFX")]
    public GameObject hasteEffect;

    public GameObject hpWarn;
    public GameObject warpIn;
    public GameObject warpWindow;
    private bool lowHP = false;

    //Audio
    protected AudioSource SFX;

    public AudioClip SFX_explode;

    public AudioClip SFX_lowHealth;

    #endregion UI and SFX

    #region Setup

    private void Start()
    {
        SFX = GetComponent<AudioSource>();
        abilityButton.myCD = abilityCooldown;
        //ultimateButton.myCD = ultimateCooldown;
    }

    public virtual void shipSetup()
    {
        alive = true;
        if (!healthBar.gameObject.activeSelf) { healthBar.gameObject.SetActive(true); }
        healthBar.Refresh(maxHealth, health);
        affect = gameManager.gameObject.GetComponent<AffectManager>();
        StartCoroutine(healthUpdate()); //Begins constant health evaluation
        stance = AttackStance.holdFire;
        targetManager = gameManager.targets;
    }

    #endregion Setup

    #region Ability and Ultimate

    public virtual void standardAbility()
    {
        if (abilityButton.canActivate())
        {
            abilityButton.sendToButton(abilityCooldown);
        }
    }

    protected void globalCooldowns()
    {
        buttonManager.globalCooldown();
    }

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

    private IEnumerator hasteTimer(float _time)
    {
        yield return new WaitForSeconds(_time);
        hasteEffect.SetActive(false);
        ultimate = false;
        hasteMod = 1.0f;
    }

    #endregion Ability and Ultimate

    #region Auto-Attack

    /// <summary>
    /// Looks at and returns a target ship
    /// </summary>
    /// <returns>New target based on attack stance</returns>
    protected EnemyBase GetTarget()
    {
        var target = targetManager.GetTarget(stance);
        lookAtShip(target);
        return target;
    }

    public void BeginAutoAttack()
    {
        if (!autoAttack)
        {
            autoAttack = true;
            StartCoroutine(AutoFire());
        }
    }

    /// <summary>
    /// Automatically fires weapons, based on attack stance
    /// </summary>
    /// <returns></returns>
    private IEnumerator AutoFire()
    {
        var timer = weaponCooldown;
        while (autoAttack)
        {
            //If there is still time left on our timer, reduce it and inform the slider
            if (timer > 0.0f)
            {
                timer -= Time.deltaTime * hasteMod;
            }
            else //Otherwise, execute attack based on attack pattern
            {
                PlayerFireControl();
                timer = FireRateCalculator();
            }

            yield return null;
        }
    }

    /// <summary>
    /// Executes the firing of a weapon, dealing damage based on multiplier and healing if regenerative.
    /// </summary>
    private void PlayerFireControl()
    {
        var target = GetTarget();

        if (stance != AttackStance.holdFire)
        {
            var damage = DamageCalculator();
            var toHeal = stance == AttackStance.regenerative;
            FireWeapons(target, "Player", damage, toHeal);
        }
    }

    /// <summary>
    /// Determines the damage an attack should do, based on stance
    /// </summary>
    /// <returns></returns>
    private float DamageCalculator()
    {
        var totalModifier = 1.0f;

        if (stance == AttackStance.regenerative)
        {
            totalModifier *= 0.5f;
        }
        if (ultimate)
        {
            totalModifier *= 2;
        }

        return totalModifier;
    }

    /// <summary>
    /// Calculates the fire rate based on stance
    /// </summary>
    /// <returns>Seconds until next firing</returns>
    private float FireRateCalculator()
    {
        var toReturn = weaponCooldown;
        if (stance == AttackStance.aggressive)
        {
            toReturn *= 0.75f;
        }
        if (stance == AttackStance.holdFire)
        {
            toReturn = 0.5f; //Yes, overwrite toReturn if we're holding fire
        }

        return toReturn;
    }

    /// <summary>
    /// Sets stance, based on incoming. Used with abilities
    /// </summary>
    /// <param name="_stance">new stance to take</param>
    public void SetStance(AttackStance _stance)
    {
        stance = _stance;
    }

    public void delayFirstFire()
    {
        StartCoroutine(FirstFire());
    }

    private IEnumerator FirstFire()
    {
        yield return new WaitForSeconds(Random.Range(2.5f, 5.0f));
        BeginAutoAttack();
    }

    #endregion Auto-Attack

    #region Health, Death, and Respawn

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

    public void fullHeal()
    {
        health = maxHealth;
        healthBar.Refresh(maxHealth, health);
    }

    protected override void die()
    {
        SFX.PlayOneShot(SFX_explode);
        base.die();
        tellGM();

        var valenceEmotion = new Emotion(EmotionDirection.decrease, EmotionStrength.strong);
        var tensionEmotion = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
        affect.CreatePastEvent(valenceEmotion, null, tensionEmotion, 15.0f);
    }

    protected override void doneDeath()
    {
        base.doneDeath();
        hpWarn.SetActive(false);
        GetComponent<MeshRenderer>().enabled = false;
        autoAttack = false;
        StartCoroutine(playerShipRespawn());
    }

    private IEnumerator playerShipRespawn()
    {
        var windowOpen = false;
        var respawn = attr.respawnTimer;

        var respawnValence = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
        var respawnArousal = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
        var respawnTension = new Emotion(EmotionDirection.increase, EmotionStrength.weak);
        affect.CreateProspectiveEvent(respawnValence, respawnArousal, respawnTension, respawn, true);

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

        var respawnValence = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
        var respawnTension = new Emotion(EmotionDirection.decrease, EmotionStrength.weak);
        affect.CreatePastEvent(respawnValence, null, respawnTension, 10.0f);

        StartCoroutine(healthUpdate());

        BeginAutoAttack();
    }

    #endregion Health, Death, and Respawn

    #region Game states and mechanics

    public void updatePlayers(EnemyShip _mainShip, List<Turret> _turrets)
    {
        mainEnemy = _mainShip;
        turrets = new List<Turret>(_turrets);
    }

    protected override void passDamageToAffect(float damage)
    {
        base.passDamageToAffect(damage);

        var valenceChange = EmotionStrength.weak;
        if (percentHealth() < 0.1f && !lowHealthProspective)
        {
            valenceChange = EmotionStrength.moderate;
            //If we're receiving an attack at a low health, we create a prospective "Get destroyed" event 15 seconds from nowe with moderate change. This will disappear if we heal.
            var playerDestroyValence = new Emotion(EmotionDirection.decrease, EmotionStrength.moderate);
            var playerDestroyTension = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
            var lowHealthPlayer = affect.CreateProspectiveEvent(playerDestroyValence, null, playerDestroyTension, 15.0f);
            StartCoroutine(lowHealthEvent(lowHealthPlayer));
            lowHealthProspective = true;
        }
        var valenceEmotion = new Emotion(EmotionDirection.decrease, valenceChange);
        affect.CreatePastEvent(valenceEmotion, null, null, 5.0f);
    }

    protected virtual void tellGM()
    {
    }

    public void endCombat()
    {
        autoAttack = false;
    }

    public void playWarp()
    {
        warpIn.GetComponent<ParticleSystem>().Play();
    }

    public virtual void upgradeShip()
    {
        upgrade = true;
    }

    #endregion Game states and mechanics
}