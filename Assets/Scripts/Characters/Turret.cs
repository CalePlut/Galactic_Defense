using System.Collections;
using UnityEngine;

public class Turret : EnemyBase
{
    public Transform upperMuzzle, lowerMuzzle;
    public GameObject turretWarpWindow;
    public GameObject mesh;

    public override void setAttributes(int _stage)
    {
        var stage = _stage - 1; //0-index strikes again

        health = attr.turretHP[stage];
        maxHealth = attr.turretHP[stage];
        baseDamage = attr.turretDamage[stage];

        base.setAttributes(_stage);
    }

    protected override void doneDeath()
    {
        //Remove from targetting
        //gameManager.removeTurretfromTargetting(this);

        alive = false;
        mesh.GetComponent<MeshRenderer>().enabled = false;

        base.doneDeath(); //Base deactivates health bar
    }

    public void destroyTurret()
    {
        Destroy(healthBar.gameObject);
        Destroy(gameObject);
    }

    /// <summary>
    /// Respawns turret, activates health bar, sets alive to true, and warps in
    /// </summary>
    public void beginTurretRespawn()
    {
        StartCoroutine(respawnTurret());
    }

    //Respawn logic here
    private IEnumerator respawnTurret()  //Creates warp window and re-spawns turret.
    {
        var window = Instantiate(turretWarpWindow, new Vector3(0, 0, -7.5f), Quaternion.Euler(90, 0, 0), this.transform);
        window.transform.localPosition = new Vector3(0, 0, -7.5f);
        window.GetComponent<portalAppear>().warp(0.75f);
        yield return new WaitForSeconds(2.5f);
        StartCoroutine(respawnWarp());
    }

    private IEnumerator respawnWarp() //This is copied from the enemycore warpIn
    {
        var targetPos = transform.position;
        var prewarpPos = transform.position;
        prewarpPos.z -= 500;
        transform.localScale = new Vector3(1, 1, 10);
        transform.position = prewarpPos;
        mesh.GetComponent<MeshRenderer>().enabled = true;
        while (Vector3.Distance(transform.position, targetPos) > 75f)
        {
            var newPosition = Vector3.MoveTowards(transform.position, targetPos, 1000.0f * Time.deltaTime);
            transform.position = newPosition;
            yield return null;
        }
        while (transform.localScale.z < 1.0f)
        {
            var newPosition = Vector3.MoveTowards(transform.position, targetPos, 40.0f * Time.deltaTime);
            transform.position = newPosition;
            var z = transform.localScale.z;
            z -= 40.0f * Time.deltaTime;
            transform.localScale = new Vector3(1, 1, z);
            yield return null;
        }
        transform.position = targetPos;
        transform.localScale = new Vector3(1, 1, 1);

        gameManager.addTurrettoTargetting(this); //Except for this part, which adds the turret back to the gm.
        healthBar.gameObject.SetActive(true);
        health = maxHealth;
        healthBar.Refresh(maxHealth, health);
        alive = true;
        StartCoroutine(healthUpdate());
    }
}