using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Video;
using SciFiArsenal;
using System.CodeDom;

public enum turretPosition { fore, aft };

public class BasicShip : MonoBehaviour
{
    #region Health and Damage varaibles

    [Header("Attributes - Health and Damage")]
    public ShipAttributes attr;

    protected float maxHealth, health;
    protected float maxShield, shield;
    protected bool shieldBroken;

    protected float armour;

    protected float turretDamage;

    protected float retaliateDamage;
    protected float laserDamage;
    public bool alive { get; private set; } = true;

    #endregion Health and Damage varaibles

    #region Ability values and mechanic varaibles

    protected float jamDuration;
    protected float jamTimer;
    protected float healPercent, healDelay;
    public float shieldCooldown = 0.5f;

    public bool attacking { get; protected set; } = false;
    public float warmupRamp = 1.0f;
    protected int warmupShots, totalShots;
    public bool healing { get; private set; } = false;
    public bool shielded { get; protected set; } = false;

    public bool absorbing { get; protected set; } = false;
    public int level = 1;

    #endregion Ability values and mechanic varaibles

    #region UI references

    [Header("UI and audio")]
    public damageText damageText;

    public healthBarAnimator healthBar;
    public shieldBarAnimator shieldBar;
    public AudioClip SFX_shieldActivate;

    #endregion UI references

    #region Weapon spawn locations and prefab

    [Header("Weapons - Base")]
    public Transform laserEmitter;

    public Transform foreTurret;
    public Transform aftTurret;

    public Transform flareLoc;

    protected Transform bulletParent;

    public GameObject basicTurretShot;

    #endregion Weapon spawn locations and prefab

    #region Effect variables

    [Header("Effects - Base")]
    public GameObject warningFlare;

    private GameObject warningFlareRuntimeObject;
    public float warningFlareSize = 10.0f;

    public GameObject jamFirePrefab;
    private GameObject jamEffect;

    public GameObject healEffect;

    public GameObject nearDeathFirePrefab;
    private GameObject nearDeathFire;

    public GameObject shieldPrefab;
    protected GameObject shieldObject;
    public float shieldSize = 2f;

    public GameObject explosion;

    public AudioClip SFX_Explosion;
    protected AudioSource SFX;

    #endregion Effect variables

    #region Laser variables

    [Header("Laser objects")]
    public GameObject beamStartPrefab;

    public GameObject beamEndPrefab, beamPrefab;

    [Header("Laser Presentation Variables")]
    public float beamEndOffset = 1f; //How far from the raycast hit point the end effect is positioned

    public float textureScrollSpeed = 8f; //How fast the texture scrolls along the beam
    public float textureLengthScale = 3; //Length of the beam texture

    #endregion Laser variables

    #region Manager references

    [Header("Other")]
    public GameManager manager;

    public AffectManager affect;

    #endregion Manager references

    #region Affect references

    protected ProspectiveEvent firingFinishEvent;

    #endregion Affect references

    #region Setup and Bookkeeping

    /// <summary>
    /// Sets up ship and finds references as needed
    /// </summary>
    public virtual void ShipSetup()
    {
        //Finds and assigns references
        SFX = GetComponent<AudioSource>();

        var managerObj = GameObject.Find("Main Camera");
        affect = managerObj.GetComponent<AffectManager>();
        manager = managerObj.GetComponent<GameManager>();

        bulletParent = GameObject.Find("BulletParent").transform;

        //Sets attributes and sets up health bar, and sets alive to true
        SetAttributes(level);
        //ShieldsUp(); //Raises shields
        alive = true;

        //Begins coroutines for updating statuses and health
        StartCoroutine(JamEvaluate());
        StartCoroutine(HealthEvaluate());

        //Sets attributes
        void SetAttributes(int level)
        {
            SetAttack(level);
            SetDefense(level);
            SetSpecial(level);
            FullHeal();
            healthBar.Refresh(maxHealth, health);
            shieldBar.Refresh(maxShield, shield);
        }
    }

    /// <summary>
    /// Sets attack attributes based on level
    /// Resets health
    /// </summary>
    /// <param name="level"></param>
    public virtual void SetAttack(int level)
    {
        turretDamage = attr.turretDamage(level);
        warmupShots = attr.warmupShots(level);
        totalShots = attr.MaxShots(level);
    }

