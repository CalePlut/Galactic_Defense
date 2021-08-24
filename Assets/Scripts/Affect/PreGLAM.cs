using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Likely type is used to store types of likely events. When the conditions are confirmed or disconfirmed, all events with the matching type are confirmed/disonfirmed
/// 
/// </summary>
public enum likely_type { known, player_attack, player_death, player_heal, enemy_death, enemy_heal }

/// <summary>
/// PreGLAM is the emotion engine implemented in Galactic Defense.
/// </summary>
public class PreGLAM : MonoBehaviour
{
    public static float emotion_trail { get; private set; } = 90.0f;
    public static float event_arousal = 0.5f;
    public AffectVariables affectVariables;
    List<Emotion_event> events;
    List<Emotion_event> to_cull;
    List<Emotion_event> to_spawn;

    float valence, arousal, tension;
    float mood_valence, mood_arousal, mood_tension;

    bool is_active = false;

    public void Start_PreGLAM()
    {
        events = new List<Emotion_event>();
        to_cull = new List<Emotion_event>();
        to_spawn = new List<Emotion_event>();
        Begin_Emotion_Process();
        Begin_Emotion_Log();
        Debug.Log("PreGLAM Started");
    }

    public void Set_Mood(int stage)
    {
        var mood = affectVariables.stage1Mood;
        if (stage == 2)
        {
            mood = affectVariables.stage2Mood;
        }
        else if (stage == 3)
        {
            mood = affectVariables.stage3Mood;
        }

        mood_valence = mood.x;
        mood_arousal = mood.y;
        mood_tension = mood.z;
    }

    public void Begin_Emotion_Log()
    {
        StartCoroutine(Log_Emotions());
    }

    public void Begin_Emotion_Process()
    {
        is_active = true;
        StartCoroutine(Process_events());
    }

    public void End_Emotion_Process()
    {
        is_active = false;
        events = new List<Emotion_event>();
    }

    public void Queue_event(Emotion_event _event)
    {
        _event.SetGLAM(this); //Set reference, so known events can create their confirmed followers
        to_spawn.Add(_event);

    }

    public void Queue_cull(Emotion_event _event)
    {
        if (events.Contains(_event))
        {
            to_cull.Add(_event);
        }
    }

