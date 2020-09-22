using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Video;
using SciFiArsenal;

public enum turretPosition { fore, aft };

public class BasicShip : MonoBehaviour
{
    [Header("Attributes")]
    public ShipAttributes attr;

    protected float maxHealth, health;

    protected float armour;

    protected float turretDamage;
    protected int foreShots, aftShots;

    protected float retaliateDamage;
    protected int healPunishShots;
    protected float shieldDuration;
    protected float laserDamage;
    protected float disableDuration;
    public bool shielded { get; protected set; } = false;

    protected float healPercent, healDelay;

    public bool healing { get; private set; } = false;
    public int level = 1;

    [Header("UI")]
    public damageText damageText;

    public healthBarAnimator healthBar;

    [Header("Weapons - Base")]
    public Transform laserEmitter;

    public Transform foreTurret;
    public Transform aftTurret;

    public Transform flareLoc;

    public GameObject basicTurretShot;

    [Header("Effects - Base")]
    public GameObject warningFlare;

    public GameObject healEffect;
    private GameObject warningFlareRuntimeObject;

    public GameObject nearDeathFirePrefab, nearDeathFire;

    public GameObject explosion;
    public GameObject fire;
    public AudioClip SFX_Explosion;
    protected AudioSource SFX;

    [Header("Laser objects")]
    public GameObject beamStartPrefab;

    public GameObject beamEndPrefab, beamPrefab;

    private GameObject laserRuntimeObject;

    [Header("Laser Presentation Variables")]
    public float beamEndOffset = 1f; //How far from the raycast hit point the end effect is positioned

    public float textureScrollSpeed = 8f; //How fast the texture scrolls along the beam
    public float textureLengthScale = 3; //Length of the beam texture

    [Header("Other")]
    public AffectManager affect;

    public GameManager manager;

    public bool alive { get; private set; } = true;

    /// <summary>
    /// Sets up ship and finds references as needed
    /// </summary>
    public virtual void ShipSetup()
    {
        SFX = GetComponent<AudioSource>();
        var cameraObj = GameObject.Find("Main Camera");
        affect = cameraObj.GetComponent<AffectManager>();
        manager = cameraObj.GetComponent<GameManager>();

        SetAttributes(level);
        healthBar.Refresh(maxHealth, health);
        alive = true;
    }

    /// <summary>
    /// Sets all attributes based on level
    /// Used in initial setups and spawning enemies
    /// Also fully heals ship
    /// </summary>
    /// <param name="level"></param>
    public void SetAttributes(int level)
    {
        SetAttack(level);
        SetDefense(level);
        SetSpecial(level);
        FullHeal();
    }

    /// <summary>
    /// Sets attack attributes based on level
    /// Resets health
    /// </summary>
    /// <param name="level"></param>
    public virtual void SetAttack(int level)
    {
        turretDamage = attr.turretDamage(level);
        foreShots = attr.foreShots(level);
        aftShots = attr.aftShots(level);
        healPunishShots = attr.healPunishShots(level);
    }

    /// <summary>
    /// Sets defense attributes based on level
    /// </summary>
    /// <param name="level"></param>
    public virtual void SetDefense(int level)
    {
        maxHealth = attr.health(level);
        disableDuration = attr.disableDuration(level);
        armour = attr.armour(level);
        shieldDuration = attr.shieldDuration(level);
    }

    /// <summary>
    /// Sets special attributes based on level
    /// </summary>
    /// <param name="level"></param>
    public virtual void SetSpecial(int level)
    {
        healDelay = attr.healDelay(level);
        laserDamage = attr.laserDamage(level);
        retaliateDamage = attr.fusionCannonDamage(level);
        healPercent = attr.healPercent(level);
    }

    /// <summary>
    /// Used between stages to fully heal ship
    /// </summary>
    public void FullHeal()
    {
        health = maxHealth;
        healthBar.addValue((int)maxHealth);
    }

    /// <summary>
    /// This takes damage from any source
    /// </summary>
    /// <param name="_damage"></param>
    public void TakeDamage(float _damage)
    {
        if (alive)
        {
            //If we're healing, the shot deals double damage and interrupts the heal
            if (healing)
            {
                HealInterrupt();
                _damage *= 2f;
            }
            //Take damage
            var damage = _damage * armour;
            var intDamage = Mathf.RoundToInt(damage);
            var percent = damage / health;

            health -= damage; //Actually adjust health...
            damageText.TakeDamage(intDamage, percent);
            healthBar.TakeDamage(intDamage);

            if (health <= 0.0f) { die(); } //If we're dead, we're dead
        }
    }

    public virtual void DisableShot(float duration)
    {
    }

    /// <summary>
    /// Callback called after firing broadside
    /// </summary>
    protected virtual void FinishFiring()
    {
    }

    /// <summary>
    /// Fires cannons "shots" number of times,
    /// </summary>
    /// <param name="target">Target</param>
    /// <param name="position">Fore (aggressive) or Aft (defensive)</param>
    /// <param name="shots"># of shots. Yes, this seems redundant, but it turns out healPunish needs this. Whoops</param>
    /// <returns></returns>
    protected IEnumerator FireBroadside(BasicShip target, turretPosition position, int shots)
    {
        //Debug.Log("Firing " + shots + " shots from " + position);
        var remainingShots = shots;

        while (remainingShots > 0)
        {
            var damage = turretDamage;
            if (target.alive)
            {
                //Sets somewhat random position based on cannon position
                var pos = foreTurret.position;
                if (position == turretPosition.aft)
                {
                    pos = aftTurret.position;
                }
                pos.z += Random.Range(-5, 5);

                //Fires cannon from position
                var cannon = Instantiate(basicTurretShot, pos, Quaternion.identity);
                cannon.transform.SetParent(this.transform);
                cannon.gameObject.tag = tag;
                cannon.layer = 9;
                cannon.GetComponent<SciFiProjectileScript>().CannonSetup(damage, target);
                cannon.transform.LookAt(target.transform);
                cannon.GetComponent<Rigidbody>().AddForce(foreTurret.transform.forward * 2500);

                //Removes shot from remaining
                remainingShots--;
            }

            yield return new WaitForSeconds(0.23075f); //Waits 1 eighth note at 130 bpm
        }
        FinishFiring();
    }

