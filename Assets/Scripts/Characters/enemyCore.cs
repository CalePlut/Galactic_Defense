using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Configuration;
using System.Threading;
using UnityEngine;

/// <summary>
/// Attack type is used only in selecting which move to fire by the AI
/// </summary>
public enum AttackType { leftTurret, rightTurret, specialAttack, heal }

/// <summary>
/// The EnemyCore Controls the enemy behaviour. Uses CombatAI to execute attack patterns and track tension.
/// </summary>
public class EnemyCore : MonoBehaviour
{
    #region Attributes

    public float attackSpeed = 1.5f;
    public enemyAttributes attr;

    #endregion Attributes

    #region References

    private AffectManager affect;
    public Turret leftTurret, rightTurret;
    public EnemyShip mainEnemy;
    private FrigateShip frigate;
    private IntelShip artillery;
    private SupportShip tender;
    public GameObject fusionChainReaction;
    private CombatAI combatAI;

    #endregion References

    #region Mechanics

    private bool alive = true;
    private bool usedSpecial = false;
    public bool healing { get; private set; } = false;
    private float jam;
    private int leftRespawn = 0, rightRespawn = 0;

    #endregion Mechanics

    #region Messages and bookkeeping

    /// <summary>
    /// Returns list of current wave
    /// </summary>
    /// <returns>References for entire wave</returns>
    public List<EnemyBase> getWaveList()
    {
        return new List<EnemyBase>() { leftTurret, mainEnemy, rightTurret };
    }

    /// <summary>
    /// Returns list of two turrets
    /// </summary>
    /// <returns>List of two turret references</returns>
    public List<Turret> getTurrets()
    {
        return new List<Turret>() { leftTurret, rightTurret };
    }

    /// <summary>
    /// Returns central ship
    /// </summary>
    /// <returns>Main enemy reference</returns>
    public EnemyShip getMainShip()
    {
        return (EnemyShip)mainEnemy;
    }

    /// <summary>
    /// Destroys the two turrets and sets self to not alive
    /// </summary>
    public void destroyTurretsOnDeath()
    {
        alive = false;
        if (leftTurret != null)
        {
            leftTurret.destroyTurret();
        }
        if (rightTurret != null)
        {
            rightTurret.destroyTurret();
        }
    }

    /// <summary>
    /// If we get hit by a Q during the heal, we overload, dealing large damage to turrets and causing big explosion
    /// </summary>
    public void fusionInterrupt()
    {
        healing = false;
        StartCoroutine(waitForFusion());

        var valenceEmotion = new Emotion(EmotionDirection.increase, EmotionStrength.strong);
        var tensionEmotion = new Emotion(EmotionDirection.decrease, EmotionStrength.weak);
        affect.CreatePastEvent(valenceEmotion, null, tensionEmotion, 10.0f);
    }

    /// <summary>
    /// Waits for the fusion cannon to connect before destroying turrets, etc.
    /// </summary>
    /// <returns></returns>
    private IEnumerator waitForFusion()
    {
        yield return new WaitForSeconds(0.75f);

        var personalExplode = Instantiate(fusionChainReaction, transform.position, Quaternion.identity, this.transform); //Kaboom!
        if (leftTurret.alive)
        {
            leftTurret.receiveDamage(20);
        }
        if (rightTurret.alive)
        {
            rightTurret.receiveDamage(20);
        }
    }

    #endregion Messages and bookkeeping

    #region Setup

    /// <summary>
    /// Performs initial reference setting and attribute setting.
    /// </summary>
    /// <param name="_stage"></param>
    /// <param name="managerObj"></param>
    public void setupWave(int _stage, GameObject managerObj)
    {
        mainEnemy.setupEnemy(_stage, managerObj);
        leftTurret.setupEnemy(_stage, managerObj);
        rightTurret.setupEnemy(_stage, managerObj);
        attackSpeed = mainEnemy.getAttackSpeed();
        affect = managerObj.GetComponent<AffectManager>();
        combatAI = new CombatAI(this, affect, attr);

        StartCoroutine(flyIn());
    }

    /// <summary>
    /// Sets references to player ships
    /// </summary>
    /// <param name="_frigate"></param>
    /// <param name="_intel"></param>
    /// <param name="_support"></param>
    public void setEnemyReferences(FrigateShip _frigate, IntelShip _intel, SupportShip _support)
    {
        frigate = _frigate;
        artillery = _intel;
        tender = _support;

        mainEnemy.setReferences(_frigate, _intel, _support);
    }

    #endregion Setup

    #region Special Effects and Animations

