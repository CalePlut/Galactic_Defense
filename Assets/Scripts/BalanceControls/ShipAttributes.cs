using System.Security.Permissions;
using UnityEngine;

[CreateAssetMenu(fileName = "Ship Attributes", menuName = "Balance/Ship Attributes", order = 1)]
public class ShipAttributes : ScriptableObject
{
    #region Basic Attributes

    [Header("Basic Attributes")]
    [Tooltip("Maximum health")]
    public float baseHealth;

    public float level2Health, level3Health;

    [HideInInspector]
    public float health(int level)
    {
        return level switch
        {
            2 => level2Health,
            3 => level3Health,
            _ => baseHealth,
        };
    }

    [Tooltip("Shield health")]
    public float baseShield;

    public float level2Shield, level3Shield;

    [HideInInspector]
    public float shield(int level)
    {
        return level switch
        {
            2 => level2Shield,
            3 => level3Shield,
            _ => baseShield,
        };
    }

    [Range(0, 1)]
    [Tooltip("Armour modifier - percentage of total damage taken")]
    public float baseArmour, level2Armour, level3Armour;

    [HideInInspector]
    public float armour(int level)
    {
        return level switch
        {
            2 => level2Armour,
            3 => level3Armour,
            _ => baseArmour,
        };
    }

    [Tooltip("Auto-attack damage")]
    public float baseTurretDamage, level2TurretDamage, level3TurretDamage;

    [HideInInspector]
    public float turretDamage(int level)
    {
        return level switch
        {
            2 => level2TurretDamage,
            3 => level3TurretDamage,
            _ => baseTurretDamage,
        };
    }

    [Tooltip("Shots in combo before beginning to fire continuously")]
    public int baseWarmupShots, level2WarmupShots, level3WarmupShots;

    [HideInInspector]
    public int warmupShots(int level)
    {
        return level switch
        {
            2 => level2WarmupShots,
            3 => level3WarmupShots,
            _ => baseWarmupShots,
        };
    }

    [Tooltip("Total number of shots in combo")]
    public int baseMaxShots, level2MaxShots, level3MaxShots;

    [HideInInspector]
    public int MaxShots(int level)
    {
        return level switch
        {
            2 => level2MaxShots,
            3 => level3MaxShots,
            _ => baseMaxShots,
        };
    }

    [Header("Abilities")]
    public float baseHeavyAttackDamage;

    public float level2HeavyAttackDamage, level3HeavyAttackDamage;

    [HideInInspector]
    public float heavyAttackDamage(int level)
    {
        return level switch
        {
            2 => level2HeavyAttackDamage,
            3 => level3HeavyAttackDamage,
            _ => baseHeavyAttackDamage,
        };
    }

    public float baseHeavyAttackDelay, level2HeavyAttackDelay, level3HeavyAttackDelay;

    [HideInInspector]
    public float heavyAttackDelay(int level)
    {
        return level switch
        {
            2 => level2HeavyAttackDelay,
            3 => level3HeavyAttackDelay,
            _ => baseHeavyAttackDelay,
        };
    }

    #endregion Basic Attributes

    #region Abilities

    [Tooltip("Parry frame duration (primarily used for player")]
    public float baseParryFrame, level2ParryFrame, level3ParryFrame;

    [HideInInspector]
    public float parryFrame(int level)
    {
        return level switch
        {
            2 => level2ParryFrame,
            3 => level3ParryFrame,
            _ => baseParryFrame,
        };
    }

    public float baseRetaliateDamage, level2RetaliateDamage, level3RetaliateDamage;

    [HideInInspector]
    public float retaliateDamage(int level)
    {
        return level switch
        {
            2 => level2RetaliateDamage,
            3 => level3RetaliateDamage,
            _ => baseRetaliateDamage,
        };
    }

    [Tooltip("Disable duration on outgoing laser")]
    public float baseDisableDuration, level2DisableDuration, level3DisableDuration;

    [HideInInspector]
    public float disableDuration(int level)
    {
        return level switch
        {
            2 => level2DisableDuration,
            3 => level3DisableDuration,
            _ => baseDisableDuration,
        };
    }

    [Range(0f, 1f)]
    public float baseHealPercent, level2HealPercent, level3HealPercent;

    [HideInInspector]
    public float healPercent(int level)
    {
        return level switch
        {
            2 => level2HealPercent,
            3 => level3HealPercent,
            _ => baseHealPercent,
        };
    }

    public float baseHealDelay, level2HealDelay, level3HealDelay;

    [HideInInspector]
    public float healDelay(int level)
    {
        return level switch
        {
            2 => level2HealDelay,
            3 => level3HealDelay,
            _ => baseHealDelay,
        };
    }

    #endregion Abilities

    #region Global attributes

    [Tooltip("Multiplier by which the shield reduces the damage of the heavy attack")]
    public float heavyAttackShieldReduction;

    public float shieldRechargeDelay;
    public float shieldBreakDelay;

    #endregion Global attributes
}