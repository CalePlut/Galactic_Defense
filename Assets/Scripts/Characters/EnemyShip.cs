using System.Collections;
using UnityEngine;

public class EnemyShip : EnemyBase
{
    #region Mechanics

    protected float attackSpeed;
    protected int specialDamage;

    #endregion Mechanics

    #region References and prefabs

    public FrigateShip frigate;
    public IntelShip intel;
    public SupportShip support;
    public GameObject beamPrefab, beamStartPrefab, beamEndPrefab;

    #endregion References and prefabs

    #region Special Effects

    [Header("Adjustable Variables")]
    public float beamEndOffset = 1f; //How far from the raycast hit point the end effect is positioned

    public float textureScrollSpeed = 8f; //How fast the texture scrolls along the beam
    public float textureLengthScale = 3; //Length of the beam texture

    public ParticleSystem beamWarning;

    #endregion Special Effects

    // protected int specialDamage;

    public void setReferences(FrigateShip _frigate, IntelShip _intel, SupportShip _support)
    {
        frigate = _frigate;
        intel = _intel;
        support = _support;
    }

    public virtual float getAttackSpeed()
    {
        return 4.5f;
    }

    public virtual void specialAttack() //Special attack is called to do the special attack - currently everyone is just a beam but that might change
    {
        frigate.receiveDamage(specialDamage);
        if (intel.alive) { intel.receiveDamage(specialDamage); }
        if (support.alive) { support.receiveDamage(specialDamage); }

        laserBarrage();
    }

    public void dummyLaser()
    {
        laserBarrage();
    }

    protected override void doneDeath()
    {
        gameManager.mainShipDie();
        core.destroyTurretsOnDeath();
        base.doneDeath();
        Destroy(gameObject);
    }

    protected override void passDamageToAffect(float damage)
    {
        base.passDamageToAffect(damage);
        affect.enemyHit(damage);

        //Also check health percentage because why not do it here?
        var healthPercent = (float)health / (float)maxHealth;
        affect.updateEnemyHealth(healthPercent, pos);
    }

    private void laserBarrage()
    {
        var emitter = weaponSpawn1.position;
        spawnLaser(emitter, frigate.transform.position);
        if (intel.alive) { spawnLaser(emitter, intel.transform.position); }
        if (support.alive) { spawnLaser(emitter, support.transform.position); }
    }

    private void spawnLaser(Vector3 startLoc, Vector3 endLoc)
    {
        var beamStart = Instantiate(beamStartPrefab, Vector3.zero, Quaternion.identity);
        var beamEnd = Instantiate(beamEndPrefab, Vector3.zero, Quaternion.identity);
        var beam = Instantiate(beamPrefab, Vector3.zero, Quaternion.identity);
        var line = beam.GetComponent<LineRenderer>();

        alignLaser(startLoc, endLoc, beamStart, beamEnd, beam, line);
    }

    private void alignLaser(Vector3 start, Vector3 target, GameObject beamStart, GameObject beamEnd, GameObject beam, LineRenderer line)
    {
        RaycastHit hit;
        if (Physics.Linecast(start, target, out hit))
        {
            beamEnd.transform.position = hit.point;
        }

        line.useWorldSpace = false;

        line.positionCount = 2;
        line.SetPosition(0, start);
        beamStart.transform.position = start;
        line.SetPosition(1, target);

        beamStart.transform.LookAt(beamEnd.transform.position);
        beamEnd.transform.LookAt(beamStart.transform.position);

        float distance = Vector3.Distance(start, target);
        line.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
        line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);

        StartCoroutine(laserLifetime(beamStart, beamEnd, beam));
    }

    private void destroyLaser(GameObject start, GameObject end, GameObject _beam)
    {
        Destroy(start);
        Destroy(end);
        Destroy(_beam);
    }

    private IEnumerator laserLifetime(GameObject start, GameObject end, GameObject beam)
    {
        var shielded = PlayerShip.shielded;
        var fullLifetime = 2.5f;
        while (fullLifetime > 0.0f && PlayerShip.shielded == shielded && alive)
        {
            fullLifetime -= Time.deltaTime;
            yield return null;
        }
        destroyLaser(start, end, beam);
    }

    //IEnumerator BeamSweep()//Activates laser, lerps position between intel and support, and deactivates laser
    //{
    //    var emitter = weaponSpawn1.position;
    //    var currentPos = frigate.transform.position;
    //    if (intel != null)
    //    {
    //        currentPos = intel.transform.position;
    //    }
    //    var targetPos = frigate.transform.position;
    //    if (support != null)
    //    {
    //        targetPos = support.transform.position;
    //    }

    //    spawnLaser(emitter, targetPos);

    //    //Here's the tricky part, the sweep.

    //    //while (Vector3.Distance(currentPos, targetPos) > 0.1f)
    //    //{
    //    //    var lerpPos = Vector3.MoveTowards(currentPos, targetPos, 10f * Time.deltaTime);
    //    //    alignLaser(emitter, lerpPos);
    //    //    currentPos = lerpPos;
    //    //    yield return null;
    //    //}
    //    //destroyLaser();

    //}

    public override void dummyFire(BasicShip target)
    {
        base.dummyFire(target);
        // weaponPrefabSpawn(weaponPrefab, weaponSpawn, target, "Enemy");
    }

    public void specialIndicator(Color col)
    {
        beamWarning.startColor = col;
        beamWarning.Play();
    }

    public void fusionInterrupt()
    {
        if (core.healing)
        {
            core.fusionInterrupt();
        }
    }
}