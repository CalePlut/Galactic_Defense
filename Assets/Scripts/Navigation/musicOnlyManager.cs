using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class musicOnlyManager : MonoBehaviour
{
    //Elias script
    private EliasPlayer elias;

    public EliasSetLevel setLevel;
    public EliasSetLevelOnTrack valenceTrack;
    public EliasSetLevelOnTrack arousalTrack;
    public EliasSetLevelOnTrack tensionTrack;

    public SliderText arousalText, valenceText, tensionText;

    //Affective dimensions that feed directly to Elias
    private int valence = 0;

    private int arousal = 0;
    private int tension = 0;

    private void Awake()
    {
        elias = GetComponent<EliasPlayer>();
    }

    public void updateEliasValence(float _valence)
    {
        valence = (int)_valence / 10;
        if (_valence > 0)
        {
            valenceTrack.level = valence;
            elias.QueueEvent(valenceTrack.CreateSetLevelOnTrackEvent(elias.Elias));
        }

        valenceText.updateValue((int)_valence);
    }

    public void updateEliasArousal(float _arousal)
    {
        arousal = (int)_arousal / 10;

        if (arousal > 0)
        {
            arousalTrack.level = arousal;
            elias.QueueEvent(arousalTrack.CreateSetLevelOnTrackEvent(elias.Elias));
        }
        arousalText.updateValue((int)_arousal);
        //This is where we'll run action presets that change on arousal
    }

    public void updateEliasTension(float _tension)
    {
        tension = (int)_tension / 10;

        if (tension > 0)
        {
            tensionTrack.level = tension;
            elias.QueueEvent(tensionTrack.CreateSetLevelOnTrackEvent(elias.Elias));
        }
        tensionText.updateValue((int)_tension);
        //This is where we'll run action presets that change on tension
    }

    public void mainMenu()
    {
        StartCoroutine(loadingScreen("Menu"));
    }

    private IEnumerator loadingScreen(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        while (!asyncLoad.isDone) { yield return null; }
    }
}