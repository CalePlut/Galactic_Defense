using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

/// <summary>
/// Attack type is used only in selecting which move to fire by the AI
/// </summary>
public enum AttackType { foreTurret, aftTurret, specialAttack, heal, delay }

public class EnemyShip : BasicShip
{
    public float globalCooldown;

    [Tooltip("Used to synchronize special attack to hit right after last shot")]
    public float specialSyncDelay;//Used to synchronize special attack

    //public float delayLength;

    public float specialAttackFrameLength;

    public float comboStartDelayMinimum = 3f, comboStartDelayMaximum = 5f;

    private GameObject playerShipObj;
    private PlayerShip playerShip;
    public GameObject warpEffect;

    #region Combat AI variables

    private bool comboHeal;

    //private CombatAI CombatAI;

    #endregion Combat AI variables

    #region Affect variables

    public bool upcomingSpecial { get; private set; } = false;

    #endregion Affect variables

    #region Setup and Bookeeping

    /// <summary>
    /// Creates CombatAI for self and sets references to player objects.
    /// </summary>
    public override void ShipSetup()
    {
        base.ShipSetup();
        SetReferences();
        ComboSetup();
    }

    /// <summary>
    /// Creates warp window, plays particle system, waits for duration, and destroys window
    /// </summary>
    /// <returns></returns>
    private IEnumerator WarpWindow()
    {
        var warp = Instantiate(warpEffect, new Vector3(0, 0, 5), Quaternion.identity, this.transform);
        warp.GetComponent<ParticleSystem>().Play();
        yield return new WaitForSeconds(1.5f);
        Destroy(warp);
    }

    /// <summary>
    /// Animates the opening of the warp gate and the flying in, starts setup block.
    /// </summary>
    /// <returns></returns>
    public IEnumerator FlyIn()
    {
        //Sets initial positions
        var targetPos = transform.position;
        var prewarpPos = transform.position;
        prewarpPos.x += 500;
        transform.position = prewarpPos;

        //Stores original scale and sets new scale
        var originalScale = transform.localScale;
        var warpScale = new Vector3(originalScale.x, originalScale.y, originalScale.z * 10);
        transform.localScale = warpScale;

        while (Vector3.Distance(transform.position, targetPos) > 75f)
        {
            var newPosition = Vector3.MoveTowards(transform.position, targetPos, 1000.0f * Time.deltaTime);
            transform.position = newPosition;
            yield return null;
        }
        while (transform.localScale.z < 1.0f)
        {
            var newPosition = Vector3.MoveTowards(transform.position, targetPos, 40.0f * Time.deltaTime);
            transform.position = newPosition;
            var z = transform.localScale.z;
            z -= 40.0f * Time.deltaTime;
            transform.localScale = new Vector3(originalScale.x, originalScale.y, z);
            yield return null;
        }
        transform.position = targetPos;
        transform.localScale = originalScale;

        StartCoroutine(WarpWindow());
    }

    /// <summary>
    /// Finds player and sets reference, should only be called during setup.
    /// </summary>
    private void SetReferences()
    {
        playerShipObj = GameObject.Find("Player Ship");
        playerShip = playerShipObj.GetComponent<PlayerShip>();
        playerShip.SetEnemyReference(this.gameObject);
    }

    #endregion Setup and Bookeeping

    #region Affect

    /// <summary>
    /// Finishes PredictAttack with adding to enemy list
    /// </summary>
    protected override void PredictAttacks()
    {
        base.PredictAttacks();
        affect.AddUpcomingEnemyAttack(firingFinishEvent);
    }

    /// <summary>
    /// Overrides attack pattern valence to provide enemy values
    /// </summary>
    /// <returns></returns>
    protected override Emotion attackPatternValence()
    {
        return new Emotion(EmotionDirection.decrease, EmotionStrength.moderate);
    }

    //NOTE: We don't need to override arousal in enemy because enemy attack doesn't change arousal level

    /// <summary>
    /// Overrides attack pattern tension to provide enemy values
    /// </summary>
    /// <returns></returns>
    protected override Emotion attackPatternTension()
    {
        return new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
    }

    public override void TakeDamage(float _damage)
    {
        base.TakeDamage(_damage);
        //Set base levels - the first few shots aren't actually important to the strength of the emotion
        var damageValence = new Emotion(EmotionDirection.none, EmotionStrength.none);
        var damageArousal = new Emotion(EmotionDirection.none, EmotionStrength.none);
        var damageTension = new Emotion(EmotionDirection.none, EmotionStrength.none);

        if (shieldPercent() < 0.75f)
        {
            damageValence = new Emotion(EmotionDirection.increase, EmotionStrength.weak);
            damageArousal = new Emotion(EmotionDirection.increase, EmotionStrength.weak);
            damageTension = new Emotion(EmotionDirection.none, EmotionStrength.none);
        }
        if (!shielded)
        {
            damageValence = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
            damageArousal = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
            damageTension = new Emotion(EmotionDirection.increase, EmotionStrength.weak);
            if (LowHealth())
            {
                damageValence = new Emotion(EmotionDirection.increase, EmotionStrength.strong);
                damageTension = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
            }
        }
        affect.CreatePastEvent(damageValence, damageArousal, damageTension, 45.0f);
    }

