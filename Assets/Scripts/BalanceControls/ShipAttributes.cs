using System.Security.Permissions;
using UnityEngine;

[CreateAssetMenu(fileName = "Ship Attributes", menuName = "Balance/Ship Attributes", order = 1)]
public class ShipAttributes : ScriptableObject
{
    #region Basic Attributes
    [Header("Basic Attributes")]
    [Tooltip("Maximum health")]
    public float baseHealth;

    //public float level2Health, level3Health;

    [HideInInspector]
    public float Health(int level)
    {
        //switch (level)
        //{
        //    case 2: return level2Health;
        //    case 3: return level3Health;
        //    default:}
            return baseHealth;
    }

    [Tooltip("Shield health")]
    public float baseShield;

   // public float level2Shield, level3Shield;

    [HideInInspector]
    public float shield(int level)
    {
       // switch (level)
        //{
            //case 2: return level2Shield;
            //case 3: return level3Shield;
            //default: 
                return baseShield;
       // }
    }

    [Range(0, 1)]
    [Tooltip("Armour modifier - percentage of total damage taken")]
    public float baseArmour;
        //, level2Armour, level3Armour;

    [HideInInspector]
    public float armour(int level)
    {
        //switch (level)
        //{
        //    case 2: return level2Armour;
        //    case 3: return level3Armour;
            //default: 
            return baseArmour;
      //  }
    }

    [Tooltip("Auto-attack damage")]
    public float baseTurretDamage;
    //, level2TurretDamage, level3TurretDamage;

    [HideInInspector]
    public float turretDamage(int level)
    {
        //switch (level)
        //{
        //    case 2: return level2TurretDamage;
        //    case 3: return level3TurretDamage;
            //default: 
            return baseTurretDamage;
       // }
    }

    [Tooltip("Shots in combo before beginning to fire continuously")]
    public int baseWarmupShots;
    //level2WarmupShots, level3WarmupShots;

    [HideInInspector]
    public int warmupShots(int level)
    {
        //switch (level)
      //  {
          //  case 2: return level2WarmupShots;
           // case 3: return level3WarmupShots;
           // default:
           return baseWarmupShots;
        //}
    }

    [Tooltip("Total number of shots in combo")]
    public int baseMaxShots;
    //, level2MaxShots, level3MaxShots;

    [HideInInspector]
    public int MaxShots(int level)
    {
        //switch (level)
        //{
        //    case 2:
        //        return level2MaxShots;
        //    case 3:
        //        return level3MaxShots;
        //    default:
                return baseMaxShots;
        //}
    }

    [Header("Abilities")]
    public float baseHeavyAttackDamage;

   // public float level2HeavyAttackDamage, level3HeavyAttackDamage;

    [HideInInspector]
    public float heavyAttackDamage(int level)
    {
        //switch (level)
        //{
        //    case 2:
        //        return level2HeavyAttackDamage;
        //    case 3:
        //        return level3HeavyAttackDamage;
        //    default:
                return baseHeavyAttackDamage;
       // }
    }

    public float baseHeavyAttackDelay;
    //level2HeavyAttackDelay, level3HeavyAttackDelay;

    [HideInInspector]
    public float heavyAttackDelay(int level)
    {
        //switch (level)
        //{
        //    case 2:
        //        return level2HeavyAttackDelay;
        //    case 3:
        //        return level3HeavyAttackDelay;
           // default:
                return baseHeavyAttackDelay;
      //  }
    }

    #endregion Basic Attributes

    #region Abilities

    [Tooltip("Parry frame duration (primarily used for player")]
    public float baseParryFrame;
    //level2ParryFrame, level3ParryFrame;

    [HideInInspector]
    public float parryFrame(int level)
    {
        //switch (level)
        //{
        //    case 2: return level2ParryFrame;
        //    case 3: return level3ParryFrame;
        //    default:
            return baseParryFrame;
        //}
    }

    public float baseRetaliateDamage;
    //level2RetaliateDamage, level3RetaliateDamage;

    [HideInInspector]
    public float retaliateDamage(int level)
    {
        //switch (level)
        //{
        //    case 2:
        //        return level2RetaliateDamage;
        //    case 3:
        //        return level3RetaliateDamage;
        //    default:
                return baseRetaliateDamage;
       // }
    }

    [Tooltip("Disable duration on outgoing laser")]
    public float baseDisableDuration;
    //level2DisableDuration, level3DisableDuration;

    [HideInInspector]
    public float disableDuration(int level)
    {
        //switch (level)
        //{
        //    case 2:
        //        return level2DisableDuration;
        //    case 3:
        //        return level3DisableDuration;
        //    default:
                return baseDisableDuration;
      //  }
    }

    [Range(0f, 1f)]
    public float baseHealPercent;
    //level2HealPercent, level3HealPercent;

    [HideInInspector]
    public float healPercent(int level)
    {
        //switch (level)
        //{
        //    case 2:
        //        return level2HealPercent;
        //    case 3:
        //        return level3HealPercent;
        //    default:
                return baseHealPercent;
        //}
    }

    public float baseHealDelay;
    //level2HealDelay, level3HealDelay;

    [HideInInspector]
    public float healDelay(int level)
    {
        //switch (level)
        //{
        //    case 2:
        //        return level2HealDelay;
        //    case 3:
        //        return level3HealDelay;
        //    default:
                return baseHealDelay;
        //}
    }

    #endregion Abilities

    #region Global attributes

    [Tooltip("Multiplier by which the shield reduces the damage of the heavy attack")]
    public float heavyAttackShieldReduction;

    public float shieldRechargeDelay;
    public float shieldBreakDelay;

    #endregion Global attributes
}