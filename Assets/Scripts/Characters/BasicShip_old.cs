using SciFiArsenal;
using System.Collections;
using UnityEngine;

public class BasicShip_old : MonoBehaviour
{
    #region Attributes

    protected int health = 100;
    protected int maxHealth = 100;
    protected int attackDamage = 10;
    public bool alive { get; protected set; } = true;
    protected bool ultimate;
    protected bool lowHealthProspective = false;
    protected float armorMod = 1.0f;
    protected float lifesteal = 0.0f;

    #endregion Attributes

    #region References

    //Managers and objects for passing
    public GameManager gameManager;

    protected AffectManager affect;

    //Weapons and effects
    public GameObject explosion;

    public GameObject weaponPrefab;
    public Transform weaponSpawn1, weaponSpawn2;
    public GameObject healEffect;

    //UI
    public damageText damageText;

    public healthBarAnimator healthBar;

    #endregion References

    #region Health and Damage

    /// <summary>
    /// Autoattack - can be extended with damage multiplioer and the possibility to heal
    /// </summary>
    /// <param name="target">What we're shooting at</param>
    /// <param name="tag">What we are</param>
    /// <param name="multiplier">Damage multiplier</param>
    public virtual void FireWeapons(BasicShip_old target, string tag, bool heal = false, float multiplier = 1.0f)
    {
        if (target != null)
        {
            lookAtShip(target);

            if (heal)
            {
                multiplier = 0.5f;
                attackLifesteal(attackDamage);
            }
            if (ultimate)
            {
                multiplier = 2.0f;
                attackLifesteal(attackDamage * 2);
            }

            StartCoroutine(doubleShot(target, tag, multiplier));
        }
    }

    /// <summary>
    /// This is how things get hit, adjusted by armormod(only used by players)
    /// </summary>
    /// <param name="_amount">Amount of damage to receive</param>
    public void receiveDamage(int _amount)
    {
        if (alive)
        {
            var amount = (float)_amount;
            amount *= armorMod;
            var percent = amount / health;

            passDamageToAffect(amount);

            var intAmount = Mathf.RoundToInt(amount);

            damageText.TakeDamage(intAmount, percent);
            healthBar.TakeDamage(intAmount);

            health -= intAmount;
        }
    }

    /// <summary>
    /// Heals for life stolen from attack
    /// </summary>
    /// <param name="damage">The damage that the attack did</param>
    public void attackLifesteal(float damage)
    {
        var healthAdd = (float)damage * lifesteal;
        var floatHealth = (float)health + healthAdd;
        health = Mathf.RoundToInt(floatHealth);
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        healthBar.addValue(Mathf.RoundToInt(healthAdd));
    }

    /// <summary>
    /// This is used to heal the ship by a percentage of missing health.
    /// </summary>
    /// <param name="percentage">Float between 0.0f and 1.0f</param>
    public virtual void ReceiveHealing(float percentage) //Heals ship for incoming health percentage
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
    }

    /// <summary>
    /// Does some basic math to return the current percentage health level
    /// </summary>
    /// <returns>Returns health as percentage</returns>
    public float percentHealth()
    {
        return (float)health / (float)maxHealth;
    }

    #region Empty virtuals

    /// <summary>
    /// Used to send damage to affect manager, but different between player and enemy ships.
    /// </summary>
    /// <param name="damage"></param>
    protected virtual void passDamageToAffect(float damage)
    { }

    protected virtual void affectHeathUpdate()
    { }

    #endregion Empty virtuals

    /// <summary>
    /// Used when we have a low health event - watches health value and culls low health event when appropriate
    /// </summary>
    /// <param name="_event"></param>
    /// <returns></returns>
    protected IEnumerator lowHealthEvent(Event _event)
    {
        while (percentHealth() < 0.1f && alive)
        {
            yield return null;
        }
        lowHealthProspective = false;
        affect.CullEvent(_event);
    }

    /// <summary>
    /// Checks to see if we've died, called while alive to make sure we're actually still alive
    /// </summary>
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

    /// <summary>
    /// Always checks health while alive, so that we don't miss death triggers.
    /// </summary>
    /// <returns></returns>
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

    protected void lookAtShip(BasicShip_old ship)
    {
        if (ship != null)
        {
            var toLook = ship.gameObject.transform;
            transform.LookAt(toLook);
        }
    }

    protected void weaponPrefabSpawn(BasicShip_old target, string tag)
    {
        StartCoroutine(doubleShot(target, tag));
    }

    protected IEnumerator doubleShot(BasicShip_old target, string tag, float damageMultiplier = 1.0f)
    {
        var damage = (float)attackDamage;
        damage *= damageMultiplier;
        damage *= 0.5f;
        var weaponDamage = Mathf.RoundToInt(damage);
        if (target != null)
        {
            weaponSpawn1.transform.LookAt(target.transform);
            var cannon = Instantiate(weaponPrefab, weaponSpawn1.position, Quaternion.identity);
            cannon.transform.SetParent(this.transform);
            cannon.gameObject.tag = tag;
            cannon.layer = 9;
            //  cannon.GetComponent<SciFiProjectileScript>().CannonSetup(weaponDamage, target);
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
            //  cannon.GetComponent<SciFiProjectileScript>().CannonSetup(weaponDamage, target);
            //cannon.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
            cannon.transform.LookAt(target.transform);
            cannon.GetComponent<Rigidbody>().AddForce(cannon.transform.forward * 2500);
        }
    }

    #endregion Effects and Animation
}