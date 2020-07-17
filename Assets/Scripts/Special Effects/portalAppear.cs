using System.Collections;
using UnityEngine;

public class portalAppear : MonoBehaviour
{
    public float speed = 10.0f;
    public float duration = 2.5f;

    public void warp(float _size)
    {
        StartCoroutine(warpIn(_size));
    }

    private IEnumerator warpIn(float _size)
    {
        var scale = Vector3.zero;
        var scalar = 0.0f;
        transform.localScale = scale;
        while (scalar < _size)
        {
            scalar += speed * Time.deltaTime;
            scale = new Vector3(scalar, scalar, scalar);
            transform.localScale = scale;
            yield return null;
        }
        yield return new WaitForSeconds(duration);
        while (scalar > 0.0f)
        {
            scalar -= speed * Time.deltaTime;
            scale = new Vector3(scalar, scalar, scalar);
            transform.localScale = scale;
            yield return null;
        }

        Destroy(this.gameObject);
    }
}