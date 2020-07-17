using UnityEngine;

public enum position { left, centre, right };

public class EnemyBase : BasicShip //Provides the common elements in all enemy ships. Base class for EnemyShips AND turrets
{
    [Header("Mechanics")]
    public enemyCore core;

    public enemyAttributes attr;

    public position pos;

    [Header("Visual effects")]
    public GameObject warpIn;

    [Header("Audio")]
    protected AudioSource audioSource;

    public AudioClip SFX_explosion;

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

    /// <summary>
    /// Plays explosion sound and informs affectManager
    /// </summary>
    protected override void die()
    {
        if (audioSource != null)
        {
            audioSource.PlayOneShot(SFX_explosion);
        }
        affect.enemydie(pos);
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