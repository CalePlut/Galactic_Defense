using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SciFiArsenal;
using UnityEditor;

public class PlayerShip : BasicShip
{
    [Header("Cooldowns")]
    public float attackCooldown = 1.5f;

    public float shieldCooldown = 0.5f;
    public float healCooldown = 2.5f;

    public float ultimateCooldown = 30.0f;

    private GameObject enemyShipObj;
    private EnemyShip enemyShip;

    [Header("Positioning objects")]
    public Transform retaliateCannon;

    [Header("Effects and Weapons")]
    public GameObject disableShotPrefab;

    public GameObject shieldPrefab;

    public shieldStamina shieldStamina;

    private int combo = 0;
    public comboTracker comboTracker;

    [Header("Ship-specific Audio")]
    public AudioClip SFX_shieldActivate;

    public AudioClip SFX_absorbAttack;//, SFX_retaliate;

    public AudioClip SFX_Heal;

    [Header("Hotbar Buttons")]
    public basicButton attackButton;

    public basicButton shieldButton;
    public basicButton healButton;
    public basicButton ultimateButton;
    public buttonManager buttonManager;

    //Internal Mechanics
    private bool retaliate = false; //Variables for shield mechanics

    private GameObject shield;
    private int attackLevel = 1, defenseLevel = 1, specialLevel = 1;

    public void SetEnemyReference(GameObject _enemyShipObj)
    {
        enemyShipObj = _enemyShipObj;
        enemyShip = enemyShipObj.GetComponent<EnemyShip>();
    }

    public override void SetDefense(int level)
    {
        base.SetDefense(level);
        shieldStamina.SetStamina(shieldDuration);
    }

    #region Abilities

    public void attackTrigger()
    {
        if (attackButton.canActivate())
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
        Cooldown();
        combo++;
        switch (combo)
        {
            case 1:
                StartCoroutine(FireBroadside(enemyShip, turretPosition.fore, 1));
                break;

            case 2:
                StartCoroutine(FireBroadside(enemyShip, turretPosition.fore, 2));
                break;

            case 3:
                StartCoroutine(FireBroadside(enemyShip, turretPosition.fore, 3));
                break;

            case 4:
                FullBroadside(enemyShip, 4);
                combo = 0;
                break;
        }
        comboTracker.SetCombo(combo + 1);
    }

    /// <summary>
    /// Fires fusion cannon and starts volley
    /// If cannon interrupts heal, fire 2x volley
    /// </summary>
    public void FireCannon()
    {
        //Debug.Log("Firing Fusion Cannon");
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
            cannon.GetComponent<SciFiProjectileScript>().CannonSetup(damage, enemyShip);

            //If we interrupt, we fire a more massive broadside
            //If we interrupt, produce a large affect swing. Otherwise, normal affect swing.
            var valenceEmotion = new Emotion(EmotionDirection.increase, EmotionStrength.weak);
            var tensionEmotion = new Emotion(EmotionDirection.none, EmotionStrength.none);
            //Debug.Log("Checking if target is healing and firing");
            if (target.healing)
            {
                HealPunish(target);
                valenceEmotion = new Emotion(EmotionDirection.increase, EmotionStrength.strong);
                tensionEmotion = new Emotion(EmotionDirection.decrease, EmotionStrength.weak);
            }
            else

            {
                StartCoroutine(FireBroadside(enemyShip, turretPosition.fore, foreShots));
            }
            affect.CreatePastEvent(valenceEmotion, null, tensionEmotion, 10.0f);
        }

        Cooldown();
    }

    public void ShieldTrigger()
    {
        if (shieldButton.canActivate())
        {
            shieldButton.sendToButton(shieldCooldown);
        }
    }

    public void HealButton()
    {
        if (healButton.canActivate())
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
            StartCoroutine(FireBroadside(enemyShip, turretPosition.aft, 1));
        }
    }

    /// <summary>
    /// Overrides heal trigger and starts cooldown
    /// </summary>
    public override void HealTrigger()
    {
        base.HealTrigger();
        Cooldown();
    }

    public void UltimateTrigger()
    {
        if (ultimateButton.canActivate())
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
        buttonManager.refreshAllCooldowns();
        if (enemyShip.healing) { FullBroadside(enemyShip, 4); }
        LaserFire();
    }

    /// <summary>
    /// Fires big ol' laser
    /// </summary>
    public void LaserFire()
    {
        var emitter = laserEmitter.position;
        var damage = laserDamage;

        SpawnLaser(emitter, enemyShipObj.transform.position);
        enemyShip.TakeDamage(damage);
        enemyShip.DisableShot(disableDuration);
    }

    /// <summary>
    /// For now, disables a random part between cannon or heal (creates 30 second cooldown)
    /// Also, resets combo
    /// </summary>
    public override void DisableShot(float duration)
    {
        combo = 0;
        var whichPart = Random.value > 0.5f;
        if (whichPart)
        {
            attackButton.StartCooldown(duration, Color.red);
        }
        else { healButton.StartCooldown(duration, Color.red); }
    }

    private void Cooldown()
    {
        buttonManager.globalCooldown();
    }

    #endregion Abilities

    #region Shields

    public void ShieldsUp()
    {
        shielded = true;
        //  parry = true;
        retaliate = false;
        SFX.PlayOneShot(SFX_shieldActivate);
        shield = Instantiate(shieldPrefab, transform.position, Quaternion.identity, this.transform);
        shield.tag = "Player";
        shield.name = "Shield";
        shield.transform.SetParent(this.transform);
        shield.transform.localScale = new Vector3(4, 4, 4);
        StartCoroutine(ShieldSustain());
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

        //Shields down
        ShieldsDown();
    }

    /// <summary>
    /// If shield is hit by a cannon shot, drain a small amount of stamina.
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
        retaliate = false;

        shieldStamina.ShieldDown();
        shielded = false;
        Destroy(shield);
    }

    public void SetRetaliate()//Absorbs attack and primes for retaliation
    {
        SFX.PlayOneShot(SFX_absorbAttack);
        //abilityButton.alterCooldown(1.5f);
        retaliate = true;

        shieldStamina.AbsorbAttack();

        var parryValence = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
        var parryArousal = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
        var parryTension = new Emotion(EmotionDirection.decrease, EmotionStrength.moderate);
        affect.CreatePastEvent(parryValence, parryArousal, parryTension, 10.0f);
        // Debug.Log("Absorbed attack");
    }

    #endregion Shields

    #region Laser Barrage

    /// <summary>
    /// Fires retaliation laser
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
            cannon.GetComponent<SciFiProjectileScript>().CannonSetup(damage, enemyShip);
        }
    }

    #endregion Laser Barrage

    protected override void doneDeath()
    {
        base.doneDeath();
        manager.LoseGame();
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