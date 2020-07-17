public enum move { Attack, missileLock, Defend } //This enum handles the RPS-combat

public class PlayerController : BasicShip
{
    //   //Player object containst the values for its stats, the bars are all controlled by the player
    //   public int reactor { get; private set; }
    //   public int sensors { get; private set; }
    //   public int engine { get; private set; }
    //   public int shield { get; private set; }

    //   public int level { get; private set; } //New leveling means each ship is responsible for its own level, rather than tracking stats.

    //   //int reactorUpgrade = 0, sensorUpgrade = 0, engineUpgrade = 0;

    //   //Attribute enum controlls which stat gets a damage boost
    //   //Attribute bonus;

    //   //[Header("Attribute controls")]
    //   //public int statStart = 1;
    //   //public int statMax = 4, shieldStart = 1, shieldMax = 3;
    //   //int upgradePoints;
    //   ////int level = 1;

    //   [Header("UI")]
    //   //public StatUI statPanel;
    //   public WarpFlash warpFlash;
    //   //public ResolutionManager combatInfo;
    //   //public missileCounter missileSilo;
    //   //public GameObject lockOnReticle;
    //   //public GameObject overShield;
    //  // public reticleUI reticle;
    //   //public TextMeshProUGUI upgradeText;

    //   [Header("Mechanics")]
    //  // public GameManager gameManager;
    //   //public float warpSpeed;
    //   //public float chaseSpeed;
    //   //public float enemyWait;
    //   //bool delayPursuit = false;
    //   //bool damageBoost = false;
    //   //public int lockOn { get; private set; } = 0;
    //  // public bool shielded { get; private set; } = false;
    //  // List<int> comboDamage = new List<int> { 0, 1, 2, 4, 6 };

    //   [Header("Special Effects")]
    //   public ParticleSystem warp;
    // //  public ParticleSystem explosion;
    //   //Animator anim;
    ////   public LaserControl laserBank;
    //  // public MissileLauncher missileLaunch;
    //   public PlayerAudio playerAudio;

    //   [Header("Music")]
    //   public MusicManager music;

    //   //Enemy enemy;

    //   // Start is called before the first frame update
    //   void Start()
    //   {
    //       //Sets stat values to starting values
    //      // reactor = statStart;
    //    //   sensors = statStart;
    //      // engine = statStart;
    //      // shield = shieldStart;

    //      // statPanel.Setup(statStart, statMax, shieldStart, shieldMax);

    //       //uses updateUI to set initial values
    //       updateUI();

    //       warp.Stop();

    //   }

    //   // Update is called once per frame
    //   void Update()
    //   {
    //   }

    //   #region Warp
    //   //public void selectDestination(int _bonus, bool pursuitChange)
    //   //{
    //   //    delayPursuit = false;
    //   //    damageBoost = false;
    //   //    if (_bonus == 1) { damageBoost = true; }
    //   //    if (_bonus == 2)
    //   //    {
    //   //        if (shield < shieldMax) { //repairHealth(); }
    //   //    }
    //   //    if (_bonus == 3)
    //   //    {
    //   //        delayPursuit = true;
    //   //    }

    //   //    if (pursuitChange)
    //   //    {
    //   //        Warp();
    //   //    }
    //   //    else
    //   //    {
    //   //        miniWarp();
    //   //    }

    //   //}

    //   void Warp()
    //   {
    //       //Sets duration of warp compared to engines
    //    //   statPanel.hideUI();
    //       var warpDuration = 20.0f;

    //       //warpFlash.flash();

    //       playerAudio.enterHyperspace();
    //       if (engine > 0)
    //       {
    //           warpDuration = 20.0f / (float)engine;
    //       }

    //       var main = warp.main;
    //       main.duration = warpDuration;
    //       main.startLifetime = warpDuration;
    //       gameManager.warp();
    //       //warp.Play();
    //       if (warpDuration < 1.75f) { warpDuration = 1.75f; } //Safety so that the animation and sound will always have time to play
    //       StartCoroutine(warpDelay(warpDuration));
    //   }

    //   //void miniWarp()
    //   //{
    //   //    statPanel.hideUI();
    //   //    var warpDuration = 10.0f;

    //   //    //warpFlash.flash();
    //   //    playerAudio.enterHyperspace();

    //   //    if (engine > 0)
    //   //    {
    //   //        warpDuration = 10.0f / (float)engine;
    //   //    }
    //   //    var main = warp.main;
    //   //    main.duration = warpDuration;
    //   //    main.startLifetime = warpDuration;
    //   //    gameManager.warp();
    //   //    //warp.Play();
    //   //    if (warpDuration < 1.75f) { warpDuration = 1.75f; } //Safety so that the animation and sound will always have time to play
    //   //    StartCoroutine(warpDelay(warpDuration));
    //   //}

