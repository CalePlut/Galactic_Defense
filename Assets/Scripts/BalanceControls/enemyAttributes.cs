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

    [Header("Attack pattern affect")]
    public float turretTension;

    public float specialAttackTension;
    public float healTension;

    [Header("Attack pattern timings")]
    public float turretDelay;

    public float initialDelayMin, initialDelayMax;
    public float specialDelay;
    public float healDelay;

    public float InitialDelay()
    {
        return Random.Range(initialDelayMin, initialDelayMax);
    }
}