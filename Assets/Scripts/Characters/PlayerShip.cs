using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SciFiArsenal;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System;
using Random = UnityEngine.Random;

public class PlayerShip : BasicShip
{
    #region Cooldown varibles

    [Header("Cooldowns")]
    public float attackCooldown = 1.5f;

    public float healCooldown = 2.5f;

    public float heavyAttackCooldown = 2.5f;

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

    public float parryFrame;
    public bool short_HeavyCharge = false;

    public GameObject disableShotPrefab;

   // private int shots = 0;
    public comboTracker comboTracker;

    [Header("Ship-specific Audio")]
    //public AudioClip SFX_absorbAttack;//, SFX_retaliate;

    public AudioClip SFX_Heal;
    public AudioClip SFX_begin_parry;
    public AudioClip SFX_Parry;
    public AudioClip SFX_Riposte;
    public AudioClip SFX_Shield_hum;

    #endregion Effect  and weapon references

    #region Button references

    [Header("Hotbar Buttons")]
    public basicButton attackButton;

    public basicButton absorbButton;
    public basicButton healButton;
    public basicButton heavyAttackButton;
    public buttonManager buttonManager;

    public UpgradeManager upgradeManager;

    public TextMeshProUGUI attackUI, defendUI, abilityUI;

    public Slider chargeBar;

    #endregion Button references

    #region Mechanic and Attribute variables

    public bool godMode = false;

    private bool retaliate = false; //Variables for shield mechanics

   // private int attackLevel = 1, defenseLevel = 1, specialLevel = 1;

    #endregion Mechanic and Attribute variables

    #region Prospective event variables

