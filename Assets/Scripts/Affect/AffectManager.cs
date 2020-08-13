using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public enum OrdinalAffect { low, medium, high }

public class AffectManager : MonoBehaviour
{
    private OrdinalAffect valenceMood = OrdinalAffect.low, arousalMood = OrdinalAffect.low, tensionMood = OrdinalAffect.low; //Underlying mood values that emotions can alter
    private OrdinalAffect valence, arousal, tension; //Full Affect states
    private List<Event> events; //List of all events, prospective and past
    private List<Event> toCull; //Faux garbage collection

    private bool modelEmotions = true;
    public AffectVariables affectVars;

    public AffectBarText valenceDisplay, arousalDisplay, tensionDisplay;

    private void Start()
    {
        events = new List<Event>();
        toCull = new List<Event>();

        StartCoroutine(ProcessEvents()); //Start to process emotions
    }

    /// <summary>
    /// Creates prospective event. Assumes determinate, unless otherwise indicated
    /// </summary>
    /// <param name="_valence">valence (max)</param>
    /// <param name="_arousal">arousal (max)</param>
    /// <param name="_tension">tension (max)</param>
    /// <param name="_duration">Time until event</param>
    /// <param name="_determinate">(Opt) Event has determinate timing</param>
    public Event CreateProspectiveEvent(Emotion _valence, Emotion _arousal, Emotion _tension, float _duration, bool _determinate = false)
    {
        var toAdd = new ProspectiveEvent(_valence, _arousal, _tension, _duration, _determinate, this);
        events.Add(toAdd);
        return toAdd;
    }

    /// <summary>
    /// Creates past event, with emotion fading over time
    /// </summary>
    /// <param name="_valence">valence (max)</param>
    /// <param name="_arousal">arousal (max)</param>
    /// <param name="_tension">tension (max)</param>
    /// <param name="_duration">Time to fade emotional change</param>
    public void CreatePastEvent(Emotion _valence, Emotion _arousal, Emotion _tension, float _duration)
    {
        var toAdd = new PastEvent(_valence, _arousal, _tension, _duration, this);
        events.Add(toAdd);
    }

    /// <summary>
    /// Indeterminate events will need to be able to be created outside of the normal voids and manually entered
    /// </summary>
    /// <param name="toAdd"></param>
    public void AddEvent(Event toAdd)
    {
        events.Add(toAdd);
    }

    /// <summary>
    /// Used for faux garbage collection. Adds event to culling list to be culled when we're done enumerating.
    /// </summary>
    /// <param name="_event"></param>
    public void CullEvent(Event _event)
    {
        toCull.Add(_event);
    }

    /// <summary>
    /// Used when wave is cleared. Clear all affective events and replace with single 5-second valence boost (essentially reset emotion)
    /// </summary>
    public void ClearWave()
    {
        events = new List<Event>();
        CreatePastEvent(new Emotion(EmotionDirection.increase, EmotionStrength.strong), null, null, 5.0f);
    }

    /// <summary>
    /// Processes events while emotion model is running
    /// Creates summed emotion amount and sends to Emotion processor
    /// </summary>
    /// <returns></returns>
    private IEnumerator ProcessEvents()
    {
        while (modelEmotions)
        {
            var valenceValue = 0.0f;
            var arousalValue = 0.0f;
            var tensionValue = 0.0f;

            foreach (Event _event in events)
            {
                _event.Advance(Time.deltaTime); //Advance event's clock and processing

                var valenceAdd = _event.GetValence();
                var arousalAdd = _event.GetArousal();
                var tensionAdd = _event.GetTension();
                //Debug.Log("Adding VAT of " + valenceAdd + "," + arousalAdd + "," + tensionAdd);

                valenceValue += valenceAdd; //Retreives VAT values from event
                arousalValue += arousalAdd;
                tensionValue += tensionAdd;
            }
            //Garbage collection - removes toCull and creates new one
            foreach(Event _event in toCull)
            {
                if (events.Contains(_event))
                {
                    events.Remove(_event);
                }
            }
            toCull = new List<Event>();

            //Debug.Log("Total VAT Values: " + valenceValue + "," + arousalValue + "," + tensionValue);

            ProcessEmotions(valenceValue, arousalValue, tensionValue);

            yield return null;
        }
    }

