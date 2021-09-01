﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Attribute { shields, reactor, sensors, engine };

public enum enemyType { main, miniboss, boss };

public enum Condition { None, Linear, Adaptive, Generative}

public class GameManager : MonoBehaviour
{
    #region Player Variables and Objects

    [Header("Player Variables and Objects")]
    public PlayerShip Player;

    #endregion Player Variables and Objects

    #region Affect Variables

    private string VAT;

    #endregion Affect Variables

    #region Managers

    //private AffectManager affect;
    private PreGLAM PreGLAM;
    private EliasPlayer music;
    private environmentAudio gmAudio;

    public stageManager stageManager;
    public TutorialManager tutorialManager;

    //  public TargetManager targets;

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
    public GameObject timeWindow;
    float requiredTime = 1200.0f;
    bool timeComplete = false;


    //public pauseButton pauseButton;
    public GameObject pauseText;

    public List<GameObject> toHide;
    public GameObject upgradeMenu;
    public upgradeButton upgrade_1, upgrade_2, upgrade_3; //3 slots for selecting possible upgrades
    public TextMeshProUGUI upgradeCount_text; //Text for count of how many upgrades we've selected
    private List<Upgrade> proposed_upgrades;
    private int upgrades_selected = 0; //Actual count of how many upgrades we've selected
    public Transform playerWarpWindow;
    public Transform enemyWarpWindow;

    #endregion UI

    #region Gameplay Values

    private bool paused = false;
    private readonly bool atBoss = false;
    private bool warpNext = false;
    private bool inCombat;

    [Header("Gameplay values")]
    public static int stage = 1, encounter = 0;

    public static bool tutorial = false;

    public static Condition condition = Condition.Generative;

    #endregion Gameplay Values

    #region Enemies and Waves

    [Header("Enemies")]
    public GameObject tutorialEnemy;

    public GameObject standardEnemy;

    public GameObject miniboss;
    public GameObject boss;
    private EnemyShip currentEnemy;

    public enemyAttributes enemyAttr;

    public GameObject enemyWarpPrefab;

    public GameObject spawnPoint;

    #endregion Enemies and Waves

    #region Hotkeys

    [Header("Hotkeys")]
    public InputAction quit;

    public InputAction screenshot, menu, restart, pause, focusToggle;

    #endregion Hotkeys

    #region tutorial

    //  public TutorialManager tutorialManager;

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
        Player.FullHeal(); //Fully heals player
        Player.ShieldsDown();

        //Plays the warping camera movement, sound, and screen flash
        anim.Play("Warp");
        gmAudio.enterHyperspace();
        warpFlash.flash();

        //Creates "window" at end for special effect and clears skybox
        warpEffect = Instantiate(warpPrefab, playerWarpWindow.position, Quaternion.Euler(new Vector3(0f, 180f, 90f)), playerWarpWindow);
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

        //Calls special effects
        anim.Play("cameraExitWarp");
        Destroy(warpEffect);
        sky.exitWarp();
        warpFlash.flash();
        gmAudio.exitHyperspace();
        Player.ShieldsUp();

        //Re-activates hidden UI
        foreach (GameObject hide in toHide)
        {
            hide.SetActive(true);
        }