    private AffectEvent retaliateEvent;

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
            heavyAttackDamage = 100.0f;
            basicAttackDamage = 100.0f;
            maxHealth = 10000000.0f;
            health = maxHealth;
            healthBar.Refresh(maxHealth, health);
            healPercent = 100.0f;
        }
    }

    //public void UpgradeAttack()
    //{
    //    attackLevel++;
    //    attackUI.text = attackLevel + "/3";
    //    SetAttack(attackLevel);
    //}

    //public void UpgradeDefense()
    //{
    //    defenseLevel++;
    //    defendUI.text = defenseLevel + "/3";
    //    SetDefense(defenseLevel);
    //}

    //public void UpgradeSpecial()
    //{
    //    specialLevel++;
    //    abilityUI.text = specialLevel + "/3";
    //    SetSpecial(specialLevel);
    //}

    protected override void doneDeath()
    {
        base.doneDeath();
        manager.LoseGame();
    }

    protected override GameObject myTargetObject()
    {
        if (enemyShipObj != null)
        {
            return enemyShipObj;
        }
        else { return base.myTargetObject(); }
    }

    protected override BasicShip myTarget()
    {
        if (enemyShip != null)
        {
            return enemyShip;
        }
        else
        {
            return base.myTarget();
        }
    }

    public void StartCombat()
    {
        absorbButton.StartCombat();
        healButton.StartCombat();
        attackButton.StartCombat();
        heavyAttackButton.StartCombat();
    }

    public void EndCombat()
    {
        absorbButton.EndCombat();
        healButton.EndCombat();
        attackButton.EndCombat();
        heavyAttackButton.EndCombat();
    }

    #endregion Setup and bookkeeping

    #region Affect

    /// <summary>
    /// Predicts attacks, and checks whether attack would be lethal
    /// </summary>
    protected override void PredictAttacks()
    {
        base.PredictAttacks();

        var firingTime = attackPatternFinishTime(warmupShots, totalShots);
        var totalDamage = attackPattern_total_damage(warmupShots, totalShots, basicAttackDamage);
        var magnitude = 1.0f + (1.0f / enemyShip.shieldPercent());
        PreGLAM.Queue_event(new Likely_event(affectVariables.P_attack.x, affectVariables.P_attack.y, magnitude, firingTime, likely_type.player_attack));

        if (totalDamage > enemyShip.health && !enemyShip.shielded)
        {
            var v = affectVariables.E_death.x;
            var t = affectVariables.E_death.y;
            magnitude = 1.0f + enemyShip.shieldCooldown;
            PreGLAM.Queue_event(new Likely_event(v, t, magnitude, firingTime, likely_type.enemy_death));
        }
    }

    ///// <summary>
    ///// Overrides attack pattern valence to provide player values
    ///// </summary>
    ///// <returns></returns>
    //protected override Emotion attackPatternValence()
    //{
    //    return new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
    //}

    ///// <summary>
    ///// Overrides attack pattern arousal to provide player values
    ///// </summary>
    ///// <returns></returns>
    //protected override Emotion attackPatternArousal()
    //{
    //    return new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
    //}

    ///// <summary>
    ///// Overrides attack pattern tension to provide player values
    ///// </summary>
    ///// <returns></returns>
    //protected override Emotion attackPatternTension()
    //{
    //    return new Emotion(EmotionDirection.increase, EmotionStrength.weak);
    //}

    /// <summary>
    /// Player overrides TakeDamage to add affect
    /// </summary>
    /// <param name="_damage"></param>
    public override void TakeDamage(float _damage)
    {
        base.TakeDamage(_damage);
        //This was too powerful and didn't give any feedback on enemy.
        if (!shielded)
        {
            //If we're winding up a heavy attack, it interrupts the attack
            if (heavyAttackWindup)
            {
                InterruptHeavyAttack();
            }
        }
        //PreGLAM.Create_event(new Past_event(-5f, 1.0f + (1.0f/shieldPercent()), PreGLAM));
    }

    public override void TakeHeavyDamage(float _damage, float _jamLength)
    {
        base.TakeHeavyDamage(_damage, _jamLength);

        //PreGLAM.Create_event(new Past_event(-10f, 1.0f + (1.0f / healthPercent()), PreGLAM));
    }

    public override void ShieldBreak()
    {
        base.ShieldBreak();
        var v = affectVariables.P_shield_down.x;
        var magnitude = 1.0f + shieldPercent();

        PreGLAM.Queue_event(new Past_event(v, magnitude, PreGLAM));
    }

    //public override ProspectiveEvent SpecialProspectiveEvent(float estimatedTime)
    //{
    //    var specialValence = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
    //    var specialArousal = new Emotion(EmotionDirection.increase, EmotionStrength.strong);
    //    var specialTension = new Emotion(EmotionDirection.decrease, EmotionStrength.moderate);

    //    return new ProspectiveEvent(specialValence, specialArousal, specialTension, estimatedTime, true, affect);
    //}

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

    protected override void BeginAttack()
    {
        base.BeginAttack();
        attackButton.HoldButton();
    }

    protected override void FinishFiring()
    {
        base.FinishFiring();

        //Debug.Log("Player finishing firing, trying to release attack button");
        attackButton.ReleaseButton();
        PreGLAM.Queue_prospective_cull(likely_type.player_attack);
        PreGLAM.Queue_prospective_cull(likely_type.enemy_death);
        PreGLAM.Queue_event(new Past_event(affectVariables.P_attack.x, 1.0f + (enemyShip.shieldPercent()), PreGLAM));
        TriggerGlobalCooldown();
    }

    public override void InterruptFiring()
    {
        base.InterruptFiring();
        PreGLAM.Queue_prospective_cull(likely_type.player_attack);
        PreGLAM.Queue_prospective_cull(likely_type.enemy_death);
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
        if (attacking != null) //If we interrupt an attack, interupt it
        {
            InterruptFiring();
        }
        else
        {
            ShieldsDown();
        }
        TriggerGlobalCooldown();
        SFX.PlayOneShot(SFX_begin_parry);
        absorbing = true;
        retaliate = false;
        var absorbEffect = Instantiate(absorbObject, this.transform);
        absorbEffect.transform.localScale = new Vector3(7.5f, 7.5f, 7.5f);
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
        absorbing = false;
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
                //affect.CullEvent(retaliateEvent);
                retaliateEvent = null;
            }
        }
        retaliate = false;
        ShieldsUp();
    }

    /// <summary>
    /// Absorbs attack and prepares to retaliate
    /// </summary>
    public override void SetRetaliate()
    {
        SFX.PlayOneShot(SFX_Parry);
        retaliate = true;

        //shieldStamina.AbsorbAttack();

        //var parryValence = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
       // var parryArousal = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
       // var parryTension = new Emotion(EmotionDirection.decrease, EmotionStrength.moderate);
        //affect.CreatePastEvent(parryValence, parryArousal, parryTension, 10.0f);
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
            cannon.GetComponent<SciFiProjectileScript>().CannonSetup(damage, enemyShip);
            retaliateEvent = null; //After firing, we clear our retaliateEvent
            target.Jam(jamDuration * 2);
            target.ShieldBreak();
            SFX.PlayOneShot(SFX_Riposte);
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
    public void HealButtonCheck()
    {
        if (healButton.CanActivate())
        {
            healButton.sendToButton(healCooldown);
        }
    }

    /// <summary>
    /// Overrides heal with affect
    /// </summary>
    public override void Heal()
    {
        base.Heal();

        if (healing)
        {
            PreGLAM.Queue_prospective_cull(likely_type.player_heal);
            PreGLAM.Queue_prospective_cull(likely_type.player_death);
            var v = affectVariables.P_heal.x;
            var magnitude = 1.0f + (1.0f - healthPercent());
            PreGLAM.Queue_event(new Past_event(v, magnitude, PreGLAM));
        }

        ShieldsUp();
        hideCharge();
    }

    /// <summary>
    /// Overrides heal trigger and starts cooldown
    /// </summary>
    public override void HealTrigger()
    {
        base.HealTrigger();
        activateCharge();
        if (attacking != null)
        {
            InterruptFiring();
        }
        else
        {
            ShieldsDown();
        }
        //healEvent = SpecialProspectiveEvent(healDelay);
        //affect.AddUpcomingPlayerAttack(healEvent);
        //ChargeShield(shieldCooldown);
        var v = affectVariables.E_heal.x;
        var t = affectVariables.E_heal.y;
        var magnitude = 1.0f + (1.0f - healthPercent());
        PreGLAM.Queue_event(new Likely_event(v, t, magnitude, healDelay, likely_type.enemy_heal));
        TriggerGlobalCooldown();
    }

    public override void HealInterrupt()
    {

        base.HealInterrupt();
        if (healing)
        {
            PreGLAM.Queue_prospective_cull(likely_type.player_heal);
            var v = affectVariables.P_heal.x;
            var magnitude = 1.0f + (1.0f - healthPercent());
            PreGLAM.Queue_event(new Past_event(-v, magnitude, PreGLAM));
        }
    }

    #endregion Heal

    #region HeavyAttack

    /// <summary>
    /// Button mechanics for Ultimate
    /// Triggers and implementation are used to allow for queueing actions
    /// </summary>
    public void HeavyAttackButtonCheck()
    {
        if (heavyAttackButton.CanActivate())
        {
            heavyAttackButton.sendToButton(heavyAttackCooldown);
        }
    }
    public override void InterruptHeavyAttack()
    {
        base.InterruptHeavyAttack();
        PreGLAM.Queue_prospective_cull(likely_type.player_attack);
        var v = affectVariables.P_heavy.x;

        var magnitude = 1.0f + (1.0f - enemyShip.shieldPercent());
        PreGLAM.Queue_event(new Past_event(-v, magnitude, PreGLAM));
    }

    public override void HeavyAttackTrigger()
    {
        base.HeavyAttackTrigger();
        HeavyAttack_Sound();
        if (attacking != null) { InterruptFiring(); }
        //We assume normal heavy attack
        var magnitude = 1.0f + (1.0f - enemyShip.shieldPercent());
        var valence = affectVariables.P_heavy.x;
        var tension = affectVariables.P_heavy.y;

        if (enemyShip.isJammed)
        {
            valence = affectVariables.P_exploit.x;
            tension = affectVariables.P_exploit.y;
        }

        PreGLAM.Queue_event(new Likely_event(valence, tension, magnitude, heavyAttackDelay, likely_type.player_attack));

        if (heavyAttackDamage > enemyShip.health && !enemyShip.shielded)
        {
            var v = affectVariables.E_death.x;
            var t = affectVariables.E_death.y;
            magnitude = 1.0f + enemyShip.shieldCooldown;
            PreGLAM.Queue_event(new Likely_event(v, t, magnitude, heavyAttackDelay, likely_type.enemy_death));
        }

        //specialFiringEvent = SpecialProspectiveEvent(heavyAttackDelay);
        //affect.AddUpcomingPlayerAttack(specialFiringEvent);
        TriggerGlobalCooldown();
        activateCharge();
    }
    public override void HeavyAttack(GameObject targetObj)
    {
        base.HeavyAttack(targetObj);
        PreGLAM.Queue_prospective_cull(likely_type.player_attack);
        PreGLAM.Queue_prospective_cull(likely_type.enemy_death);
        var v = affectVariables.P_heavy.x;
        var magnitude = 1.0f + (1.0f - enemyShip.shieldPercent());

        if (enemyShip.isJammed) { v = affectVariables.P_exploit.x;
            magnitude = 1.0f + (1.0f - enemyShip.healthPercent());
        }


        PreGLAM.Queue_event(new Past_event(v, magnitude, PreGLAM));

        hideCharge();
    }

    protected override void setChargeIndicator(float current, float max)
    {
        base.setChargeIndicator(current, max);
        var currentMax = chargeBar.maxValue;
        if (max != currentMax)
        {
            chargeBar.maxValue = max;
        }
        chargeBar.value = current;
    }

    void activateCharge()
    {
        chargeBar.gameObject.SetActive(true);
    }

    void hideCharge()
    {
        chargeBar.gameObject.SetActive(false);
    }

    void HeavyAttack_Sound()
    {
        var clip = SFX_Heavy_attack_windup[0];
        if (short_HeavyCharge)
        {
            clip = SFX_Heavy_attack_windup[2];
        }
        SFX.PlayOneShot(clip);
    }

    #endregion HeavyAttack

    #region Shield_overrides
    public override void ShieldsDown()
    {
        base.ShieldsDown();
        if (SFX !=null && SFX.isPlaying)
        {
            SFX.Stop();
        }
    }
    public override void ShieldsUp()
    {
        base.ShieldsUp();
        SFX.clip = SFX_Shield_hum;
        SFX.Play();
    }


    #endregion

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
        upgradeManager = new UpgradeManager(this);
    }

    // Update is called once per frame
    private void Update()
    {
    }
}

