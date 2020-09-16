//using System.Collections.Generic;
//using UnityEngine;

//public class TargetManager : MonoBehaviour
//{
//    public List<EnemyBase> ships;
//    private EnemyShip_old main;
//    private Turret leftTurret, rightTurret;

//    public void setShips(EnemyShip_old _main, Turret _leftTurret, Turret _rightTurret)
//    {
//        main = _main;
//        leftTurret = _leftTurret;
//        rightTurret = _rightTurret;
//    }

//    /// <summary>
//    /// Returns a target based on current attack stance
//    /// </summary>
//    /// <param name="stance">Player Ship's attack stance</param>
//    /// <returns>Target - Random if Aggressive, Core if other</returns>
//    public EnemyBase GetTarget(AttackStance stance)
//    {
//        if (stance == AttackStance.aggressive)
//        {
//            return RandomTarget();
//        }
//        else
//        {
//            return main;
//        }
//    }

//    /// <summary>
//    /// Returns random target, falls through to return main.
//    /// </summary>
//    /// <returns></returns>
//    private EnemyBase RandomTarget()
//    {
//        var targetMain = Random.value < 0.5f;
//        if (!targetMain)
//        {
//            var leftTurretTarget = Random.value < 0.5f;
//            if (leftTurretTarget)
//            {
//                if (leftTurret.alive)
//                {
//                    return leftTurret;
//                }
//                else
//                {
//                    if (rightTurret.alive)
//                    {
//                        return rightTurret;
//                    }
//                    else return main;
//                }
//            }
//            else
//            {
//                if (rightTurret.alive)
//                {
//                    return rightTurret;
//                }
//                else
//                {
//                    if (leftTurret.alive)
//                    {
//                        return leftTurret;
//                    }
//                    else return main;
//                }
//            }
//        }
//        else return main;
//    }

//    public void removeShip(EnemyBase ship)
//    {
//        if (ships.Contains(ship))
//        {
//            ships.Remove(ship);
//        }
//    }

//    public void addShip(EnemyBase ship)
//    {
//        ships.Add(ship);
//    }
//}