using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SciFiArsenal;
using UnityEditor;
using System;

public class PlayerShip : BasicShip
{
    #region Cooldown varibles

    [Header("Cooldowns")]
    public float attackCooldown = 1.5f;

    public float healCooldown = 2.5f;

    public float ultimateCooldown = 30.0f;

    #endregion Cooldown varibles

    #region Enemy references

    private GameObject enemyShipObj;
    private EnemyShip enemyShip;

    #endregion Enemy references

    #region Spawn points

    [Header("Positioning objects")]
    public Transform retaliateCannon;

    #endregion Spawn points

    #region Effect  and weapon references

    [Header("Effects and Weapons")]
    public GameObject absorbObject;

    private float parryFrame;

    public GameObject disableShotPrefab;

    private int shots = 0;
    public comboTracker comboTracker;

    [Header("Ship-specific Audio")]
    public AudioClip SFX_absorbAttack;//, SFX_retaliate;

    public AudioClip SFX_Heal;

    #endregion Effect  and weapon references

    #region Button references

    [Header("Hotbar Buttons")]
    public basicButton attackButton;

    public basicButton absorbButton;
    public basicButton healButton;
    public basicButton ultimateButton;
    public buttonManager buttonManager;

    #endregion Button references

    #region Mechanic and Attribute variables

    public bool godMode = false;

    private bool retaliate = false; //Variables for shield mechanics

    private int attackLevel = 1, defenseLevel = 1, specialLevel = 1;

    #endregion Mechanic and Attribute variables

    #region Prospective event variables

    private Event retaliateEvent;

    private ProspectiveEvent playerDeath;
    private ProspectiveEvent nextCombo;

    #endregion Prospective event variables

    #region Setup and bookkeeping

    /// <summary>
    /// Called whenever there's a new enemy
    /// </summary>
    /// <param name="_enemyShipObj"></param>
    public void SetEnemyReference(GameObject _enemyShipObj)
    {
        enemyShipObj = _enemyShipObj;
        enemyShip = enemyShipObj.GetComponent<EnemyShip>();
    }

    /// <summary>
    /// Overrides SetDefense to also scale shield stamina
    /// </summary>
    /// <param name="level"></param>
    public override void SetDefense(int level)
    {
        base.SetDefense(level);
        parryFrame = attr.parryFrame(level);

        //shieldStamina.SetStamina(shieldDuration);

        if (godMode) //Here's where all the godmode code is because I'm lazy
        {
            laserDamage = 100.0f;
            turretDamage = 100.0f;
            maxHealth = 10000000.0f;
            health = maxHealth;
            healthBar.Refresh(maxHealth, health);
            healPercent = 100.0f;
        }
    }

    public void UpgradeAttack()
    {
        attackLevel++;
        SetAttack(attackLevel);
    }

    public void UpgradeDefense()
    {
        defenseLevel++;
        SetDefense(defenseLevel);
    }

    public void UpgradeSpecial()
    {
        specialLevel++;
        SetSpecial(specialLevel);
    }

    protected override void doneDeath()
    {
        base.doneDeath();
        manager.LoseGame();
    }

    #endregion Setup and bookkeeping

    #region Affect

    /// <summary>
    /// finishes PredictAttack by adding to affectmanager lists
    /// </summary>
    protected override void PredictAttacks()
    {
        base.PredictAttacks();
        affect.AddUpcomingPlayerAttack(firingFinishEvent);
    }

    /// <summary>
    /// Overrides attack pattern valence to provide player values
    /// </summary>
    /// <returns></returns>
    protected override Emotion attackPatternValence()
    {
        return new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
    }

    /// <summary>
    /// Overrides attack pattern arousal to provide player values
    /// </summary>
    /// <returns></returns>
    protected override Emotion attackPatternArousal()
    {
        return new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
    }

    /// <summary>
    /// Overrides attack pattern tension to provide player values
    /// </summary>
    /// <returns></returns>
    protected override Emotion attackPatternTension()
    {
        return new Emotion(EmotionDirection.increase, EmotionStrength.weak);
    }