    /// <summary>
    /// Sets defense attributes based on level
    /// </summary>
    /// <param name="level"></param>
    public virtual void SetDefense(int level)
    {
        maxHealth = attr.health(level);
        maxShield = attr.shield(level);
        jamDuration = attr.disableDuration(level);
        armour = attr.armour(level);
    }

    /// <summary>
    /// Sets special attributes based on level
    /// </summary>
    /// <param name="level"></param>
    public virtual void SetSpecial(int level)
    {
        healDelay = attr.healDelay(level);
        laserDamage = attr.laserDamage(level);
        retaliateDamage = attr.fusionCannonDamage(level);
        healPercent = attr.healPercent(level);
    }

    /// <summary>
    /// Used between stages to fully heal ship
    /// </summary>
    public void FullHeal()
    {
        health = maxHealth;
        healthBar.Refresh(maxHealth, maxHealth);
        healthBar.SetValue(maxHealth);

        shield = maxShield;
        shieldBar.Refresh(maxShield, maxShield);
        shieldBar.SetValue(maxShield);
    }

    public float healthPercent()
    {
        return health / maxHealth;
    }

    public float shieldPercent()
    {
        return shield / maxShield;
    }

    #endregion Setup and Bookkeeping

    #region Affect

    /// <summary>
    ///Predicts attack and sets firingFinishEvent to the new event that predicts end of combo. Called when starting attack
    ///Override sets whether it's a player or enemy attack
    /// </summary>
    protected virtual void PredictAttacks()
    {
        var firingTime = attackPatternFinishTime(warmupShots, totalShots);
        var valence = attackPatternValence();
        var arousal = attackPatternArousal();
        var tension = attackPatternTension();
        firingFinishEvent = new ProspectiveEvent(valence, arousal, tension, firingTime, true, affect);
    }

    /// <summary>
    /// Hacky interface, overridden in children
    /// Calculates valence of attack pattern at point of firing
    /// </summary>
    /// <returns></returns>
    protected virtual Emotion attackPatternValence()
    {
        return new Emotion(EmotionDirection.none, EmotionStrength.none);
    }

    /// <summary>
    /// Hacky interface, overridden in children
    /// Calculates arousaal of attack pattern at point of firing
    /// </summary>
    /// <returns></returns>
    protected virtual Emotion attackPatternArousal()
    {
        return new Emotion(EmotionDirection.none, EmotionStrength.none);
    }

    /// <summary>
    /// Hacky interface, overridden in children
    /// Calculates tension of attack pattern at point of firing
    /// </summary>
    /// <returns></returns>
    protected virtual Emotion attackPatternTension()
    {
        return new Emotion(EmotionDirection.none, EmotionStrength.none);
    }

    /// <summary>
    /// Way more complcated than it seems
    /// Simulates attack pattern and returns the time that the simulation took
    /// I'm sure there's a math way to do this, but I have two music degrees
    /// </summary>
    /// <returns>Returns total time taken for attack pattern</returns>
    private float attackPatternFinishTime(int _warmupShots, int _totalShots)
    {
        var takenShots = 0;
        var totalTime = 0f;
        var stockFiringDelay = 0.234075f;
        for (int i = 0; i < _warmupShots; i++) ///First, warmupShots
        {
            var firingDelay = stockFiringDelay;
            var remainingShots = _warmupShots - takenShots;
            firingDelay *= (float)remainingShots * warmupRamp;
            totalTime += firingDelay;
        }
        //Once we've simulated the warmup, we can pretty easily calculate the rest - each shot after this takes the standard delay
        totalTime += (_totalShots - _warmupShots) * stockFiringDelay;
        return totalTime;
    }

    #endregion Affect

    #region Mechanics

    #region Attacks

    /// <summary>
    /// Overridden for targetting, but starts or ends attack
    /// </summary>
    public virtual void AttackToggle()
    {
    }

    /// <summary>
    /// Fires single cannon w. delay until warmed up, then fire both cannons quickly
    /// Think minigun
    /// </summary>
    /// <param name="target">Target</param>
    /// <param name="_warmupShots">Shots until starting to fire both cannons</param>

