using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class mapManager : MonoBehaviour
{
    public GameObject currentLocationIndicator;
    public Toggle stage1, stage2;
    public TextMeshProUGUI buttonText;

    public void advanceStage(int stage)
    {
        var indicatorRect = currentLocationIndicator.GetComponent<RectTransform>();
        if (stage == 2)
        {
            buttonText.text = "Begin Stage 2";
            stage1.isOn = true;
            indicatorRect.localPosition = new Vector3(0, indicatorRect.localPosition.y);
        }
        if (stage == 3)
        {
            buttonText.text = "Begin Stage 3";
            stage1.isOn = true;
            stage2.isOn = true;
            indicatorRect.localPosition = new Vector3(250, indicatorRect.localPosition.y);
        }
    }
}