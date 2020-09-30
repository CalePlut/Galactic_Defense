using System.Security.Permissions;
using UnityEngine;

[CreateAssetMenu(fileName = "Ship Attributes", menuName = "Balance/Ship Attributes", order = 1)]
public class ShipAttributes : ScriptableObject
{
    [Header("Basic Attributes")]
    [Tooltip("Maximum health")]
    public float baseHealth;

    public float level2Health, level3Health;

    [HideInInspector]
    public float health(int level)
    {
        switch (level)
        {
            default:
                return baseHealth;

            case 2:
                return level2Health;

            case 3:
                return level3Health;
        }
    }

    [Tooltip("Shield health")]
    public float baseShield;

    public float level2Shield, level3Shield;

    [HideInInspector]
    public float shield(int level)
    {
        switch (level)
        {
            default:
                return baseShield;

            case 2:
                return level2Shield;

            case 3:
                return level3Shield;
        }
    }

    [Range(0, 1)]
    [Tooltip("Armour modifier - percentage of total damage taken")]
    public float baseArmour, level2Armour, level3Armour;

    [HideInInspector]
    public float armour(int level)
    {
        switch (level)
        {
            default:
                return baseArmour;

            case 2:
                return level2Armour;

            case 3:
                return level3Armour;
        }
    }

    [Tooltip("Auto-attack damage")]
    public float baseTurretDamage, level2TurretDamage, level3TurretDamage;

    [HideInInspector]
    public float turretDamage(int level)
    {
        switch (level)
        {
            default:
                return baseTurretDamage;

            case 2:
                return level2TurretDamage;

            case 3:
                return level3TurretDamage;
        }
    }

    [Tooltip("Shots in combo before beginning to fire continuously")]
    public int baseWarmupShots, level2WarmupShots, level3WarmupShots;

    [HideInInspector]
    public int warmupShots(int level)
    {
        switch (level)
        {
            default:
                return baseWarmupShots;

            case 2:
                return level2WarmupShots;

            case 3:
                return level3WarmupShots;
        }
    }

    [Tooltip("Number of shots to fire with both cannons, once warmed up")]
    public int baseDoubleShots, level2DoubleShots, level3DoubleShots;

    [HideInInspector]
    public int DoubleShots(int level)
    {
        switch (level)
        {
            default:
                return baseDoubleShots + baseWarmupShots;

            case 2:
                return level2DoubleShots + level2WarmupShots;

            case 3:
                return level3DoubleShots + level3WarmupShots;
        }
    }

    [Tooltip("Total number of shots in combo")]
    public int baseMaxShots, level2MaxShots, level3MaxShots;

    [HideInInspector]
    public int MaxShots(int level)
    {
        switch (level)
        {
            default:
                return baseMaxShots;

            case 2:
                return level2MaxShots;

            case 3:
                return level3MaxShots;
        }
    }

    [Header("Abilities")]
    public float baseFusionCannonDamage;

    public float level2FusionCannonDamage, level3FusionCannonDamage;

    [HideInInspector]
    public float fusionCannonDamage(int level)
    {
        switch (level)
        {
            default:
                return baseFusionCannonDamage;

            case 2:
                return level2FusionCannonDamage;

            case 3:
                return level3FusionCannonDamage;
        }
    }

    [Tooltip("Parry frame duration (primarily used for player")]
    public float baseParryFrame, level2ParryFrame, level3ParryFrame;

    [HideInInspector]
    public float parryFrame(int level)
    {
        switch (level)
        {
            default:
                return baseParryFrame;

            case 2:
                return level2ParryFrame;

            case 3:
                return level3ParryFrame;
        }
    }

    public float baseLaserDamage, level2LaserDamage, level3LaserDamage;

    [HideInInspector]
    public float laserDamage(int level)
    {
        switch (level)
        {
            default:
                return baseLaserDamage;

            case 2:
                return level2LaserDamage;

            case 3:
                return level3LaserDamage;
        }
    }

    [Tooltip("Disable duration on outgoing laser")]
    public float baseDisableDuration, level2DisableDuration, level3DisableDuration;

    [HideInInspector]
    public float disableDuration(int level)
    {
        switch (level)
        {
            default:
                return baseDisableDuration;

            case 2:
                return level2DisableDuration;

            case 3:
                return level3DisableDuration;
        }
    }

    [Range(0f, 1f)]
    public float baseHealPercent, level2HealPercent, level3HealPercent;

    [HideInInspector]
    public float healPercent(int level)
    {
        switch (level)
        {
            default:
                return baseHealPercent;

            case 2:
                return level2HealPercent;

            case 3:
                return level3HealPercent;
        }
    }

    public float baseHealDelay, level2HealDelay, level3HealDelay;

    [HideInInspector]
    public float healDelay(int level)
    {
        switch (level)
        {
            default:
                return baseHealDelay;

            case 2:
                return level2HealDelay;

            case 3:
                return level3HealDelay;
        }
    }
}