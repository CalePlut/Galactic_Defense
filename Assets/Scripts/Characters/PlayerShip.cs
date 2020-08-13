using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

    protected float attackSpeed;

    protected float attackSpeedBoost = 1.0f;

    //Auto Attack
    public float weaponCooldown;

    private bool autoAttack;
    protected float stanceAttackSpeedBonus = 1.0f;
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
    }

    /// <summary>
    /// Basic ship setup. Runs attribute setup and then sets up references etc.
    /// </summary>
    /// <param name="upgradeLevel"></param>
    public void ShipSetup(int attack = 0, int defend = 0, int skill = 0)
    {
        SetAttributes(attack, defend, skill);

        affect = gameManager.gameObject.GetComponent<AffectManager>();
        StartCoroutine(healthUpdate()); //Begins constant health evaluation
        stance = AttackStance.holdFire;
        targetManager = gameManager.targets;
    }

    /// <summary>
    /// Sets all attributes based on player build.
    /// </summary>
    /// <param name="attack"></param>
    /// <param name="defend"></param>
    /// <param name="skills"></param>
    public void SetAttributes(int attack, int defend, int skills)
    {
        setAttacks(attack);
        setDefend(defend);
        setSkill(skills);

        alive = true;
        if (!healthBar.gameObject.activeSelf) { healthBar.gameObject.SetActive(true); }
        healthBar.Refresh(maxHealth, health);
    }

    protected virtual void setAttacks(int _upgrade)
    {
        if (_upgrade == 0)
        {
            attackSpeed = attr.attackSpeed;
            stanceAttackSpeedBonus = attr.cannonSpeedBoost;
        }
        else if (_upgrade == 1)
        {
            attackSpeed = attr.upgradeAttackSpeed;
            stanceAttackSpeedBonus = attr.cannonSpeedBoost;
        }
        else if (_upgrade == 2)
        {
            attackSpeed = attr.maxAttackSpeed;
            stanceAttackSpeedBonus = attr.maxCannonSpeedBoost;
        }
        else { Debug.Log("Tried to upgrade beyond max level"); }
    }

    protected virtual void setDefend(int _upgrade)
    {
        if (_upgrade == 0)
        {
            armorMod = attr.armorModifier;
        }
        else if (_upgrade == 1)
        {
            armorMod = attr.upgradedArmorMoifier;
        }
        else if (_upgrade == 2)
        {
            armorMod = attr.maxArmorModifier;
        }
        else { Debug.Log("Tried to upgrade beyond max level"); }
    }

    protected virtual void setSkill(int _upgrade)
    {
        if (_upgrade == 0)
        {
            lifesteal = attr.lifesteal;
        }
        else if (_upgrade == 1)
        {
            lifesteal = attr.lifestealUpgrade;
        }
        else if (_upgrade == 2)
        {
            lifesteal = attr.maxLifesteal;
        }
        else { Debug.Log("Tried to upgrade beyond max level"); }
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
        stanceAttackSpeedBonus = 2.0f;
        ultimate = true;
        StartCoroutine(hasteTimer(_time));
    }

    private IEnumerator hasteTimer(float _time)
    {
        yield return new WaitForSeconds(_time);
        hasteEffect.SetActive(false);
        ultimate = false;
        stanceAttackSpeedBonus = 1.0f;
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
            Debug.Log("Starting AutoFire");
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
                timer -= Time.deltaTime;
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
            var toHeal = stance == AttackStance.regenerative;
            FireWeapons(target, "Player", toHeal);
        }
    }

    /// <summary>
    /// Calculates the fire rate based on stance
    /// </summary>
    /// <returns>Seconds until next firing</returns>
    private float FireRateCalculator()
    {
        var toReturn = attackSpeed * attackSpeedBoost;
        Debug.Log("Starting value is " + attackSpeed + "* aspeedBoost " + attackSpeedBoost + " = " + toReturn);
        if (stance == AttackStance.aggressive)
        {
            toReturn *= stanceAttackSpeedBonus;
            Debug.Log("Multiplied by " + stanceAttackSpeedBonus);
        }
        if (stance == AttackStance.holdFire)
        {
            toReturn = 0.5f; //Yes, overwrite toReturn if we're holding fire. This lets us constantly re-evaluate fire rate
        }
        Debug.Log("Stance = " + stance + ", aSpeed = " + toReturn);

        return toReturn;
    }

    /// <summary>
    /// Sets stance, based on incoming. Used with abilities
    /// </summary>
    /// <param name="_stance">new stance to take</param>
    public void SetStance(AttackStance _stance)
    {
        stance = _stance;
        if (stance == AttackStance.aggressive)
        {
            attackSpeedBoost = 0.75f;
        }
        else { attackSpeedBoost = 1.0f; }
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

    #endregion Game states and mechanics
}