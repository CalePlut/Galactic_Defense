using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class AffectBarText : MonoBehaviour
{
    public Toggle Low_Medium, Medium, Medium_High, High;
    // public TextMeshProUGUI text;
    //public string prepend, append; //Prepend and Append an optional string to the text

    [Range(1, 5)]
    public int Affect;

    private OrdinalAffect currentAffect;

    private void Start()
    {
        currentAffect = OrdinalAffect.low;
        Medium.isOn = false;
        High.isOn = false;
    }

    /// <summary>
    /// Updates affect bar given incoming affect. This only changes if the affect has changed, so just call it every frame.
    /// </summary>
    /// <param name="affect"></param>
    public void UpdateAffect(OrdinalAffect affect)
    {
        //If we've changed affect, let's consider our bars
        if (affect != currentAffect)
        {
            if (affect == OrdinalAffect.low2)
            {
                Low_Medium.isOn = false;
                Medium.isOn = false;
                Medium_High.isOn = false;
                High.isOn = false;
            }
            else if (affect == OrdinalAffect.low)
            {
                Low_Medium.isOn = true;
                Medium.isOn = false;
                Medium_High.isOn = false;
                High.isOn = false;
            }
            else if (affect == OrdinalAffect.medium)
            {
                Low_Medium.isOn = true;
                Medium.isOn = true;
                Medium_High.isOn = false;
                High.isOn = false;
            }
            else if (affect == OrdinalAffect.high)
            {
                Low_Medium.isOn = true;
                Medium.isOn = true;
                Medium_High.isOn = true;
                High.isOn = false;
            }
            else if (affect == OrdinalAffect.high2)
            {
                Low_Medium.isOn = true;
                Medium.isOn = true;
                Medium_High.isOn = true;
                High.isOn = true;
            }
            currentAffect = affect;
        }
    }
}