    #endregion Affect

    #region action implementation

    public void SpecialAttackTrigger()
    {
        StartCoroutine(SpecialAttackFlare());
        SpecialIndicator(Color.red, specialAttackFrameLength);
    }

    private IEnumerator SpecialAttackFlare()
    {
        var timer = specialAttackFrameLength;
        while (timer > 0.0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        SpecialAttack();
    }

    /// <summary>
    /// Deals heavy damage to player ship and disables a part. If player ship is shielded, primes retaliation
    /// </summary>
    public void SpecialAttack()
    {
        var emitter = laserEmitter.position;
        var damage = laserDamage;

        SpawnLaser(emitter, playerShipObj.transform.position);

        if (playerShip.absorbing)
        {
            playerShip.SetRetaliate();
        }
        else
        {
            playerShip.TakeDamage(damage);
            if (!playerShip.shielded)
            {
                playerShip.Jam(jamDuration);
            }
        }
        upcomingSpecial = false;

        ChargeShield(globalCooldown);
    }

    public override void Heal()
    {
        base.Heal();
        Jam(jamDuration);
        ChargeShield(globalCooldown);
    }

    #endregion action implementation

    protected override void die()
    {
        base.die();
        manager.WarpPan();
    }

    protected override void doneDeath()
    {
        base.doneDeath();
        manager.EnemyDie();
        Destroy(this.gameObject);
    }

    #region Combat AI

    /// <summary>
    /// Sets initial combo values
    /// Begins wait before first AdvanceCombo
    /// </summary>
    private void ComboSetup()
    {
        //PredictAttacks(); //Once we have a combo length, we

        var shieldPercent = shield / maxShield; //Chance of triggering healing increases as health decreases
        comboHeal = Random.value > shieldPercent;

        if (alive)
        {
            var toWait = Random.Range(comboStartDelayMaximum, comboStartDelayMaximum);
            StartCoroutine(ComboInitialWait(toWait));
        }
    }

    /// <summary>
    /// Used before starting to attack
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private IEnumerator ComboInitialWait(float time)
    {
        var timer = time;
        while (timer > 0.0f)
        {
            if (jamTimer <= 0.0f) //If we're jammed, we don't advance
            {
                timer -= Time.deltaTime;
            }
            yield return null;
        }
        AttackToggle(); //Begins attack after waiting
    }

    /// <summary>
    /// Toggles attack on or off
    /// </summary>
    public override void AttackToggle()
    {
        if (alive)
        {
            if (!attacking)
            {
                ShieldsDown();
                StartCoroutine(AutoAttack(playerShip, warmupShots, totalShots));
            }
            else
            {
                InterruptFiring();
            }
        }
    }

    /// <summary>
    /// Keeps track of how many shots have been fired, to properly end combo.
    /// </summary>
    protected override void FinishFiring()
    {
        base.FinishFiring();
        // Debug.Log("Finished firing");
        StartCoroutine(SpecialSyncDelay());
        StartCoroutine(ComboInitialWait(Random.Range(comboStartDelayMinimum, comboStartDelayMaximum)));
    }

    /// <summary>
    /// Used to sync up
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpecialSyncDelay()
    {
        yield return new WaitForSeconds(specialSyncDelay);
        if (comboHeal)
        {
            HealTrigger();
        }
        else
        {
            SpecialAttackTrigger();
        }
    }

    #endregion Combat AI

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }
}

///// <summary>
///// Manages enemy actions via "CombatAI.Advance, and sends unified enemy Tension value to affect manager"
/////
///// </summary>
//public class CombatAI
//{
//    private EnemyShip ship;
//    private AffectManager affect;
//    private ShipAttributes attr;
//    private List<EnemyAttack> enemyAttacks;

//    public CombatAI(EnemyShip _core, AffectManager _affect)
//    {
//        ship = _core;
//        affect = _affect;
//        enemyAttacks = new List<EnemyAttack>();
//    }

//    private int ComboLength()
//    {
//        return Random.Range(0, 4);
//    }

//    ///// <summary>
//    ///// Advances clock on all enemyActions, calculates tension value and sends to Affect Manager.
//    ///// </summary>
//    ///// <param name="deltaTime">Unity's Time.deltaTime</param>
//    ///// <param name="jammed">Are we jammed or not</param>
//    //public void Advance(float deltaTime)
//    //{
//    //    if (enemyAttacks.Count == 0) //If we don't have a pattern, create one.
//    //    {
//    //        CreatePattern();
//    //    }
//    //    else
//    //    {
//    //        //Faux garbage collection - list to be removed from list after enumeration
//    //        var toCull = new List<EnemyAttack>();

