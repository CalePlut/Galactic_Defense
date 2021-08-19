using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class mapManager : MonoBehaviour
{
    //public GameObject currentLocationIndicator;
    public Toggle stage1, stage2;
    public GameObject stage1_highlight, stage2_highlight, stage3_higlight;
    public TextMeshProUGUI buttonText;

    public void advanceStage(int stage)
    {
        if (stage == 2)
        {
            buttonText.text = "Begin Stage 2";
            stage1.isOn = true;
            stage1_highlight.SetActive(false);
            stage2_highlight.SetActive(true);
        }
        if (stage == 3)
        {
            buttonText.text = "Begin Stage 3";
            stage1.isOn = true;
            stage2.isOn = true;
            stage2_highlight.SetActive(false);
            stage3_higlight.SetActive(true);
        }
    }
}