    //   //public void warpDump()
    //   //{
    //   //    warp.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    //   //    StopAllCoroutines();
    //   //    warpFlash.flash();
    //   //    statPanel.showUI();
    //   //    playerAudio.exitHyperspace();
    //   //    updateUI();
    //   //}

    //   //New warp stuff
    //   public void calmWarp() { }

    //   void upgradeScreen()
    //   {
    //   }

    //   IEnumerator warpEntry(float _duration)
    //   {
    //       var duration = _duration;
    //       while (duration > 0.0f) {
    //           duration -= Time.deltaTime;
    //           yield return null;
    //               }
    //       upgradeScreen();
    //   }

    //   //Old warp for turn-based. With ATB, warp is extended without duration
    //   IEnumerator warpDelay(float _duration)
    //   {
    //       var duration = _duration;
    //       var warpPlay = false; //Tracks whether warp is playing
    //       while (duration > 0.0f)
    //       {
    //           duration -= Time.deltaTime;

    //           if ((_duration - duration) > 1.5f && !warpPlay) //if we're more than 1.5 seconds beyond warp duration, do duration.
    //           {
    //               warp.Play();
    //               warpFlash.flash();
    //               warpPlay = true;
    //           }

    //           //This is where we'll update the chase slider for the warp.
    //          // statPanel.addProgress(warpSpeed * Time.deltaTime);
    //           //
    //           //Chase begins at pursuit 3:
    //           //var pursuit = gameManager.pursuitLevel;
    //           if (pursuit >= 3)
    //           {
    //             //  var toAdd = chaseSpeed * pursuit * Time.deltaTime;
    //             //  if (delayPursuit) { toAdd *= 0.5f; }
    //             //  statPanel.addChase(toAdd);
    //           }
    //           yield return null;
    //       }
    //       warp.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    //       //if (shield < shieldMax)
    //       //{
    //       //    //repairHealth(1);
    //       //}

    //       warpFlash.flash();
    //       gameManager.exitWarp();
    //       playerAudio.exitHyperspace();
    //       updateUI();
    //       gameManager.startCombat();
    //     //  statPanel.showUI();
    //   }
    //   #endregion

    //   void updateUI()
    //   {
    //       //statPanel.updateUI(reactor, sensors, engine);
    //       //statPanel.updateUpgradeUI(reactorUpgrade, engineUpgrade, sensorUpgrade);
    //   }

    //   //#region Combat

    //   ////public void setEnemy(Enemy _enemy)
    //   ////{
    //   ////    enemy = _enemy;
    //   ////}

    //   ////public override void missileLock()
    //   ////{
    //   ////    base.missileLock();
    //   ////    //Debug.Log("Player override");
    //   ////    playerAudio.lockMissile();
    //   ////    gameManager.updateCombatUI();
    //   ////    music.changeLockOn(lockOn);
    //   ////}

    //   ////public override void chargeShield()
    //   ////{
    //   ////    base.chargeShield();
    //   ////    playerAudio.chargeShield();
    //   ////}

    //   ////public void shieldRemove()
    //   ////{
    //   ////    shielded = false;
    //   ////    overShield.SetActive(false);
    //   ////}

    //   //void removeHealth(int toRemove)
    //   //{
    //   //    shield -= toRemove;
    //   //    statPanel.passDamage(toRemove);
    //   //    if (shield < shieldMax * 0.25)
    //   //    {
    //   //        playerAudio.lowHealthWarning();
    //   //    }
    //   //    if (shield <= 0)
    //   //    {
    //   //        die();
    //   //    }
    //   //}

    //   //void repairHealth()
    //   //{
    //   //    if (shield < shieldMax)
    //   //    {
    //   //        var toAdd = shieldMax - shield;
    //   //        shield = shieldMax;
    //   //        statPanel.rechargeShield(toAdd);
    //   //    }
    //   //}
    //   //void repairHealth(int toAdd)
    //   //{
    //   //    if (shield < shieldMax)
    //   //    {
    //   //        shield += toAdd;
    //   //        if (shield >= shieldMax)
    //   //        {
    //   //            toAdd -= shield - shieldMax;
    //   //            shield = shieldMax;
    //   //        }
    //   //        statPanel.rechargeShield(toAdd);
    //   //    }
    //   //}

    //   //void die()
    //   //{
    //   //    explosion.Play();
    //   //    statPanel.hideUI();
    //   //    StartCoroutine(explode());
    //   //}

    //   //IEnumerator explode()
    //   //{
    //   //    yield return new WaitForSeconds(1.0f);
    //   //    gameManager.lose();
    //   //}

    //   //public void takeFire(int damage)
    //   //{
    //   //   // var totalDamage = damageEquation(damage, lockOn, shielded);
    //   // //   removeHealth(totalDamage);
    //   // //   damageText.takeDamage(totalDamage);