//    //        foreach (EnemyAttack attack in enemyAttacks)
//    //        {
//    //            //Advance attack clocks - stand in for Update()
//    //            attack.Advance(deltaTime);

//    //            //Faux garbage collection - add to list
//    //            if (attack.toCull)
//    //            {
//    //                toCull.Add(attack);
//    //            }
//    //        }

//    //        //Faux garbage collection - remove expired attacks
//    //        foreach (EnemyAttack toRemove in toCull)
//    //        {
//    //            enemyAttacks.Remove(toRemove);
//    //        }
//    //    }
//    //}

//    /// <summary>
//    /// Callback for Enemy Attacks - Fires attack based on type via callback to EnemyCore and removes attack from list. If this clear list, creates next pattern.
//    /// </summary>
//    /// <param name="attack">This is the attack object that is firing</param>
//    public void completeAction(EnemyAttack attack)
//    {
//        ship.AttackImplement(attack.type);
//    }

//    ///// <summary>
//    ///// Creates random pattern of EnemyAttacks and stores them to list, with timing lined up for combo.
//    ///// </summary>
//    ///// <param name="pattern">List of all attacks to create</param>
//    //private void CreatePattern()
//    //{
//    //    enemyAttacks = new List<EnemyAttack>();
//    //    var timer = Random.Range(ship.firstAttackMinimum, ship.firstAttackMaximum); //This will be some random initial delay
//    //    var pattern = PatternSelect(Random.Range(0, 5));
//    //    foreach (AttackType attackType in pattern)
//    //    {
//    //        timer += ship.globalCooldown; //Add delay before attack to timer, to use to line up the attacks in time

//    //        if (attackType == AttackType.delay) { timer += ship.delayLength; }
//    //        else
//    //        {
//    //            var newAttack = new EnemyAttack(this, attackType, timer);
//    //            enemyAttacks.Add(newAttack);
//    //        }
//    //    }
//    //}

//    private EmotionStrength tensionByAttack(AttackType type)
//    {
//        switch (type)
//        {
//            case AttackType.foreTurret:
//            case AttackType.aftTurret:
//                return EmotionStrength.weak;

//            case AttackType.specialAttack:
//                return EmotionStrength.strong;

//            case AttackType.heal:
//                return EmotionStrength.moderate;

//            default:
//                return 0.0f; //This is the bad place!
//        }
//    }

//    /// <summary>
//    /// Converts int into attack pattern
//    /// </summary>
//    /// <param name="pattern">Max 4. Integer coreresponding to pattern</param>
//    /// <returns>List of attacks to be run through as attack pattern</returns>
//    public List<AttackType> PatternSelect(int pattern)
//    {
//        switch (pattern)
//        {
//            case 0:
//                return new List<AttackType>() { AttackType.foreTurret, AttackType.delay, AttackType.foreTurret, AttackType.specialAttack };

//            case 1:
//                return new List<AttackType>() { AttackType.aftTurret, AttackType.delay, AttackType.aftTurret, AttackType.heal };

//            case 2:
//                return new List<AttackType>() { AttackType.foreTurret, AttackType.foreTurret, AttackType.specialAttack };

//            case 3:
//                return new List<AttackType>() { AttackType.aftTurret, AttackType.aftTurret, AttackType.heal };

//            case 4:

//                return new List<AttackType>() { AttackType.foreTurret, AttackType.foreTurret, AttackType.specialAttack, AttackType.aftTurret, AttackType.heal };

//            default:
//                return new List<AttackType>() { AttackType.aftTurret, AttackType.foreTurret, AttackType.foreTurret, AttackType.specialAttack };
//        }
//    }
//}

///// <summary>
///// Automatically counts down, executes
///// Created during attack pattern creation
///// </summary>
//public class EnemyAttack
//{
//    #region Mechanics

//    public AttackType type { get; private set; }
//    private float timing;

//    #endregion Mechanics

//    #region Bookkeeping and References

//    public bool toCull { get; private set; } = false;
//    private CombatAI combatAI;

//    #endregion Bookkeeping and References

//    public EnemyAttack(CombatAI _AI, AttackType _type, float _time)
//    {
//        combatAI = _AI;
//        type = _type;
//        timing = _time;
//    }

//    public EnemyAttack(CombatAI _AI, AttackType _type, float _time, float _triggerTime)
//    {
//        combatAI = _AI;
//        type = _type;
//        timing = _time;
//    }

//    /// <summary>
//    /// Advances timing towards attack and adjusts tension variable. Fires attack if timing<=0
//    /// </summary>
//    /// <param name="deltaTime">Time.deltaTime passed from CombatAI.Advance</param>
//    public void Advance(float deltaTime)
//    {
//        //Timing counts down until the attack is implemented. Tracks flares, tension, and implementation
//        timing -= deltaTime;
//        if (timing <= 0.0f) //If timing is exhausted, execute action via callback (probably? gotta figure this out)
//        {
//            combatAI.completeAction(this);
//            toCull = true;
//        }
//        //tensionCalculate(timing);
//    }
//}