    /// <summary>
    /// Processes emotion values as derived from event processing.
    /// </summary>
    private void ProcessEmotions(float valenceChange, float arousalChange, float tensionChange)
    {
        //Process valence for normal and strong thresholds
        valence = ProcessEmotion(valenceMood, valenceChange);
        arousal = ProcessEmotion(arousalMood, arousalChange);
        tension = ProcessEmotion(tensionMood, tensionChange);

        //Debug.Log("V: " + valence + "A: " + arousal + "T:" + tension);

        valenceDisplay.UpdateAffect(valence);
        arousalDisplay.UpdateAffect(arousal);
        tensionDisplay.UpdateAffect(tension);
    }

    private OrdinalAffect ProcessEmotion(OrdinalAffect mood, float emotionValue)
    {
        var strongThreshold = affectVars.strongThreshold;
        var threshold = affectVars.changeThreshold;

        //First, process strong emotions
        if (emotionValue > strongThreshold)
        {
            return LayerAffect(mood, 2);
        }
        else if (emotionValue < -strongThreshold)
        {
            return LayerAffect(mood, -2);
        }
        //Next, we process weak emotions.
        else if (emotionValue > threshold)
        {
            return LayerAffect(mood, 1);
        }
        else if (emotionValue < -threshold)
        {
            return LayerAffect(mood, -1);
        }
        //If we don't meet any threshold, return mood
        else return mood;
    }

    private OrdinalAffect LayerAffect(OrdinalAffect mood, int change)
    {
        if (mood == OrdinalAffect.low) //If low, we can only adjust up
        {
            switch (change)
            {
                case -2:
                case -1:
                    return OrdinalAffect.low;

                case 1:
                    return OrdinalAffect.medium;

                case 2:
                    return OrdinalAffect.high;

                default:
                    return mood;
            }
        }
        else if (mood == OrdinalAffect.medium) //If medium, we can't adjust strength, but can adjust up or down
        {
            switch (change)
            {
                case -2:

                case -1:
                    return OrdinalAffect.low;

                case 1:

                case 2:
                    return OrdinalAffect.high;

                default: return mood;
            }
        }
        else if (mood == OrdinalAffect.high) //If high, we can only adjust down
        {
            switch (change)
            {
                case -2:
                    return OrdinalAffect.low;

                case -1:
                    return OrdinalAffect.medium;

                case 1:

                case 2:
                    return OrdinalAffect.high;

                default: return mood;
            }
        }
        else return mood; //Very confusing if this ever happens
    }

    public void setMood(OrdinalAffect _valenceMood, OrdinalAffect _arousalMood, OrdinalAffect _tensionMood)
    {
        valenceMood = _valenceMood;
        arousalMood = _arousalMood;
        tensionMood = _tensionMood;
    }

    //This is all the old stuff

    //private float valence, arousal, tension;
    //private float baseValence, baseArousal, baseTension;
    //private float frigateHP, intelHP, supportHP;

    //private bool intelLow, frigateLow, supportLow;
    //private bool leftEnemyLow, centreEnemyLow, rightEnemyLow, parryFrame;  //Booleans track states that increase player tension.
    //private bool intelRespawning, supportRespawning;

    //private float intelTension, frigateTension, supportTension;
    //private float leftEnemyHPTension, centreEnemyHPTension, rightEnemyHPTension;
    //private float parryTension;

    //private List<ArousalModify> arousalModifiers;

    //public AffectVariables affectVariables;

    //private float weakValence, moderateValence, strongValence;
    //private float weakTension, moderateTension, strongTension;
    //private float arousalScale;

    //private int valenceLevel = 2, arousalLevel = 2, tensionLevel = 2;

    //private bool valenceLockout = false, arousalLockout = false, tensionLockout = false; //These booleans remove problems of music adapting too fast and sending conflicting messages - only one change per level per 5 seconds is allowed

    //private float resetSpeed;

    //public bool musicControl = false;

