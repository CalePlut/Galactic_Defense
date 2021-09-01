using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MusicExplorer : MonoBehaviour
{
    public EliasPlayer music;
    public Music_Slider valence, arousal, tension;

    public void Set_Music_Level()
    {
        var valence_level = valence.level;
        var arousal_level = arousal.level;
        var tension_level = tension.level;

        var music_string = string.Format("{0}-{1}-{2}", valence_level, arousal_level, tension_level);

        if(valence.strong || arousal.strong || tension.strong) { music_string += "-2"; }
        Debug.Log("Calling action preset " + music_string);


        music.RunActionPreset(music_string);
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

    public void Level_Change()
    {
        Set_Music_Level();
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}