    /// <summary>
    /// Animates the opening of the warp gate and the flying in, starts setup block.
    /// </summary>
    /// <returns></returns>
    protected IEnumerator flyIn()
    {
        //randomTimeSetup();
        var targetPos = transform.position;
        var prewarpPos = transform.position;
        prewarpPos.z += 500;
        transform.localScale = new Vector3(1, 1, 10);
        transform.position = prewarpPos;
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
            transform.localScale = new Vector3(1, 1, z);
            yield return null;
        }
        transform.position = targetPos;
        transform.localScale = new Vector3(1, 1, 1);
        mainEnemy.playWarp();
        leftTurret.playWarp();
        rightTurret.playWarp();
        //readyForCombat(); //triggers beginning of combat stuff
        StartCoroutine(RunCombatAI());
    }

    #endregion Special Effects and Animations

    #region Attacks and Targetting

    /// <summary>
    /// Callback from CombatAI that implements the current attack
    /// </summary>
    /// <param name="attack">sets the attack to implement</param>
    public void AttackImplement(AttackType attack)
    {
        var target = RandomTargetSelect();
        if (attack == AttackType.leftTurret)  //Attacks with left turret, falls back to centre if left turret is destroyed
        {
            AutoAttack(target, true);
        }
        else if (attack == AttackType.rightTurret) //Attacks with right turret, falls back to centre if rt is destroyed.
        {
            AutoAttack(target, false);
        }
        else if (attack == AttackType.specialAttack) //If the player isn't shielded, fire normal lasers. Otherwise, fire fake laser and trigger riposte.
        {
            SpecialAttack();
        }
        else if (attack == AttackType.heal)
        {
            Heal();
        }
    }

    /// <summary>
    /// Respawns a turret. Respawns only the dead turret, and if both are dead, respawns one at random.
    /// </summary>
    private void RespawnTurret()
    {
        //If neither turret is alive, pick a random one to respawn
        if (!leftTurret.alive && !rightTurret.alive)
        {
            var left = Random.value < 0.5f;
            if (left) { leftTurret.beginTurretRespawn(); }
            else { rightTurret.beginTurretRespawn(); }
        }
        //If only right is alive, respawn right
        else if (!rightTurret.alive)
        {
            rightTurret.beginTurretRespawn();
        }
        //Samesies for other side
        else if (!leftTurret.alive)
        {
            leftTurret.beginTurretRespawn();
        }
    }

    /// <summary>
    /// Respawns a specific turret
    /// </summary>
    /// <param name="left">true if respawning left turret, false if respawning right turret.</param>
    private void RespawnTurret(bool left)
    {
        if (left)
        {
            if (!leftTurret.alive)
            {
                leftTurret.beginTurretRespawn();
            }
            else { Debug.Log("Tried to respawn living left turret"); }
        }
        else
        {
            if (!rightTurret.alive)
            {
                rightTurret.beginTurretRespawn();
            }
            else { Debug.Log("Tried to respawn living right turret"); }
        }
    }

    /// <summary>
    /// Heals everyone for 75% of their missing health
    /// </summary>
    private void Heal()
    {
        if (healing)
        {
            if (leftTurret.alive)
            {
                leftTurret.ReceiveHealing(0.75f);
            }
            if (rightTurret.alive)
            {
                rightTurret.ReceiveHealing(0.75f);
            }
            mainEnemy.ReceiveHealing(0.75f);
        }
    }

    /// <summary>
    /// Fires the special attack, unless the player is shielded, in which case we fire a fake laser and prep for the riposte mechanics
    /// </summary>
    private void SpecialAttack()
    {
        if (!PlayerShip.shielded)
        {
            mainEnemy.specialAttack();
        }
        else
        {
            jam = frigate.jamDuration;
            mainEnemy.reactiveShieldJam(jam);
            if (leftTurret.alive) { leftTurret.reactiveShieldJam(jam); }
            if (rightTurret.alive) { rightTurret.reactiveShieldJam(jam); }
            mainEnemy.dummyLaser();
            frigate.absorbedSpeccialAttack();
        }
    }

    /// <summary>
    /// Fires a basic attack at the provided target, from the selected turret
    /// If the turret is unavailable, fire weaker central blast and add to respawn count
    /// After 2 failed attacks, respawn the turret
    /// </summary>
    /// <param name="target"></param>
    private void AutoAttack(PlayerShip target, bool left)
    {
        if (left)
        {
            if (leftTurret.alive)
            {
                leftTurret.FireWeapons(target, "Enemy");
            }
            else
            {
                leftRespawn++;
                if (leftRespawn > 2)
                {
                    RespawnTurret(true);
                    leftRespawn = 0;
                }
                else
                {
                    mainEnemy.FireWeapons(target, "Enemy");
                }
            }
        }
        else
        {
            if (rightTurret.alive)
            {
                rightTurret.FireWeapons(target, "Enemy");
            }
            else
            {
                rightRespawn++;
                if (rightRespawn > 2)
                {
                    RespawnTurret(false);
                    rightRespawn = 0;
                }
                else
                {
                    mainEnemy.FireWeapons(target, "Enemy");
                }
            }
        }
    }

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
                combatAI.Advance(Time.deltaTime);
            }
            yield return null;
        }
    }

    /// <summary>
    /// Creates flare and sets logic for Spec Attack, countered by Reactive Shield
    /// </summary>
    public void SetSpecialAttackFrame()
    {
        mainEnemy.specialIndicator(Color.red, attr.specialDelay);
    }

    /// <summary>
    /// Creates flare and sets logic for Heal, countered by Fusion Cannon
    /// </summary>
    public void SetHealFrame()
    {
        mainEnemy.specialIndicator(Color.green, attr.healDelay);
        healing = true;
    }

    /// <summary>
    /// Selects one of the player ships as a target at random. If non-alive ship selected, select random alive ship.
    /// </summary>
    /// <returns></returns>
    private PlayerShip RandomTargetSelect()
    {
        var randomSelect = Random.Range(0, 3);
        if (randomSelect == 1) //Attempts to target artillery
        {
            if (artillery.alive)
            {
                return artillery;
            }
            else
            {
                var tend = Random.value < 0.5f;
                if (tend)
                {
                    if (tender.alive) { return tender; }
                    else return frigate;
                }
                else return frigate;
            }
        }
        else if (randomSelect == 2) //Attempts to target tender
        {
            if (tender.alive)
            {
                return tender;
            }
            else
            {
                var arty = Random.value < 0.5f;
                if (arty)
                {
                    if (artillery.alive) { return artillery; }
                    else return frigate;
                }
                else return frigate;
            }
        }
        else //No need for logic on this one, if the frigate's dead we've lost.
        {
            return frigate;
        }
    }

    #endregion Attacks and Targetting
}