        StartCombat();
    }

    /// <summary>
    /// Called during tutorial, exits warp and sets up dummy versions of enemies for tutorial
    /// </summary>
    public void TutorialExit()
    {
        //Disable menus
        mapMenu.SetActive(false);

        //Calls special effects
        anim.Play("cameraExitWarp");
        Destroy(warpEffect);
        sky.exitWarp();
        warpFlash.flash();
        gmAudio.exitHyperspace();
        Player.ShieldsUp();

        //Re-activates hidden UI
        foreach (GameObject hide in toHide)
        {
            hide.SetActive(true);
        }

        tutorialCombatStart();
    }

    private IEnumerator victoryWarp()
    {
        //First we do the special effects
        //affect.musicControl = false;
        music.RunActionPreset("Hyperspace");
        yield return new WaitForSeconds(2.5f);
        EnterHyperspace();
        yield return new WaitForSeconds(1.0f);
        map.advanceStage(stage);

        setupUpgrades();
    }

    private void setupUpgrades()
    {
        proposed_upgrades = new List<Upgrade>();
        proposed_upgrades = Player.upgradeManager.ProposeUpgrades();
        upgrades_selected = 0;
        upgradeCount_text.text = upgrades_selected + "/2";
        upgradeMenu.SetActive(true);
        upgrade_1.gameObject.SetActive(true);
        upgrade_2.gameObject.SetActive(true);
        upgrade_3.gameObject.SetActive(true);

        upgrade_1.SetInfo(proposed_upgrades[0]);
        upgrade_2.SetInfo(proposed_upgrades[1]);
        upgrade_3.SetInfo(proposed_upgrades[2]);

        upgradeCount_text.text = "0/2";
    }



    /// <summary>
    /// Closes upgrade and stage clear menus and brigns up the map
    /// </summary>
    private void ToMapMenu()
    {
        upgradeMenu.SetActive(false);
        mapMenu.SetActive(true);
    }

    /// <summary>
    /// Upgrades basic attack damage and speed
    /// </summary>
    public void Select_Upgrade_1()
    {
        var selectedUpgrade = proposed_upgrades[0];
        Player.upgradeManager.Select_Upgrade(selectedUpgrade);
        //Player.UpgradeAttack();
        // AdjustShipStats();

        upgrades_selected++; //Increments selected upgrades
        if (upgrades_selected < 2)
        { //If we haven't selected both upgrades, just update text and deactivate button
            upgrade_1.gameObject.SetActive(false);//Deactivate button
            upgradeCount_text.text = upgrades_selected + "/2";
        }
        else //Otherwise, we return to map
        {
            ToMapMenu();
        }
    }

    /// <summary>
    /// Upgrades health and reduces incoming damage
    /// </summary>
    public void Select_Upgrade_2()
    {
        var selectedUpgrade = proposed_upgrades[1];
        Player.upgradeManager.Select_Upgrade(selectedUpgrade);

        upgrades_selected++; //Increments selected upgrades
        if (upgrades_selected < 2)
        { //If we haven't selected both upgrades, just update text and deactivate button
            upgrade_2.gameObject.SetActive(false);//Deactivate button
            upgradeCount_text.text = upgrades_selected + "/2";
        }
        else //Otherwise, we return to map
        {
            ToMapMenu();
        }
    }

    /// <summary>
    /// Upgrades abilities
    /// </summary>
    public void Select_Upgrade_3()
    {
        var selectedUpgrade = proposed_upgrades[2];
        Player.upgradeManager.Select_Upgrade(selectedUpgrade);

        upgrades_selected++; //Increments selected upgrades
        if (upgrades_selected < 2)
        { //If we haven't selected both upgrades, just update text and deactivate button
            upgrade_3.gameObject.SetActive(false);//Deactivate button
            upgradeCount_text.text = upgrades_selected + "/2";
        }
        else //Otherwise, we return to map
        {
            ToMapMenu();
        }
    }

    #endregion Hyperspace/Map Screen

    #region Pause and Resume

    /// <summary>
    /// "Pauses" (massively slows down) game
    /// </summary>
    public void pauseGame()
    {
        Time.timeScale = 0.0025f;
        // pauseText.SetActive(true);
        paused = true;
        //pauseButton.pauseG();
    }

    /// <summary>
    /// Resumes normal gameplay
    /// </summary>
    public void resumeGame()
    {
        Time.timeScale = 1.0f;
        //pauseText.SetActive(false);
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

    public void StartCombat()
    {
        Player.StartCombat();
        inCombat = true;
        EnemyCompositionSetup();
        music.RunActionPreset("StartCombat");
        PreGLAM.Set_Mood(stage);
        StartCoroutine(CombatAffectUpdate());
    }

    /// <summary>
    /// Called by tutorialExit - follows most of startcombat, but doesn't spawn normal enemies.
    /// </summary>
    private void tutorialCombatStart()
    {
        inCombat = true;
        StartCoroutine(CombatAffectUpdate());
        //affect.StartCombat();
        //PreGLAM.Begin_Emotion_Process();
        music.RunActionPreset("StartCombat");
        PreGLAM.Set_Mood(stage);

        tutorialEnemyCompositionSetup();
    }

    /// <summary>
    ///While in combat, evalutes combat for affect
    /// </summary>
    /// <returns></returns>
    private IEnumerator CombatAffectUpdate()
    {
        while (inCombat)
        {
            var VATLevels = PreGLAM.VATLevel();
            //Debug.Log("Vat levels: " + VATLevels);
            if (VAT != VATLevels)
            {
                //Debug.Log("Change in Vat Levels. New VAT Levels = " + VATLevels);
                VAT = VATLevels;
                music.RunActionPreset(VAT);
            }
            yield return null;
        }
    }

    /// <summary>
    /// Tells players to end combat and begins hyperspace jump
    /// </summary>
    private void EndCombat()
    {
        inCombat = false;
        PreGLAM.Queue_prospective_cull(); //Removes all future events, as they can no longer happen.
        stageManager.RemoveUI();
        Player.EndCombat();

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
        var warpGate = Instantiate(enemyWarpPrefab, enemyWarpWindow.position, Quaternion.Euler(0, 0, -90), enemyWarpWindow);
        warpGate.GetComponent<portalAppear>().warp(10);
        yield return new WaitForSeconds(1.5f);
        EnemyCompositionSetup();
    }

    public void EnemyDie() //If we aren't at the boss, we continue playing. Otherwise, we've won!
    {
        //affect.ClearWave(); //Calls the clearWave void, resetting emotions
        if (!atBoss)
        {
            StageLogic();
        }
        else { win(); }
    }

    /// <summary>
    /// Called when main enemy is destroyed. Decides whether to setup next wave or jump to hyperspace.
    /// </summary>
    private void StageLogic()
    {
        if (stage == 1)
        {
            StageOneLogic();
        }
        else if (stage == 2)
        {
            StageTwoLogic();
        }
        else if (stage == 3)
        {
            StageThreeLogic();
        }

        void StageOneLogic()
        {
            if (encounter == 0)
            {
                AdvanceWave();
            }
            else
            {
                FinalWave(2);
            }
        }

        void StageTwoLogic()
        {
            if (encounter == 0 || encounter ==1)
            {
                AdvanceWave();
            }
            else
            {
                FinalWave(3);
            }
        }

        void StageThreeLogic()
        {
            if (encounter == 0 || encounter == 1)
            {
                encounter++;
                stageManager.setEncounterProgress(1);
                AdvanceToNextWave();
            }
            else
            {
                win();
            }
        }

        void AdvanceWave()
        {
            encounter++;
            stageManager.setEncounterProgress(encounter);
            AdvanceToNextWave();
        }

        void FinalWave(int _stage)
        {
            encounter = 0;
            stage = _stage;
            EndCombat();
        }

    }

    /// <summary>
    /// Spawns the correct enemy composition, if necessary spawns the encounter ui, and sets the encounter text
    /// </summary>
    private void EnemyCompositionSetup()
    {
        if (stage == 1)
        {
            StageOneEncounterLogic();
        }
        else if (stage == 2)
        {
            StageTwoEncounterLogic();
        }
        else if (stage == 3)
        {
            StageThreeEncounterLogic();
        }
        else
        {
            Debug.Log("Fell through all cases for spawning, which is probably bad");
        }

        void StageOneEncounterLogic()
        {
            if (encounter == 0)
            { 
                stageManager.setWaveLength(2);
                stageManager.setText("Stage 1");
                SpawnEnemy(enemyType.main, false);
                warpNext = true;
            }
            else
            {
                stageManager.setText("1-2");
                SpawnEnemy(enemyType.main, true);
                warpNext = false;
            }
        }

        void StageTwoEncounterLogic()
        {
            if (encounter == 0)
            {
                stageManager.setWaveLength(3);
                stageManager.setText("2-1");
                SpawnEnemy(enemyType.main, false);
                warpNext = true;
            }
            else if (encounter == 1)
            {
                stageManager.setText("2-2");
                SpawnEnemy(enemyType.main, true);
                warpNext = true;
            }
            else
            {
                stageManager.setText("2-3");
                SpawnEnemy(enemyType.miniboss, true);
                warpNext = false;
            }
            //else
            //{
            //    stageManager.setText("2-3");
            //    SpawnEnemy(enemyType.miniboss, true);
            //}
        }

        void StageThreeEncounterLogic()
        {
            if (encounter == 0)
            {
                stageManager.setWaveLength(3);
                stageManager.setText("3-1");
                SpawnEnemy(enemyType.main, false);
                warpNext = true;
            }
            else if (encounter == 1)
            {
                stageManager.setText("3-2");
                SpawnEnemy(enemyType.miniboss, true);
                warpNext = true;
            }
            else
            {
                stageManager.setText("3-3");
                SpawnEnemy(enemyType.boss, true);
            }
        }
    }

    private void tutorialEnemyCompositionSetup()
    {
        stageManager.setWaveLength(1);
        stageManager.setText("Tutorial");
        SpawnTutorialEnemy();
    }

    private void SpawnEnemy(enemyType type, bool warp)
    {
        //Selects appropriate main enemy
        var enemyToInstantiate = standardEnemy;
        if (type == enemyType.miniboss) { enemyToInstantiate = miniboss; }
        else if (type == enemyType.boss) { enemyToInstantiate = boss; }

        //Instantiates wave, sets position, and gets reference
        var newEnemy = GameObject.Instantiate(enemyToInstantiate);
       // Debug.Log("Instantiating Enemy");
        newEnemy.transform.position = spawnPoint.transform.position;
        //newEnemy.transform.parent = spawnPoint.transform;
        newEnemy.SetActive(true);
      //  Debug.Log("Enemy is activated!");
        currentEnemy = newEnemy.GetComponent<EnemyShip>();
        currentEnemy.ShipSetup();
        currentEnemy.ShieldsUp();

        Player.SetEnemyReference(newEnemy);
        //affect.SetEnemy(currentEnemy);
        if (warp)
        {
            //Debug.Log("Telling enemy to warp in");
            StartCoroutine(currentEnemy.FlyIn());
           // Debug.Log("Is enemy still active?" + currentEnemy.isActiveAndEnabled);
        }
    }

    private void SpawnTutorialEnemy()
    {        //Selects appropriate main enemy
        var enemyToInstantiate = tutorialEnemy;

        //Instantiates wave, sets position, and gets reference
        var newEnemy = GameObject.Instantiate(enemyToInstantiate);
        newEnemy.transform.position = spawnPoint.transform.position;
        currentEnemy = newEnemy.GetComponent<EnemyShip>();
        currentEnemy.tutorialSetup();
        currentEnemy.ShieldsUp();

        Player.SetEnemyReference(newEnemy);
        //affect.SetEnemy(currentEnemy);
        GameObject.Find("Tutorial").GetComponent<TutorialManager>().SetTutorialEnemy(currentEnemy);
    }

    /// <summary>
    ///Advances to the next wave after a short delay
    /// </summary>
    private void AdvanceToNextWave()
    {
        StartCoroutine(warpWindow());
    }

    /// <summary>
    /// Pans camera for enemy arrival - called during enemy death
    /// </summary>
    public void WarpPan()
    {
        if (warpNext)
        {
            anim.Play("cameraEnemyArrive");
        }
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
       // affect = GetComponent<AffectManager>();
        PreGLAM = GetComponent<PreGLAM>();
        PreGLAM.Start_PreGLAM();

        EnterHyperspace();
        if (tutorial) { tutorialManager.StartTutorial(); }
        else
        {
            mapMenu.SetActive(true);
        }
        stage = 1;
        encounter = 0;

        //Enables hotkeys for use
        quit.Enable();
        restart.Enable();
        screenshot.Enable();
        menu.Enable();
        //pause.Enable();
        focusToggle.Enable();
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

        //Debug.Log("Time.time = " + Time.time);
        if (Time.time >= requiredTime &&!timeComplete)
        {
            timeComplete = true;
            studyTimer();
        }
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

    void studyTimer()
    {
        timeWindow.SetActive(true);
        Time.timeScale = 0.05f;
    }

    public void ContinueGame()
    {
        Time.timeScale = 1.0f;
        //Debug.Log("Continuing game");
        timeWindow.SetActive(false);

    }

    private IEnumerator loadingScreen(string sceneName)
    {
        //loadPanel.SetActive(true);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        while (!asyncLoad.isDone) { yield return null; }
    }

    #endregion menuNavigation
}