using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SciFiArsenal;
using UnityEditor;

public class PlayerShip : BasicShip
{
    [Header("Cooldowns")]
    public float skillCooldown = 2.5f;

    public float ultimateCooldown = 30.0f;

    private GameObject enemyShipObj;
    private EnemyShip enemyShip;

    [Header("Positioning objects")]
    public Transform fusionCannon;

    [Header("Effects and Weapons")]
    public GameObject fusionCannonPrefab;

    public GameObject shieldPrefab;

    public shieldStamina shieldStamina;

    [Header("Ship-specific Audio")]
    public AudioSource SFX;

    public AudioClip SFX_shieldActivate;

    public AudioClip SFX_absorbAttack;//, SFX_retaliate;

    public AudioClip SFX_Heal;

    [Header("Hotbar Buttons")]
    public basicButton cannonButton;

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
        shieldStamina.setStamina(shieldDuration);
    }

    #region Abilities

    public void CannonTrigger()
    {
        if (cannonButton.canActivate())
        {
            cannonButton.sendToButton(skillCooldown);
        }
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
        var damage = fusionCannonDamage;
        if (target.alive)
        {
            fusionCannon.transform.LookAt(target.transform);
            var cannon = Instantiate(fusionCannonPrefab, fusionCannon.position, Quaternion.identity);
            cannon.transform.SetParent(fusionCannon);
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
            shieldButton.sendToButton(skillCooldown);
        }
    }

    /// <summary>
    /// Overrides heal with affect and fires two shots from aft cannon
    /// </summary>
    public override void Heal()
    {
        base.Heal();

        var healValence = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
        var healTension = new Emotion(EmotionDirection.decrease, EmotionStrength.weak);
        affect.CreatePastEvent(healValence, null, healTension, 10.0f);
        StartCoroutine(FireBroadside(enemyShip, turretPosition.aft, aftShots));
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
    /// Refreshes all cooldowns and fires 5 shots from each cannon
    /// </summary>
    public void Ultimate()
    {
        buttonManager.refreshAllCooldowns();
        StartCoroutine(FireBroadside(enemyShip, turretPosition.fore, healPunishShots));
        StartCoroutine(FireBroadside(enemyShip, turretPosition.aft, healPunishShots));
    }

    /// <summary>
    /// For now, disables a random part between cannon or heal (creates 30 second cooldown)
    /// </summary>
    public override void DisableShot(float duration)
    {
        var whichPart = Random.value > 0.5f;
        if (whichPart)
        {
            cannonButton.StartCooldown(duration, Color.red);
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
        shield.transform.localScale = new Vector3(10, 10, 10);
        StartCoroutine(ShieldSustain());
    }

    /// <summary>
    /// Keeps shield up as long as key is held down
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShieldSustain()  //Keeps shield up for duration, and removes afterwards
    {
        shieldStamina.shieldsUp(); //Passes shields up message to shieldsustain bar

        while (shieldStamina.shielded)
        {
            yield return null;
        }

        //Shields down
        ShieldsDown();
    }

    /// <summary>
    /// Disables shields - if retaliate is triggered, do so
    /// </summary>
    public void ShieldsDown()
    {
        if (retaliate)
        {
            Retaliate();
        }
        retaliate = false;

        shieldStamina.shieldsDown();
        shielded = false;
        Destroy(shield);
    }

    public void SetRetaliate()//Absorbs attack and primes for retaliation
    {
        SFX.PlayOneShot(SFX_absorbAttack);
        //abilityButton.alterCooldown(1.5f);
        retaliate = true;

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
        var emitter = laserEmitter.position;
        var damage = laserDamage;

        SpawnLaser(emitter, enemyShipObj.transform.position);
        enemyShip.TakeDamage(damage);
        enemyShip.DisableShot(disableDuration);
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