/// <summary>
/// Manages enemy actions via "CombatAI.Advance, and sends unified enemy Tension value to affect manager"
///
/// </summary>
public class CombatAI
{
    private EnemyCore core;
    private AffectManager affect;
    private enemyAttributes attr;
    private List<EnemyAttack> enemyAttacks;

    public CombatAI(EnemyCore _core, AffectManager _affect, enemyAttributes _attr)
    {
        core = _core;
        affect = _affect;
        attr = _attr;

        CreatePattern();
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
        core.SetSpecialAttackFrame();
    }

    /// <summary>
    /// Sends heal (Fusion cannon counter) callback to EnemyCore
    /// </summary>
    public void SetHealFrame()
    {
        core.SetHealFrame();
    }

    /// <summary>
    /// Callback for Enemy Attacks - Fires attack based on type via callback to EnemyCore and removes attack from list. If this clear list, creates next pattern.
    /// </summary>
    /// <param name="attack">This is the attack object that is firing</param>
    public void completeAction(EnemyAttack attack)
    {
        core.AttackImplement(attack.type);
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
            case AttackType.leftTurret:
            case AttackType.rightTurret:
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
            case AttackType.leftTurret:
            case AttackType.rightTurret:
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
                return new List<AttackType>() { AttackType.rightTurret, AttackType.leftTurret, AttackType.specialAttack }; //R-L-Special

            case 1:
                return new List<AttackType>() { AttackType.leftTurret, AttackType.rightTurret, AttackType.heal }; //L-R-Heal

            case 2:
                return new List<AttackType>() { AttackType.leftTurret, AttackType.rightTurret, AttackType.rightTurret, AttackType.specialAttack, AttackType.heal }; //L-R-R-Special-Heal

            case 3:
                return new List<AttackType>() { AttackType.rightTurret, AttackType.leftTurret, AttackType.leftTurret, AttackType.heal, AttackType.specialAttack }; //R-L-L-Heal-Special

            case 4:
                if (Random.value < 0.5f)
                {
                    return new List<AttackType>() { AttackType.leftTurret, AttackType.rightTurret, AttackType.leftTurret, AttackType.rightTurret, AttackType.specialAttack, AttackType.heal }; //L-R-L-R-Special-Heal
                }
                else
                {
                    return new List<AttackType>() { AttackType.rightTurret, AttackType.leftTurret, AttackType.rightTurret, AttackType.leftTurret, AttackType.specialAttack, AttackType.heal }; //R-L-R-L-Special_Heal
                }
            default:
                return new List<AttackType>() { AttackType.rightTurret, AttackType.leftTurret, AttackType.specialAttack };
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
                if (type == AttackType.heal)
                {
                    combatAI.SetHealFrame();
                }
                specialTrigger = true;
            }
        }
    }
}