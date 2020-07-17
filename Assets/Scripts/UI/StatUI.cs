using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatUI : MonoBehaviour
{
    public Slider reactorHealth, sensorHealth, engineHealth;  //Slider objects for displaying bar version of stats
    public TextMeshProUGUI reactorText, sensorText, engineText; //TMPro assets for displaying numerical values of stats

    // public Slider progress, chase;
    public GameObject pursuitTracker;

    public healthBarAnimator shields;
    // public GameObject missileCounter;

    public List<Toggle> reactorUpgrade1, sensorUpgrade1, engineUpgrade1;
    public List<Toggle> reactorUpgrade2, sensorUpgrade2, engineUpgrade2;
    public List<Toggle> reactorUpgrade3, sensorUpgrade3, engineUpgrade3;

    private List<Toggle> reactorActiveUpgrade, sensorActiveUpgrade, engineActiveUpgrade; //these lists hold hte active upgrade levels for easier ui calls

    public GameObject reactor1, reactor2, reactor3, sensor1, sensor2, sensor3, engine1, engine2, engine3;

    public GameObject gameManager;
    private GameManager gm;
    private MusicManager music;

    private bool caught = false;

    // Start is called before the first frame update
    private void Awake()
    {
        gm = gameManager.GetComponent<GameManager>();
        music = gameManager.GetComponent<MusicManager>();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    //public void upgradeShieldMax(int newMax)
    //{
    //    shields.Refresh(newMax, newMax);
    //}

    //public void Setup(int _statStart, int _statMax, int _shieldStart, int _shieldMax)
    //{
    //    //Sets upper bounds for stats - this shouldn't change during play
    //    reactorHealth.maxValue = _statMax;
    //    sensorHealth.maxValue = _statMax;
    //    engineHealth.maxValue = _statMax;

    //    //Sets initial upgradepaths
    //    reactorActiveUpgrade = reactorUpgrade1;
    //    sensorActiveUpgrade = sensorUpgrade1;
    //    engineActiveUpgrade = engineUpgrade1;

    //    shields.Refresh(_shieldMax, _shieldStart);
    //}

    public void updateUI(int reactor, int sensors, int engine)
    {
        //Updates the stat bars first
        reactorHealth.value = reactor;
        sensorHealth.value = sensors;
        engineHealth.value = engine;

        //Now updates text
        reactorText.text = reactor + "/" + reactorHealth.maxValue;
        engineText.text = engine + "/" + engineHealth.maxValue;
        sensorText.text = sensors + "/" + sensorHealth.maxValue;
    }

    public void passDamage(int damage)
    {
        shields.takeDamage(damage);
    }

    public void rechargeShield(int toAdd)
    {
        shields.addValue(toAdd);
    }

    //public void addProgress(float amt) {
    //    progress.value += amt;

    //    //updates music manager's progress
    //    music.changeProgress(progress.value);
    //}

    //public void addChase(float amt)
    //{
    //    if (!caught)
    //    {
    //        chase.value += amt;
    //        if (chase.value >= progress.value)
    //        {
    //            chaseCaught();
    //            caught = true;
    //        }

    //        //This probably doesn't need to be a function, but might as well
    //        updateMusicDifferential();
    //    }

    //}

    //void updateMusicDifferential()
    //{
    //    //progress differential is the difference between progress and chase
    //    var differential = progress.value - chase.value;
    //    music.changeProgressDifferential(differential);
    //}

    public void chaseCaught()
    {
        //gm.spawnBoss();
    }

    public void attributeLevel(Attribute toLevel, int level)
    {
        switch (toLevel)
        {
            case Attribute.engine:
                if (level == 2)
                {
                    engine1.SetActive(false);
                    engine2.SetActive(true);
                    engineActiveUpgrade = engineUpgrade2;
                }
                if (level == 3)
                {
                    engine2.SetActive(false);
                    engine3.SetActive(true);
                    engineActiveUpgrade = engineUpgrade3;
                }
                if (level == 4)
                {
                    engine3.SetActive(false);
                }
                break;

            case Attribute.reactor:
                if (level == 2)
                {
                    reactor1.SetActive(false);
                    reactor2.SetActive(true);
                    reactorActiveUpgrade = reactorUpgrade2;
                }
                if (level == 3)
                {
                    reactor2.SetActive(false);
                    reactor3.SetActive(true);
                    reactorActiveUpgrade = reactorUpgrade3;
                }
                if (level == 4)
                {
                    reactor3.SetActive(false);
                }
                break;

            case Attribute.sensors:
                if (level == 2)
                {
                    sensor1.SetActive(false);
                    sensor2.SetActive(true);
                    sensorActiveUpgrade = sensorUpgrade2;
                }
                if (level == 3)
                {
                    sensor2.SetActive(false);
                    sensor3.SetActive(true);
                    sensorActiveUpgrade = sensorUpgrade3;
                }
                if (level == 4)
                {
                    sensor3.SetActive(false);
                }
                break;
        }
    }

    public void setBonus(Attribute _bonus)
    {
        switch (_bonus)
        {
            case Attribute.shields:
                break;

            case Attribute.reactor:
                reactorText.text = reactorHealth.value + "(+1)/" + reactorHealth.maxValue;
                break;

            case Attribute.engine:
                engineText.text = engineHealth.value + "(+1)/" + engineHealth.maxValue;
                break;

            case Attribute.sensors:
                sensorText.text = sensorHealth.value + "(+1)/" + sensorHealth.maxValue;
                break;
        }
    }

    public void updateUpgradeUI(int _reactorUpgrade, int _engineUpgrade, int _sensorUpgrade)
    {
        //clears all upgrades to be safe
        foreach (Toggle pip in reactorActiveUpgrade)
        {
            pip.isOn = false;
        }
        foreach (Toggle pip in engineActiveUpgrade)
        {
            pip.isOn = false;
        }
        foreach (Toggle pip in sensorActiveUpgrade)
        {
            pip.isOn = false;
        }

        //Activates pips matching upgrade level
        for (int i = 0; i < _reactorUpgrade; i++)
        {
            reactorActiveUpgrade[i].isOn = true;
        }
        for (int i = 0; i < _engineUpgrade; i++)
        {
            engineActiveUpgrade[i].isOn = true;
        }
        for (int i = 0; i < _sensorUpgrade; i++)
        {
            sensorActiveUpgrade[i].isOn = true;
        }
    }

    public void hideUI()
    {
        shields.gameObject.SetActive(false);
        reactorHealth.gameObject.SetActive(false);
        sensorHealth.gameObject.SetActive(false);
        engineHealth.gameObject.SetActive(false);

        foreach (Toggle pip in reactorActiveUpgrade) { pip.gameObject.SetActive(false); }
        foreach (Toggle pip in sensorActiveUpgrade) { pip.gameObject.SetActive(false); }
        foreach (Toggle pip in engineActiveUpgrade) { pip.gameObject.SetActive(false); }
        pursuitTracker.SetActive(false);
        //  missileCounter.SetActive(false);
    }

    public void showUI()
    {
        shields.gameObject.SetActive(true);
        reactorHealth.gameObject.SetActive(true);
        sensorHealth.gameObject.SetActive(true);
        engineHealth.gameObject.SetActive(true);

        foreach (Toggle pip in reactorActiveUpgrade) { pip.gameObject.SetActive(true); }
        foreach (Toggle pip in sensorActiveUpgrade) { pip.gameObject.SetActive(true); }
        foreach (Toggle pip in engineActiveUpgrade) { pip.gameObject.SetActive(true); }
        pursuitTracker.SetActive(true);
        //missileCounter.SetActive(true);
    }
}