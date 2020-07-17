using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderText : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI text;
    public string prepend, append; //Prepend and Append an optional string to the text

    public void updateValue(int value)
    {
        slider.value = value;
        text.text = prepend + value + "/" + slider.maxValue + append;
    }

    public void updateMaxValue(int maxVal)
    {
        slider.maxValue = maxVal;
        text.text = prepend + slider.value + "/" + slider.maxValue + append;
    }
}