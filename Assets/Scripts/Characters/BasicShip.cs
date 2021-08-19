using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Video;
using SciFiArsenal;
using System.CodeDom;
using UnityEngine.InputSystem.XR.Haptics;

public enum turretPosition { fore, aft };

public class BasicShip : MonoBehaviour
{
    #region Health and Damage varaibles

    [Header("Attributes - Health and Damage")]
    public ShipAttributes attr;

    public float maxHealth, health;
    public float maxShield, shield;
    public float shieldRechargeRate = 0.25f;
    protected float armour;

    public float basicAttackDamage;
    public float heavyAttackDamage;
    public float heavyAttackDelay;
    protected float retaliateDamage;

    public bool alive { get; private set; } = true;

    #endregion Health and Damage varaibles

    #region Ability values and mechanic varaibles

    #region Attack

    public float warmupRamp = 1.0f;

    public int warmupShots, totalShots;

    protected Coroutine attacking;
    private bool isAttacking;

    #endregion Attack

    #region Heavy Attack

    public bool heavyAttackWindup { get; protected set; } = false;
    public AnimationCurve targetCurve;
    public LineRenderer target1, target2;
    public GameObject[] targetAnchors;

    #endregion Heavy Attack

    #region Absorb

    public bool absorbing { get; protected set; } = false;

    #endregion Absorb

    #region Healing

    public float healPercent, healDelay;
    public bool healing { get; private set; } = false;

    #endregion Healing

    #region Shield

    public bool shielded { get; protected set; } = false;
    public float shieldCooldown = 0.5f;
    protected float shieldCharge;
    protected bool shieldBroken;

    #endregion Shield

    #region Other

    public float jamDuration;
    protected float jamTimer;
    public int level = 1;

    #endregion Other

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

    #region Attack and Heal effects

    [Header("Effects - Base")]
    public GameObject warningFlare;

    private GameObject warningFlareRuntimeObject;
    public float warningFlareSize = 10.0f;

    public GameObject healEffect;

    #endregion Attack and Heal effects

    #region Shield Effects

    public GameObject shieldPrefab;
    protected GameObject shieldObject;
    public float shieldSize = 2f;

    #endregion Shield Effects

    #region Status Effects

    public GameObject jamFirePrefab;
    private GameObject jamEffect;

    public GameObject nearDeathFirePrefab;
    private GameObject nearDeathFire;

    #endregion Status Effects

    public GameObject explosion;
    public AudioClip SFX_Explosion;
    public AudioClip interrupt; 
    public AudioClip[] SFX_Heavy_attack_windup;

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
    protected ProspectiveEvent specialFiringEvent;
    protected ProspectiveEvent healEvent;

    #endregion Affect references

    #region Setup and Bookkeeping