public class UpgradeManager
{
    List<Upgrade> PossibleUpgrades;
    List<Upgrade> ActiveUpgrades;
    PlayerShip player;

    public UpgradeManager(PlayerShip _player)
    {
        player = _player;
        Setup();
    }

    //Unfortunately, at some point I just need to load all these possible upgrades into a list
    #region Setup
    public void Setup()
    {
        PossibleUpgrades = Populate_Upgrades();
        ActiveUpgrades = new List<Upgrade>();
    }

    public List<Upgrade> ProposeUpgrades()
    {
        var proposed_upgrades = new List<Upgrade>();
        for (int i = 0; i <3; i++)
        {
            var randUpg = Random.Range(0, PossibleUpgrades.Count);
            var proposal = PossibleUpgrades[randUpg];
            while (proposed_upgrades.Contains(proposal)) //Re-do if this would be a duplicate, until it's not a duplicate
            {
                randUpg = Random.Range(0, PossibleUpgrades.Count);
                proposal = PossibleUpgrades[randUpg];
            }

            proposed_upgrades.Add(proposal);
        }
        return proposed_upgrades;
    }

    public void Select_Upgrade(Upgrade _selected)
    {
        if (PossibleUpgrades.Contains(_selected))
        {
            PossibleUpgrades.Remove(_selected);
        }
        else
        {
            Debug.Log("Possible upgrades didn't contain selected upgrade? " + _selected.Name());
        }
        ActiveUpgrades.Add(_selected);
        _selected.Activate();
    }


