using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using SciFiArsenal;

public enum cannonPosition { fore, aft };

public class BasicShip : MonoBehaviour
{
    [Header("Basic Attributes - Base")]
    public float maxHealth = 100.0f;

    public float attackDamage;

    public float health { get; protected set; } = 100.0f;
    public float healTime = 0.75f; //Time until the heal starts after triggered
    public bool healing { get; protected set; } = false;
    public int punishShots;

    [Header("Modifiers")]
    public float damageMod = 1.0f;

    public float armorMod = 1.0f;

    [Range(0.0f, 1.0f)]
    public float healPercent;

    [Header("UI")]
    public damageText damageText;

    public healthBarAnimator healthBar;

    [Header("Weapons - Base")]
    public Transform laserEmitter;

    public Transform foreCannons;
    public Transform aftCannons;

    public Transform flareLoc;

    public GameObject basicCannon;

    [Header("Effects - Base")]
    public GameObject warningFlare;

    public GameObject healEffect;

    public GameObject explosion;

    [Header("Laser objects")]
    public GameObject beamStartPrefab;

    public GameObject beamEndPrefab, beamPrefab;

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
        var cameraObj = GameObject.Find("Main Camera");
        affect = cameraObj.GetComponent<AffectManager>();
        manager = cameraObj.GetComponent<GameManager>();

        health = maxHealth;
        healthBar.Refresh(maxHealth, health);
        alive = true;
    }

    /// <summary>
    /// This takes damage from any source
    /// </summary>
    /// <param name="_damage"></param>
    public void TakeDamage(float _damage)
    {
        if (alive)
        {
            var damage = _damage * armorMod;
            var intDamage = Mathf.RoundToInt(damage);
            var percent = damage / health;

            health -= damage; //Actually adjust health...
            damageText.TakeDamage(intDamage, percent);
            healthBar.TakeDamage(intDamage);

            if (health <= 0.0f) { die(); } //If we're dead, we're dead
        }
    }

    /// <summary>
    /// Fires cannons "shots" number of times,
    /// </summary>
    /// <param name="target">Target</param>
    /// <param name="position">Fore (aggressive) or Aft (defensive)</param>
    /// <param name="shots">Number of shots to fire</param>
    /// <returns></returns>
    protected IEnumerator FireBroadside(BasicShip target, cannonPosition position, int shots)
    {
        //Debug.Log("Firing " + shots + " shots from " + position);
        var remainingShots = shots;

        while (remainingShots > 0)
        {
            var damage = attackDamage * damageMod;
            if (target.alive)
            {
                //Sets somewhat random position based on cannon position
                var pos = foreCannons.position;
                if (position == cannonPosition.aft)
                {
                    pos = aftCannons.position;
                }
                pos.z += Random.Range(-5, 5);

                //Fires cannon from position
                var cannon = Instantiate(basicCannon, pos, Quaternion.identity);
                cannon.transform.SetParent(this.transform);
                cannon.gameObject.tag = tag;
                cannon.layer = 9;
                cannon.GetComponent<SciFiProjectileScript>().CannonSetup(damage, target);
                cannon.transform.LookAt(target.transform);
                cannon.GetComponent<Rigidbody>().AddForce(foreCannons.transform.forward * 2500);

                //Removes shot from remaining
                remainingShots--;
            }

            yield return new WaitForSeconds(0.23075f); //Waits 1 eighth note at 130 bpm
        }
    }

    /// <summary>
    /// Begins healing process
    /// </summary>
    public virtual void HealTrigger()
    {
        StartCoroutine(HealDelay());
        SpecialIndicator(Color.green, healTime);
    }

    /// <summary>
    /// Plays a flare with supplied color
    /// </summary>
    /// <param name="col">Color of flare</param>
    /// <param name="duration">Time that it should take</param>
    public void SpecialIndicator(Color col, float duration)
    {
        var beamWarning = Instantiate(warningFlare, transform.position, Quaternion.identity, this.transform);
        beamWarning.transform.position = flareLoc.position;
        var flare = beamWarning.GetComponent<ParticleSystem>();
        var main = flare.main;
        main.duration = duration;
        main.startLifetime = duration;
        main.startColor = col;
        flare.Play();
    }

    /// <summary>
    /// Called during special attack, if a heal is being interrupted
    /// </summary>
    public virtual void HealPunish(BasicShip target, int shots)
    {
        //Debug.Log("Punishing Heal");
        target.healing = false;
        StartCoroutine(FireBroadside(target, cannonPosition.fore, shots));
        StartCoroutine(FireBroadside(target, cannonPosition.aft, shots));
    }

    public IEnumerator HealDelay()
    {
        healing = true;
        var timer = healTime;
        while (timer > 0.0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        Heal();
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

    protected void SpawnLaser(Vector3 startLoc, Vector3 endLoc)
    {
        var beamStart = Instantiate(beamStartPrefab, Vector3.zero, Quaternion.identity);
        var beamEnd = Instantiate(beamEndPrefab, Vector3.zero, Quaternion.identity);
        var beam = Instantiate(beamPrefab, Vector3.zero, Quaternion.identity);
        var line = beam.GetComponent<LineRenderer>();

        AlignLaser(startLoc, endLoc, beamStart, beamEnd, beam, line);
    }

    private void AlignLaser(Vector3 start, Vector3 target, GameObject beamStart, GameObject beamEnd, GameObject beam, LineRenderer line)
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

    private IEnumerator laserLifetime(GameObject start, GameObject end, GameObject beam)
    {
        yield return new WaitForSeconds(2.5f);
        DestroyLaser(start, end, beam);
    }

    private void DestroyLaser(GameObject start, GameObject end, GameObject _beam)
    {
        Destroy(start);
        Destroy(end);
        Destroy(_beam);
    }

    #endregion Laser Fire
}