    /// <summary>
    /// Sets up ship and finds references as needed
    /// </summary>
    public virtual void ShipSetup()
    {
        //Finds and assigns references
        SFX = GetComponent<AudioSource>();

        var managerObj = GameObject.Find("MainCamera");
        //Debug.Log("Manger object found: " + managerObj);
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
        StartCoroutine(ShieldEvaluate());

        //Sets attributes
        void SetAttributes(int level)
        {
            SetAttack(level);
            SetDefense(level);
            SetSpecial(level);
            FullHeal();
            healthBar.Refresh(maxHealth, health);
            shieldBar.Refresh(maxShield, shield);

            target1.gameObject.SetActive(false);
            target2.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Sets attack attributes based on level
    /// Resets health
    /// </summary>
    /// <param name="level"></param>
    public virtual void SetAttack(int level)
    {
        basicAttackDamage = attr.turretDamage(level);
        heavyAttackDamage = attr.heavyAttackDamage(level);
        warmupShots = attr.warmupShots(level);
        totalShots = attr.MaxShots(level);
        heavyAttackDelay = attr.heavyAttackDelay(level);
    }

    /// <summary>
    /// Sets defense attributes based on level
    /// </summary>
    /// <param name="level"></param>
    public virtual void SetDefense(int level)
    {
        maxHealth = attr.Health(level);
        maxShield = attr.shield(level);
        armour = attr.armour(level);
    }

    /// <summary>
    /// Sets special attributes based on level
    /// </summary>
    /// <param name="level"></param>
    public virtual void SetSpecial(int level)
    {
        healDelay = attr.healDelay(level);
        retaliateDamage = attr.retaliateDamage(level);
        healPercent = attr.healPercent(level);
        jamDuration = attr.disableDuration(level);
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

    protected virtual GameObject myTargetObject()
    {
        return null;
    }

    protected virtual BasicShip myTarget()
    {
        return null;
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
    protected float attackPatternFinishTime(int _warmupShots, int _totalShots)
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

    #region Attack

    /// <summary>
    /// Overridden for targetting, but starts or ends attack
    /// </summary>
    public void AttackToggle()
    {
        if (alive)
        {
            if (!isAttacking)
            {
                BeginAttack();
                PredictAttacks();
            }
            else
            {
                InterruptFiring();
            }
        }
    }

    /// <summary>
    /// Turns shields off - overridden in player to hold button
    /// </summary>
    protected virtual void BeginAttack()
    {
        ShieldsDown();
        isAttacking = true;
        if (attacking == null)
        {
            attacking = StartCoroutine(AutoAttack(myTarget(), warmupShots, totalShots));
        }
        else { Debug.Log("Attack started, but not null?"); }
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
        var firing = true;
        var takenShots = 0;
        var turret = turretPosition.fore;

        while (firing)
        {
            if (jamTimer <= 0.0f)
            {
                var damage = basicAttackDamage;
                var firingDelay = 0.234075f;

                ExecuteFiringPattern(target, _warmupShots, _totalShots, ref takenShots, ref turret, damage, ref firingDelay);
                yield return new WaitForSeconds(firingDelay); // Waits for the firing delay and continues firing
            }
            else { yield return null; }
        }
        Vector3 AlternatePosition(ref turretPosition _turret)
        {
            Vector3 pos;
            if (_turret == turretPosition.fore)
            {
                pos = foreTurret.position;
                _turret = turretPosition.aft;
            }
            else
            {
                pos = aftTurret.position;
                _turret = turretPosition.fore;
            }

            return pos;
        }

        void ExecuteFiringPattern(BasicShip _target, int total_warmup_Shots, int total_shots_to_take, ref int shots_taken, ref turretPosition firing_turret, float damage, ref float firingDelay)
        {
            if (_target.alive)
            {
                if (shots_taken < total_warmup_Shots) //If we're warming up our shots, take single alternating shot
                {
                    //Calculate timing
                    var remainingShots = total_warmup_Shots - shots_taken;
                    firingDelay *= ((float)remainingShots * warmupRamp);

                    //Fire turret and track number of shots taken
                    Vector3 pos = AlternatePosition(ref firing_turret);
                    FireTurret(_target, damage, pos);
                    shots_taken++;
                }
                else if (shots_taken < total_shots_to_take) //Once we've hit our warmupTarget, we fire double broadsides until we've hit total shots.
                {
                    FireTurret(_target, damage, foreTurret.position);
                    FireTurret(_target, damage, aftTurret.position);
                    shots_taken++;
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
        var toLook = target.transform.position - transform.position;
        cannon.transform.LookAt(target.transform);
        cannon.GetComponent<Rigidbody>().AddForce(toLook * 5);
    }

    /// <summary>
    /// Callback called after firing full broadside
    /// </summary>
    protected virtual void FinishFiring()
    {
        isAttacking = false;
        if (attacking != null)
        {
            StopCoroutine(attacking);
            attacking = null;
            affect.CullEvent(firingFinishEvent);
            ShieldsUp();
        }
    }

    /// <summary>
    /// Called to interrupt firing midway
    /// </summary>
    public virtual void InterruptFiring()
    {
        isAttacking = false;
        if (attacking != null)
        {
            StopCoroutine(attacking);
            attacking = null;
            affect.CullEvent(firingFinishEvent);
            ShieldsUp();
        }
    }

    #endregion Attack

    #region Heavy Attack

    public virtual void HeavyAttackTrigger()
    {
        ShieldsDown();
        heavyAttackWindup = true;
        StartCoroutine(HeavyAttackFlare());
        SpecialIndicator(Color.red, heavyAttackDelay);
    }

    public virtual ProspectiveEvent SpecialProspectiveEvent(float estimatedTime)
    {
        return new ProspectiveEvent(null, null, null, estimatedTime, true, affect);
    }

    private IEnumerator HeavyAttackFlare()
    {
        var timer = heavyAttackDelay;
        var barTimer = 0.0f;

        var anchors = myTarget().targetBoxes();
        foreach (GameObject anchor in anchors)
        {
            anchor.GetComponent<targetAnchor>().targetBegin(heavyAttackDelay);
        }

        target1.gameObject.SetActive(true);
        target2.gameObject.SetActive(true);

        while (timer > 0.0f)
        {
                target1.SetPosition(0, target1.transform.InverseTransformPoint(anchors[0].transform.position));
                target1.SetPosition(2, target1.transform.InverseTransformPoint(anchors[1].transform.position));
                target2.SetPosition(0, target2.transform.InverseTransformPoint(anchors[2].transform.position));
                target2.SetPosition(2, target2.transform.InverseTransformPoint(anchors[3].transform.position));

            timer -= Time.deltaTime;
            barTimer += Time.deltaTime;
            setChargeIndicator(barTimer, heavyAttackDelay);
            yield return null;
        }
        HeavyAttack(myTargetObject());



    }

    protected virtual void setChargeIndicator(float current, float max)
    {

    }

    public virtual void InterruptHeavyAttack()
    {
        heavyAttackWindup = false;
        Destroy(warningFlareRuntimeObject);
        SFX.PlayOneShot(interrupt);
        target1.gameObject.SetActive(false);
        target2.gameObject.SetActive(false);
        //ShieldsUp();
    }

    public GameObject[] targetBoxes()
    {
        return targetAnchors;
    }

    /// <summary>
    /// Deals heavy damage to player ship and disables a part. If player ship is shielded, primes retaliation
    /// </summary>
    public virtual void HeavyAttack(GameObject targetObj)
    {
        if (targetObj != null)
        {
            if (heavyAttackWindup)
            {
                heavyAttackWindup = false;
                var emitter = laserEmitter.position;
                var damage = heavyAttackDamage;
                var target = targetObj.GetComponent<BasicShip>();

                target1.gameObject.SetActive(false);
                target2.gameObject.SetActive(false);

                SpawnLaser(emitter, targetObj.transform.position);

                if (target.absorbing)
                {
                    target.SetRetaliate();
                }
                else
                {
                    target.TakeHeavyDamage(damage, jamDuration);
                }
            }
        }
        ShieldsUp();
    }

    #endregion Heavy Attack

    #region Absorb

    public virtual void SetRetaliate()
    {
    }

    #endregion Absorb

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
        var chargeTimer = 0.0f;
        while (timer > 0.0f)
        {
            timer -= Time.deltaTime;
            chargeTimer += Time.deltaTime;
            setChargeIndicator(chargeTimer, healDelay);
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
            health = CalculateHeal(health, maxHealth);
            healthBar.SetValue(health);

            healing = false;

            var healingEffect = Instantiate(healEffect, transform.position, Quaternion.identity, this.transform);
            healingEffect.transform.SetParent(this.transform);
        }
        ShieldsUp();

        ///Calculates the amount to heal
        float CalculateHeal(float current, float max)
        {
            var newCurrent = current;
            var missing = max - current;
            var amount = healPercent * missing;

            newCurrent += amount;
            if (newCurrent > max)
            {
                newCurrent = max;
            }

            return newCurrent;
        }
    }

    #endregion Heal

    #region Shield

    /// <summary>
    /// Constantly updates the status of the shield
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShieldEvaluate()
    {
        while (alive)
        {
            //If we don't have our shield broken, shield behaves as normal
            if (!shieldBroken)
            {
                if (shielded) //If we are shielded, watch for charge and break
                {
                    if (shield < maxShield)
                    {
                        shield += Time.deltaTime * shieldRechargeRate;
                    }
                    if (shieldCharge > 0f)  //If we still have shieldCharge time remaining, charge shield
                    {
                        shieldCharge -= Time.deltaTime;
                    }
                    else //Otherwise
                    {
                        if (shield <= 0.0f)
                        {
                            ShieldBreak();
                        }
                    }
                }
            }
            else //If our shield is broken, we re-charge it
            {
                shield += Time.deltaTime;
                if (shield >= maxShield)
                {
                    shield = maxShield;
                    shieldBar.ShieldRestore();
                    shieldBroken = false;
                    shieldCharge = 0f;
                    ShieldsUp();
                }
            }
            ShieldCheck();
            shieldBar.SetValue(shield);
            yield return null;
        }
    }

    /// <summary>
    /// Ensures that shield object matches status
    /// </summary>
    private void ShieldCheck()
    {
        if (shielded)
        {
            if (shieldObject == null)
            {
                InstantiateShield();
            }
        }
        else
        {
            if (shieldObject != null)
            {
                ShieldDestroy();
            }
        }
    }

    /// <summary>
    /// If the shield is up, we take shield damage
    /// </summary>
    /// <param name="damage"></param>
    public void ShieldHit(float _damage)
    {
        shield -= _damage;
    }

    /// <summary>
    /// If we're not doing something else, we can bring shields up.
    /// IMPORTANT: Must be called AFTER changing value
    /// </summary>
    public virtual void ShieldsUp()
    {
        if (!isAttacking && !healing && !heavyAttackWindup && !absorbing && !shieldBroken)
        {
            shielded = true;
        }
        else
        {
            //Debug.Log("Did not raise shields: Attacking: " + isAttacking + " | H. Attack: " + heavyAttackWindup + " | Absorbing: " + absorbing + " | Healing: " + healing + " | Shield Broken: " + shieldBroken);
        }
    }

    /// <summary>
    /// Turns off shield and sets recharge time
    /// </summary>
    public virtual void ShieldsDown()
    {
        shielded = false;
        shieldCharge = shieldCooldown;
    }

    protected void InstantiateShield()
    {
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

    public void ShieldDestroy()
    {
        Destroy(shieldObject);
    }

    /// <summary>
    /// ShieldBreak sets shield break and
    /// </summary>
    public void ShieldBreak()
    {
        ShieldsDown();
        shieldBroken = true;
        shieldBar.ShieldBreak();
    }

    /// <summary>
    /// Sets shield charge. Shield charge sets how much time it will take to bring shields up when appropriate
    /// </summary>
    /// <param name="delay"></param>
    protected void SetShieldCharge(float delay)
    {
        shieldCharge = delay;
    }

    #endregion Shield

    #region Health

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

            //If we're shielded, we take damage to our shield
            if (shielded)
            {
                ShieldHit(damage);
            }
            else
            {
                //If we're healing, the shot deals double damage and interrupts the heal
                if (healing)
                {
                    HealInterrupt();
                    _damage *= 1.5f;
                }


                //Take damage
                damage = _damage * armour;
                health -= damage;
                //Update health bar
                healthBar.SetValue(health);
            }

            //Calculate int and percent damage for UI effects, and pass damage to UI
            var intDamage = Mathf.RoundToInt(damage);
            var percent = damage / health;
            damageText.TakeDamage(intDamage, percent, shielded);
        }
    }

    /// <summary>
    /// Takes damage from heavy attack
    /// </summary>
    /// <param name="_damage"></param>
    public virtual void TakeHeavyDamage(float _damage, float _jamLength)
    {
        if (alive)
        {
            var damage = _damage;

            if (shielded) //Shield reduces damage
            {
                damage *= attr.heavyAttackShieldReduction;
                ShieldHit(damage);
            }
            else //Otherwise, take full damage and jam
            {
                if (healing) //Done fucked up
                {
                    HealInterrupt();
                    damage *= 1.5f;
                }
                if (heavyAttackWindup)
                {
                    InterruptHeavyAttack();
                    //damage *= 1.5f;
                }

                //Take damage
                health -= damage;
                healthBar.SetValue(health);
                Jam(_jamLength);
            }
            //Calculate int and percent damage for UI effects, and pass damage to UI
            var percent = damage / health;
            var intDamage = Mathf.RoundToInt(damage);
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

    #endregion Health

    #region Status Effects

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
        jamTimer = duration;
    }

    #endregion Status Effects

    #endregion Mechanics

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

    #region Death

    /// <summary>
    /// Sets alive to false, explodes, and begins wait for doneDeath
    /// </summary>
    protected virtual void die()
    {
        Debug.Log("Death triggered");
        alive = false;
        myTarget().InterruptFiring();

        SFX.PlayOneShot(SFX_Explosion);
        var deathExplode = Instantiate(explosion, this.transform.position, Quaternion.identity, this.transform);
        StartCoroutine(death());
    }

    private IEnumerator death()
    {
        InterruptLaser();
        InterruptFiring();
        yield return new WaitForSeconds(1.0f);
        doneDeath();
    }

    protected virtual void doneDeath()
    {
        myTarget().InterruptLaser();
    }

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
        if (firing)
        {
            firing = false;
        }
    }

    private void DestroyLaser(GameObject start, GameObject end, GameObject _beam)
    {
        Destroy(start);
        Destroy(end);
        Destroy(_beam);
    }

    #endregion Laser Fire
}