using System.Collections;
using UnityEngine;

public class FrigateShip : PlayerShip
{
    //Reactive Shield
    [Header("Reactive Shield ability")]
    public GameObject shieldPrefab;

    private GameObject shield;

    public float shieldDuration = 1.5f;
    public shieldStamina shieldStamina;

    //Retaliation
    [Header("Retaliation")]
    public GameObject beamStartPrefab;

    public GameObject beamEndPrefab, beamPrefab;
    public Transform retaliateSpawn;
    private bool retaliate;

    [Header("Ultimate")]
    //Ultimate
    public pressButton ultimateButton;

    [Header("Ship-specific Audio")]
    public AudioClip SFX_shieldActivate;

    public AudioClip SFX_absorbAttack;//, SFX_retaliate;

    [Header("Adjustable Variables")]
    public float beamEndOffset = 1f; //How far from the raycast hit point the end effect is positioned

    public float textureScrollSpeed = 8f; //How fast the texture scrolls along the beam
    public float textureLengthScale = 3; //Length of the beam texture

    #region Ship

    public override void shipSetup()
    {
        health = attr.frigateHealth;
        maxHealth = attr.frigateHealth;
        baseDamage = attr.frigateDamage;
        upgrade = false;
        // laserCharge = false;

        base.shipSetup();
    }

    protected override void die()
    {
        base.die();
        gameManager.LoseGame();
    }

    public override void upgradeShip()
    {
        base.upgradeShip();

        health = attr.frigateUpgradeHealth;
        maxHealth = attr.frigateUpgradeHealth;
        baseDamage = attr.frigateUpgradeDamage;
        base.shipSetup();
    }

    #endregion Ship

    #region ultimate

    public void triggerUltimate()
    {
        var valenceEmotion = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
        var arousalEmotion = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);

        affect.CreatePastEvent(valenceEmotion, arousalEmotion, null, 15.0f);

        activateUltimate(attr.hasteTime);
        foreach (PlayerShip ship in otherShips)
        {
            ship.activateUltimate(attr.hasteTime);
        }
    }

    public virtual void ultimateAbility()
    {
        if (ultimateButton.canActivate())
        {
            ultimateButton.sendToButton(attr.ultimateCD);
        }
    }

    #endregion ultimate

    #region Reactive Shield and Retaliate

    /// <summary>
    /// Activates Reactive Shield
    /// </summary>
    public void ShieldsUp() //Activates shield
    {
        //    Debug.Log("Shields up");
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
        SetStance(AttackStance.defensive);
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

        //If we absorbed an attack, return fire when shield goes down.
        shieldDown();
        if (retaliate)
        {
            retaliateAttack();
        }

        retaliate = false;
    }

    public void absorbSimpleAttack()
    {
    }

    public void releaseShield()
    {
    }

    public void shieldDown()
    {
        shieldStamina.shieldsDown();
        shielded = false;
        Destroy(shield);
    }

    public void absorbedSpeccialAttack()//Absorbs attack and primes for retaliation
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

    public void retaliateAttack()
    {
        laserBarrage();
        //globalCooldowns();
    }

    private void laserBarrage()  //Fires retaliation lasers
    {
        //Debug.Log("Firing lasers");
        var emitter = retaliateSpawn.position;
        var damage = attr.retaliateDamage;
        if (upgrade) { damage = attr.retaliateDamageUpgrade; }

        spawnLaser(emitter, mainEnemy.transform.position);
        mainEnemy.receiveDamage(damage);

        foreach (Turret turret in turrets)
        {
            if (turret.alive)
            {
                turret.receiveDamage(damage);
                spawnLaser(emitter, turret.transform.position);
            }
        }
    }

    private void spawnLaser(Vector3 startLoc, Vector3 endLoc)
    {
        var beamStart = Instantiate(beamStartPrefab, Vector3.zero, Quaternion.identity);
        var beamEnd = Instantiate(beamEndPrefab, Vector3.zero, Quaternion.identity);
        var beam = Instantiate(beamPrefab, Vector3.zero, Quaternion.identity);
        var line = beam.GetComponent<LineRenderer>();

        alignLaser(startLoc, endLoc, beamStart, beamEnd, beam, line);
    }

    private void alignLaser(Vector3 start, Vector3 target, GameObject beamStart, GameObject beamEnd, GameObject beam, LineRenderer line)
    {
        RaycastHit hit;
        if (Physics.Linecast(start, target, out hit))
        {
            beamEnd.transform.position = hit.point;
        }

        line.useWorldSpace = false;

        line.positionCount = 2;
        line.SetPosition(0, start);
        beamStart.transform.position = start;
        //beamEnd.transform.position = target;
        line.SetPosition(1, target);

        beamStart.transform.LookAt(beamEnd.transform.position);
        beamEnd.transform.LookAt(beamStart.transform.position);

        float distance = Vector3.Distance(start, target);
        line.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
        line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);

        StartCoroutine(laserLifetime(beamStart, beamEnd, beam));
    }

    private void destroyLaser(GameObject start, GameObject end, GameObject _beam)
    {
        Destroy(start);
        Destroy(end);
        Destroy(_beam);
    }

    private IEnumerator laserLifetime(GameObject start, GameObject end, GameObject beam)
    {
        yield return new WaitForSeconds(2.5f);
        destroyLaser(start, end, beam);
    }

    #endregion Reactive Shield and Retaliate
}