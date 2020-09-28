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

    public float shieldCooldown = 0.5f;
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
    public GameObject disableShotPrefab;

    public GameObject shieldPrefab;
    private GameObject shield;

    public ShieldStamina shieldStamina;

    private int combo = 0;
    public comboTracker comboTracker;

    [Header("Ship-specific Audio")]
    public AudioClip SFX_shieldActivate;

    public AudioClip SFX_absorbAttack;//, SFX_retaliate;

    public AudioClip SFX_Heal;

    #endregion Effect  and weapon references

    #region Button references

    [Header("Hotbar Buttons")]
    public basicButton attackButton;

    public basicButton shieldButton;
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
        shieldStamina.SetStamina(shieldDuration);

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
    /// Predicts future attacks from the player - we assume that the player will continue to attack, as this is how they progress the game.
    /// This is where the math is done to determine affective levels
    /// </summary>
    protected override void PredictAttacks()
    {
        base.PredictAttacks();

        //Set up basic emotion changes for normal shot
        var valenceChange = new Emotion(EmotionDirection.increase, EmotionStrength.weak); //Each shot from player increases valence slightly
        var arousalChange = new Emotion(EmotionDirection.increase, EmotionStrength.weak); //Also, each shot from player increases arousal
        var tensionChange = new Emotion(EmotionDirection.none, EmotionStrength.none); //Tension with a normal shot doesn't change
        var nextCombo = combo + 1;

        //Consider special cases when healths are low
        EvaluateHealthAffect(ref valenceChange, ref tensionChange);

        //Finally, load the correct number of events into the queue and to the affect manager
        for (int i = 0; i < nextCombo; i++)
        {
            var newEvent = new ProspectiveEvent(valenceChange, arousalChange, tensionChange, 5.0f, false, affect);
            affect.AddEvent(newEvent);
            AddEventToQueue(newEvent);
        }

        void EvaluateHealthAffect(ref Emotion valenceChange, ref Emotion tensionChange)
        {
            //Special cases change the emotion strengths
            var lowHealth = healthPercent() < 0.25f;
            var enemyLowHealth = enemyShip.healthPercent() < 0.25f;
            if (enemyLowHealth)  //If this may be the shot that finishes the battle, increase valence strength and add tension
            {
                valenceChange = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);

                if (lowHealth) //If we are also low health, our shot is even more tense ("Can we pull it off?!"
                {
                    tensionChange = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
                }
                else { tensionChange = new Emotion(EmotionDirection.increase, EmotionStrength.weak); }
            }
        }
    }

    #endregion Affect

    #region Attack and Abilities

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
    /// Basic player attack - increases combo by one and fires shots equal to combo.
    /// At 4, fires from both turrets and resets combo.
    /// </summary>
    public void Attack()
    {
        TriggerGlobalCooldown();
        combo++;
        switch (combo)
        {
            case 1:
                StartCoroutine(FireBroadside(enemyShip, turretPosition.aft, 1));
                break;

            case 2:
                StartCoroutine(FireBroadside(enemyShip, turretPosition.fore, attackLevel));
                break;

            case 3:
                StartCoroutine(FireBroadside(enemyShip, turretPosition.fore, 2 + (attackLevel * 2)));
                break;

            case 4:
                FullBroadside(enemyShip, attackLevel + 4);
                //LaserFire();
                enemyShip.Jam(jamDuration * 2.0f); //No laser, but still a jam on full combo
                combo = 0;
                break;
        }
        comboTracker.SetCombo(combo + 1);
        PredictAttacks(); //Predict next series of attacks from player
    }

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
            StartCoroutine(FireBroadside(enemyShip, turretPosition.aft, 2));
        }
    }

    /// <summary>
    /// Overrides heal trigger and starts cooldown
    /// </summary>
    public override void HealTrigger()
    {
        base.HealTrigger();
        TriggerGlobalCooldown();
    }

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
        if (enemyShip.healing)
        {
            FullBroadside(enemyShip, 10);
        }
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

    /// <summary>
    /// Override of Jam
    /// Re-sets combo, and disables all parts for duration
    /// </summary>
    /// <param name="duration"></param>
    public override void Jam(float duration)
    {
        base.Jam(duration);

        combo = 0;
        attackButton.StartCooldown(duration, Color.red);
        shieldButton.StartCooldown(duration, Color.red);
        healButton.StartCooldown(duration, Color.red);
    }

    private void TriggerGlobalCooldown()
    {
        buttonManager.globalCooldown();
    }

    #endregion Attack and Abilities

    #region Shields

    /// <summary>
    /// Sets mechanics, creates shield object, sends shield message to various references
    /// </summary>
    public void ShieldsUp()
    {
        //Sets mechanics
        shielded = true;
        retaliate = false;

        //Plays sound, creates object
        SFX.PlayOneShot(SFX_shieldActivate);
        shield = Instantiate(shieldPrefab, transform.position, Quaternion.identity, this.transform);
        shield.tag = "Player";
        shield.name = "Shield";
        shield.transform.SetParent(this.transform);
        shield.transform.localScale = new Vector3(4, 4, 4);

        //Sustains shield and disables other buttons
        StartCoroutine(ShieldSustain());
        buttonManager.ShieldHold();
    }

    /// <summary>
    /// Keeps shield up as long as key is held down
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShieldSustain()  //Keeps shield up for duration, and removes afterwards
    {
        shieldStamina.ShieldsUp(); //Passes shields up message to shieldsustain bar

        while (shieldStamina.shielded)
        {
            yield return null;
        }

        //As soon as we're not shielded in stamina bar, remove shield. Back and forth is required for stamina to run out OR disable on release
        ShieldsDown();
    }

    /// <summary>
    /// If shield is hit by a cannon shot, drain a small amount of stamina
    /// </summary>
    public void ShieldHit(float _damage)
    {
        shieldStamina.StaminaChunk(_damage * 0.25f);
    }

    /// <summary>
    /// Disables shields - if retaliate is triggered, do so
    /// </summary>
    public void ShieldsDown()
    {
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

        shieldStamina.ShieldDown();
        shielded = false;
        Destroy(shield);
        buttonManager.ShieldRelease();
    }

    /// <summary>
    /// Absorbs attack and prepares to retaliate
    /// </summary>
    public void SetRetaliate()
    {
        SFX.PlayOneShot(SFX_absorbAttack);
        retaliate = true;

        shieldStamina.AbsorbAttack();

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
            var cannon = Instantiate(disableShotPrefab, retaliateCannon.position, Quaternion.identity);
            cannon.transform.SetParent(retaliateCannon);
            cannon.gameObject.tag = tag;
            cannon.layer = 9;
            cannon.GetComponent<SciFiProjectileScript>().CannonSetup(damage, enemyShip, retaliateEvent, affect);
            retaliateEvent = null; //After firing, we clear our retaliateEvent

            target.Jam(jamDuration);
        }
    }

    #endregion Shields

    /// <summary>
    /// Button mechanics for shield
    /// Triggers and implementation are used to allow for queueing actions
    /// </summary>
    public void ShieldTrigger()
    {
        if (shieldButton.CanActivate())
        {
            shieldButton.sendToButton(shieldCooldown);
        }
    }

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