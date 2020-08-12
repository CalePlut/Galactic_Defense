using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class AffectBarText : MonoBehaviour
{
    public Toggle Medium, High;
    // public TextMeshProUGUI text;
    //public string prepend, append; //Prepend and Append an optional string to the text

    [Range(1, 3)]
    public int Affect;

    int myAffect;

    private void Start()
    {
        Affect = 1;
        Medium.isOn = false;
        High.isOn = false;
    }
    private void Update()
    {
        if (Affect != myAffect)
        {
            if (Affect == 1)
            {
                if (Medium.isOn) { Medium.isOn = false; }
                if (High.isOn) { High.isOn = false; }
            }
            if (Affect == 2)
            {
                if (!Medium.isOn) { Medium.isOn = true; }
                if (High.isOn) { High.isOn = false; }
            }
            if (Affect == 3)
            {
                if (!Medium.isOn) { Medium.isOn = true; }
                if (!High.isOn) { High.isOn = true; }
            }
        }
    }

}