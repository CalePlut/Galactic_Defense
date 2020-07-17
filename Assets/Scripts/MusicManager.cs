using UnityEngine;

public class MusicManager : MonoBehaviour
{
    //Elias script
    private EliasPlayer elias;

    public EliasSetLevel setLevel;
    public EliasSetLevelOnTrack valenceTrack;
    public EliasSetLevelOnTrack arousalTrack;
    public EliasSetLevelOnTrack tensionTrack;

    //Affective dimensions that feed directly to Elias
    private int valence = 0;

    private int arousal = 0;
    private int tension = 0;

    private float attributeAverage = 1, progress = 0, enemyLockOn = 0.0f;//These two measures add together to set Valence
    private float engineLevel = 1, pursuitLevel = 0; //These two measures dictate the arousal level
    private float healthDifferential = 0, progressDifferential = 0, lockOn = 0;//These two measures are derived from stats compared to their max, and determines tension

    private void Awake()
    {
        elias = GetComponent<EliasPlayer>();
    }

    #region eliasCommunication

    private void updateLevels()
    {
        var newValence = attributeAverage + progress + (enemyLockOn * 10);
        var newArousal = engineLevel + pursuitLevel;
        var newTension = healthDifferential + progressDifferential + (lockOn * 10);

        var intValence = Mathf.RoundToInt(newValence);
        if (valence != intValence)
        {
            valence = intValence;
            updateEliasValence(valence);
        }

        var intArousal = Mathf.RoundToInt(newArousal);
        if (arousal != intArousal)
        {
            arousal = intArousal;
            updateEliasArousal(arousal);
        }

        var intTension = Mathf.RoundToInt(newTension);
        if (tension != intTension)
        {
            tension = intTension;
            updateEliasTension(tension);
        }

        setSliderValues();
    }

    public void startMusic()
    {
        elias.StartTheme(setLevel.CreateSetLevelEvent(elias.Elias));
    }

    private void updateEliasValence(int _valence)
    {
        var floatLevel = (float)_valence / 10.0f;
        var level = Mathf.RoundToInt(floatLevel);

        if (level > 0)
        {
            valenceTrack.level = level;
            elias.QueueEvent(valenceTrack.CreateSetLevelOnTrackEvent(elias.Elias));
        }
    }

    private void updateEliasArousal(int _arousal)
    {
        var floatLevel = (float)_arousal / 10.0f;
        var level = Mathf.RoundToInt(floatLevel);

        if (level > 0)
        {
            arousalTrack.level = level;
            elias.QueueEvent(arousalTrack.CreateSetLevelOnTrackEvent(elias.Elias));
        }
        //This is where we'll run action presets that change on arousal
    }

    private void updateEliasTension(int _tension)
    {
        var floatLevel = (float)_tension / 10.0f;
        var level = Mathf.RoundToInt(floatLevel);

        if (level > 0)
        {
            tensionTrack.level = level;
            elias.QueueEvent(tensionTrack.CreateSetLevelOnTrackEvent(elias.Elias));
        }
        //This is where we'll run action presets that change on tension
    }

    #endregion eliasCommunication

    #region attributeUpdates

    //Attribute level and progress change Valence
    public void changeAttributeLevel(int reactor, int sensor, int engine)
    {
        var newAvg = (float)reactor + (float)sensor + (float)engine;
        newAvg /= 3.0f;

        attributeAverage = newAvg;

        //Also update engine level
        changeEngines(engine);

        updateLevels();
    }

    public void changeProgress(float _progress)
    {
        progress = _progress;
        updateLevels();
    }

    //engine Level and pursuit level change arousal;
    private void changeEngines(int newLevel)
    {
        engineLevel = (float)newLevel;
        updateLevels();
    }

    public void changePursuit(int _pursuitLevel)
    {
        pursuitLevel = (float)_pursuitLevel;
        updateLevels();
    }

    public void changeLockOn(int _lockOn) //confusing, but lockOn adds tension as it measures the player's lock on
    {
        lockOn = _lockOn;
        updateLevels();
    }

    public void changeEnemyLockOn(int _lockOn) //lockOn adds valence as it measures how close the player is to unleashing a big attack
    {
        enemyLockOn = _lockOn;
        updateLevels();
    }

    //Health and progress differentials determine tension
    public void changeHealth(float hp, float max)
    {
        healthDifferential = 1.0f - (hp / max);
        healthDifferential *= 50.0f;
        updateLevels();
    }

    public void changeProgressDifferential(float diff)
    {
        progressDifferential = diff;
        updateLevels();
    }

    #endregion attributeUpdates

    #region debug

    public SliderText valenceSlider, arousalSlider, tensionSlider;

    private void setSliderValues()
    {
        valenceSlider.updateValue(valence);
        arousalSlider.updateValue(arousal);
        tensionSlider.updateValue(tension);
    }

    #endregion debug
}