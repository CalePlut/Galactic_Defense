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
    public float health(int level)
    {
<<<<<<< HEAD
        //switch (level)
        //{
        //    case 2: return level2Health;
        //    case 3: return level3Health;
        //    default:}
            return baseHealth;
=======
        return level switch
        {
            2 => level2Health,
            3 => level3Health,
            _ => baseHealth,
        };
>>>>>>> parent of 6488396f (Unity Update/Upkeep)
    }

    [Tooltip("Shield health")]
    public float baseShield;

   // public float level2Shield, level3Shield;

    [HideInInspector]
    public float shield(int level)
    {
<<<<<<< HEAD
       // switch (level)
        //{
            //case 2: return level2Shield;
            //case 3: return level3Shield;
            //default: 
                return baseShield;
       // }
=======
        return level switch
        {
            2 => level2Shield,
            3 => level3Shield,
            _ => baseShield,
        };
>>>>>>> parent of 6488396f (Unity Update/Upkeep)
    }

    [Range(0, 1)]
    [Tooltip("Armour modifier - percentage of total damage taken")]
    public float baseArmour;
        //, level2Armour, level3Armour;

    [HideInInspector]
    public float armour(int level)
    {
<<<<<<< HEAD
        //switch (level)
        //{
        //    case 2: return level2Armour;
        //    case 3: return level3Armour;
            //default: 
            return baseArmour;
      //  }
=======
        return level switch
        {
            2 => level2Armour,
            3 => level3Armour,
            _ => baseArmour,
        };
>>>>>>> parent of 6488396f (Unity Update/Upkeep)
    }

    [Tooltip("Auto-attack damage")]
    public float baseTurretDamage;
    //, level2TurretDamage, level3TurretDamage;

    [HideInInspector]
    public float turretDamage(int level)
    {
<<<<<<< HEAD
        //switch (level)
        //{
        //    case 2: return level2TurretDamage;
        //    case 3: return level3TurretDamage;
            //default: 
            return baseTurretDamage;
       // }
=======
        return level switch
        {
            2 => level2TurretDamage,
            3 => level3TurretDamage,
            _ => baseTurretDamage,
        };
>>>>>>> parent of 6488396f (Unity Update/Upkeep)
    }

    [Tooltip("Shots in combo before beginning to fire continuously")]
    public int baseWarmupShots;
    //level2WarmupShots, level3WarmupShots;

    [HideInInspector]
    public int warmupShots(int level)
    {
<<<<<<< HEAD
        //switch (level)
      //  {
          //  case 2: return level2WarmupShots;
           // case 3: return level3WarmupShots;
           // default:
           return baseWarmupShots;
        //}
=======
        return level switch
        {
            2 => level2WarmupShots,
            3 => level3WarmupShots,
            _ => baseWarmupShots,
        };
>>>>>>> parent of 6488396f (Unity Update/Upkeep)
    }

    [Tooltip("Total number of shots in combo")]
    public int baseMaxShots;
    //, level2MaxShots, level3MaxShots;

    [HideInInspector]
    public int MaxShots(int level)
    {
<<<<<<< HEAD
        //switch (level)
        //{
        //    case 2:
        //        return level2MaxShots;
        //    case 3:
        //        return level3MaxShots;
        //    default:
                return baseMaxShots;
        //}
=======
        return level switch
        {
            2 => level2MaxShots,
            3 => level3MaxShots,
            _ => baseMaxShots,
        };
>>>>>>> parent of 6488396f (Unity Update/Upkeep)
    }

    [Header("Abilities")]
    public float baseHeavyAttackDamage;

   // public float level2HeavyAttackDamage, level3HeavyAttackDamage;

    [HideInInspector]
    public float heavyAttackDamage(int level)
    {
<<<<<<< HEAD
        //switch (level)
        //{
        //    case 2:
        //        return level2HeavyAttackDamage;
        //    case 3:
        //        return level3HeavyAttackDamage;
        //    default:
                return baseHeavyAttackDamage;
       // }
=======
        return level switch
        {
            2 => level2HeavyAttackDamage,
            3 => level3HeavyAttackDamage,
            _ => baseHeavyAttackDamage,
        };
>>>>>>> parent of 6488396f (Unity Update/Upkeep)
    }

    public float baseHeavyAttackDelay;
    //level2HeavyAttackDelay, level3HeavyAttackDelay;

    [HideInInspector]
    public float heavyAttackDelay(int level)
    {
<<<<<<< HEAD
        //switch (level)
        //{
        //    case 2:
        //        return level2HeavyAttackDelay;
        //    case 3:
        //        return level3HeavyAttackDelay;
           // default:
                return baseHeavyAttackDelay;
      //  }
=======
        return level switch
        {
            2 => level2HeavyAttackDelay,
            3 => level3HeavyAttackDelay,
            _ => baseHeavyAttackDelay,
        };
>>>>>>> parent of 6488396f (Unity Update/Upkeep)
    }

    #endregion Basic Attributes

    #region Abilities

    [Tooltip("Parry frame duration (primarily used for player")]
    public float baseParryFrame;
    //level2ParryFrame, level3ParryFrame;

    [HideInInspector]
    public float parryFrame(int level)
    {
<<<<<<< HEAD
        //switch (level)
        //{
        //    case 2: return level2ParryFrame;
        //    case 3: return level3ParryFrame;
        //    default:
            return baseParryFrame;
        //}
=======
        return level switch
        {
            2 => level2ParryFrame,
            3 => level3ParryFrame,
            _ => baseParryFrame,
        };
>>>>>>> parent of 6488396f (Unity Update/Upkeep)
    }

    public float baseRetaliateDamage;
    //level2RetaliateDamage, level3RetaliateDamage;

    [HideInInspector]
    public float retaliateDamage(int level)
    {
<<<<<<< HEAD
        //switch (level)
        //{
        //    case 2:
        //        return level2RetaliateDamage;
        //    case 3:
        //        return level3RetaliateDamage;
        //    default:
                return baseRetaliateDamage;
       // }
=======
        return level switch
        {
            2 => level2RetaliateDamage,
            3 => level3RetaliateDamage,
            _ => baseRetaliateDamage,
        };
>>>>>>> parent of 6488396f (Unity Update/Upkeep)
    }

    [Tooltip("Disable duration on outgoing laser")]
    public float baseDisableDuration;
    //level2DisableDuration, level3DisableDuration;

    [HideInInspector]
    public float disableDuration(int level)
    {
<<<<<<< HEAD
        //switch (level)
        //{
        //    case 2:
        //        return level2DisableDuration;
        //    case 3:
        //        return level3DisableDuration;
        //    default:
                return baseDisableDuration;
      //  }
=======
        return level switch
        {
            2 => level2DisableDuration,
            3 => level3DisableDuration,
            _ => baseDisableDuration,
        };
>>>>>>> parent of 6488396f (Unity Update/Upkeep)
    }

    [Range(0f, 1f)]
    public float baseHealPercent;
    //level2HealPercent, level3HealPercent;

    [HideInInspector]
    public float healPercent(int level)
    {
<<<<<<< HEAD
        //switch (level)
        //{
        //    case 2:
        //        return level2HealPercent;
        //    case 3:
        //        return level3HealPercent;
        //    default:
                return baseHealPercent;
        //}
=======
        return level switch
        {
            2 => level2HealPercent,
            3 => level3HealPercent,
            _ => baseHealPercent,
        };
>>>>>>> parent of 6488396f (Unity Update/Upkeep)
    }

    public float baseHealDelay;
    //level2HealDelay, level3HealDelay;

    [HideInInspector]
    public float healDelay(int level)
    {
<<<<<<< HEAD
        //switch (level)
        //{
        //    case 2:
        //        return level2HealDelay;
        //    case 3:
        //        return level3HealDelay;
        //    default:
                return baseHealDelay;
        //}
=======
        return level switch
        {
            2 => level2HealDelay,
            3 => level3HealDelay,
            _ => baseHealDelay,
        };
>>>>>>> parent of 6488396f (Unity Update/Upkeep)
    }

    #endregion Abilities

    #region Global attributes

    [Tooltip("Multiplier by which the shield reduces the damage of the heavy attack")]
    public float heavyAttackShieldReduction;

    public float shieldRechargeDelay;
    public float shieldBreakDelay;

    #endregion Global attributes
}