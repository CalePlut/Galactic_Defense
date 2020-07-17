public class Boss : EnemyShip
{
    public override void setAttributes(int _stage)
    {
        base.setAttributes(_stage);
        var stage = _stage - 1;

        health = attr.bossHP;
        maxHealth = attr.bossHP;

        baseDamage = attr.bossDamage;
        specialDamage = attr.bossBeam;

        core.attackSpeed = attr.bossAttackSpeed;
    }

    public override float getAttackSpeed()
    {
        return attr.bossAttackSpeed;
    }
}