using UnityEngine;
using UnityEngine.UI;

public class mapManager : MonoBehaviour
{
    public GameObject exitButton;
    public Toggle stage1, stage2;

    public void advanceStage(int stage)
    {
        var exitRect = exitButton.GetComponent<RectTransform>();
        if (stage == 2)
        {
            stage1.isOn = true;
            exitRect.localPosition = new Vector3(0, 0);
        }
        if (stage == 3)
        {
            stage1.isOn = true;
            stage2.isOn = true;
            exitRect.localPosition = new Vector3(250, 0);
        }
    }
}