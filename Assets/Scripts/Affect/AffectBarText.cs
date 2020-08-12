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
            if (affect == OrdinalAffect.low)
            {
                Medium.isOn = false;
                High.isOn = false;
            }
            else if (affect == OrdinalAffect.medium)
            {
                Medium.isOn = true;
                High.isOn = false;
            }
            else if (affect == OrdinalAffect.high)
            {
                Medium.isOn = true;
                High.isOn = true;
            }
            currentAffect = affect;
        }
    }
}