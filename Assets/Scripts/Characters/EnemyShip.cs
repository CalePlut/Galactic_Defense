using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShip : BasicShip
{
    public float specialAttackDamage;
    private GameObject playerShipObj;
    private PlayerShip playerShip;
    public GameObject warpEffect;

    private CombatAI CombatAI;

    public enemyAttributes attr;

    private float jam = 0.0f;

    #region Setup and Bookeeping

    /// <summary>
    /// Creates CombatAI for self and sets references to player objects.
    /// </summary>
    public override void ShipSetup()
    {
        base.ShipSetup();
        CombatAI = new CombatAI(this, affect, attr);
        SetReferences();
        StartCoroutine(RunCombatAI());
    }

    /// <summary>
    /// Tracks disabled status and advances Combat AI Clock
    /// </summary>
    /// <returns></returns>
    private IEnumerator RunCombatAI()
    {
        while (alive)
        {
            if (jam > 0.0f) //if we're jammed, reduce jam.
            {
                jam -= Time.deltaTime;
            }
            else //Otherwise, advance the combat ai
            {
                CombatAI.Advance(Time.deltaTime);
            }
            yield return null;
        }
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

    #region action implementation

    /// <summary>
    /// Callback from CombatAI that implements the current attack
    /// </summary>
    /// <param name="attack">sets the attack to implement</param>
    public void AttackImplement(AttackType attack)
    {
        if (attack == AttackType.foreTurret)  //Attacks with left turret, falls back to centre if left turret is destroyed
        {
            StartCoroutine(FireBroadside(playerShip, cannonPosition.fore, 3));
        }
        else if (attack == AttackType.aftTurret) //Attacks with right turret, falls back to centre if rt is destroyed.
        {
            StartCoroutine(FireBroadside(playerShip, cannonPosition.aft, 2));
        }
        else if (attack == AttackType.specialAttack) //If the player isn't shielded, fire normal lasers. Otherwise, fire fake laser and trigger riposte.
        {
            SpecialAttack();
        }
        else if (attack == AttackType.heal)
        {
            HealTrigger();
        }
    }

    /// <summary>
    /// Deals heavy damage to player ship and disables a part. If player ship is shielded, primes retaliation
    /// </summary>
    public void SpecialAttack()
    {
        var emitter = laserEmitter.position;
        var damage = specialAttackDamage;

        SpawnLaser(emitter, playerShipObj.transform.position);

        if (playerShip.shielded)
        {
            playerShip.SetRetaliate();
        }
        else if (playerShip.healing)
        {
            HealPunish(playerShip, punishShots);
        }
        else
        {
            playerShip.TakeDamage(damage);
            playerShip.PartDisable();
        }
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

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }
}

/// <summary>
/// Manages enemy actions via "CombatAI.Advance, and sends unified enemy Tension value to affect manager"
///
/// </summary>
public class CombatAI
{
    private EnemyShip ship;
    private AffectManager affect;
    private enemyAttributes attr;
    private List<EnemyAttack> enemyAttacks;

    public CombatAI(EnemyShip _core, AffectManager _affect, enemyAttributes _attr)
    {
        ship = _core;
        affect = _affect;
        attr = _attr;
        enemyAttacks = new List<EnemyAttack>();
    }

    /// <summary>
    /// Advances clock on all enemyActions, calculates tension value and sends to Affect Manager.
    /// </summary>
    /// <param name="deltaTime">Unity's Time.deltaTime</param>
    /// <param name="jammed">Are we jammed or not</param>
    public void Advance(float deltaTime)
    {
        if (enemyAttacks.Count == 0) //If we don't have a pattern, create one.

        {
            CreatePattern();
        }
        else
        {
            //Faux garbage collection - list to be removed from list after enumeration
            var toCull = new List<EnemyAttack>();

            foreach (EnemyAttack attack in enemyAttacks)
            {
                //Advance attack clocks - stand in for Update()
                attack.Advance(deltaTime);

                //Faux garbage collection - add to list
                if (attack.toCull)
                {
                    toCull.Add(attack);
                }
            }

            //Faux garbage collection - remove expired attacks
            foreach (EnemyAttack toRemove in toCull)
            {
                enemyAttacks.Remove(toRemove);
            }
        }
    }

    /// <summary>
    /// Sends laser barrage (Reactive shield counter) callback to EnemyCore
    /// </summary>
    public void SetSpecialAttackFrame()
    {
        ship.SpecialIndicator(Color.red, timeByAttack(AttackType.specialAttack));
    }

    /// <summary>
    /// Sends heal (Fusion cannon counter) callback to EnemyCore
    /// </summary>
    public void SetHealFrame()
    {
        ship.HealTrigger();
    }

    /// <summary>
    /// Callback for Enemy Attacks - Fires attack based on type via callback to EnemyCore and removes attack from list. If this clear list, creates next pattern.
    /// </summary>
    /// <param name="attack">This is the attack object that is firing</param>
    public void completeAction(EnemyAttack attack)
    {
        ship.AttackImplement(attack.type);
    }

    /// <summary>
    /// Creates random pattern of EnemyAttacks and stores them to list, with timing lined up for combo.
    /// </summary>
    /// <param name="pattern">List of all attacks to create</param>
    private void CreatePattern()
    {
        enemyAttacks = new List<EnemyAttack>();
        var timer = attr.InitialDelay(); //This will be some random initial delay
        var pattern = PatternSelect(Random.Range(0, 5));
        foreach (AttackType attackType in pattern)
        {
            timer += timeByAttack(attackType); //Add delay before attack to timer, to use to line up the attacks in time
            if (attackType == AttackType.heal || attackType == AttackType.specialAttack)
            {
                var newAttack = new EnemyAttack(this, attackType, timer, timeByAttack(attackType));
                enemyAttacks.Add(newAttack);
            }
            else
            {
                var newAttack = new EnemyAttack(this, attackType, timer);
                enemyAttacks.Add(newAttack);
            }
        }
    }

    /// <summary>
    /// Creates specific EnemyAttacks and stores them to list, with timing lined up for combo.
    /// </summary>
    /// <param name="pattern">List of all attacks to create</param>
    private void CreatePattern(List<AttackType> pattern)
    {
        enemyAttacks = new List<EnemyAttack>();
        var timer = attr.InitialDelay(); //This will be some random initial delay
        foreach (AttackType attackType in pattern)
        {
            timer += timeByAttack(attackType); //Add delay before attack to timer, to use to line up the attacks in time
            if (attackType == AttackType.heal || attackType == AttackType.specialAttack)
            {
                var newAttack = new EnemyAttack(this, attackType, timer, timeByAttack(attackType));
                enemyAttacks.Add(newAttack);
            }
            else
            {
                var newAttack = new EnemyAttack(this, attackType, timer);
                enemyAttacks.Add(newAttack);
            }

            //Create the related affective prospectve event
            var tensionEmotion = new Emotion(EmotionDirection.increase, tensionByAttack(attackType));
            affect.CreateProspectiveEvent(null, null, tensionEmotion, timer, true);
        }
    }

    private float timeByAttack(AttackType type)
    {
        switch (type)
        {
            case AttackType.foreTurret:
            case AttackType.aftTurret:
                return attr.turretDelay;

            case AttackType.specialAttack:
                return attr.specialDelay;

            case AttackType.heal:
                return attr.healDelay;

            default:
                return 0.0f; //Shouldn't happen!
        }
    }

    private EmotionStrength tensionByAttack(AttackType type)
    {
        switch (type)
        {
            case AttackType.foreTurret:
            case AttackType.aftTurret:
                return EmotionStrength.weak;

            case AttackType.specialAttack:
                return EmotionStrength.strong;

            case AttackType.heal:
                return EmotionStrength.moderate;

            default:
                return 0.0f; //This is the bad place!
        }
    }

    /// <summary>
    /// Converts int into attack pattern
    /// </summary>
    /// <param name="pattern">Max 4. Integer coreresponding to pattern</param>
    /// <returns>List of attacks to be run through as attack pattern</returns>
    public List<AttackType> PatternSelect(int pattern)
    {
        switch (pattern)
        {
            case 0:
                return new List<AttackType>() { AttackType.foreTurret, AttackType.foreTurret, AttackType.specialAttack };

            case 1:
                return new List<AttackType>() { AttackType.aftTurret, AttackType.aftTurret, AttackType.heal };

            case 2:
                return new List<AttackType>() { AttackType.foreTurret, AttackType.foreTurret, AttackType.specialAttack, AttackType.aftTurret, AttackType.aftTurret, AttackType.heal };//FFSAAH
            case 3:
                return new List<AttackType>() { AttackType.aftTurret, AttackType.aftTurret, AttackType.heal, AttackType.foreTurret, AttackType.foreTurret, AttackType.specialAttack };

            case 4:
                if (Random.value < 0.5f)
                {
                    return new List<AttackType>() { AttackType.aftTurret, AttackType.heal, AttackType.foreTurret, AttackType.foreTurret, AttackType.specialAttack };
                }
                else
                {
                    return new List<AttackType>() { AttackType.foreTurret, AttackType.foreTurret, AttackType.specialAttack, AttackType.aftTurret, AttackType.heal }; //R-L-L-R-L-L-Special_Heal
                }
            default:
                return new List<AttackType>() { AttackType.aftTurret, AttackType.foreTurret, AttackType.foreTurret, AttackType.specialAttack };
        }
    }
}

/// <summary>
/// Automatically counts down, executes
/// Created during attack pattern creation
/// </summary>
public class EnemyAttack
{
    #region Mechanics

    public AttackType type { get; private set; }
    private float timing;
    private Emotion tension;
    private bool specialTrigger = false; //Tracks whether we've sent the parry frame
    public float triggerTime { get; private set; }

    #endregion Mechanics

    #region Bookkeeping and References

    public bool toCull { get; private set; } = false;
    private CombatAI combatAI;

    #endregion Bookkeeping and References

    public EnemyAttack(CombatAI _AI, AttackType _type, float _time)
    {
        combatAI = _AI;
        type = _type;
        timing = _time;
        triggerTime = 0.0f;
    }

    public EnemyAttack(CombatAI _AI, AttackType _type, float _time, float _triggerTime)
    {
        combatAI = _AI;
        type = _type;
        timing = _time;
        triggerTime = _triggerTime;
    }

    /// <summary>
    /// Advances timing towards attack and adjusts tension variable. Fires attack if timing<=0
    /// </summary>
    /// <param name="deltaTime">Time.deltaTime passed from CombatAI.Advance</param>
    public void Advance(float deltaTime)
    {
        //Timing counts down until the attack is implemented. Tracks flares, tension, and implementation
        timing -= deltaTime;

        specialTriggerCalculate(timing);

        if (timing <= 0.0f) //If timing is exhausted, execute action via callback (probably? gotta figure this out)
        {
            combatAI.completeAction(this);
            toCull = true;
        }
        //tensionCalculate(timing);
    }

    private void specialTriggerCalculate(float timing)
    {
        if (timing < triggerTime)
        {
            if (!specialTrigger)
            {
                if (type == AttackType.specialAttack)
                {
                    combatAI.SetSpecialAttackFrame();
                }
                // if (type == AttackType.heal)
                //    {
                //combatAI.SetHealFrame();
                //   }
                specialTrigger = true;
            }
        }
    }
}