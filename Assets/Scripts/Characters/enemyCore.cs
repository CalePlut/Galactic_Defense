using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack type is used only in selecting which move to fire by the AI
/// </summary>
public enum attackType { autoAttack, specialAttack, heal, respawnTurret }

/// <summary>
/// The EnemyCore (not to be confused with enemyBase) is the point of control for each wave. This script manages the fly-in, setups, and controls the move selection (e.g. what fires)
/// </summary>
public class enemyCore : MonoBehaviour
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

    #endregion References

    #region Mechanics

    private bool alive = true;
    private bool usedSpecial = false;
    public bool healing { get; private set; } = false;
    private bool jammed;

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
        affect.interruptFusionCannon();
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
        StartCoroutine(AutoAttack());
    }

    #endregion Special Effects and Animations

    #region Combat AI

    /// <summary>
    /// Implements the selected attack by triggering associated functions
    /// </summary>
    /// <param name="attack"></param>
    private void attackImplement(attackType attack)
    {
        var target = randomTargetSelect();
        if (attack == attackType.autoAttack)  //Selects turret. If turret is alive, fire from it, otherwise fire the less-powerful main enemies cannon. We keep it this way so that destroying a single turret still helps.
        {
            autoAttack(target);
        }
        else if (attack == attackType.specialAttack) //If the player isn't shielded, fire normal lasers. Otherwise, fire fake laser and trigger riposte.
        {
            specialAttack();
            affect.enemyAbility();
        }
        else if (attack == attackType.heal)
        {
            Heal();
        }
        else if (attack == attackType.respawnTurret) //Note: should only be called if at least one turret is dead anyways.
        {
            RespawnTurret();
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
    /// Heals everyone for 75% of their missing health
    /// </summary>
    private void Heal()
    {
        if (healing)
        {
            if (leftTurret.alive)
            {
                leftTurret.receiveHealing(0.75f);
            }
            if (rightTurret.alive)
            {
                rightTurret.receiveHealing(0.75f);
            }
            mainEnemy.receiveHealing(0.75f);
        }
    }

    /// <summary>
    /// Fires the special attack, unless the player is shielded, in which case we fire a fake laser and prep for the riposte mechanics
    /// </summary>
    private void specialAttack()
    {
        if (!PlayerShip.shielded)
        {
            mainEnemy.specialAttack();
        }
        else
        {
            jammed = true;
            mainEnemy.reactiveShieldJam();
            if (leftTurret.alive) { leftTurret.reactiveShieldJam(); }
            if (rightTurret.alive) { rightTurret.reactiveShieldJam(); }
            mainEnemy.dummyLaser();
            frigate.absorbedAttack();
        }
    }

    /// <summary>
    /// Fires a basic attack at the provided target.
    /// This is vaguely confusing --- Prioritize using turrets to attack, with random selection.
    /// If the selected turret is dead, fire the (far weaker) central enemy's weapon.
    /// </summary>
    /// <param name="target"></param>
    private void autoAttack(PlayerShip target)
    {
        var left = Random.value < 0.5f;
        if (left)
        {
            if (leftTurret.alive)
            {
                leftTurret.fireWeapons(target, "Enemy");
            }
            else { mainEnemy.fireWeapons(target, "Enemy"); }
        }
        else
        {
            if (rightTurret.alive)
            {
                rightTurret.fireWeapons(target, "Enemy");
            }
            else { mainEnemy.fireWeapons(target, "Enemy"); }
        }
    }

    /// <summary>
    /// Selects which type of attack to use, based on random chance and some logic.
    /// </summary>
    /// <returns></returns>
    private attackType attackSelect()
    {
        var toReturn = attackType.autoAttack;
        var randAttack = Random.Range(0, 100);
        //Debug.Log("RandAttack = " + randAttack);
        if (randAttack < 75) //75% chance of using standard attack (turret-empowered if available)
        {
            toReturn = attackType.autoAttack;
            usedSpecial = false;
        }
        else if (randAttack < 90) //15% chance of using Special Attack
        {
            if (!usedSpecial)
            {
                toReturn = attackType.specialAttack;
                usedSpecial = true;
            }
            else
            {
                toReturn = attackType.autoAttack; //If we used special attack last cycle, auto-attack instead
                usedSpecial = false;
            }
        }
        else if (randAttack < 100) //10% chance of resurrecting. If all turrets are alive, heal
        {
            if (!leftTurret.alive || !rightTurret.alive) //if either turret is dead, set action to respawn
            {
                toReturn = attackType.respawnTurret;
            }
            else //If both are alive, heal.
            {
                toReturn = attackType.heal;
            }
            usedSpecial = false;
        }
        if (GameManager.tutorial) { toReturn = attackType.autoAttack; }
        return toReturn;
    }

    /// <summary>
    /// Tracks time to automatically select and implement attacks and actions
    /// </summary>
    /// <returns></returns>
    private IEnumerator AutoAttack()
    {
        var counter = 0.0f;
        var toAttack = attackType.autoAttack;
        var flare = false;

        while (alive)
        {
            if (jammed) //If we were jammed, add 2 seconds to counter.
            {
                counter += 2f;
                jammed = false;
            }

            if (counter > 0.0f)
            {
                counter -= Time.deltaTime;
                if (counter <= 2.5f)
                {
                    if (toAttack == attackType.specialAttack) //Special Attack is countered by reactive shield, and glows red
                    {
                        if (!flare)
                        {
                            affect.setParryFrame(true);
                            mainEnemy.specialIndicator(Color.red);
                            flare = true;
                        }
                    }
                    if (toAttack == attackType.heal) //Heal is interruptible by fusion cannon, and glows green
                    {
                        if (!flare)
                        {
                            mainEnemy.specialIndicator(Color.green);
                            flare = true;
                            healing = true;
                        }
                    }
                    if (toAttack == attackType.respawnTurret) //Respawn glows yellow - nothing you can do
                    {
                        if (!flare)
                        {
                            mainEnemy.specialIndicator(Color.yellow);
                            flare = true;
                            //healing = true;
                        }
                    }
                }
            }
            else
            {
                //Trigger ability and re-set the tracking variables
                attackImplement(toAttack);
                flare = false;
                healing = false;

                //Next, selects next attack and begins countdown to it.
                toAttack = attackSelect();
                if (toAttack == attackType.autoAttack) //If we have selected AutoAttack, set time remaining to the attack speed
                {
                    counter = attackSpeed;
                }
                else //Otherwise, set timer to 3 seconds (2 measures) to give parry possibility.
                {
                    counter = 3.0f;
                    affect.setParryFrame(false);
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Selects one of the player ships as a target at random. If non-alive ship selected, select random alive ship.
    /// </summary>
    /// <returns></returns>
    private PlayerShip randomTargetSelect()
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

    #endregion Combat AI
}