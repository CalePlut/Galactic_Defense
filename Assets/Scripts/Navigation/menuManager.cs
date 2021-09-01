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

    public void Set_Condition(int condition)
    {
        switch (condition)
        {
            case 0:
                GameManager.condition = Condition.Generative;
                break;
            case 1:
                GameManager.condition = Condition.Adaptive;
                break;
            case 2:
                GameManager.condition = Condition.Linear;
                break;
            case 3:
                GameManager.condition = Condition.None;
                break;
            default:
                Debug.Log("Condition out of range");
                break;
        }
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
