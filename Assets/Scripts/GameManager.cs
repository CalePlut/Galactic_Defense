using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Attribute { shields, reactor, sensors, engine };

public enum waveType { main, miniboss, boss };

public class GameManager : MonoBehaviour
{
    #region Player Variables and Objects

    [Header("Player Variables and Objects")]
    public FrigateShip frigate;

    public IntelShip artillery;

    public SupportShip tender;
    public GameObject frigateHealth, artilleryHealth, tenderHealth;

    #endregion Player Variables and Objects

    #region Managers

    private AffectManager affect;
    private EliasPlayer music;
    private environmentAudio gmAudio;

    // public GameObject pursuitText;
    //public PursuitTracker encounterUI;
    public stageManager stageManager;

    //private bool inWarp;
    public TargetManager targets;

    [Header("Managers")]
    public buttonManager playerButtons;

    public mapManager map;

    #endregion Managers

    #region UI

    [Header("UI")]
    public Toggle focusUI;

    public GameObject lossMenu;
    public TextMeshProUGUI gameOverText;
    public GameObject mapMenu;

    //public pauseButton pauseButton;
    public GameObject pauseText;

    public List<GameObject> toHide;
    public GameObject upgradeMenu;

    #endregion UI

    #region Gameplay Values

    private bool paused = false;
    private readonly bool atBoss = false;

    [Header("Gameplay values")]
    public static bool autoPause = true;

    public static int stage = 1, encounter = 0;

    public static bool tutorial = false;

    #endregion Gameplay Values

    #region Enemies and Waves

    private List<Turret> turrets;

    [Header("Enemies")]
    public GameObject droneWave;

    public GameObject minibossWave;
    public GameObject bossWave;

    public enemyAttributes enemyAttr;

    public GameObject enemyWarpPrefab;

    public GameObject spawnPoint;
    public GameObject stage1Clear;

    #endregion Enemies and Waves

    #region Hotkeys

    [Header("Hotkeys")]
    public InputAction quit;

    public InputAction screenshot, menu, restart, pause, focusToggle;

    #endregion Hotkeys

    #region tutorial

    public TutorialManager tutorialManager;

    #endregion tutorial

    #region Special Effects

    private Animator anim;

    private skyboxManager sky;
    private GameObject warpEffect;

    [Header("Special effects")]
    public WarpFlash warpFlash;

    public GameObject warpPrefab;

    #endregion Special Effects

    #region Deaths and Respawns

    //public void removeTurretfromTargetting(Turret _toDie) //Removes turrets from Target manager
    //{
    //    targets.removeShip(_toDie);

    //    if (TargetManager.target == _toDie)
    //    {
    //        targets.advanceTarget();
    //    }
    //}

    public void addTurrettoTargetting(Turret _toRespawn)
    {
        targets.addShip(_toRespawn);
    }

    public void disableArtilleryUI()
    {
        artilleryHealth.SetActive(false);
    }

    public void disableTenderUI()
    {
        tenderHealth.SetActive(false);
    }

    #endregion Deaths and Respawns

    #region Win/Loss

    public void LoseGame()
    {
        lossMenu.SetActive(true);
        gameOverText.text = "Defeat";
    }

    /// <summary>
    /// This one's pretty obvious I hope.
    /// </summary>
    public void win()
    {
        lossMenu.SetActive(true);
        gameOverText.text = "Victory!";
    }

    #endregion Win/Loss

    #region Hyperspace/Map Screen

    /// <summary>
    /// Plays special effects for hyperspace jump
    /// </summary>
    public void EnterHyperspace()
    {
        //Plays the warping camera movement, sound, and screen flash
        anim.Play("Warp");
        gmAudio.enterHyperspace();
        warpFlash.flash();

        //Creates "window" at end for special effect and clears skybox
        warpEffect = Instantiate(warpPrefab);
        warpEffect.transform.position = new Vector3(0, 0, 100);
        sky.warp();

        //Hides UI
        foreach (GameObject hide in toHide)
        {
            hide.SetActive(false);
        }
    }

