using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SupportShip : PlayerShip
{
    public GameObject weapons;
    public Transform spawn1, spawn2;

    public PlayerShip supportTarget;
    public GameObject hasteActive;
    public Slider hasteRemain;

    public AudioClip SFX_Heal;

    public override void shipSetup()
    {
        health = attr.supportHealth;
        maxHealth = attr.supportHealth;
        baseDamage = attr.supportDamage;
        upgrade = false;

        base.shipSetup();
    }

    public override void upgradeShip()
    {
        base.upgradeShip();

        health = attr.supportUpgradeHealth;
        maxHealth = attr.supportUpgradeHealth;
        baseDamage = attr.supportUpgradeDamage;
        base.shipSetup();
    }

    public void heal()
    {
        SFX.PlayOneShot(SFX_Heal);
        var toHeal = attr.percentHeal;
        if (upgrade) { toHeal = attr.percentHealUpgrade; }
        //supportTarget.receiveHealing(toHeal);
        receiveHealing(toHeal);
        foreach (PlayerShip ship in otherShips)
        {
            ship.receiveHealing(toHeal);
        }
        affect.healShip(upgrade);
        globalCooldowns();
    }

    public void haste()
    {
        //audio.ability2Sound();
        var hasteMultiplier = attr.hasteMultiplier;
        if (upgrade) { hasteMultiplier = attr.hasteMultiplierUpgrade; }
        //supportTarget.hasteShip(hasteMultiplier, 15.0f);

        //buttonManager.setHaste(hasteMultiplier, 20.0f);
        buttonManager.globalCooldown(15.0f);
        StartCoroutine(hasteIndicator(15.0f));

        //activateUltimate(15.0f, 4.0f);
        foreach (PlayerShip ship in otherShips)
        {
            //ship.activateUltimate(15.0f, hasteMultiplier);
        }
        //affect.hasteBuff();
        globalCooldowns();
    }

    private IEnumerator hasteIndicator(float time)
    {
        hasteActive.SetActive(true);
        hasteRemain.gameObject.SetActive(true);
        hasteRemain.maxValue = time;
        hasteRemain.value = time;
        while (hasteRemain.value > 0)
        {
            hasteRemain.value -= Time.deltaTime;
            yield return null;
        }
        hasteActive.SetActive(false);
        hasteRemain.gameObject.SetActive(false);
        //affect.clearHaste();
    }

    //public void setHealTarget(PlayerShip _tar)
    //{
    //    supportTarget = _tar;
    //    abilityButton.clearTargetting();
    //    abilityButton.sendToButton(abilityCooldown);
    //}
    //public void setHasteTarget(PlayerShip _tar)
    //{
    //    supportTarget = _tar;
    //    ultimateButton.clearTargetting();
    //    ultimateButton.sendToButton(ultimateCooldown);
    //}
    protected override void affectHeathUpdate()
    {
        base.affectHeathUpdate();
        affect.updateSupportHealth(percentHealth());
    }

    protected override void tellGM()
    {
        base.tellGM();
        gameManager.disableTenderUI();
    }
}