    /// <summary>
    /// Player overrides TakeDamage to add affect
    /// </summary>
    /// <param name="_damage"></param>
    public override void TakeDamage(float _damage)
    {
        base.TakeDamage(_damage);
        //Set base levels - the first few shots aren't actually important to the strength of the emotion
        var damageValence = new Emotion(EmotionDirection.none, EmotionStrength.none);
        var damageTension = new Emotion(EmotionDirection.none, EmotionStrength.none);

        if (shieldPercent() < 0.75f)
        {
            damageValence = new Emotion(EmotionDirection.decrease, EmotionStrength.weak);
            damageTension = new Emotion(EmotionDirection.increase, EmotionStrength.weak);
        }
        if (!shielded)
        {
            damageValence = new Emotion(EmotionDirection.decrease, EmotionStrength.moderate);
            damageTension = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
            if (LowHealth())
            {
                damageValence = new Emotion(EmotionDirection.decrease, EmotionStrength.strong);
                damageTension = new Emotion(EmotionDirection.increase, EmotionStrength.strong);
            }
        }

        affect.CreatePastEvent(damageValence, null, damageTension, 45.0f);
    }

    #endregion Affect

    #region Attack and Abilities

    #region Attack

    /// <summary>
    /// Button mechanics for attack
    /// Triggers and implementation are used to allow for queueing actions
    /// </summary>
    public void AttackTrigger()
    {
        if (attackButton.CanActivate())
        {
            attackButton.sendToButton(attackCooldown);
        }
    }

    /// <summary>
    /// Basic player attack toggle
    /// If not currently firing, begins firing
    /// </summary>
    public override void AttackToggle()
    {
        if (!attacking)
        {
            //Debug.Log("Starting Attack");
            ShieldsDown();
            StartCoroutine(AutoAttack(enemyShip, warmupShots, totalShots));
            attackButton.HoldButton();
        }
        else
        {
            InterruptFiring();
        }
    }

    protected override void FinishFiring()
    {
        base.FinishFiring();

        //Debug.Log("Player finishing firing, trying to release attack button");
        attackButton.ReleaseButton();
        TriggerGlobalCooldown();
    }

    protected override void InterruptFiring()
    {
        base.InterruptFiring();
        attackButton.ReleaseButton();
    }

    #endregion Attack

    #region Absorb (Parry Riposte)

    /// <summary>
    /// Button mechanics for shield
    /// Triggers and implementation are used to allow for queueing actions
    /// </summary>
    public void AbsorbTrigger()
    {
        if (absorbButton.CanActivate())
        {
            absorbButton.sendToButton(shieldCooldown);
        }
    }

    /// <summary>
    /// Brings shields down, gets ready to absorb attack
    /// </summary>
    public void BeginAbsorb()
    {
        if (attacking) //If we interrupt an attack, interupt it
        {
            InterruptFiring();
        }
        else
        {
            ShieldsDown();
        }
        TriggerGlobalCooldown();
        absorbing = true;
        retaliate = false;
        var absorbEffect = Instantiate(absorbObject, this.transform);
        StartCoroutine(AbsorbFrame(parryFrame, absorbEffect));
    }

