using SciFiArsenal;
using UnityEngine;

public class IntelShip : PlayerShip_old
{
    public GameObject FusionCannon;

    private float cannonDamage;

    #region ship

    protected override void setAttacks(int _upgrade)
    {
        if (_upgrade == 0)
        {
            attackDamage = attr.supportDamage;
        }
        else if (_upgrade == 1)
        {
            attackDamage = attr.upgradedSupportDamage;
        }
        else if (_upgrade == 2)
        {
            attackDamage = attr.maxSupportDamage;
        }
        else { Debug.Log("Tried to upgrade beyond max level"); }

        base.setAttacks(_upgrade);
    }

    protected override void setDefend(int _upgrade)
    {
        if (_upgrade == 0)
        {
            maxHealth = attr.supportHealth;
        }
        else if (_upgrade == 1)
        {
            maxHealth = attr.upgradedSupportHealth;
        }
        else if (_upgrade == 2)
        {
            maxHealth = attr.maxSupportHealth;
        }
        else { Debug.Log("Tried to upgrade beyond max level"); }
        health = maxHealth;
        base.setDefend(_upgrade);
    }

    protected override void setSkill(int _upgrade)
    {
        if (_upgrade == 0)
        {
            cannonDamage = attr.fusionCannon;
        }
        else if (_upgrade == 1)
        {
            cannonDamage = attr.fusionCannonUpgrade;
        }
        else if (_upgrade == 2)
        {
            cannonDamage = attr.maxFusionCannon;
        }
        else { Debug.Log("Tried to upgrade beyond max level"); }
        base.setSkill(_upgrade);
    }

    protected override void tellGM()
    {
        base.tellGM();
        // gameManager.disableArtilleryUI();
    }

    #endregion ship

    #region Fusion Cannon

    public void fireFusionCannon() //New artillery move is just a big cannon
    {
        var target = mainEnemy;
        var damage = cannonDamage;
        if (target != null)
        {
            weaponSpawn1.transform.LookAt(target.transform);
            var cannon = Instantiate(FusionCannon, weaponSpawn1.position, Quaternion.identity);
            cannon.transform.SetParent(this.transform);
            cannon.gameObject.tag = tag;
            cannon.layer = 9;
            // cannon.GetComponent<SciFiProjectileScript>().CannonSetup(Mathf.RoundToInt(damage), target);
            //cannon.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
            cannon.transform.LookAt(target.transform);
            cannon.GetComponent<Rigidbody>().AddForce(cannon.transform.forward * 2500);

            globalCooldowns();

            mainEnemy.fusionInterrupt();

            SetStance(AttackStance.aggressive);
        }
    }

    #endregion Fusion Cannon
}