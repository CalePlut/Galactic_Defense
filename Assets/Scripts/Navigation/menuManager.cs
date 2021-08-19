using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menuManager : MonoBehaviour
{
    public GameObject loadPanel;

    public GameObject button_Holder;

    public void Play(bool tutorial)
    {
        GameManager.tutorial = tutorial;
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
        button_Holder.SetActive(false);
        loadPanel.SetActive(true);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        while (!asyncLoad.isDone) { yield return null; }
    }

    private void Start()
    {
        var music = GetComponent<EliasPlayer>();

        music.StartEliasWithActionPreset("Menu");
    }
}