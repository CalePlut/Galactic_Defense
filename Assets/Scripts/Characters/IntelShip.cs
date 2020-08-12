using SciFiArsenal;
using UnityEngine;

public class IntelShip : PlayerShip
{
    public GameObject FusionCannon;

    #region ship

    public override void shipSetup()
    {
        SFX = GetComponent<AudioSource>();
        health = attr.intelHealth;
        maxHealth = attr.intelHealth;
        baseDamage = attr.intelDamage;
        upgrade = false;

        base.shipSetup();
    }

    public override void upgradeShip()
    {
        base.upgradeShip();
        health = attr.intelUpgradeHealth;
        maxHealth = attr.intelUpgradeHealth;
        baseDamage = attr.intelUpgradeDamage;
        base.shipSetup();
    }

    protected override void tellGM()
    {
        base.tellGM();
        gameManager.disableArtilleryUI();
    }

    #endregion ship

    #region Fusion Cannon

    public void fireFusionCannon() //New artillery move is just a big cannon
    {
        var target = mainEnemy;
        var damage = attr.bigCannon;
        if (upgrade) { damage = attr.bigCannonUpgrade; }
        if (target != null)
        {
            weaponSpawn1.transform.LookAt(target.transform);
            var cannon = Instantiate(FusionCannon, weaponSpawn1.position, Quaternion.identity);
            cannon.transform.SetParent(this.transform);
            cannon.gameObject.tag = tag;
            cannon.layer = 9;
            cannon.GetComponent<SciFiProjectileScript>().CannonSetup(damage, target);
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