    //   //    //determines how much enemy lock on is shaken from the damage
    //   //   // var toLose = lockOnLoss(lockOn);
    //   //  //  enemy.decreaseLock(toLose);
    //   //  //  music.changeEnemyLockOn(enemy.lockOn);

    //   //    //consumes our lock-on
    //   //  //  clearMissileLock();

    //   //    //removes overshield if we've taken fire
    //   //    //shieldRemove();
    //   //    //Updates UI
    //   //    updateUI();
    //   //    //updates music
    //   //    music.changeHealth(shield, shieldMax);
    //   //   // music.changeLockOn(lockOn);
    //   //}

    //   //public void CombatResolve(int _move) //Resolves combat
    //   //{
    //   //    statPanel.hideUI();
    //   //    var playerMove = (move)_move;

    //   //    //combatInfo.gameObject.SetActive(true);
    //   //    gameManager.hideMove();
    //   //    //combatInfo.setMoves(playerMove, enemyMove);

    //   //    //New resolution mechanics - RPS inspired rather than pure rps
    //   //    if (playerMove == move.missileLock)
    //   //    {
    //   //        //enemy.missileLock();
    //   //    }
    //   //    if (playerMove == move.Defend)
    //   //    {
    //   //       // chargeShield();
    //   //        repairHealth(1);
    //   //        statPanel.updateUI(reactor, sensors, engine);
    //   //    }
    //   //    if (playerMove == move.Attack)
    //   //    {
    //   //       // if (enemy.lockOn>0) { launchMissiles(); }
    //   //       // enemy.takeDamage(reactor);

    //   //      //  enemy.clearMissileLock();
    //   //        fireLasers();
    //   //        playerAudio.fireLaser();

    //   //    }

    //   //    StartCoroutine(CombatUnfold());
    //   //}

    //   //IEnumerator CombatUnfold()
    //   //{
    //   //    yield return new WaitForSeconds(0.75f);
    //   //    //enemy.pickMove();
    //   //    yield return new WaitForSeconds(0.75f);
    //   //    statPanel.showUI();
    //   //   // statPanel.addChase(gameManager.pursuitLevel * 0.025f);

    //   //    gameManager.updateCombatUI();
    //   //    gameManager.showMove();
    //   //   // enemy.hideMove();
    //   //}

    //   //void fireLasers()
    //   //{
    //   //   // var enemyPos = enemy.transform.position;
    //   //  //  laserBank.fireLasers(reactor, enemyPos);
    //   //}

    //   //void missLasers()
    //   //{
    //   //   // var enemyPos = enemy.transform.position;
    //   //   // var missX = enemyPos.x + Random.Range(-10.0f, 10.0f);
    //   //   // var missY = enemyPos.y + Random.Range(-10.0f, 10.0f);
    //   //   // laserBank.fireLasers(reactor, new Vector3(missX, missY, enemyPos.z));
    //   //}

    //   //public void singleLaser()
    //   //{
    //   //  //  var enemyPos = enemy.transform.position;
    //   //   // laserBank.fireSingleLaser(enemyPos);
    //   //}

    //   //void launchMissiles()
    //   //{
    //   //   // missileLaunch.launchMissiles(enemy.lockOn, enemy.gameObject);
    //   //}

    //   //#endregion

    //   public void resetStatus()
    //   {
    //       //clearMissileLock();
    //       //shieldRemove();
    //   }

    //   #region Repair

    //   public void upgradesComplete()
    //   {
    //       //
    //       //var toImprove = (Attribute)_attr;

    //       //if (toImprove == Attribute.reactor)
    //       //{
    //       //    reactorUpgrade++;
    //       //    if (reactorUpgrade >= reactor)
    //       //    {
    //       //        reactor++;
    //       //        reactorUpgrade = 0;
    //       //        statPanel.attributeLevel(Attribute.reactor, reactor);
    //       //    }
    //       //}
    //       //if (toImprove == Attribute.sensors)
    //       //{
    //       //    sensorUpgrade++;
    //       //    if (sensorUpgrade >= sensors)
    //       //    {
    //       //        sensors++;
    //       //        sensorUpgrade = 0;
    //       //        statPanel.attributeLevel(Attribute.sensors, sensors);
    //       //    }
    //       //}
    //       //if (toImprove == Attribute.engine)
    //       //{
    //       //    engineUpgrade++;
    //       //    if (engineUpgrade >= engine)
    //       //    {
    //       //        engine++;
    //       //        engineUpgrade = 0;
    //       //        statPanel.attributeLevel(Attribute.engine, engine);
    //       //    }
    //       //}

    //      // shieldMax += 5;
    //      // shield = shieldMax;
    //      // statPanel.upgradeShieldMax(shieldMax);

    //       updateUI();
    //       music.changeAttributeLevel(reactor, sensors, engine);

    //       gameManager.repeatLoop();

    //   }
}