    ////private int actions;

    //// public TextMeshProUGUI apmText;

    //public AffectBarText valenceDisplay, arousalDisplay, tensionDisplay;
    //private EliasPlayer music;

    //public InputAction anything;

    //private void Start()
    //{
    //    baseValence = affectVariables.V11;
    //    baseArousal = affectVariables.A11;
    //    baseTension = affectVariables.T11;

    //    valence = baseValence;
    //    arousal = baseArousal;
    //    tension = baseTension;

    //    adjustValence(valence);
    //    adjustTension(tension);

    //    weakValence = affectVariables.weakValence;
    //    moderateValence = affectVariables.moderateValence;
    //    strongValence = affectVariables.strongValence;

    //    weakTension = affectVariables.weakTension;
    //    moderateTension = affectVariables.moderateTension;
    //    strongTension = affectVariables.strongTension;

    //    resetSpeed = affectVariables.resetSpeed;

    //    arousalScale = affectVariables.arousalScalar;

    //    arousalModifiers = new List<ArousalModify>();

    //    music = GetComponent<EliasPlayer>();
    //}

    //#region valenceEvents

    ////First, positive events
    //public void enemyHit(float damage) //Dealing damage adds valence scaling on damage
    //{
    //    var valenceAdd = weakValence + damage;
    //    adjustValence(valenceAdd);

    //    arousalModifiers.Add(new ArousalModify(damage * arousalScale));
    //}

    //public void enemydie(position pos)
    //{
    //    //adjustTension(-10.0f);

    //    if (pos == position.left)
    //    {
    //        adjustValence(weakValence);
    //        leftEnemyLow = false;
    //        leftEnemyHPTension = 0.0f;
    //    }
    //    if (pos == position.centre)
    //    {
    //        adjustValence(strongValence);
    //        centreEnemyLow = false;
    //        centreEnemyHPTension = 0.0f;

    //        parryFrame = false;
    //        parryTension = 0.0f;
    //    }
    //    if (pos == position.right)
    //    {
    //        adjustValence(weakValence);
    //        rightEnemyLow = false;
    //        rightEnemyHPTension = 0.0f;
    //    }
    //}

    //public void interruptFusionCannon()
    //{
    //    adjustValence(weakValence);
    //}

    //public void healShip(bool upgrade) //Disabled
    //{
    //    var toChange = moderateValence;
    //    if (upgrade) { toChange = strongValence; }
    //    //  adjustValence(toChange);
    //}

    //public void respawnShip() //Disabled
    //{
    //    //adjustValence(moderateValence);
    //}

    //public void ultimateAbility()
    //{
    //    adjustValence(strongValence);
    //}

    //public void parry()
    //{
    //    adjustValence(weakValence);
    //}

    ////Second, negative valence events
    //public void playerHit(float damage)
    //{
    //    var toAdjust = weakValence + damage;
    //    toAdjust = -toAdjust;
    //    adjustValence(toAdjust);
    //}

    //public void loseShip(PlayerShip ship)
    //{
    //    adjustValence(strongValence * (-1));
    //    if (ship.gameObject.name == "Support")
    //    {
    //        supportRespawning = true;
    //    }
    //    if (ship.gameObject.name == "Intel")
    //    {
    //        intelRespawning = true;
    //    }
    //    //adjustTension(+moderateTension);
    //}

    //public void enemyAbility()
    //{
    //    adjustValence(-moderateValence);
    //}

    //#endregion valenceEvents

    //#region tensionStates

    ///// <summary>
    ///// Used to pass total tension value from incoming enemy attacks
    ///// </summary>
    ///// <param name="_enemyTension">Total enemy tension value</param>
    //public void UpdateEnemyTension(float _enemyTension)
    //{
    //    //Placeholder for now, will update when fixing affet manager
    //}

