using UnityEngine;

public class ColorManager : MonoBehaviour
{
    [ColorHtmlProperty]
    public Color shieldMain, shieldDark, shieldLight;

    [ColorHtmlProperty]
    public Color crewMain, crewDark, crewLight;

    [ColorHtmlProperty]
    public Color engineMain, engineDark, engineLight;

    [ColorHtmlProperty]
    public Color sensorMain, sensorDark, sensorLight;
}