    /// <returns></returns>
    protected IEnumerator AutoAttack(BasicShip target, int _warmupShots, int _totalShots)
    {
        attacking = true;
        var takenShots = 0;
        var turret = turretPosition.fore;

        while (attacking)
        {
            if (jamTimer <= 0.0f)
            {
                var damage = turretDamage;
                var firingDelay = 0.234075f;

                ExecuteFiringPattern(target, _warmupShots, _totalShots, ref takenShots, ref turret, damage, ref firingDelay);
                yield return new WaitForSeconds(firingDelay); // Waits for the firing delay and continues firing
            }
            else { yield return null; }
        }
        Vector3 AlternatePosition(ref turretPosition turret)
        {
            Vector3 pos;
            if (turret == turretPosition.fore)
            {
                pos = foreTurret.position;
                turret = turretPosition.aft;
            }
            else
            {
                pos = aftTurret.position;
                turret = turretPosition.fore;
            }

            return pos;
        }

        void ExecuteFiringPattern(BasicShip target, int _warmupShots, int _totalShots, ref int takenShots, ref turretPosition turret, float damage, ref float firingDelay)
        {
            if (target.alive)
            {
                if (takenShots < _warmupShots) //If we're warming up our shots, take single alternating shot
                {
                    //Calculate timing
                    var remainingShots = _warmupShots - takenShots;
                    firingDelay *= ((float)remainingShots * warmupRamp);

                    //Fire turret and track number of shots taken
                    Vector3 pos = AlternatePosition(ref turret);
                    FireTurret(target, damage, pos);
                    takenShots++;
                }
                else if (takenShots < _totalShots) //Once we've hit our warmupTarget, we fire double broadsides until we've hit total shots.
                {
                    FireTurret(target, damage, foreTurret.position);
                    FireTurret(target, damage, aftTurret.position);
                    takenShots++;
                }
                else { FinishFiring(); } //If we complete all shots, finish firing.
            }
        }
    }

    private void FireTurret(BasicShip target, float damage, Vector3 pos)
    {
        //Fires cannon from position
        var cannon = Instantiate(basicTurretShot, pos, Quaternion.identity, bulletParent);
        cannon.gameObject.tag = tag;
        cannon.layer = 9;
        cannon.GetComponent<SciFiProjectileScript>().CannonSetup(damage, target, affect);
        cannon.transform.LookAt(target.transform);
        cannon.GetComponent<Rigidbody>().AddForce(foreTurret.transform.forward * 2500);
    }

    /// <summary>
    /// Callback called after firing full broadside
    /// </summary>
    protected virtual void FinishFiring()
    {
        attacking = false;
        ChargeShield(shieldCooldown);
    }

    /// <summary>
    /// Called to interrupt firing midway
    /// </summary>
    protected virtual void InterruptFiring()
    {
        attacking = false;
        affect.CullEvent(firingFinishEvent);
        ChargeShield(shieldCooldown);
    }

    #endregion Attacks

    /// <summary>
    /// This takes damage from any source
    /// If healing, damage is doubled and healing is interrupted
    /// </summary>
    /// <param name="_damage">Damage to take, modified by armour </param>
    public virtual void TakeDamage(float _damage)
    {
        if (alive)
        {
            var damage = _damage;
            var intDamage = Mathf.RoundToInt(damage);

            //If we're shielded, we take damage to our shield
            if (shielded)
            {
                ShieldHit(damage);
                shieldBar.SetValue(shield);
            }
            else
            {
                //If we're healing, the shot deals double damage and interrupts the heal
                if (healing)
                {
                    HealInterrupt();
                    _damage *= 2f;
                }

                //Take damage
                damage = _damage * armour;
                intDamage = Mathf.RoundToInt(damage);

                health -= damage;
                //Update health bar
                healthBar.SetValue(health);
                Jam(0.25f); //Getting hit without shields adds short stagger
            }

            //Calculate int and percent damage for UI effects, and pass damage to UI
            var percent = damage / health;
            damageText.TakeDamage(intDamage, percent, shielded);
        }
    }

    /// <summary>
    /// Evaluates health for effects and mechanics
    /// If low, catch fire
    /// If high and on fire, put out fire
    /// If dead, die
    /// </summary>
    private IEnumerator HealthEvaluate()
    {
        while (alive)
        {
            LowHealthEvaluate();

            if (health <= 0.0f) { die(); }
            yield return null;
        }
    }

