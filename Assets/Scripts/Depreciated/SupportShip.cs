//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;

//public class SupportShip : PlayerShip_old
//{
//    public GameObject weapons;
//    public Transform spawn1, spawn2;

//    public PlayerShip supportTarget;
//    public GameObject hasteActive;
//    public Slider hasteRemain;

//    private float healAmount;

//    public AudioClip SFX_Heal;

//    protected override void setAttacks(int _upgrade)
//    {
//        if (_upgrade == 0)
//        {
//            attackDamage = attr.supportDamage;
//        }
//        else if (_upgrade == 1)
//        {
//            attackDamage = attr.upgradedSupportDamage;
//        }
//        else if (_upgrade == 2)
//        {
//            attackDamage = attr.maxSupportDamage;
//        }
//        else { Debug.Log("Tried to upgrade beyond max level"); }

//        base.setAttacks(_upgrade);
//    }

//    protected override void setDefend(int _upgrade)
//    {
//        if (_upgrade == 0)
//        {
//            maxHealth = attr.supportHealth;
//        }
//        else if (_upgrade == 1)
//        {
//            maxHealth = attr.upgradedSupportHealth;
//        }
//        else if (_upgrade == 2)
//        {
//            maxHealth = attr.maxSupportHealth;
//        }
//        else { Debug.Log("Tried to upgrade beyond max level"); }
//        health = maxHealth;
//        base.setDefend(_upgrade);
//    }

//    protected override void setSkill(int _upgrade)
//    {
//        if (_upgrade == 0)
//        {
//            healAmount = attr.percentHeal;
//        }
//        else if (_upgrade == 1)
//        {
//            healAmount = attr.percentHealUpgrade;
//        }
//        else if (_upgrade == 2)
//        {
//            healAmount = attr.maxPercentHeal;
//        }
//        else { Debug.Log("Tried to upgrade beyond max level"); }
//        base.setSkill(_upgrade);
//    }

//    public void heal()
//    {
//        SFX.PlayOneShot(SFX_Heal);
//        var toHeal = healAmount;
//        //supportTarget.receiveHealing(toHeal);
//        ReceiveHealing(toHeal);
//        foreach (PlayerShip ship in otherShips)
//        {
//            //ship.ReceiveHealing(toHeal);
//        }

//        var healValence = new Emotion(EmotionDirection.increase, EmotionStrength.moderate);
//        var healTension = new Emotion(EmotionDirection.decrease, EmotionStrength.weak);
//        affect.CreatePastEvent(healValence, null, healTension, 10.0f);

//        globalCooldowns();

//        SetStance(AttackStance.regenerative);
//    }

//    protected override void tellGM()
//    {
//        base.tellGM();
//        // gameManager.disableTenderUI();
//    }
//}