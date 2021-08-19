using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MusicExplorer : MonoBehaviour
{
    private OrdinalAffect valence, arousal, tension;
    private string VAT = "Mid-Mid-Mid";
    public EliasPlayer music;

    public TMP_Dropdown valenceMenu, arousalMenu, tensionMenu;

    private string ConvertToString(OrdinalAffect affect)
    {
        if (affect == OrdinalAffect.low || affect==OrdinalAffect.low2)
        {
            return "Low";
        }
        else if (affect == OrdinalAffect.high || affect==OrdinalAffect.high2)
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

        //If any strong affect expression, go to version 2. All weak affect expressions go to 1
        var musicLevel = ConvertToString(valence) + "-" + ConvertToString(arousal) + "-" + ConvertToString(tension);
        if (strongAffect(valence, arousal, tension))
        {
            musicLevel += "-2";
            //Debug.Log("Music Level = " + musicLevel);
        }

        return musicLevel;
    }

    private bool strongAffect(OrdinalAffect _valence, OrdinalAffect _arousal, OrdinalAffect _tension)
    {
        var allAffects=new List<OrdinalAffect>(){ _valence, _arousal, _tension};
        if(allAffects.Contains(OrdinalAffect.high2) || allAffects.Contains(OrdinalAffect.low2))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Test(bool testVal)
    {
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
            //Debug.Log(VAT);
            var VATLevels = GetMusicLevel();
            if (VAT != VATLevels)
            {
                VAT = VATLevels;
                //Debug.Log(VAT);
                music.RunActionPreset(VAT);
            }
            yield return null;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        //music = GetComponent<EliasPlayer>();
        StartCoroutine(AffectExplore());
        //SetGuitarSolo(false);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}