    //public void updatePlayerTension()
    //{
    //    //Evaluate tension levels of various player ships
    //    if (intelLow) { intelTension += weakTension * Time.deltaTime; }
    //    else if (intelRespawning) { intelTension += moderateTension * Time.deltaTime; }
    //    else { intelTension = 0; }
    //    if (frigateLow) { frigateTension += moderateTension * Time.deltaTime; }
    //    else { frigateTension = 0; }
    //    if (supportLow) { supportTension += weakTension * Time.deltaTime; }
    //    else if (supportRespawning) { supportTension += moderateTension * Time.deltaTime; }
    //    else { supportTension = 0; }

    //    var playerValues = intelTension + frigateTension + supportTension;

    //    if (leftEnemyLow) { leftEnemyHPTension += weakTension * Time.deltaTime; }
    //    else { leftEnemyHPTension = 0; }
    //    if (centreEnemyLow) { centreEnemyHPTension += moderateTension * Time.deltaTime; }
    //    else { centreEnemyHPTension = 0; }
    //    if (rightEnemyLow) { rightEnemyHPTension += weakTension * Time.deltaTime; }
    //    else { rightEnemyHPTension = 0; }

    //    if (parryFrame) { parryTension += strongTension * Time.deltaTime; }
    //    else { parryTension = 0; }

    //    var enemyValues = leftEnemyHPTension + centreEnemyHPTension + rightEnemyHPTension + parryTension;

    //    var tensionTotal = baseTension + playerValues + enemyValues;
    //    adjustTension(tensionTotal);
    //}

    //public void respawn(PlayerShip ship)
    //{
    //    if (ship.gameObject.name == "Support")
    //    {
    //        supportRespawning = false;
    //        respawnShip();
    //    }
    //    if (ship.gameObject.name == "Intel")
    //    {
    //        respawnShip();
    //        intelRespawning = false;
    //    }
    //}

    //public void setParryFrame(bool frame)
    //{
    //    if (frame) { parryFrame = true; }
    //    else
    //    {
    //        parryFrame = false;
    //        parryTension = 0.0f;
    //    }
    //}

    //#endregion tensionStates

    //#region moodVariables

    //public void setProgess(int stage, int encounter)
    //{
    //    switch (stage)
    //    {
    //        case 1:
    //            switch (encounter)
    //            {
    //                case 1:
    //                    setBase(affectVariables.V11, affectVariables.A11, affectVariables.T11);
    //                    break;

    //                case 2:
    //                    setBase(affectVariables.V12, affectVariables.A12, affectVariables.T12);
    //                    break;
    //            }
    //            break;

    //        case 2:
    //            switch (encounter)
    //            {
    //                case 1:
    //                    setBase(affectVariables.V21, affectVariables.A21, affectVariables.T21);
    //                    break;

    //                case 2:
    //                    setBase(affectVariables.V22, affectVariables.A22, affectVariables.T22);
    //                    break;

    //                case 3:
    //                    setBase(affectVariables.V23, affectVariables.A23, affectVariables.T23);
    //                    break;
    //            }
    //            break;

    //        case 3:
    //            switch (encounter)
    //            {
    //                case 1:
    //                    setBase(affectVariables.V31, affectVariables.A31, affectVariables.T31);
    //                    break;

    //                case 2:
    //                    setBase(affectVariables.V32, affectVariables.A32, affectVariables.T32);
    //                    break;

    //                case 3:
    //                    setBase(affectVariables.V33, affectVariables.A33, affectVariables.T33);
    //                    break;

    //                case 4:
    //                    setBase(affectVariables.V34, affectVariables.A34, affectVariables.T34);
    //                    break;
    //            }
    //            break;
    //    }
    //} //Sets stage progress from affectvariables levels

    //private void setBase(float V, float A, float T)
    //{
    //    baseValence = V;
    //    baseArousal = A;
    //    baseTension = T;
    //}

    ////Trying a thing where tension creating situations also control valence - layered model, remember
    //public void updateEnemyHealth(float percent, position pos)
    //{
    //    if (pos == position.left)
    //    {
    //        if (percent < 0.4f)
    //        {
    //            leftEnemyLow = true;
    //            adjustValence(weakValence);
    //            // Debug.Log("Updating valence rom enemy health");
    //        }
    //        else { leftEnemyLow = false; }
    //    }
    //    if (pos == position.centre)
    //    {
    //        if (percent < 0.4f)
    //        {
    //            centreEnemyLow = true;
    //            adjustValence(weakValence);

