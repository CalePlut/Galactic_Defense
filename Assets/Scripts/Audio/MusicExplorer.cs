using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MusicExplorer : MonoBehaviour
{
    private OrdinalAffect valence, arousal, tension;
    private string VAT = "Mid-Mid-Mid";
    private EliasPlayer music;

    public TMP_Dropdown valenceMenu, arousalMenu, tensionMenu;

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

    public void CheckValue(TMP_Dropdown menu, ref OrdinalAffect affect)
    {
        var value = menu.value;
        if (affect != (OrdinalAffect)value)
        {
            affect = (OrdinalAffect)value;
        }
    }

    public string GetMusicLevel()
    {
        CheckValue(valenceMenu, ref valence);
        CheckValue(arousalMenu, ref arousal);
        CheckValue(tensionMenu, ref tension);

        var musicLevel = ConvertToString(valence) + "-" + ConvertToString(arousal) + "-" + ConvertToString(tension);
        return musicLevel;
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

    /// <summary>
    /// Evaluates and updates music as it changes
    /// </summary>
    /// <returns></returns>
    private IEnumerator AffectExplore()
    {
        while (gameObject.activeSelf)
        {
            var VATLevels = GetMusicLevel();
            if (VAT != VATLevels)
            {
                VAT = VATLevels;
                music.RunActionPreset(VAT);
            }
            yield return null;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        music = GetComponent<EliasPlayer>();
        StartCoroutine(AffectExplore());
        SetGuitarSolo(false);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}