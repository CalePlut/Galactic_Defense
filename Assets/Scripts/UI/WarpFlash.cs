using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WarpFlash : MonoBehaviour
{
    private Image flashScreen;

    public float flashTime;

    bool flashing = false;

    void Awake()
    {
        flashScreen = GetComponent<Image>();
    }

    public void flash()
    {
        if (!flashing)
        {
            flashing = true;
            //StartCoroutine(singleFlash(flashTime));
        }

    }

    private IEnumerator singleFlash(float duration)
    {
        var remainingTime = duration;
        var maxTime = duration;
        while (remainingTime > 0.0f)
        {
            var col = Color.white;
            col.a = remainingTime / maxTime;
            flashScreen.color = col;
            remainingTime -= Time.deltaTime;
            yield return null;
        }
        flashing = false;
    }
}