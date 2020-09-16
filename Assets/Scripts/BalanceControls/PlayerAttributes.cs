using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttributes", menuName = "Balance/Player Attributes", order = 1)]
public class PlayerAttributes : ScriptableObject
{
    [Header("Basic Attributes")]
    [Tooltip("Maximum health")]
    public float baseHealth;

    public float level2Health, level3Health;

    [Tooltip("Auto-attack damage")]
    public float baseDamage, level2Damage, level3Damage;

    [Tooltip("Shots from fore cannon when offensive action taken")]
    public float baseForeShots, level2ForeShots, level3ForeShots;

    [Tooltip("Shots from aft cannon when defensive action taken")]
    public float baseAftShots, level2AftShots, level3AftShots;

    public float baseArmour, level2Armour, level3Armour;

    [Header("Abilities")]
    public float baseFusionCannonDamage;

    public float level2FusionCannonDamage, level3FusionCannonDamage;

    [Tooltip("Shots fired when punishing heal frame - fired from both cannons")]
    public int baseHealPunishShots, level2HealPunishShots, level3HealPunishShots;

    public float baseShieldDuration, level2ShieldDuration, level3ShieldDuration;

    public int baseRetaliateDamage, level2RetaliateDamage, level3RetaliateDamage;

    [Tooltip("Disable duration on outgoing laser")]
    public float baseDisableDuration, level2DisableDuration, level3DisableDuration;

    [Range(0f, 1f)]
    public float baseHealPercent, level2HealPercent, level3HealPercent;

    public float baseHealDelay, level2HealDelay, level3HealDelay;

    [Header("Cooldowns")]
    public float skillCooldown;

    public float ultimateCD;
}