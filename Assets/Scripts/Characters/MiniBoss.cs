public class MiniBoss : EnemyShip_old
{
    public override void setAttributes(int _stage)
    {
        base.setAttributes(_stage);
        var stage = _stage - 1;

        health = attr.minibossHP[stage];
        maxHealth = attr.minibossHP[stage];

        attackDamage = attr.minibossDamage[stage];
        specialDamage = attr.minibossBeam[stage];

        core.attackSpeed = attr.minibossAttackSpeed;
    }

    public override float getAttackSpeed()
    {
        return attr.minibossAttackSpeed;
    }
}