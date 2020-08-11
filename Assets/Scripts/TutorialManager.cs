using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    public GameObject sWelcome1, sScreenIntro2, sPause21, sYourShips3, sFlagship31, sArtillery32, sTender33, sRespawn34, sEnemies4, sMainEnemy41, sTurrets411, sWaves42;
    public GameObject sTabTargeting5, sAbilities6, sFusionCannon601, sEnemyAbilities602, sReactiveShield603, sRetaliate62;
    public GameObject sHeal7, sPostHeal71, sAbilityChart8, sFinal;
    public GameManager gm;

    public FrigateShip frigate;
    public IntelShip artillery;
    public SupportShip tender;

    //  public InputAction frigateShoot;
    public InputAction fusionCannon;

    public InputAction reactiveShield;
    public InputAction heal;

    public EnemyShip centreEnemy;

    // Start is called before the first frame update
    private void Start()
    {
        if (GameManager.tutorial)
        {
            sWelcome1.SetActive(true);
            //  frigateShoot.Enable();
            // missileBarrage.Enable();
        }
        else { sWelcome1.SetActive(false); }
    }

    //When the player leaves the first warp, we bring up the next slide and pause
    public void Welcome()
    {
        sWelcome1.SetActive(false);
        sScreenIntro2.SetActive(true);

        gm.ExitHyperspace();
        StartCoroutine(pauseWait());
    }

    public void ScreenIntro() //Player hits the next button on slide 2 (ScreenIntro)
    {
        sScreenIntro2.SetActive(false);
        sPause21.SetActive(true);
        // sYourShips.SetActive(true);
    }

    public void pauseIntro()
    {
        sPause21.SetActive(false);
        sYourShips3.SetActive(true);
    }

    public void YourShips() //Introduces flaship
    {
        sYourShips3.SetActive(false);
        sFlagship31.SetActive(true);
    }

    public void Flagship() //Introduces artiller
    {
        sFlagship31.SetActive(false);
        sArtillery32.SetActive(true);
    }

    public void Artillery()//Introduces tender
    {
        sArtillery32.SetActive(false);
        sTender33.SetActive(true);
    }

    public void Tender() //Introduces respawn mechanic
    {
        sTender33.SetActive(false);
        // sEnemies4.SetActive(true);
        sRespawn34.SetActive(true);
    }

    public void respawn()//Introduces enemies
    {
        sRespawn34.SetActive(false);
        sEnemies4.SetActive(true);
    }

    public void Enemies() //Introduces waves
    {
        sEnemies4.SetActive(false);
        sMainEnemy41.SetActive(true);
        // sWaves42.SetActive(true);
    }

    public void MainEnemy()
    {
        sMainEnemy41.SetActive(false);
        sTurrets411.SetActive(true);
    }

    public void turrets()
    {
        sTurrets411.SetActive(false);
        sWaves42.SetActive(true);
    }

    public void Waves() //Brings up tab targetting info
    {
        sWaves42.SetActive(false);
        sTabTargeting5.SetActive(true);
        // sdemonstrateFire5.SetActive(true);
        //sAbilities6.SetActive(true);
    }

    //public void DemonstrateFire() //To Target Fire
    //{
    //    frigateShoot.Disable();
    //    sdemonstrateFire5.SetActive(false);
    //    sTargetFire51.SetActive(true);
    //}

    //public void TargetFire() //Starts coroutine, then to goodJob
    //{
    //    //frigate.setAutoAttackTarget();
    //    sTargetFire51.SetActive(false);
    //    StartCoroutine(fireWait());
    //}

    //public void GoodJob() { //To OtherShips
    //    sGoodJob52.SetActive(false);
    //    sOtherShips53.SetActive(true);
    //   }

    //public void OtherShips() //To Abilities
    //{
    //    sOtherShips53.SetActive(false);
    //    sAbilities6.SetActive(true);
    //}
    public void tabTarget()
    {
        sTabTargeting5.SetActive(false);
        sAbilities6.SetActive(true);
    }

    public void Abilities() //Activates and waits for fusion Cana
    {
        sAbilities6.SetActive(false);
        sFusionCannon601.SetActive(true);
        fusionCannon.Enable();
        //StartCoroutine(cannonWait());
        //laserShoot.Enable();
    }

    public void fusionCannonFire() //Start coroutine, then enemy abilities
    {
        sFusionCannon601.SetActive(false);
        artillery.fireFusionCannon();
        gm.resumeGame();
        StartCoroutine(cannonWait());
    }

    private IEnumerator cannonWait()
    {
        fusionCannon.Disable();
        yield return new WaitForSeconds(2.5f);
        gm.pauseGame();
        sEnemyAbilities602.SetActive(true);
    }

    public void enemyAbilities() //Triggered by player firing fusion cannon and a 2 second wait. Flashes specail indicator and starts enemyindicatewait coroutine.
    {
        sEnemyAbilities602.SetActive(false);
        centreEnemy.specialIndicator(Color.red, 2.5f);
        StartCoroutine(enemyIndicateWait());
    }

    private IEnumerator enemyIndicateWait()
    {
        gm.resumeGame();
        yield return new WaitForSeconds(1.0f);
        gm.pauseGame();
        sReactiveShield603.SetActive(true);
        reactiveShield.Enable();
        // sEnemyIndicator602.SetActive(true);
    } //When done, doesn't activate anything, as the player must hit W

    //PLAYER ACTIVATES REACTIVE SHIELD HERE (SEE UPDATE FUNCTION)

    public void ReactiveShield() //This is called by the player activatign their shield
    {
        sReactiveShield603.SetActive(false);
        reactiveShield.Disable();
        frigate.ShieldsUp();
        //  sEnemyIndicator602.SetActive(false);
        StartCoroutine(retaliateWait());
    }

    private IEnumerator retaliateWait()
    {
        gm.resumeGame();
        yield return new WaitForSeconds(1.0f);
        centreEnemy.dummyLaser();
        frigate.absorbedSpeccialAttack();
        yield return new WaitForSeconds(4.0f);
        gm.pauseGame();
        sRetaliate62.SetActive(true);
    }

    public void Retaliate()  //Activates heal slide and waits for player input
    {
        sRetaliate62.SetActive(false);
        heal.Enable();
        sHeal7.SetActive(true);
    }

    public void Heal()
    {
        heal.Disable();
        sHeal7.SetActive(false);
        tender.heal();
        StartCoroutine(healWait());
    }

    private IEnumerator healWait()
    {
        gm.resumeGame();
        yield return new WaitForSeconds(0.5f);
        gm.pauseGame();
        sPostHeal71.SetActive(true);
    }

    public void PostHeal()
    {
        sPostHeal71.SetActive(false);
        sAbilityChart8.SetActive(true);
    }

    public void abilityChart()
    {
        sAbilityChart8.SetActive(false);
        sFinal.SetActive(true);
    }

    //public void missileTutorial()
    //{
    //    missileBarrage.Disable();
    //    sMissileTut62.SetActive(false);
    //    artillery.retaliateAttack();
    //    StartCoroutine(missileWait());
    //}

    //public void missileHit()
    //{
    //    sMissileHit62.SetActive(false);
    //    sAbilityChart8.SetActive(true);
    //}

    public void FinalSlide()
    {
        gm.resumeGame();
        GameManager.tutorial = false;
        gm.restartGame();
    }

    private IEnumerator pauseWait()
    {
        gm.resumeGame();
        yield return new WaitForSeconds(2.0f);
        gm.pauseGame();
    }

    //IEnumerator fireWait()
    //{
    //    gm.resumeGame();
    //    yield return new WaitForSeconds(1.0f);
    //    centreEnemy.fireWeapons(frigate, "Enemy");
    //    yield return new WaitForSeconds(1.0f);
    //  //  sGoodJob52.SetActive(true);
    //    gm.pauseGame();
    //}
    //IEnumerator missileWait()
    //{
    //    gm.resumeGame();
    //    yield return new WaitForSeconds(2.0f);
    //    sMissileHit62.SetActive(true);
    //    gm.pauseGame();
    //}

    private void Update()
    {
        //if (frigateShoot.triggered)
        //{
        //    DemonstrateFire();
        //}
        if (reactiveShield.triggered)
        {
            ReactiveShield(); //RE-do th is toactivate retal slide
        }
        if (fusionCannon.triggered)
        {
            fusionCannonFire();
        }
        if (heal.triggered)
        {
            Heal();
        }
    }
}