using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttributes", menuName = "Balance/enemyAttributes", order = 1)]
public class enemyAttributes : ScriptableObject
{
    [Header("Turrets")]
    public int[] turretHP;

    public int[] turretDamage;

    [Header("Drones")]
    public int[] droneHP, droneDamage, droneBeam;

    public float droneAttackSpeed;

    [Header("Miniboss")]
    public int[] minibossHP, minibossDamage, minibossBeam;

    public float minibossAttackSpeed;

    [Header("Boss")]
    public int bossHP, bossDamage, bossBeam;

    public float bossAttackSpeed;
}