    /// <summary>
    /// Evaluates triggers for low health
    /// </summary>
    protected void LowHealthEvaluate()
    {
        if (LowHealth())
        {
            if (nearDeathFire == null) //If we're not on fire, catch fire
            {
                nearDeathFire = Instantiate(nearDeathFirePrefab, this.transform);
            }
        }
        else
        {
            if (nearDeathFire != null) //If health isn't low, we can put out the fire
            {
                Destroy(nearDeathFire);
            }
        }
    }

    /// <summary>
    /// Evaluates whether ship has low health (<25%)
    /// </summary>
    /// <returns></returns>
    public bool LowHealth()
    {
        return health <= (0.25f * maxHealth);
    }

    /// <summary>
    /// Evaluates the Jam timer
    /// Spawns jam effect if we're jammed and it doesn't exist
    /// Destroys jam effect if we're not jammed and it does
    /// </summary>
    private IEnumerator JamEvaluate()
    {
        while (alive)
        {
            if (jamTimer > 0.0f)
            {
                jamTimer -= Time.deltaTime;

                if (jamEffect == null)
                {
                    jamEffect = Instantiate(jamFirePrefab, this.transform);
                }
            }
            else
            {
                if (jamEffect != null)
                {
                    Destroy(jamEffect);
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Starts JamFire for duration of jam. Overridden in playerships and enemyships for different effects
    /// </summary>
    /// <param name="duration">Duration of Jam</param>
    public virtual void Jam(float duration)
    {
        ShieldsDown();
        jamTimer = duration;
        ChargeShield(duration);
    }

    #endregion Mechanics

    #region Shield Mechanics

    /// <summary>
    /// Shields default to up most of the time
    /// </summary>
    public void ShieldsUp()
    {
        if (!shieldBroken && !shielded)
        {
            //Sets mechanics
            shielded = true;

            //Plays sound, creates object
            CreateShieldObject();

            void CreateShieldObject()
            {
                SFX.PlayOneShot(SFX_shieldActivate);
                shieldObject = Instantiate(shieldPrefab, transform.position, Quaternion.identity, this.transform);
                shieldObject.tag = gameObject.tag;
                shieldObject.name = "Shield";
                shieldObject.transform.SetParent(this.transform);
                shieldObject.transform.localScale = new Vector3(shieldSize, shieldSize, shieldSize);
            }
        }
    }

    /// <summary>
    /// Starts shield recharging for delay, - just to avoid problems with forgetting to call startCoroutine
    /// </summary>
    /// <param name="delay"></param>
    protected void ChargeShield(float delay)
    {
        StartCoroutine(ShieldWait(delay));
    }

    /// <summary>
    /// Waits the given time and reactivates shield (if shield is down)
    /// </summary>
    /// <param name="_time"></param>
    /// <returns></returns>
    protected IEnumerator ShieldWait(float _time)
    {
        var timer = _time;
        while (timer > 0.0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        if (!shielded)
        {
            ShieldsUp();
        }
    }

    /// <summary>
    /// If the shield is up, we take shield damage
    /// </summary>
    /// <param name="damage"></param>
    public void ShieldHit(float _damage)
    {
        shield -= _damage;
        shieldBar.SetValue(shield);

        if (shield <= 0.0f)
        {
            ShieldBreak();
        }
    }

    /// <summary>
    /// If we run out of shield, it goes down for a long time and
    /// </summary>
    public void ShieldBreak()
    {
        shieldBroken = true;
        Jam(2.5f);
        ShieldsDown();
        shieldBar.ShieldBreak();
        ChargeShield(10.0f);
        StartCoroutine(ShieldRecharge(10.0f));
    }

    /// <summary>
    /// Scales shield amount to timer
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    protected IEnumerator ShieldRecharge(float time)
    {
        var timer = 0.0f;
        while (timer < time)
        {
            timer += Time.deltaTime;
            var timerPercent = timer / time;
            shield = timerPercent * maxShield;
            shieldBar.SetValue(shield);
            yield return null;
        }
        shield = maxShield;
        shieldBar.ShieldRestore();
        shieldBroken = false;
    }

    /// <summary>
    /// Turns off shield
    /// </summary>
    public void ShieldsDown()
    {
        shielded = false;
        Destroy(shieldObject);
    }

    #endregion Shield Mechanics

    #region Effects and indicators

    /// <summary>
    /// Plays a flare with supplied color
    /// </summary>
    /// <param name="col">Color of flare</param>
    /// <param name="duration">Time that it should take</param>
    public void SpecialIndicator(Color col, float duration)
    {
        warningFlareRuntimeObject = Instantiate(warningFlare, transform.position, Quaternion.identity, this.transform);
        warningFlareRuntimeObject.transform.position = flareLoc.position;
        var flare = warningFlareRuntimeObject.GetComponent<ParticleSystem>();
        var main = flare.main;
        main.duration = duration;
        main.startLifetime = duration;
        main.startColor = col;
        main.startSize = warningFlareSize;
        flare.Play();
    }

    #endregion Effects and indicators

    #region Heal

    /// <summary>
    /// Begins healing process
    /// </summary>
    public virtual void HealTrigger()
    {
        StartCoroutine(HealDelay());
        SpecialIndicator(Color.green, healDelay);
    }

    public IEnumerator HealDelay()
    {
        healing = true;
        var timer = healDelay;
        while (timer > 0.0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        Heal();
    }

    /// <summary>
    /// Interrupts heal, destroys warning flare.
    /// </summary>
    public void HealInterrupt()
    {
        healing = false;
        Destroy(warningFlareRuntimeObject);
    }

    /// <summary>
    /// Heals shield for % of missing health, determined by healPercent modifier attribute
    /// </summary>
    public virtual void Heal()
    {
        if (healing)
        {
            var missingShield = maxShield - shield;
            var amount = healPercent * missingShield;
            shield += amount;

            if (shield > maxHealth)
            {
                shield = maxHealth;
            }
            shieldBar.SetValue(shield);

            healing = false;

            var healingEffect = Instantiate(healEffect, transform.position, Quaternion.identity, this.transform);
            healingEffect.transform.SetParent(this.transform);
        }
        ChargeShield(shieldCooldown);
    }

    #endregion Heal

    #region Death

    /// <summary>
    /// Sets alive to false, explodes, and begins wait for doneDeath
    /// </summary>
    protected virtual void die()
    {
        alive = false;
        SFX.PlayOneShot(SFX_Explosion);
        var deathExplode = Instantiate(explosion, this.transform.position, Quaternion.identity, this.transform);
        StartCoroutine(death());
    }

    private IEnumerator death()
    {
        yield return new WaitForSeconds(1.0f);
        doneDeath();
    }

    protected virtual void doneDeath()
    { }

    #endregion Death

    #region Laser Fire

    private bool firing = false;

    protected void SpawnLaser(Vector3 startLoc, Vector3 endLoc)
    {
        firing = true;
        var beamStart = Instantiate(beamStartPrefab, Vector3.zero, Quaternion.identity);
        var beamEnd = Instantiate(beamEndPrefab, Vector3.zero, Quaternion.identity);
        var beam = Instantiate(beamPrefab, Vector3.zero, Quaternion.identity);
        var line = beam.GetComponent<LineRenderer>();

        AlignLaser(startLoc, endLoc, beamStart, beamEnd, beam, line);
    }

    private void AlignLaser(Vector3 start, Vector3 target, GameObject beamStart, GameObject beamEnd, GameObject beam, LineRenderer line)
    {
        if (Physics.Linecast(start, target, out RaycastHit hit))
        {
            beamEnd.transform.position = hit.point;
        }

        line.useWorldSpace = false;

        line.positionCount = 2;
        line.SetPosition(0, start);
        beamStart.transform.position = start;
        //beamEnd.transform.position = target;
        line.SetPosition(1, target);

        beamStart.transform.LookAt(beamEnd.transform.position);
        beamEnd.transform.LookAt(beamStart.transform.position);

        float distance = Vector3.Distance(start, target);
        line.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
        line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);

        StartCoroutine(laserLifetime(beamStart, beamEnd, beam));
    }

    private IEnumerator laserLifetime(GameObject start, GameObject end, GameObject beam)
    {
        var timer = 2.5f;
        while (firing && timer > 0.0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        // Debug.Log("Calling destroy laser");
        DestroyLaser(start, end, beam);
        firing = false;
    }

    public void InterruptLaser()
    {
        //Debug.Log("Interrupting laser");
        firing = false;
    }

    private void DestroyLaser(GameObject start, GameObject end, GameObject _beam)
    {
        Destroy(start);
        Destroy(end);
        Destroy(_beam);
    }

    #endregion Laser Fire
}