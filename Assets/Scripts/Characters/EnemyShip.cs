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

    public float comboStartDelayMinimum = 3f, comboStartDelayMaximum = 5f;

    private GameObject playerShipObj;
    private PlayerShip playerShip;
    public GameObject warpEffect;

    #region Combat AI variables

    private bool comboHeal;
    private bool executeFinisher;

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
        affect.AddUpcomingEnemyAttack(firingFinishEvent);
    }

    protected override Emotion attackPatternValence()
    {
        return new Emotion(EmotionDirection.decrease, EmotionStrength.moderate);
    }

    protected override Emotion attackPatternArousal()
    {
        return new Emotion(EmotionDirection.none, EmotionStrength.none);
    }

    protected override Emotion attackPatternTension()
    {
        return new Emotion(EmotionDirection.increase, EmotionStrength.weak);
    }

    public override void TakeDamage(float _damage)
    {
        base.TakeDamage(_damage);
        //Set base levels - the first few shots aren't actually important to the strength of the emotion
        var damageValence = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
        var damageArousal = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
        var damageTension = new Emotion(EmotionDirection.increase, EmotionStrength.weak);

        affect.CreatePastEvent(damageValence, damageArousal, damageTension, 45.0f);
    }

    public override void TakeHeavyDamage(float _damage, float _jamLength)
    {
        base.TakeHeavyDamage(_damage, _jamLength);
        //Set base levels - the first few shots aren't actually important to the strength of the emotion
        var damageValence = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
        var damageArousal = new Emotion(EmotionDirection.increase, EmotionStrength.strong);
        var damageTension = new Emotion(EmotionDirection.decrease, EmotionStrength.weak);

        affect.CreatePastEvent(damageValence, damageArousal, damageTension, 45.0f);
    }

    public override ProspectiveEvent SpecialProspectiveEvent(float estimatedTime)
    {
        var specialValence = new Emotion(EmotionDirection.decrease, EmotionStrength.moderate);
        var specialArousal = new Emotion(EmotionDirection.decrease, EmotionStrength.weak);
        var specialTension = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);

        return new ProspectiveEvent(specialValence, specialArousal, specialTension, estimatedTime, true, affect);
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
        SelectAttack(); //Begins attack after waiting
    }

    /// <summary>
    /// Selects and executes attack combo
    /// Combo is of random length up to maximum length
    /// </summary>
    protected void SelectAttack()
    {
        var combo = Random.value > 0.5f;
        if (combo)
        {
            var comboTotalShots = Random.Range(warmupShots, totalShots);
            var comboWarmup = Random.Range(warmupShots, comboTotalShots);
            executeFinisher = Random.value > 0.5f; //Coin flip for whether we'll do the finisher or not
            if (executeFinisher)
            {
                specialFiringEvent = SpecialProspectiveEvent(attackPatternFinishTime(comboWarmup, comboTotalShots)); //If we're going to execute a finisher, start building up now
            }
            AttackToggle();
        }
        else
        {
            var toWait = Random.Range(comboStartDelayMinimum, comboStartDelayMaximum);

            specialFiringEvent = SpecialProspectiveEvent(heavyAttackDelay + toWait);
            if (comboHeal) { HealTrigger(); }
            else { HeavyAttackTrigger(); }

            StartCoroutine(ComboInitialWait(toWait));
        }
    }

    /// <summary>
    /// Keeps track of how many shots have been fired, to properly end combo.
    /// </summary>
    protected override void FinishFiring()
    {
        base.FinishFiring();
        // Debug.Log("Finished firing");
        if (executeFinisher)
        {
            StartCoroutine(SpecialSyncDelay());
        }
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
            HeavyAttackTrigger();
        }
    }

    #endregion Combat AI

    public override void HeavyAttackTrigger()
    {
        base.HeavyAttackTrigger();
        HeavyAttack_Sound();
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