    /// <summary>
    /// Plays a flare with supplied color
    /// </summary>
    /// <param name="col">Color of flare</param>
    /// <param name="duration">Time that it should take</param>
    public void SpecialIndicator(Color col, float duration)
    {
        warningFlareRuntimeObject = Instantiate(warningFlare, transform.position, Quaternion.identity, this.transform);
        warningFlareRuntimeObject.transform.position = flareLoc.position;
        var flare = warningFlareRuntimeObject.GetComponent<ParticleSystem>();
        var main = flare.main;
        main.duration = duration;
        main.startLifetime = duration;
        main.startColor = col;
        flare.Play();
    }

    /// <summary>
    /// Called during special attack, if a heal is being interrupted
    /// </summary>
    public virtual void HealPunish(BasicShip target)
    {
        //Debug.Log("Punishing Heal");
        target.healing = false;
        StartCoroutine(FireBroadside(target, turretPosition.fore, healPunishShots));
        StartCoroutine(FireBroadside(target, turretPosition.aft, healPunishShots));
    }

    /// <summary>
    /// Fires both cannons shots times
    /// </summary>
    /// <param name="target">ship to fire at</param>
    /// <param name="shots">Number of sh ots to fire from both cannons</param>
    protected void FullBroadside(BasicShip target, int shots)
    {
        StartCoroutine(FireBroadside(target, turretPosition.fore, shots));
        StartCoroutine(FireBroadside(target, turretPosition.aft, shots));
    }

    /// <summary>
    /// Begins healing process
    /// </summary>
    public virtual void HealTrigger()
    {
        StartCoroutine(HealDelay());
        SpecialIndicator(Color.green, healDelay);
    }

    public IEnumerator HealDelay()
    {
        healing = true;
        var timer = healDelay;
        while (timer > 0.0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        Heal();
    }

    /// <summary>
    /// Interrupts heal and creates fire explosion thing.
    /// </summary>
    public void HealInterrupt()
    {
        healing = false;
        Destroy(warningFlareRuntimeObject);
        var healFire = Instantiate(fire, flareLoc.position, Quaternion.identity, this.transform);
        //Debug.Log("Interrupted heal");
        //StartCoroutine(HealInterruptBurn(2f));
    }

    /// <summary>
    /// Heals self for % of missing health, determined by healPercent modifier attribute
    /// </summary>
    public virtual void Heal()
    {
        if (healing)
        {
            var missingHealth = maxHealth - health;
            // Debug.Log("missing " + missingHealth + "health");
            var amount = healPercent * missingHealth;
            //  Debug.Log("Amount = " + healPercent + "* missing = " + amount);

            // Debug.Log("Healing for " + amount);
            health += amount;

            if (health > maxHealth)
            {
                health = maxHealth;
            }
            healthBar.addValue(Mathf.RoundToInt(amount));

            healing = false;

            var healingEffect = Instantiate(healEffect, transform.position, Quaternion.identity, this.transform);
            healingEffect.transform.SetParent(this.transform);
        }
    }

    #region Death

    /// <summary>
    /// Sets alive to false, explodes, and begins wait for doneDeath
    /// </summary>
    protected virtual void die()
    {
        alive = false;
        SFX.PlayOneShot(SFX_Explosion);
        var deathExplode = Instantiate(explosion, this.transform.position, Quaternion.identity, this.transform);
        StartCoroutine(death());
    }

    private IEnumerator death()
    {
        yield return new WaitForSeconds(1.0f);
        doneDeath();
    }

    protected virtual void doneDeath()
    { }

    #endregion Death

    #region Laser Fire

    private bool firing = false;

    protected void SpawnLaser(Vector3 startLoc, Vector3 endLoc)
    {
        firing = true;
        var beamStart = Instantiate(beamStartPrefab, Vector3.zero, Quaternion.identity);
        var beamEnd = Instantiate(beamEndPrefab, Vector3.zero, Quaternion.identity);
        var beam = Instantiate(beamPrefab, Vector3.zero, Quaternion.identity);
        var line = beam.GetComponent<LineRenderer>();

        AlignLaser(startLoc, endLoc, beamStart, beamEnd, beam, line);
    }

    private void AlignLaser(Vector3 start, Vector3 target, GameObject beamStart, GameObject beamEnd, GameObject beam, LineRenderer line)
    {
        if (Physics.Linecast(start, target, out RaycastHit hit))
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

    private IEnumerator laserLifetime(GameObject start, GameObject end, GameObject beam)
    {
        var timer = 2.5f;
        while (firing && timer > 0.0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        Debug.Log("Calling destroy laser");
        DestroyLaser(start, end, beam);
        firing = false;
    }

    public void InterruptLaser()
    {
        Debug.Log("Interrupting laser");
        firing = false;
    }

    private void DestroyLaser(GameObject start, GameObject end, GameObject _beam)
    {
        Destroy(start);
        Destroy(end);
        Destroy(_beam);
    }

    #endregion Laser Fire
}