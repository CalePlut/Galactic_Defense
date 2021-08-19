using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class targetAnchor : MonoBehaviour
{
    public AnimationCurve moveCurve;
    Vector3 startPos;


    public void targetBegin(float time)
    {
        StartCoroutine(targetLock(time));
    }

    private void Start()
    {
        startPos = transform.localPosition;
    }


    IEnumerator targetLock(float time)
    {
        var timer = 0.0f;
        while (timer < time)
        {
            timer += Time.deltaTime;
            var scaledTime = timer/time; //Scaled between 0 and 1
            var curveTime = moveCurve.Evaluate(scaledTime);
            transform.localPosition = Vector3.Lerp(startPos, Vector3.zero, curveTime);
            yield return null;
        }

        transform.position = startPos;
    }
}
