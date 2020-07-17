using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttributes", menuName = "Balance/playerAttributes", order = 1)]
public class PlayerAttributes : ScriptableObject
{
    [Header("Basic Attributes")]
    public int frigateHealth;

    public int intelHealth;
    public int supportHealth;

    [Space]
    public int frigateDamage;

    public int intelDamage;
    public int supportDamage;

    [Space]
    public float respawnTimer;

    [Header("Upgraded Attributes")]
    public int frigateUpgradeHealth;

    public int intelUpgradeHealth;
    public int supportUpgradeHealth;

    [Space]
    public int frigateUpgradeDamage;

    public int intelUpgradeDamage;
    public int supportUpgradeDamage;

    [Header("Basic Abilities")]
    public int bigCannon;

    public int retaliateDamage;

    [Range(0, 1)]
    public float percentHeal;

    [Header("Upgraded Abilities")]
    public int bigCannonUpgrade;

    public int retaliateDamageUpgrade;

    [Range(0, 1)]
    public float percentHealUpgrade;

    [Header("Ultimate")]
    public float hasteMultiplier;

    public float hasteMultiplierUpgrade;
    public float hasteTime;

    [Header("Cooldowns")]
    // public float fusionCannonCD;
    //  public float shieldCD;
    // public float healCD;
    //  public float GCD;
    public float ultimateCD;
}