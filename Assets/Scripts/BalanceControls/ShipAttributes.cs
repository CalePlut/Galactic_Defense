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

    [Tooltip("Shots from fore cannon when offensive action taken")]
    public int baseForeShots, level2ForeShots, level3ForeShots;

    [HideInInspector]
    public int foreShots(int level)
    {
        switch (level)
        {
            default:
                return baseForeShots;

            case 2:
                return level2ForeShots;

            case 3:
                return level3ForeShots;
        }
    }

    [Tooltip("Shots from aft cannon when defensive action taken")]
    public int baseAftShots, level2AftShots, level3AftShots;

    [HideInInspector]
    public int aftShots(int level)
    {
        switch (level)
        {
            default:
                return baseAftShots;

            case 2:
                return level2AftShots;

            case 3:
                return level3AftShots;
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

    [Tooltip("Shots fired when punishing heal frame - fired from both cannons")]
    public int baseHealPunishShots, level2HealPunishShots, level3HealPunishShots;

    [HideInInspector]
    public int healPunishShots(int level)
    {
        switch (level)
        {
            default:
                return baseHealPunishShots;

            case 2:
                return level2HealPunishShots;

            case 3:
                return level3HealPunishShots;
        }
    }

    [Tooltip("Maximum duration of a full shield - max shield stamina")]
    public float baseShieldDuration, level2ShieldDuration, level3ShieldDuration;

    [HideInInspector]
    public float shieldDuration(int level)
    {
        switch (level)
        {
            default:
                return baseShieldDuration;

            case 2:
                return level2ShieldDuration;

            case 3:
                return level3ShieldDuration;
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