    public void Queue_prospective_cull(likely_type type)
    {
        //After a bunch of checks, cull all events of provided type
        if (type != likely_type.known)
        {
            foreach (Emotion_event _event in events)
            {
                if (_event is Likely_event)
                {
                    if (_event.Get_Likely_Type() == type)
                    {
                        to_cull.Add(_event);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Tried to cull known events");
        }
    }

    public void Queue_prospective_cull()
    {
        foreach (Emotion_event _event in events)
        {
            if (_event is Likely_event || _event is Known_event)
            {
                Queue_cull(_event);
            }
        }
    }

    void Cull_events()
    {
        foreach(Emotion_event _event in to_cull)
        {
            if (events.Contains(_event))
            {
                events.Remove(_event);
            }
            else
            {
                Debug.LogWarning("Culled nonexistent event");
            }
        }
        to_cull = new List<Emotion_event>();
    }

    /// <summary>
    /// Spawns events
    /// </summary>
    void Spawn_events()
    {
        foreach(Emotion_event _event in to_spawn)
        {
            events.Add(_event);
        }
        to_spawn = new List<Emotion_event>();
    }

    /// <summary>
    /// Process events sends timer to every event, and 
    /// </summary>
    IEnumerator Process_events()
    {
        while (is_active)
        {
            //Spawns any new events since last loop
            Spawn_events();

            //Ticks events
            tickEvents(Time.deltaTime);

            //Computes affect values for VAT model
            var affect_values = Compute_Affect_Value();
            valence = affect_values.x;
            arousal = affect_values.y;
            tension = affect_values.z;

            //Culls any events that expired this loop
            Cull_events();

            yield return null;
        }
    }

    IEnumerator Log_Emotions()
    {
#if UNITY_EDITOR
        while (true)
        {
            Grapher.Log(valence, "Valence");
            Grapher.Log(arousal, "Arousal");
            Grapher.Log(tension, "Tension");
            //Debug.Log("Output VAT: [" + valence + "," + arousal + "," + tension + "]");
            yield return new WaitForSecondsRealtime(0.25000f);
        }
#endif
    }

    Vector3 Compute_Affect_Value()
    {
        //Values begin at mood levels
        var valence_value = mood_valence;
        var arousal_value = mood_arousal;
        var tension_value = mood_tension;

        //We add the scaled valence, arousal, and tension values to the mood values
        foreach (Emotion_event _event in events)
        {
            valence_value += _event.Valence();
            arousal_value += _event.Arousal();
            tension_value += _event.Tension();
        }

        //We then return the values
        return new Vector3(valence_value, arousal_value, tension_value);
    }

    void tickEvents(float deltaTime)
    {
        foreach (Emotion_event _event in events)
        {
            _event.Tick(deltaTime);
        }

    }

    public string VATLevel()
    {
        var valence_level = level(valence);
        var arousal_level = level(arousal);
        var tension_level = level(tension);

        var return_string = valence_level + "-" + arousal_level + "-" + tension_level;
        if (is_strong_music_change()) { return_string += "-2"; }
        return return_string;
    }

    bool is_strong_music_change()
    {
        var threshold = 45.0f;

        if (Mathf.Abs(valence) > threshold) { return true; }
        else if (Mathf.Abs(arousal) > threshold) { return true; }
        else if (Mathf.Abs(tension) > threshold) { return true; }
        else return false;
    }

    string level(float value)
    {
        if (value < -15.0f) { return "Low"; }
        else if (value < 15.0f) { return "Medium"; }
        else return "High";
    }
}

//Events can be ticked, and can be polled for values
public interface Emotion_event
{
    public void SetGLAM(PreGLAM GLAM);
    public void Tick(float deltaTime);
    public float Valence();
    public float Arousal();
    public float Tension();
    public likely_type Get_Likely_Type();

}

public class Past_event : Emotion_event
{
    protected PreGLAM GLAM;
    protected float valence;//Full valence value
    protected float arousal;
    protected float modifier;//Modifier
    protected float remaining_time;//Time remaining in event, for calculating modifier and event end
    protected float timer_percent;

    //Past events have an associated valenced reaction, adjusted by the modifier
    public Past_event(float _valence, float _modifier, PreGLAM _GLAM)
    {
        GLAM = _GLAM;
        valence = _valence;
        modifier = _modifier;

        remaining_time = PreGLAM.emotion_trail;
        arousal = PreGLAM.event_arousal;
    }
    public void SetGLAM(PreGLAM _GLAM) { GLAM = _GLAM; }

    //Past event ticks decrease strength over time, until the event has completely faded
    public void Tick(float deltaTime)
    {
        remaining_time -= deltaTime;
        if (remaining_time <= 0.0f)
        { //If we're fully faded, cull the event
            GLAM.Queue_cull(this);
        }
        else
        {
            timer_percent = remaining_time / PreGLAM.emotion_trail;
        }
    }

    //Valence returns the valence value as modified by modifier and time
    public float Valence()
    {
        return valence * modifier * timer_percent;
    }
    //Arousal is just modified by time
    public float Arousal()
    {
        return arousal * timer_percent;
    }
    //By definition, past events do not have tension values
    public float Tension()
    {
        return 0;
    }
    //If we know the type, it's just known
    public likely_type Get_Likely_Type()
    {
        return likely_type.known;
    }
}

public class Known_event : Emotion_event
{
    protected PreGLAM GLAM;
    protected float valence;//Full valence value
    protected float arousal;
    protected float tension;
    protected float modifier;//Modifier
    protected float time_until; //Total time until event
    protected float remaining_time_until;//Time remaining until event occurs
    protected float timer_percent;

    public Known_event(float _valence, float _tension, float _modifier, float time)
    {
        valence = _valence;
        tension = _tension;
        arousal = PreGLAM.event_arousal;
        modifier = _modifier;
        time_until = time;
        remaining_time_until = time;
    }

    public void SetGLAM(PreGLAM _GLAM) { GLAM = _GLAM; }

    public void Tick(float deltaTime)
    {
        remaining_time_until -= deltaTime;
        if (remaining_time_until <= 0.0f)
        { //If we've arrived, cull the event and create a new one
            GLAM.Queue_event(new Past_event(valence, modifier, GLAM));
            GLAM.Queue_cull(this);
        }
        else
        {
            timer_percent = remaining_time_until / time_until;
        }
    }

    //Valence returns the valence value as modified by modifier and time
    public float Valence()
    {
        return valence * modifier * timer_percent;
    }
    //Arousal is just modified by time
    public float Arousal()
    {
        return arousal * timer_percent;
    }
    //Tension is also modified by modifier and time
    public float Tension()
    {
        return tension * modifier * timer_percent;
    }
    public likely_type Get_Likely_Type()
    {
        return likely_type.known;
    }
}

public class Likely_event : Emotion_event
{
    protected PreGLAM GLAM;
    protected float valence;//Full valence value
    protected float arousal;
    protected float tension;
    protected float modifier;//Modifier
    protected float time_until;
    protected float remaining_time_until;//Time remaining in event, for calculating modifier and event end
    protected float timer_percent;
    likely_type type; //Stores which type of likely event this is, since we don't cull ourselves

    public Likely_event(float _valence, float _tension, float _modifier, float time, likely_type _type)
    {
        valence = _valence;
        tension = _tension;
        arousal = PreGLAM.event_arousal;
        modifier = _modifier;
        time_until = time;
        remaining_time_until = time;
        type = _type;
    }
    public void SetGLAM(PreGLAM _GLAM) { GLAM = _GLAM; }

    //Likely events don't have to worry about being culled by time, because they just go forever if un-culled by type.
    public void Tick(float deltaTime)
    {
        remaining_time_until -= deltaTime;
        timer_percent = remaining_time_until / time_until;
    }

    //Valence returns the valence value as modified by modifier and time
    public float Valence()
    {
        return valence * modifier * timer_percent;
    }
    //Arousal is just modified by time
    public float Arousal()
    {
        return arousal * timer_percent;
    }
    //Tension is also modified by modifier and time
    public float Tension()
    {
        return tension * modifier * timer_percent;
    }

    public likely_type Get_Likely_Type()
    {
        return type;
    }
}


