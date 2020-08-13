using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttributes", menuName = "Balance/playerAttributes", order = 1)]
public class PlayerAttributes : ScriptableObject
{
    [Header("Basic Attributes")]
    public int frigateDamage;

    public int supportDamage;
    public float attackSpeed; //Speed of attacks when attacking

    [Space]
    public int frigateHealth; //Health of centre ship

    public int supportHealth; //Health of support ships

    public float armorModifier; //Modifier for strength of armor

    [Space]
    public float respawnTimer;

    [Header("Basic Abilities")]
    public int fusionCannon;

    public float cannonSpeedBoost;
    public int retaliateDamage;
    public float retaliateJamLength;

    [Range(0, 1)]
    public float percentHeal;

    [Range(0, 1)]
    public float lifesteal;

    [Header("Attack Upgrades")]
    public int upgradedFrigateDamage;

    public int upgradedSupportDamage;

    public float upgradeAttackSpeed;

    public int maxFrigateDamage;
    public int maxSupportDamage;
    public float maxAttackSpeed;

    [Header("Defense Upgrades")]
    public int upgradedFrigateHealth;

    public int upgradedSupportHealth;
    public float upgradedArmorMoifier;

    public int maxFrigateHealth;
    public int maxSupportHealth;
    public float maxArmorModifier;

    [Header("Skills upgrade")]
    public int fusionCannonUpgrade;

    public float cannonSpeedBoostUpgrade;

    public int retaliateDamageUpgrade;
    public float retaliateJamUpgrade;

    [Range(0, 1)]
    public float percentHealUpgrade;

    [Range(0, 1)]
    public float lifestealUpgrade;

    public int maxFusionCannon;
    public float maxCannonSpeedBoost;
    public int maxRetaliateDamage, maxRetaliateJam;

    [Range(0, 1)]
    public float maxPercentHeal;

    [Range(0, 1)]
    public float maxLifesteal;

    [Header("Ultimate")]
    public float hasteMultiplier;

    public float hasteMultiplierUpgrade;
    public float hasteTime;

    [Header("Cooldowns")]
    public float ultimateCD;
}