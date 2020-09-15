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
public enum AttackType { foreTurret, aftTurret, specialAttack, heal }

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
    private CombatAI combatAI;
    private PlayerShip player;
    private EnemyShip enemy;

    #endregion References

    #region Mechanics

    private bool alive = true;
    private bool usedSpecial = false;
    public bool healing { get; private set; } = false;
    private float jam;

    #endregion Mechanics

    #region Setup

    /// <summary>
    /// Performs initial reference setting and attribute setting.
    /// </summary>
    /// <param name="_stage"></param>
    /// <param name="managerObj"></param>
    public void SetupWave(int _stage, GameObject managerObj)
    {
        //attackSpeed = mainEnemy.getAttackSpeed();
        //combatAI = new CombatAI(this, affect, attr);
        affect = managerObj.GetComponent<AffectManager>();

        StartCoroutine(flyIn());
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

        //readyForCombat(); //triggers beginning of combat stuff
        //StartCoroutine(RunCombatAI());
    }

    #endregion Special Effects and Animations

    #region Attacks and Targetting

    /// <summary>
    /// Creates flare and sets logic for Heal, countered by Fusion Cannon
    /// </summary>
    public void SetHealFrame()
    {
        //   mainEnemy.specialIndicator(Color.green, attr.healDelay);
        healing = true;
    }

    #endregion Attacks and Targetting
}