    private IEnumerator AbsorbFrame(float duration, GameObject absorbEffect)
    {
        var timer = duration;
        while (timer > 0.0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        AbsorbEnd(absorbEffect);
    }

    /// <summary>
    /// Disables shields - if retaliate is triggered, do so
    /// </summary>
    public void AbsorbEnd(GameObject absorbEffect)
    {
        Destroy(absorbEffect);
        if (retaliate)
        {
            enemyShip.InterruptLaser();
            Retaliate();
        }
        else
        {
            if (retaliateEvent != null) //If we bring down shields without retaliating and we have a retaliate event, destroy it
            {
                affect.CullEvent(retaliateEvent);
                retaliateEvent = null;
            }
        }
        retaliate = false;
        ChargeShield(shieldCooldown);
    }

    /// <summary>
    /// Absorbs attack and prepares to retaliate
    /// </summary>
    public void SetRetaliate()
    {
        SFX.PlayOneShot(SFX_absorbAttack);
        retaliate = true;

        //shieldStamina.AbsorbAttack();

        var parryValence = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
        var parryArousal = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
        var parryTension = new Emotion(EmotionDirection.decrease, EmotionStrength.moderate);
        affect.CreatePastEvent(parryValence, parryArousal, parryTension, 10.0f);
    }

    /// <summary>
    /// Fires retaliation burst
    /// </summary>
    public void Retaliate()
    {
        var targetObj = enemyShipObj;
        var target = targetObj.GetComponent<EnemyShip>();
        var damage = retaliateDamage;
        if (target.alive)
        {
            retaliateCannon.transform.LookAt(target.transform);
            var cannon = Instantiate(disableShotPrefab, retaliateCannon.position, Quaternion.identity, bulletParent);
            cannon.transform.SetParent(retaliateCannon);
            cannon.gameObject.tag = tag;
            cannon.layer = 9;
            cannon.GetComponent<SciFiProjectileScript>().CannonSetup(damage, enemyShip, affect);
            retaliateEvent = null; //After firing, we clear our retaliateEvent
        }

        buttonManager.RefreshAllCooldowns();
    }

    #endregion Absorb (Parry Riposte)

    #region Heal

    /// <summary>
    /// Button mechanics for attack
    /// Triggers and implementation are used to allow for queueing actions
    /// Naming isn't consistent because HealTrigger() is used for the mechanical heal
    /// </summary>
    public void HealButton()
    {
        if (healButton.CanActivate())
        {
            healButton.sendToButton(healCooldown);
        }
    }

    /// <summary>
    /// Overrides heal with affect and fires two shots from aft cannon
    /// </summary>
    public override void Heal()
    {
        base.Heal();

        if (healing)
        {
            var healValence = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
            var healTension = new Emotion(EmotionDirection.decrease, EmotionStrength.weak);
            affect.CreatePastEvent(healValence, null, healTension, 10.0f);
        }

        ChargeShield(shieldCooldown);
    }

    /// <summary>
    /// Overrides heal trigger and starts cooldown
    /// </summary>
    public override void HealTrigger()
    {
        base.HealTrigger();
        if (attacking)
        {
            InterruptFiring();
        }
        else
        {
            ShieldsDown();
        }
        //ChargeShield(shieldCooldown);
        TriggerGlobalCooldown();
    }

    #endregion Heal

    #region Ultimate

    /// <summary>
    /// Button mechanics for Ultimate
    /// Triggers and implementation are used to allow for queueing actions
    /// </summary>
    public void UltimateTrigger()
    {
        if (ultimateButton.CanActivate())
        {
            ultimateButton.sendToButton(ultimateCooldown);
        }
    }

    /// <summary>
    /// Repairs all systems
    /// Fires laser for large damage
    /// If interrupting heal, fires full broadside
    /// </summary>
    public void Ultimate()
    {
        buttonManager.RefreshAllCooldowns();
        LaserFire();
    }

    /// <summary>
    /// Fires big ol' laser
    /// Deals big damage and jams enemy (allows for heal!)
    /// </summary>
    public void LaserFire()
    {
        var emitter = laserEmitter.position;
        var damage = laserDamage;

        SpawnLaser(emitter, enemyShipObj.transform.position);

        enemyShip.TakeDamage(damage);
        enemyShip.Jam(jamDuration);
    }

    #endregion Ultimate

    /// <summary>
    /// Override of Jam
    /// Re-sets combo, and disables all parts for duration
    /// </summary>
    /// <param name="duration"></param>
    public override void Jam(float duration)
    {
        base.Jam(duration);
        buttonManager.globalCooldown(duration);
        attackButton.StartCooldown(duration, Color.red);
        absorbButton.StartCooldown(duration, Color.red);
        healButton.StartCooldown(duration, Color.red);
    }

    private void TriggerGlobalCooldown()
    {
        buttonManager.globalCooldown();
    }

    #endregion Attack and Abilities

    // Start is called before the first frame update
    private void Start()
    {
        ShipSetup();
    }

    // Update is called once per frame
    private void Update()
    {
    }
}