    //            // Debug.Log("Updating valence rom enemy health");
    //        }
    //        else { centreEnemyLow = false; }
    //    }
    //    if (pos == position.right)
    //    {
    //        if (percent < 0.4f)
    //        {
    //            rightEnemyLow = true;
    //            adjustValence(weakValence);

    //            //  Debug.Log("Updating valence rom enemy health");
    //        }
    //        else { rightEnemyLow = false; }
    //    }
    //}

    //public void updateFrigateHealth(float percent)
    //{
    //    frigateHP = percent;
    //    if (frigateHP < 0.2)
    //    {
    //        adjustValence(-weakValence);

    //        // Debug.Log("Updating valence from player health");
    //        frigateLow = true;
    //    }
    //    else
    //    {
    //        frigateLow = false;
    //        frigateTension = 0.0f;
    //    }
    //}

    //public void updateIntelHealth(float percent)
    //{
    //    intelHP = percent;
    //    if (intelHP < 0.2)
    //    {
    //        adjustValence(-weakValence);
    //        intelLow = true;

    //        // Debug.Log("Updating valence from artillery health");
    //    }
    //    else
    //    {
    //        intelLow = false;
    //        intelTension = 0.0f;
    //    }
    //}

    //public void updateSupportHealth(float percent)
    //{
    //    supportHP = percent;

    //    if (supportHP < 0.2)
    //    {
    //        adjustValence(-weakValence);
    //        supportLow = true;
    //    }
    //    else
    //    {
    //        supportLow = false;
    //        supportTension = 0.0f;
    //    }
    //}

    //#endregion moodVariables

    //#region AffectImplementation

    //private void adjustValence(float amount)
    //{
    //    valence += amount;
    //    updateValence();
    //}

    //private void updateValence()
    //{
    //    if (valence > 100.0f) { valence = 100.0f; }
    //    if (valence < -100.0f) { valence = -100.0f; }
    //    if (!valenceLockout)
    //    {
    //        if (valence < 33.0f)
    //        {
    //            if (valenceLevel != 1)
    //            {
    //                valenceDisplay.Affect = 1;
    //                if (musicControl)
    //                {
    //                    if (arousalLevel == 1) { music.RunActionPreset("LowVLowA"); }
    //                    else if (arousalLevel == 3) { music.RunActionPreset("LowVHighA"); }
    //                    else { music.RunActionPreset("LowVMidA"); }
    //                }
    //                //music.RunActionPreset("ValenceLow");
    //                valenceLevel = 1;
    //                StartCoroutine(valenceLock());
    //            }
    //        }
    //        else if (valence < 66.0f)
    //        {
    //            if (valenceLevel != 2)
    //            {
    //                valenceDisplay.Affect = 2;
    //                if (musicControl)
    //                {
    //                    if (arousalLevel == 1) { music.RunActionPreset("MidVLowA"); }
    //                    else if (arousalLevel == 3) { music.RunActionPreset("MidVHighA"); }
    //                    else { music.RunActionPreset("MidVMidA"); }
    //                }
    //                //music.RunActionPreset("ValenceMid");
    //                valenceLevel = 2;
    //                StartCoroutine(valenceLock());
    //            }
    //        }
    //        else
    //        {
    //            if (valenceLevel != 3)
    //            {
    //                valenceDisplay.Affect = 3;
    //                if (musicControl)
    //                {
    //                    if (arousalLevel == 1) { music.RunActionPreset("HighVLowA"); }
    //                    else if (arousalLevel == 3) { music.RunActionPreset("HighVHighA"); }
    //                    else { music.RunActionPreset("HighVMidA"); }
    //                }
    //                // music.RunActionPreset("ValenceHigh");
    //                valenceLevel = 3;
    //                StartCoroutine(valenceLock());
    //            }
    //        }
    //    }
    //}

    //private void adjustArousal()
    //{
    //    arousal = baseArousal;
    //    if (arousalModifiers == null) { arousalModifiers = new List<ArousalModify>(); }
    //    if (arousalModifiers.Count > 0)
    //    {
    //        var toRemove = new List<ArousalModify>(); //This will be used to clear the arousalmodifiers when they reach zero hopefully

