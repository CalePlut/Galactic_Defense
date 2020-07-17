using TMPro;
using UnityEngine;

public class missileCounter : MonoBehaviour
{
    public TextMeshProUGUI text;

    public void setMissileCount(int count)
    {
        text.text = count.ToString();
    }
}