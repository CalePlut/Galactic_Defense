using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
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

    public float comboStartDelayMinimum = 2f, comboStartDelayMaximum = 4f;

    private GameObject playerShipObj;
    private PlayerShip playerShip;
    public GameObject warpEffect;

    #region Combat AI variables

    private bool comboHeal;
    private bool finish_with_heavy;
    private bool isHeavyQueued;

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
    /// Sets up important ship stuff, but doesn't start firing at all - used for tutorial
    /// </summary>
    public void tutorialSetup()
    {
        base.ShipSetup();
        SetReferences();
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
    /// Finds player and sets reference, should only be called during setup.
    /// </summary>
    private void SetReferences()
    {
        playerShipObj = GameObject.Find("Player Ship");
        playerShip = playerShipObj.GetComponent<PlayerShip>();
        playerShip.SetEnemyReference(this.gameObject);
    }

    protected override BasicShip myTarget()
    {
        if (playerShip != null)
        {
            return playerShip;
        }
        else
        {
            return base.myTarget();
        }
    }


    protected override GameObject myTargetObject()
    {
        if (playerShipObj != null) { return playerShipObj; }
        else
        {
            return base.myTargetObject();
        }
    }

    #endregion Setup and Bookeeping

    #region Affect

    /// <summary>
    /// Finishes PredictAttack with adding to enemy list
    /// </summary>
    protected override void PredictAttacks()
    {
        base.PredictAttacks();
        var firingTime = attackPatternFinishTime(warmupShots, totalShots);
        PreGLAM.Queue_event(new Known_event(affectVariables.E_attack.x, affectVariables.E_attack.y, 1.0f+playerShip.shieldPercent(), firingTime));
        var totalDamage = attackPattern_total_damage(warmupShots, totalShots, basicAttackDamage);

        if (totalDamage > playerShip.health && !playerShip.shielded)
        {
            var v = affectVariables.P_death.x;
            var t = affectVariables.P_death.y;
            var magnitude = 1.0f + playerShip.shieldCooldown;
            PreGLAM.Queue_event(new Likely_event(v, t, magnitude, firingTime, likely_type.player_death));
        }
    }


    public override void ShieldBreak()
    {
        base.ShieldBreak();
        var v = affectVariables.E_shield_down.x;
        var magnitude = 1.0f + shieldPercent();

        PreGLAM.Queue_event(new Past_event(v, magnitude, PreGLAM));
    }

    #endregion Affect

    #region death

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

    #endregion death

    #region Combat AI

    /// <summary>
    /// Sets initial combo values
    /// Begins wait before first AdvanceCombo
    /// </summary>
    private void ComboSetup()
    {
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
        var hpPercent = health / maxHealth; //Chance of triggering healing increases as health decreases
        comboHeal = Random.value > hpPercent;
        //Debug.Log("Combo heal = " + comboHeal);

        var timer = time;
        while (timer > 0.0f)
        {
            if (jamTimer <= 0.0f) //If we're jammed, we don't advance
            {
                timer -= Time.deltaTime;
            }
            yield return null;
        }
        SelectAttack(); //Begins attack after waiting
    }

    /// <summary>
    /// Selects and executes attack combo
    /// Combo is of random length up to maximum length
    /// </summary>
    protected void SelectAttack()
    {
       // Debug.Log("Enemy selecting attack");
        var combo = Random.value > 0.5f; //The combo attack is most common, and fires attack combos before possibly ending with heavy attack
        if (combo)
        {
            var comboTotalShots = Random.Range(warmupShots, totalShots);
            var comboWarmup = Random.Range(warmupShots, comboTotalShots);
            finish_with_heavy = Random.value > 0.5f; //Coin flip for whether we'll do the finisher or not
            if (finish_with_heavy)
            {
                var delay = attackPatternFinishTime(comboWarmup, comboTotalShots) + heavyAttackDelay;

                var v = affectVariables.E_heavy.x;
                var t = affectVariables.E_heavy.y;
                var magnitude = 1.0f + (1.0f - playerShip.healthPercent());

                PreGLAM.Queue_event(new Known_event(v, t, magnitude, delay));
            }
            AttackToggle();
        }
        else
        {
            if (isHeavyQueued == false)
            {
                var toWait = Random.Range(comboStartDelayMinimum, comboStartDelayMaximum);

                var v = affectVariables.E_heavy.x;
                var t = affectVariables.E_heavy.y;
                var magnitude = 1.0f + (1.0f - playerShip.healthPercent());

                if (comboHeal) {
                    HealTrigger();
                }
                else {
                    PreGLAM.Queue_event(new Known_event(v, t, magnitude, toWait));
                    HeavyAttackTrigger();
                }
            }
        }
    }




    /// <summary>
    /// Keeps track of how many shots have been fired, to properly end combo.
    /// </summary>
    protected override void FinishFiring()
    {
        base.FinishFiring();
        PreGLAM.Queue_prospective_cull(likely_type.player_death); //If we haven't killed the player with this attack by the end of a combo, we no longer think we might
    }

    /// <summary>
    /// Called whenever an action is finished. Selects next action after a short delay.
    /// </summary>
    protected override void finishAction()
    {
        base.finishAction();
        StartCoroutine(ComboInitialWait(Random.Range(comboStartDelayMinimum, comboStartDelayMaximum)));
    }


    #endregion Combat AI

    public override void HeavyAttackTrigger()
    {
        isHeavyQueued = true;
        base.HeavyAttackTrigger();

        HeavyAttack_Sound();

        if (heavyAttackDamage > playerShip.health && !playerShip.shielded)
        {
            var v = affectVariables.E_death.x;
            var t = affectVariables.E_death.y;
            var magnitude = 1.0f + playerShip.shieldCooldown;
            PreGLAM.Queue_event(new Likely_event(v, t, magnitude, heavyAttackDelay, likely_type.player_death));
        }
    }

    public override void HeavyAttack(GameObject targetObj)
    {
        isHeavyQueued = false;
        base.HeavyAttack(targetObj);
        PreGLAM.Queue_prospective_cull(likely_type.player_death);
    }

    public override void HealTrigger()
    {
        Debug.Log("Enemy healing");
        ShieldsDown();
        base.HealTrigger();
        var v = affectVariables.E_heal.x;
        var t = affectVariables.E_heal.y;
        var magnitude = 1.0f + (1.0f-healthPercent());
        PreGLAM.Queue_event(new Likely_event(v, t, magnitude, healDelay, likely_type.enemy_heal));
    }

    public override void HealInterrupt()
    {
        Debug.Log("Enemy heal interrupted");
        base.HealInterrupt();
        PreGLAM.Queue_prospective_cull(likely_type.enemy_heal);
        var v = affectVariables.E_heal.x;
        var magnitude = 1.0f + (1.0f - healthPercent());
        PreGLAM.Queue_event(new Past_event(-v, magnitude, PreGLAM));
    }
    public override void Heal()
    {
        Debug.Log("Enemy healed");
        base.Heal();
        PreGLAM.Queue_prospective_cull(likely_type.enemy_heal);
        PreGLAM.Queue_prospective_cull(likely_type.enemy_death); //This probably isn't necessary, but it makes me feel safe
        var v = affectVariables.E_heal.x;
        var magnitude = 1.0f + (1.0f - healthPercent());
        PreGLAM.Queue_event(new Past_event(v, magnitude, PreGLAM));
    }




    #region Effects and Overrides
    void HeavyAttack_Sound()
    {
        var clip = SFX_Heavy_attack_windup[0];
        if (level == 2)
        {
            clip = SFX_Heavy_attack_windup[1];
        }
        else if (level == 3)
        {
            clip = SFX_Heavy_attack_windup[2];
        }
        SFX.PlayOneShot(clip);
    }

        #endregion

        // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }
}