    List<Upgrade> Populate_Upgrades()
    {
        var Upgrades = new List<Upgrade>();
        var UpgradeCount = Enum.GetNames(typeof(Upgrade_Type)).Length;
        for (int i = 0; i < UpgradeCount; i++)
        {
            var new_Upgrade = new Upgrade((Upgrade_Type)i, player);
            Upgrades.Add(new_Upgrade);
        }

        return Upgrades;
    }
    #endregion

}

public class Upgrade
{
    Upgrade_Type type;
    bool active = false;
    string name = "No_Name";
    string desc = "No Desc";
    PlayerShip player;


    public Upgrade(Upgrade_Type _type, PlayerShip _player)
    {
        type = _type;
        active = false;
        setInfo(_type);
        player = _player;
    }

    public void Activate()
    {
        active = true;
        SetPlayerAttribute();
    }

    public bool isActive()
    {
        return active;
    }

    public string Name()
    {
        return name;
    }
    public string Description()
    {
        return desc;
    }

    private void SetPlayerAttribute()
    {
        switch (type)
        {
            case Upgrade_Type.HeavyAttack_Charge:
                player.heavyAttackDelay *= 0.5f;
                player.short_HeavyCharge = true;
                Debug.Log("New heavy attack charge = " + player.heavyAttackDelay);
                break;
            case Upgrade_Type.Heal_Charge:
                player.healDelay *= 0.5f;
                Debug.Log("New heal attack charge = " + player.healDelay);

                break;
            case Upgrade_Type.LightAttack_Charge:
                player.warmupShots /= 2;
                Debug.Log("New warmup shots = " + player.warmupShots);
                break;
            case Upgrade_Type.LightAttack_Damage:
                player.basicAttackDamage *= 2;
                Debug.Log("New attack damage = " + player.basicAttackDamage);
                break;
            case Upgrade_Type.Shield_Points:
                player.maxShield *= 2;
                Debug.Log("New max shield = " + player.maxShield);
                break;
            case Upgrade_Type.Health_Points:
                player.maxHealth *= 2;
                Debug.Log("New max health = " + player.maxHealth);
                break;
            case Upgrade_Type.HeavyAttack_Damage:
                player.heavyAttackDamage *= 2;
                Debug.Log("New heavy attack damage = " + player.heavyAttackDamage);
                break;
            case Upgrade_Type.Parry_Time:
                player.parryFrame *= 2.0f;
                Debug.Log("New parry frame = " + player.parryFrame);
                break;
            case Upgrade_Type.Parry_Disable:
                player.jamDuration *= 2.0f;
                Debug.Log("New jam duration = " + player.jamDuration);
                break;
            case Upgrade_Type.Heal_Strength:
                player.healPercent = 0.9f;
                Debug.Log("New heal percent = " + player.healPercent);
                break;
            default:
                break;
        }
    }

