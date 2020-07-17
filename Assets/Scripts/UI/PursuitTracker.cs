using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PursuitTracker : MonoBehaviour
{
    public TextMeshProUGUI encounterText;
    public TextMeshProUGUI waveText;
    public Slider encounterSlider;

    public void waveSetup(int _wave, int _encounters)
    {
        waveText.text = "Wave " + _wave + "/4";
        encounterText.text = "Encounter 0/" + _encounters;
        encounterSlider.maxValue = _encounters;
        encounterSlider.value = 0;
    }

    public void updateEncounter(int encounter)
    {
        encounterText.text = "Encounter " + encounter + "/" + encounterSlider.maxValue;
        StartCoroutine(sliderFill((float)encounter));
    }

    private IEnumerator sliderFill(float destination)
    {
        while (encounterSlider.value < destination)
        {
            encounterSlider.value += Time.deltaTime;
            yield return null;
        }
        encounterSlider.value = destination;
    }

    //public void addPursuit()
    //{
    //    encounterSlider.value++;
    //    text.text = encounterSlider.value + "/" + encounterSlider.maxValue;
    //}
    //public void nextLevel(int toNextLevel)
    //{
    //    encounterSlider.value = 0;
    //    encounterSlider.maxValue = toNextLevel;
    //    text.text = encounterSlider.value + "/" + encounterSlider.maxValue;
    //}
}