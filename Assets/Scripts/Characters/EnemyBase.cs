using System.Collections;
using UnityEngine;

public enum position { left, centre, right };

public class EnemyBase : BasicShip //Provides the common elements in all enemy ships. Base class for EnemyShips AND turrets
{
    #region mechanic variables

    [Header("Mechanics")]
    public EnemyCore core;

    public enemyAttributes attr;
    public AffectVariables affectVars;
    public position pos;

    #endregion mechanic variables

    #region Special Effects vars

    [Header("Visual effects")]
    public GameObject warpIn;

    public GameObject disableFire;
    private GameObject fireObject;

    #endregion Special Effects vars

    #region audio vars

    [Header("Audio")]
    protected AudioSource audioSource;

    public AudioClip SFX_explosion;

    #endregion audio vars

    #region mechanics

    public virtual void setupEnemy(int _stage, GameObject _managerObject)
    {
        gameManager = _managerObject.GetComponent<GameManager>();
        affect = _managerObject.GetComponent<AffectManager>();

        audioSource = GetComponent<AudioSource>();
        alive = true;
        setAttributes(_stage);

        checkActiveBars();
        healthBar.Refresh(maxHealth, health);
        StartCoroutine(healthUpdate());
    }

    public virtual void dummyFire(BasicShip target)
    {
        StartCoroutine(doubleShot(target, "Enemy"));
    }

    public virtual void setAttributes(int _stage)
    {
    }

    #endregion mechanics

    #region bookKeeping

    protected void checkActiveBars()
    {
        if (!healthBar.gameObject.activeSelf) { healthBar.gameObject.SetActive(true); }
    }

    public void playWarp()
    {
        warpIn.GetComponent<ParticleSystem>().Play();
    }

    public void reactiveShieldJam()
    {
        StartCoroutine(disableFireSpawn(2f));
    }

    private IEnumerator disableFireSpawn(float _duration)
    {
        var disable = Instantiate(disableFire, transform.position, Quaternion.identity, this.transform);
        disable.transform.localScale = new Vector3(2, 2, 2);
        yield return new WaitForSeconds(_duration);
        Destroy(disable);
    }

    /// <summary>
    /// Plays explosion sound and informs affectManager
    /// </summary>
    protected override void die()
    {
        if (audioSource != null)
        {
            audioSource.PlayOneShot(SFX_explosion);
        }
        affect.CreatePastEvent(affectVars.moderateValence, 0, -affectVars.weakTension, 10.0f);
        base.die();
    }

    protected override void doneDeath()
    {
        base.doneDeath();
        healthBar.deActivate();
        // Destroy(gameObject);
    }

    #endregion bookKeeping
}