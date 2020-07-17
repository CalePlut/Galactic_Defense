using SciFiArsenal;
using System.Collections;
using UnityEngine;

public class BasicShip : MonoBehaviour
{
    //Attributes
    protected int health = 100;

    protected int maxHealth = 100;
    protected bool upgrade = false;
    protected int baseDamage = 10;
    //  protected float damageMultipler = 1.0f; //OUTGOING damage multiplier
    //protected float laserBase = 0;
    //protected bool isInspired = false;
    //protected float armorMultiplier = 1.0f; //INCOMING damage multiplier. Modified only in frigate

    //References
    public GameManager gameManager;

    protected AffectManager affect;
    public damageText damageText;
    public healthBarAnimator healthBar;

    public GameObject explosion;

    public GameObject weaponPrefab;
    public Transform weaponSpawn1, weaponSpawn2;

    public GameObject healEffect;

    public bool alive { get; protected set; } = true;
    protected bool ultimate;

    #region Health and Damage

    public virtual void fireWeapons(BasicShip target, string tag) //Basic attack - all ships have this, deals damage to enemy. Can optionally include a bool for special attack that is used in playership override
    {
        if (target != null)
        {
            lookAtShip(target);
            StartCoroutine(doubleShot(target, tag));

            var outgoing = baseDamage;
            if (ultimate)
            {
                outgoing *= 2;
                receiveHealing((float)outgoing * 0.1f);
            }
        }
    }

    public void receiveDamage(int _amount) //Receives damage, lowering health, telling damage text and health bar
    {
        if (alive)
        {
            var amount = _amount;
            var percent = (float)amount / health;

            passDamageToAffect((float)amount);

            damageText.takeDamage(amount, percent);
            healthBar.takeDamage(amount);

            health -= amount;
        }
    }

    public virtual void receiveDamage(int _amount, bool special)
    {
        if (alive)
        {
            var amount = _amount;
            var percent = (float)amount / health;

            passDamageToAffect((float)amount);

            damageText.takeDamage(amount, percent);
            healthBar.takeDamage(amount);

            health -= amount;
        }
    }

    public virtual void receiveHealing(float percentage) //Heals ship for incoming health percentage
    {
        var missingHealth = maxHealth - health;
        var amount = percentage * missingHealth;

        health += Mathf.RoundToInt(amount);
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        healthBar.addValue(Mathf.RoundToInt(amount));

        var healingEffect = Instantiate(healEffect, transform.position, Quaternion.identity, this.transform);
        var missingDisp = maxHealth - health;
        var toDisplay = missingDisp * percentage;
        damageText.receiveHealing((int)toDisplay, percentage);
    }

    protected virtual void affectHeathUpdate()
    { }

    public float percentHealth()
    {
        return (float)health / (float)maxHealth;
    }

    protected virtual void passDamageToAffect(float damage)
    {
    } //Overriden in both enemy and player ships, as the affect results from both are different

    protected virtual void healthEval()
    {
        if (health <= 0)
        {
            die();
        }
        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    protected IEnumerator healthUpdate()
    {
        while (alive)
        {
            healthEval();
            yield return null;
        }
    }

    #endregion Health and Damage

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

    #region Effects and Animation

    protected void lookAtShip(BasicShip ship)
    {
        var toLook = ship.gameObject.transform;
        transform.LookAt(toLook);
    }

    protected void weaponPrefabSpawn(BasicShip target, string tag)
    {
        StartCoroutine(doubleShot(target, tag));
    }

    protected IEnumerator doubleShot(BasicShip target, string tag)
    {
        if (target != null)
        {
            weaponSpawn1.transform.LookAt(target.transform);
            var cannon = Instantiate(weaponPrefab, weaponSpawn1.position, Quaternion.identity);
            cannon.transform.SetParent(this.transform);
            cannon.gameObject.tag = tag;
            cannon.layer = 9;
            cannon.GetComponent<SciFiProjectileScript>().CannonSetup(baseDamage / 2, target);
            //cannon.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
            cannon.transform.LookAt(target.transform);
            cannon.GetComponent<Rigidbody>().AddForce(cannon.transform.forward * 2500);
        }
        yield return new WaitForSeconds(0.188f);
        if (target != null)
        {
            weaponSpawn2.transform.LookAt(target.transform);
            var cannon = Instantiate(weaponPrefab, weaponSpawn2.position, Quaternion.identity);
            // weaponSpawn2.transform.LookAt(target.transform);
            cannon.transform.SetParent(this.transform);
            cannon.gameObject.tag = tag;
            cannon.layer = 9;
            cannon.GetComponent<SciFiProjectileScript>().CannonSetup(baseDamage / 2, target);
            //cannon.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
            cannon.transform.LookAt(target.transform);
            cannon.GetComponent<Rigidbody>().AddForce(cannon.transform.forward * 2500);
        }
    }

    #endregion Effects and Animation
}