    //        foreach (ArousalModify modifier in arousalModifiers)
    //        {
    //            modifier.tick(Time.deltaTime);
    //            if (modifier.zeroWeight()) { toRemove.Add(modifier); }
    //            else { arousal += modifier.queryArousal(); }
    //        }

    //        foreach (ArousalModify removal in toRemove) //Removes any arousalModifiers that were below zero
    //        {
    //            if (arousalModifiers.Contains(removal))
    //            {
    //                arousalModifiers.Remove(removal);
    //            }
    //        }
    //    }

    //    //Here's the hook-up to everything else, once we have an arousal value
    //    if (arousal > 100.0f) { arousal = 100.0f; }
    //    if (arousal < -100.0f) { arousal = -100.0f; }
    //    //arousalDisplay.updateValue(Mathf.RoundToInt(arousal));
    //    if (!arousalLockout)
    //    {
    //        if (arousal < 33.0f)
    //        {
    //            if (arousalLevel != 1)
    //            {
    //                arousalDisplay.Affect = 1;
    //                if (musicControl)
    //                {
    //                    if (valenceLevel == 1) { music.RunActionPreset("LowVLowA"); }
    //                    else if (valenceLevel == 3) { music.RunActionPreset("HighVLowA"); }
    //                    else { music.RunActionPreset("MidVLowA"); }
    //                }
    //                //music.RunActionPreset("ArousalLow");
    //                arousalLevel = 1;
    //                StartCoroutine(arousalLock());
    //            }
    //        }
    //        else if (arousal < 66.0f)
    //        {
    //            if (arousalLevel != 2)
    //            {
    //                arousalDisplay.Affect = 2;
    //                if (musicControl)
    //                {
    //                    if (valenceLevel == 1) { music.RunActionPreset("LowVMidA"); }
    //                    else if (valenceLevel == 3) { music.RunActionPreset("HighVMidA"); }
    //                    else { music.RunActionPreset("MidVMidA"); }
    //                }
    //                //music.RunActionPreset("ArousalMid");
    //                arousalLevel = 2;
    //                StartCoroutine(arousalLock());
    //            }
    //        }
    //        else
    //        {
    //            if (arousalLevel != 3)
    //            {
    //                arousalDisplay.Affect = 3;
    //                if (musicControl)
    //                {
    //                    if (valenceLevel == 1) { music.RunActionPreset("LowVHighA"); }
    //                    else if (valenceLevel == 3) { music.RunActionPreset("HighVHighA"); }
    //                    else { music.RunActionPreset("MidVHighA"); }
    //                }
    //                // music.RunActionPreset("ArousalHigh");
    //                arousalLevel = 3;
    //                StartCoroutine(arousalLock());
    //            }
    //        }
    //    }
    //}

    //private void adjustTension(float amount)
    //{
    //    tension = amount;
    //    if (tension > 100.0f) { tension = 100.0f; }
    //    if (tension < -100.0f) { tension = -100.0f; }
    //    //tensionDisplay.updateValue(Mathf.RoundToInt(tension));

    //    if (!tensionLockout)
    //    {
    //        if (tension < 33.0f)
    //        {
    //            if (tensionLevel != 1)
    //            {
    //                tensionDisplay.Affect = 1;
    //                if (musicControl)
    //                {
    //                    music.RunActionPreset("TensionLow");
    //                    tensionLevel = 1;
    //                    StartCoroutine(tensionLock());
    //                }
    //            }
    //        }
    //        else if (tension < 66.0f)
    //        {
    //            if (tensionLevel != 2)
    //            {
    //                tensionDisplay.Affect = 2;
    //                if (musicControl)
    //                {
    //                    music.RunActionPreset("TensionMid");
    //                    tensionLevel = 2;
    //                    StartCoroutine(tensionLock());
    //                }
    //            }
    //        }
    //        else
    //        {
    //            if (tensionLevel != 3)
    //            {
    //                tensionDisplay.Affect = 3;
    //                if (musicControl)
    //                {
    //                    music.RunActionPreset("TensionHigh");
    //                    tensionLevel = 3;
    //                    StartCoroutine(tensionLock());
    //                }
    //            }
    //        }
    //    }
    //}

