using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Video;
using SciFiArsenal;

public enum turretPosition { fore, aft };

public class BasicShip : MonoBehaviour
{
    #region Health and Damage varaibles

    [Header("Attributes - Health and Damage")]
    public ShipAttributes attr;

    protected float maxHealth, health;
    protected float maxShield, shieldHealth;

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

    public bool healing { get; private set; } = false;
    public bool shielded { get; protected set; } = false;

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

    private Transform bulletParent;

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
    protected GameObject shield;
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

    private LinkedList<Event> attackQueue;
    private LinkedListNode<Event> nextAttack;

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
        ShieldsUp(); //Raises shields
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
            shieldBar.Refresh(maxShield, shieldHealth);
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
        healthBar.addValue((int)maxHealth);

        shieldHealth = maxShield;
        shieldBar.addValue((int)maxShield);
    }

    public float healthPercent()
    {
        return health / maxHealth;
    }

    #endregion Setup and Bookkeeping

    #region Affect

    /// <summary>
    /// Hacky interface
    ///Creates an event that predicts a weapon firing - called as soon as shot is decided upon
    ///When called by player, this is called at the same time as firing, when called by enemy, this is called when AI decides on event
    /// </summary>
    protected virtual void PredictAttacks()
    { }

    /// <summary>
    /// Adds event to queue - called once event is created
    /// </summary>
    /// <param name="event"></param>
    protected void AddEventToQueue(Event @event)
    {
        //If there's not a current queue, create one on the first event call
        if (attackQueue == null)
        {
            attackQueue = new LinkedList<Event>();
        }

        var node = new LinkedListNode<Event>(@event);
        attackQueue.AddLast(node);
    }

    /// <summary>
    /// Gets next attack and removes it from queue
    /// </summary>
    /// <returns></returns>
    protected Event NextAttack()
    {
        //If we don't have an attack loaded, first see if the attack queue has one
        if (nextAttack == null)
        {
            if (attackQueue == null) //If attack queue doesn't exist yet, we can't attach the event
            {
                return new Event(null, null, null, 0.0f, affect);
            }
            if (attackQueue.Count > 0)
            {
                nextAttack = attackQueue.First;
            }
            else
            {
                //Debug.Log("NextAttack called without any attacks in queue");
                return new Event(null, null, null, 0.0f, affect);
            }
        }
        var nextEvent = nextAttack.Value;
        nextAttack = nextAttack.Next;
        attackQueue.RemoveFirst();

        return nextEvent;
    }

    /// <summary>
    /// Removes toRemove events from front of queue, and sets next attack to the next attack
    /// </summary>
    /// <param name="ToRemove">number of events to remove</param>
    protected void RemoveEvents(int toRemove)
    {
        for (int i = 0; i < toRemove; i++)
        {
            attackQueue.RemoveFirst();
        }
        nextAttack = attackQueue.First;
    }

    #endregion Affect

    #region Mechanics

    /// <summary>
    /// Fires cannons "shots" number of times,
    /// </summary>
    /// <param name="target">Target</param>
    /// <param name="position">Fore (aggressive) or Aft (defensive)</param>
    /// <param name="shots">Number of shots to fire</param>
    /// <returns></returns>
    protected IEnumerator FireBroadside(BasicShip target, turretPosition position, int shots)
    {
        //Debug.Log("Firing " + shots + " shots from " + position);
        var remainingShots = shots;

        while (remainingShots > 0)
        {
            var damage = turretDamage;
            if (target.alive)
            {
                //Sets somewhat random position based on cannon position
                var pos = foreTurret.position;
                if (position == turretPosition.aft)
                {
                    pos = aftTurret.position;
                }
                pos.z += Random.Range(-5, 5);

                //Fires cannon from position
                var cannon = Instantiate(basicTurretShot, pos, Quaternion.identity, bulletParent);
                cannon.gameObject.tag = tag;
                cannon.layer = 9;
                cannon.GetComponent<SciFiProjectileScript>().CannonSetup(damage, target, NextAttack(), affect);
                cannon.transform.LookAt(target.transform);
                cannon.GetComponent<Rigidbody>().AddForce(foreTurret.transform.forward * 2500);

                //Removes shot from remaining
                remainingShots--;
            }

            yield return new WaitForSeconds(0.23075f); //Waits 1 eighth note at 130 bpm
        }
        FinishFiring();
    }

    /// <summary>
    /// Callback called after firing full broadside
    /// </summary>
    protected virtual void FinishFiring()
    {
    }

    /// <summary>
    /// Fires both cannons shots times
    /// </summary>
    /// <param name="target">ship to fire at</param>
    /// <param name="shots">Number of sh ots to fire from both cannons</param>
    protected void FullBroadside(BasicShip target, int shots)
    {
        StartCoroutine(FireBroadside(target, turretPosition.fore, shots));
        StartCoroutine(FireBroadside(target, turretPosition.aft, shots));
    }

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
                shieldBar.TakeDamage(intDamage);
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
                healthBar.TakeDamage(intDamage);
            }
            //Calculate int and percent damage for UI effects, and pass damage to UI
            var percent = damage / health;
            damageText.TakeDamage(intDamage, percent);
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
    protected virtual void LowHealthEvaluate()
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
        jamTimer = duration;
    }

    /// <summary>
    /// Resets combo, overridetn
    /// </summary>
    protected virtual void ResetCombo()
    {
        Debug.Log("Whoops! Forgot to write ResetCombo");
    }

    #endregion Mechanics

    #region Shield Mechanics

    /// <summary>
    /// Shields default to up most of the time
    /// </summary>
    protected void ShieldsUp()
    {
        //Sets mechanics
        shielded = true;

        //Plays sound, creates object
        CreateShieldObject();

        void CreateShieldObject()
        {
            SFX.PlayOneShot(SFX_shieldActivate);
            shield = Instantiate(shieldPrefab, transform.position, Quaternion.identity, this.transform);
            shield.tag = gameObject.tag;
            shield.name = "Shield";
            shield.transform.SetParent(this.transform);
            shield.transform.localScale = new Vector3(shieldSize, shieldSize, shieldSize);
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
        shieldHealth -= _damage;
        shieldBar.TakeDamage(Mathf.RoundToInt(_damage));

        if (shieldHealth <= 0.0f)
        {
            ShieldBreak();
        }
    }

    /// <summary>
    /// If we run out of shield, it goes down for a long time and
    /// </summary>
    public void ShieldBreak()
    {
        Jam(2.5f);
        ShieldsDown();
        shieldBar.ShieldBreak();
        ChargeShield(30.0f);
        StartCoroutine(ShieldRecharge(30.0f));
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
            shieldHealth = timerPercent * maxShield;
            yield return null;
        }
        shieldHealth = maxShield;
        shieldBar.ShieldRestore();
    }

    /// <summary>
    /// Turns off shield
    /// </summary>
    protected void ShieldsDown()
    {
        shielded = false;
        Destroy(shield);
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
            var missingHealth = maxShield - shieldHealth;
            var amount = healPercent * missingHealth;
            shieldHealth += amount;

            if (shieldHealth > maxHealth)
            {
                shieldHealth = maxHealth;
            }
            shieldBar.addValue(Mathf.RoundToInt(amount));

            healing = false;

            var healingEffect = Instantiate(healEffect, transform.position, Quaternion.identity, this.transform);
            healingEffect.transform.SetParent(this.transform);
        }
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