    /// <summary>
    /// Called by selecting stage on map screen. Exits warp and begins combat
    /// </summary>
    public void ExitHyperspace()
    {
        //Disable menus
        mapMenu.SetActive(false);
        stage1Clear.SetActive(false);

        //Calls special effects
        anim.Play("cameraExitWarp");
        Destroy(warpEffect);
        sky.exitWarp();
        warpFlash.flash();
        gmAudio.exitHyperspace();

        //Re-activates hidden UI
        foreach (GameObject hide in toHide)
        {
            hide.SetActive(true);
        }

        //Probably redundant --- heals all ships (Should be healed immediately at end of combat, and rezzed if possible)
        frigate.fullHeal();
        tender.fullHeal();
        artillery.fullHeal();

        //Calls beginning of combat
        startCombat();
    }

    private IEnumerator victoryWarp()
    {
        //First we do the special effects
        affect.musicControl = false;
        music.RunActionPreset("HyperSpace");
        yield return new WaitForSeconds(2.5f);
        EnterHyperspace();
        yield return new WaitForSeconds(1.0f);
        map.advanceStage(stage);

        //Now we do additional logic
        if (stage == 2)
        {
            //Unlocks ultimate ability at stage 2
            playerButtons.unlockUltimate();

            //Sets UI elements
            stage1Clear.SetActive(true);
            mapMenu.SetActive(true);
        }
        if (stage == 3)
        {
            //Gives player option to select one ship to upgrade.
            upgradeMenu.SetActive(true);
        }
    }

    /// <summary>
    /// Callable after 2nd Stage - upgrades single ship
    /// </summary>
    /// <param name="toUpgrade"></param>
    public void upgradeShip(string toUpgrade)
    {
        if (toUpgrade == "frigate") { frigate.upgradeShip(); }
        if (toUpgrade == "intel") { artillery.upgradeShip(); }
        if (toUpgrade == "support") { tender.upgradeShip(); }

        upgradeMenu.SetActive(false);
        mapMenu.SetActive(true);
    }

    #endregion Hyperspace/Map Screen

    #region Pause and Resume

    /// <summary>
    /// "Pauses" (massively slows down) game
    /// </summary>
    public void pauseGame()
    {
        Time.timeScale = 0.0025f;
        pauseText.SetActive(true);
        paused = true;
        //pauseButton.pauseG();
    }

    /// <summary>
    /// Resumes normal gameplay
    /// </summary>
    public void resumeGame()
    {
        Time.timeScale = 1.0f;
        pauseText.SetActive(false);
        paused = false;
        // pauseButton.resume();
    }

    /// <summary>
    ///  General-purpose pausing for if we don't know which way we want
    /// </summary>
    public void pauseLogic()
    {
        if (!paused)
        {
            pauseGame();
        }
        else
        {
            resumeGame();
        }
    }

    #endregion Pause and Resume

    #region Combat

    public void startCombat()
    {
        music.RunActionPreset("StartCombat");
        affect.musicControl = true;
        enemyCompositionSetup();

        frigate.delayFirstFire();
        artillery.delayFirstFire();
        tender.delayFirstFire();
    }

    /// <summary>
    /// Tells players to end combat and begins hyperspace jump
    /// </summary>
    private void endCombat()
    {
        frigate.endCombat();
        artillery.endCombat();
        tender.endCombat();

        stageManager.RemoveUI();

        StartCoroutine(victoryWarp());
    }

    #endregion Combat

    #region Stage Managerment

    /// <summary>
    /// Creates warp window effect, waits, then sets up next wave
    /// </summary>
    /// <returns></returns>
    private IEnumerator warpWindow()
    {
        var warpGate = Instantiate(enemyWarpPrefab, new Vector3(0, 0, 100), Quaternion.Euler(-90, 0, 0), this.transform);
        warpGate.GetComponent<portalAppear>().warp(10);
        yield return new WaitForSeconds(1.5f);
        enemyCompositionSetup();
    }

