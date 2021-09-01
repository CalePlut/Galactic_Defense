using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class Music_Slider : MonoBehaviour
{
    Slider mySlider;
    public TextMeshProUGUI myText;
    public MusicExplorer explorer;
    public string level { get; private set; } = "Mid";
    public bool strong { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        mySlider = GetComponent<Slider>();
    }

    public void Set_Value(float _value)
    {
        var value = (int)_value;
        switch (value)
        {
            case 0:
                level = "Low";
                strong = true;
                break;
            case 1:
                level = "Low";
                strong = false;
                break;
            case 2:
                level = "Mid";
                strong = false;
                break;
            case 3:
                level = "High";
                strong = false;
                break;
            case 4:
                level = "High";
                strong = true;
                break;
            default:
                Debug.Log("Affect value out of range");
                break;
        }
        myText.text = level;
        explorer.Level_Change();
    }
}