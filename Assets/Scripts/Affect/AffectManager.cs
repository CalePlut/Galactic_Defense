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
            foreach (Event _event in toCull)
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
    /// Processes emotion values as derived from event processing, and updates music string if there is a change in emotion.
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

    public string GetMusicLevel()
    {
        var musicLevel = ConvertToString(valence) + "-" + ConvertToString(arousal) + "-" + ConvertToString(tension);
        return musicLevel;
    }

    private string ConvertToString(OrdinalAffect affect)
    {
        if (affect == OrdinalAffect.low)
        {
            return "Low";
        }
        else if (affect == OrdinalAffect.high)
        {
            return "High";
        }
        else
        {
            return "Mid";
        }
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