    private void setInfo(Upgrade_Type _type)
    {
        switch (_type)
        {
            case Upgrade_Type.HeavyAttack_Charge:
                name = "Laser Supercharger";
                desc = "Reduces charge time of Laser attack by 50%";
                break;
            case Upgrade_Type.Heal_Charge:
                name = "Repair Supercharger";
                desc = "Reduces charge time of Self-repair by 50%";
                break;
            case Upgrade_Type.LightAttack_Charge:
                name = "Cannon Supercharger";
                desc = "Fires both cannons 50% faster when attacking";
                break;
            case Upgrade_Type.LightAttack_Damage:
                name = "Cannon focusing crystal";
                desc = "Light attacks deal 2x damage";
                break;
            case Upgrade_Type.Shield_Points:
                name = "Shield capacitor";
                desc = "Doubles shield points";
                break;
            case Upgrade_Type.Health_Points:
                name = "Reinforced Hull";
                desc = "Doubles health points";
                break;
            case Upgrade_Type.HeavyAttack_Damage:
                name = "Laser focusing crystal";
                desc = "Laser attack deals 2x damage";
                break;
            case Upgrade_Type.Parry_Time:
                name = "Absorbitive capacitor";
                desc = "Increases timing window for absorbing incoming laser";
                break;
            case Upgrade_Type.Parry_Disable:
                name = "Ion supercharger";
                desc = "Increases disable time after absorbing incoming laser";
                break;
            case Upgrade_Type.Heal_Strength:
                name = "Repair nanite swarm";
                desc = "Self-repair now heals 90% of missing health";
                break;
            default:
                break;
        }
    }

}

public enum Upgrade_Type { 
    HeavyAttack_Charge, 
    Heal_Charge, 
    LightAttack_Charge,
    LightAttack_Damage,
    Shield_Points,
    Health_Points,
    HeavyAttack_Damage,
    Parry_Time,
    Parry_Disable,
    Heal_Strength
}