    //private void autoReset()
    //{
    //    valence = Mathf.Lerp(valence, baseValence, resetSpeed * Time.deltaTime);
    //    //arousal = Mathf.Lerp(arousal, baseArousal, resetSpeed * Time.deltaTime); //Trying this out where arousal doesn't auto-reset because the arousal modifiers already decay over time
    //    tension = Mathf.Lerp(tension, baseTension, resetSpeed * Time.deltaTime);

    //    adjustTension(tension);
    //    updateValence();
    //    //valenceDisplay.updateValue((int)valence);
    //    //arousalDisplay.updateValue((int)arousal);
    //    //tensionDisplay.updateValue((int)tension);
    //}

    //#endregion AffectImplementation

    //private void Update()
    //{
    //    updatePlayerTension();
    //    autoReset();
    //    adjustArousal();

    //    // apmText.text = "APM: " + actions;
    //}

    //private IEnumerator valenceLock()
    //{
    //    valenceLockout = true;
    //    yield return new WaitForSeconds(1.5f);
    //    valenceLockout = false;
    //}

    //private IEnumerator arousalLock()
    //{
    //    arousalLockout = true;
    //    yield return new WaitForSeconds(1.5f);
    //    arousalLockout = false;
    //}

    //private IEnumerator tensionLock()
    //{
    //    tensionLockout = true;
    //    yield return new WaitForSeconds(1.5f);
    //    tensionLockout = false;
    //}
}

#region Events

/// <summary>
/// Affective event class. Tracks time and scales affect with Advance(). Can be prospective or past, determinate or not.
/// </summary>
public class Event
{
    //private float maxValence, maxArousal, maxTension; //Maximum values of events, to be modified by time
    public Emotion valence { get; protected set; } //Emotions are stored as Emotion and Value - Value is used to pass upstream, Emotion is used to process

    public Emotion arousal { get; protected set; }
    public Emotion tension { get; protected set; }
    protected float duration; //Duration of past events, or time to prospective events.
    protected float timer;  //Counts down over time
    protected bool determinate; //Controls over time
    protected AffectManager manager;

    private float scalar;

#nullable enable

    public Event(Emotion? _valence, Emotion? _arousal, Emotion? _tension, float _duration, AffectManager _manager)
    {
        if (_valence != null)
        {
            valence = _valence;
        }
        else { valence = new Emotion(0, 0); }
        if (_arousal != null)
        {
            arousal = _arousal;
        }
        else { arousal = new Emotion(0, 0); }
        if (_tension != null)
        {
            tension = _tension;
        }
        else { tension = new Emotion(0, 0); }
        duration = _duration;
        manager = _manager;
    }

#nullable disable

    /// <summary>
    /// Tracks scalar as a function of time, will be used for recall
    /// Overridden with custom behaviour for timing and garbage collection
    /// </summary>
    public virtual void Advance(float deltaTime)
    {
        scalar = timer / duration;
    }

    /// <summary>
    /// Scsales valence
    /// </summary>
    /// <returns>Scaled valence value</returns>
    public float GetValence()
    {
        var value = valence.GetScaledEmotion(scalar);
        return value;
    }

    /// <summary>
    /// Scales arousal
    /// </summary>
    /// <returns>Scaled arousal value</returns>
    public float GetArousal()
    {
        var value = arousal.GetScaledEmotion(scalar);
        return value;
    }

    /// <summary>
    /// Scales tension
    /// </summary>
    /// <returns>Scaled tension value</returns>
    public float GetTension()
    {
        var value = tension.GetScaledEmotion(scalar);
        return value;
    }
}

/// <summary>
/// Event that has happened - Emotion will fade over a provided duration
/// </summary>
public class PastEvent : Event
{
    public PastEvent(Emotion _valence, Emotion _arousal, Emotion _tension, float _duration, AffectManager _manager) : base(_valence, _arousal, _tension, _duration, _manager)
    {
        timer = _duration;
    }