    /// <summary>
    /// Called when main enemy is destroyed. Decides whether to setup next wave or jump to hyperspace.
    /// </summary>
    private void stageLogic()
    {
        //Update other fields with general progress settings
        affect.setProgess(stage, encounter);

        if (stage == 1)
        {
            encounter = 0;
            stage = 2;
            endCombat();
        }
        else if (stage == 2)
        {
            if (encounter == 0)
            {
                encounter++;
                stageManager.setEncounterProgress(1);
                advanceToNextWave();
            }
            else
            {
                encounter = 0;
                stage = 3;
                endCombat();
            }
        }
        else if (stage == 3)
        {
            if (encounter == 0)
            {
                encounter++;
                stageManager.setEncounterProgress(1);
                advanceToNextWave();
            }
            else if (encounter == 1)
            {
                encounter++;
                stageManager.setEncounterProgress(2);
                advanceToNextWave();
            }
            else
            {
                endCombat();
            }
        }

        //if (stage == 1)
        //{
        //    if (encounter == 0) //If we just finished the first encounter, we move to the second
        //    {
        //        encounter++;
        //        stageManager.setToggles(1, 1);
        //        advanceToNextWave();
        //    }
        //    else if (encounter == 1) //If we just finished the second encounter, we victoryWarp
        //    {
        //        encounter = 0;
        //        stage = 2;
        //        stageManager.setToggles(1, 2);
        //        endCombat();
        //    }
        //}
        //else if (stage == 2)
        //{
        //    if (encounter == 0 || encounter == 1)
        //    {
        //        encounter++;
        //        stageManager.setToggles(2, encounter);
        //        advanceToNextWave();
        //    }
        //    else if (encounter == 2)
        //    {
        //        encounter = 0;
        //        stage = 3;
        //        stageManager.setToggles(2, 3);
        //        endCombat();
        //    }
        //}
        //else if (stage == 3)
        //{
        //    if (encounter == 0 || encounter == 1 || encounter == 2)
        //    {
        //        encounter++;
        //        stageManager.setToggles(3, encounter);
        //        advanceToNextWave();
        //    }
        //    else if (encounter == 4)
        //    {
        //        stageManager.setToggles(3, 4);
        //        endCombat();
        //    }
        //}
    }

    /// <summary>
    /// Spawns the correct enemy composition, if necessary spawns the encounter ui, and sets the encounter text
    /// </summary>
    private void enemyCompositionSetup()
    {
        if (stage == 1)
        {
            stageManager.setWaveLength(1);
            stageManager.setText("1-1");
            spawnEnemies(waveType.miniboss);
        }
        else if (stage == 2)
        {
            if (encounter == 0)
            {
                stageManager.setWaveLength(2);
                stageManager.setText("2-1");
                spawnEnemies(waveType.main);
            }
            else
            {
                stageManager.setText("2-2");
                spawnEnemies(waveType.miniboss);
            }
        }
        else if (stage == 3)
        {
            if (encounter == 0)
            {
                stageManager.setWaveLength(3);
                stageManager.setText("3-1");
                spawnEnemies(waveType.main);
            }
            else if (encounter == 1)
            {
                stageManager.setText("3-2");
                spawnEnemies(waveType.miniboss);
            }
            else
            {
                stageManager.setText("3-3");
                spawnEnemies(waveType.boss);
            }
        }
        else
        {
            spawnEnemies(waveType.main);
            Debug.Log("Fell through all cases for spawning, which is probably bad");
        }
    }

    private void spawnEnemies(waveType type)
    {
        //Selects appropriate main enemy
        var enemyToInstantiate = droneWave;
        if (type == waveType.miniboss) { enemyToInstantiate = minibossWave; }
        else if (type == waveType.boss) { enemyToInstantiate = bossWave; }

        //Instantiates wave, sets position, and gets reference
        var wave = GameObject.Instantiate(enemyToInstantiate);
        wave.transform.position = spawnPoint.transform.position;
        var core = wave.GetComponent<EnemyCore>();
        //enemyCore = core;

        //Tells core to run setup for the wave

        core.setEnemyReferences(frigate, artillery, tender);
        core.setupWave(stage, this.gameObject);

        //Get divisions of ships from core
        var enemyShips = core.getWaveList();
        var mainEnemy = core.getMainShip();
        turrets = core.getTurrets();

        //Finally, use the  lists to inform the other objects references
        targets.setShips(mainEnemy, turrets[0], turrets[1]);
        frigate.updatePlayers(mainEnemy, turrets);
        artillery.updatePlayers(mainEnemy, turrets);
        tender.updatePlayers(mainEnemy, turrets);

        if (tutorial) { tutorialManager.centreEnemy = core.getMainShip(); }
    }

