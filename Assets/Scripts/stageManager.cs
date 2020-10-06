using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class stageManager : MonoBehaviour
{
    //public Toggle s1e1, s1e2, s2e1, s2e2, s2e3, s3e1, s3e2, s3e3, s3e4;
    //public GameObject stage1Toggles, stage2Toggles, stage3Toggles;

    public TextMeshProUGUI stageText;

    public GameObject Wave1, Wave2, Wave3;
    private GameObject waveUI;
    private List<Toggle> encounterToggles;

    /// <summary>
    /// Creates the stage progress bar with the number of encounters in the wave
    /// </summary>
    /// <param name="length">Number of encoutners in the wave</param>
    public void setWaveLength(int length)
    {
        if (length == 1)
        {
            waveUI = Instantiate(Wave1, transform.position, Quaternion.identity, this.transform);
        }
        else if (length == 2)
        {
            waveUI = Instantiate(Wave2, transform.position, Quaternion.identity, this.transform);
        }
        else
        {
            waveUI = Instantiate(Wave3, transform.position, Quaternion.identity, this.transform);
        }

        encounterToggles = new List<Toggle>(waveUI.GetComponentsInChildren<Toggle>());
    }

    /// <summary>
    /// Fills in UI for filling in progress bubbles
    /// </summary>
    /// <param name="progress">The most recently defeated encounter number</param>
    public void setEncounterProgress(int progress)
    {
        for (int i = 0; i < progress; i++)
        {
            try
            {
                encounterToggles[progress].isOn = true;
            }
            catch { Debug.Log("Encoutner Toggles " + progress + "called, but encounter toggles only has " + encounterToggles.Count); }
        }
    }

    /// <summary>
    /// Deletes UI object to prepare for next wave
    /// </summary>
    public void RemoveUI()
    {
        Destroy(waveUI);
        stageText.text = "";

        encounterToggles = new List<Toggle>();
    }

    /// <summary>
    /// Sets the stage text
    /// </summary>
    /// <param name="_text">Current wave and encounter</param>
    public void setText(string _text)
    {
        stageText.text = _text;
    }

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }
}