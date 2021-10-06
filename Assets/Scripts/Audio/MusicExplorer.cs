using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MusicExplorer : MonoBehaviour
{
    public EliasPlayer music;
    public Music_Slider valence, arousal, tension;
    Condition condition = Condition.Generative;
    string prev_string;

    public void Set_Music_Level()
    {
        var valence_level = valence.level;
        var arousal_level = arousal.level;
        var tension_level = tension.level;
        
        var music_string = string.Format("{0}-{1}-{2}", valence_level, arousal_level, tension_level);

        switch (condition)
        {
            case Condition.None:
                break;
            case Condition.Linear:
                break;
            case Condition.Adaptive:
                music_string = string.Format("Adaptive_{0}-{1}-{2}", valence_level, arousal_level, tension_level);
                if (prev_string != music_string)
                {
                    music.RunActionPreset(music_string);
                    prev_string = music_string;
                }
                if (is_strong()) { music_string += "-2"; }
                music.RunActionPreset(music_string);
                break;
            case Condition.Generative:
                music_string = string.Format("{0}-{1}-{2}", valence_level, arousal_level, tension_level);
                if (prev_string != music_string)
                {
                    music.RunActionPreset(music_string);
                    prev_string = music_string;
                }
                if (is_strong()) { music_string += "-2"; }
                music.RunActionPreset(music_string);
                break;
        }
        Debug.Log("Music string: " + music_string);
    }

    bool is_strong()
    {
        Debug.Log(string.Format("V-Strong: {0}, A-strong: {1}, T_Strong: {2}", valence.strong, arousal.strong, tension.strong));
        if (valence.strong || arousal.strong || tension.strong) { return true; }
        else return false;

    }

    public void SetGuitarSolo(bool solo)
    {
        if (solo)
        {
            music.RunActionPreset("UnmuteGuit");
        }
        else
        {
            music.RunActionPreset("MuteGuit");
        }
    }

    public void SetCondition(int _condition)
    {
        switch (_condition)
        {
            case 0:
                condition = Condition.Generative;
                music.RunActionPreset("StartCombat");
                break;
            case 1:
                condition = Condition.Adaptive;
                music.RunActionPreset("Adaptive_StartCombat");
                break;
            case 2:
                condition = Condition.Linear;
                music.RunActionPreset("Linear_StartCombat");
                break;
            case 3:
                condition = Condition.None;
                break;
            default:
                Debug.Log("Nonexistent condition");
                break;
        }
    }


    public void Level_Change()
    {
        Set_Music_Level();
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }


}