using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menuManager : MonoBehaviour
{
    public GameObject loadPanel;

    public void Play()
    {
        StartCoroutine(loadingScreen("ATB"));
    }

    public void quitGame()
    {
        Application.Quit();
    }

    public void musicOnly()
    {
        StartCoroutine(loadingScreen("MusicOnly"));
    }

    public void setTut(bool _tut)
    {
        if (_tut) { GameManager.tutorial = true; }
        else { GameManager.tutorial = false; }
    }

    private IEnumerator loadingScreen(string sceneName)
    {
        loadPanel.SetActive(true);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        while (!asyncLoad.isDone) { yield return null; }
    }
}