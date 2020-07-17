using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WarpFlash : MonoBehaviour
{
    private Image flashScreen;

    public float flashTime;

    private void Start()
    {
        flashScreen = GetComponent<Image>();
    }

    public void flash()
    {
        StartCoroutine(singleFlash(flashTime));
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
    }
}