    /// <summary>
    ///Advances to the next wave after a short delay
    /// </summary>
    private void advanceToNextWave()
    {
        StartCoroutine(warpWindow());
    }

    public void mainShipDie() //If we aren't at the boss, we continue playing. Otherwise, we've won!
    {
        if (!atBoss)
        {
            stageLogic();
        }
        else { win(); }
    }

    #endregion Stage Managerment

    // Start is called before the first frame update
    private void Start() //Does a bunch of initial setup stuff
    {
        //Gets components and references
        gmAudio = GetComponent<environmentAudio>();
        anim = GetComponent<Animator>();
        music = GetComponent<EliasPlayer>();
        sky = GetComponent<skyboxManager>();
        affect = GetComponent<AffectManager>();

        //Calls relevant setup functions and sets variables
        frigate.shipSetup();
        artillery.shipSetup();
        tender.shipSetup();
        music.RunActionPreset("HyperSpace");
        EnterHyperspace();
        mapMenu.SetActive(true);
        stage = 1;
        encounter = 0;

        //Enables hotkeys for use
        quit.Enable();
        restart.Enable();
        screenshot.Enable();
        menu.Enable();
        //pause.Enable();
        focusToggle.Enable();

        //Unlocks ultimate early for tutorial
        if (tutorial) { playerButtons.unlockUltimate(); }
    }

    // Update is called once per frame
    private void Update() //non-combat hotkeys
    {
        if (screenshot.triggered)
        {
            var now = System.DateTime.Now.ToString("M-dd-H-mm");
            var randomAdd = Random.value;
            ScreenCapture.CaptureScreenshot(("Screenshot-" + now + randomAdd + ".png"));
        }
        if (quit.triggered) { Application.Quit(); }

        if (menu.triggered) { goToMenu(); }

        if (restart.triggered) { restartGame(); }

        if (pause.triggered)
        {
            pauseLogic();
        }

        //if (focusToggle.triggered)
        //{
        //    targets.advanceTarget();
        //}
    }

    #region menuNavigation

    public void goToMenu()
    {
        StartCoroutine(loadingScreen("Menu"));
    }

    public void quitGame()
    {
        Application.Quit();
    }

    public void restartGame()
    {
        StartCoroutine(loadingScreen(SceneManager.GetActiveScene().name));
    }

    private IEnumerator loadingScreen(string sceneName)
    {
        //loadPanel.SetActive(true);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        while (!asyncLoad.isDone) { yield return null; }
    }

    #endregion menuNavigation

    //public void spawnBoss()
    //{
    //    Destroy(currentEnemy.gameObject);
    //    destinationPanel.SetActive(false);
    //    repairPanel.SetActive(false);
    //    combatPanel.SetActive(true);
    //    combatPanel.GetComponent<CombatStatCheck>().disableCheck(player.reactor, player.sensors, player.engine);
    //    var bigBoss = GameObject.Instantiate(boss);
    //    var bossBrain = bigBoss.GetComponent<Enemy>();
    //    bossBrain.setGM(this);
    //    bossBrain.setPlayer(player);
    //    bossBrain.setStrength(10);
    //    currentEnemy = bossBrain;
    //    player.warpDump();
    //}

    //public void winCombat()
    //{
    //    inCombat = false;
    //    combatPanel.SetActive(false);
    // //   player.resetStatus();
    //    //repairPanel.SetActive(true);

    //    toNextLevel--;

    //    if (toNextLevel <= 0)
    //    {
    //        //If we're at the next level, level up, reset change, update music
    //        pursuitChange = true;
    //        repairPanel.SetActive(true);
    //        //player.chargeShields();
    //     //   toNextLevel = enemiesPerWave[pursuitLevel];
    //        pursuitLevel++;
    //        //pursuitText.GetComponent<TextMeshProUGUI>().text = ("Level: " + pursuitLevel);

    //        //update music
    //        music.changePursuit(pursuitLevel);

    //     //   trackerUI.nextLevel(toNextLevel);
    //    }
    //    else {
    //      //  trackerUI.addPursuit();
    //        destinationMenu();
    //        pursuitChange = false;
    //    }
    //}

    //public void repeatLoop()
    //{
    //    repairPanel.SetActive(false);
    //    destinationMenu();
    //}
}