    /// <summary>
    /// Counts down timer to scale reactive emotion weakening over time.
    /// Also faux garbage collection
    /// </summary>
    /// <param name="deltaTime">Passes Unity Time.deltaTime</param>
    public override void Advance(float deltaTime)
    {
        //Scales emotions weakening after event
        timer -= deltaTime;
        base.Advance(deltaTime);

        //Easy garbage - if our timer is expired, remove from list
        if (timer <= 0.0f)
        {
            //If we are no longer altering emotion, cull selves
            manager.CullEvent(this);
        }
    }
}

/// <summary>
/// Event that has not yet happened - may be determinate (known end point) or indeterminate (max 60 seconds)
/// </summary>
public class ProspectiveEvent : Event
{
    private float safetyTimer;

    public ProspectiveEvent(Emotion _valence, Emotion _arousal, Emotion _tension, float _duration, bool _determinate, AffectManager _manager) : base(_valence, _arousal, _tension, _duration, _manager)
    {
        determinate = _determinate;
        safetyTimer = 0.0f;
        timer = 0.0f;
    }

    /// <summary>
    /// Counts up timer to scale prospective emotion strengthening over time.
    /// Also faux garbage collection
    /// </summary>
    /// <param name="deltaTime">Passes Unity Time.deltaTime</param>
    public override void Advance(float deltaTime)
    {
        //Scales emotions strengthening as event approaches
        timer = Mathf.Clamp(timer + deltaTime, 0.0f, duration);
        safetyTimer += deltaTime;
        base.Advance(deltaTime);

        //Harder garbate - if we're determinate, we can clear ourself
        if (determinate)
        {
            if (timer == duration)
            {
                //If we're determinate and have reached the time, we are no longer prospective
                manager.CullEvent(this);
            }
        }
        else
        {
            if (safetyTimer >= 60.0f)
            {
                Debug.Log("ERROR: Indeterminate event longer than 60 seconds");
                //Purely for safety --- indeterminate events should be externally culled before a minute has passed. If not, we need to re-design some stuff
                manager.CullEvent(this);
            }
        }
    }
}

#endregion Events

#region Emotions

/// <summary>
/// Emotions can be increasing or decreasing, pretty straightforwards
/// Multiply with EmotionStrength to describe emotional change from event
/// </summary>
public enum EmotionDirection { none = 0, decrease = -1, increase = 1 }

/// <summary>
/// Emotions have a strength of weak, moderate, or strong.
/// Multiply with EmotionDirection to describe emotional increase or decrease
/// </summary>
public enum EmotionStrength { none = 0, weak = 1, moderate = 2, strong = 3 }

/// <summary>
/// Stores strength and direction of the emotion associated with an event
/// </summary>
public class Emotion
{
    private EmotionDirection direction;
    private EmotionStrength strength;

    /// <summary>
    /// Constructor for emotion, stores direction and strength
    /// </summary>
    /// <param name="_direction">Increase/decrease in emotion</param>
    /// <param name="_strength">Strength of emotional change</param>
    public Emotion(EmotionDirection _direction, EmotionStrength _strength)
    {
        direction = _direction;
        strength = _strength;
    }

    /// <summary>
    /// Scales emotion value by provided scalar
    /// </summary>
    /// <param name="scalar">Scalar provided by event</param>
    /// <returns></returns>
    public float GetScaledEmotion(float scalar)
    {
        var unscaledEmotion = (float)direction * (float)strength;
        return unscaledEmotion * scalar;
    }
}

#endregion Emotions

public class ArousalModify
{
    //ArousalModify stores events and weights, and decreases the weight over 6 seconds.

    private float amount;
    private float weight;

    public ArousalModify(float _amount)
    {
        amount = _amount;
        weight = 1.0f;
    }

    public float queryArousal()
    {
        return amount * weight;
    }

    public void tick(float deltaTime)
    {
        weight -= deltaTime / 6.0f;
    }

    public bool zeroWeight()
    {
        if (weight <= 0.0f) { return true; }
        else return false;
    }
}