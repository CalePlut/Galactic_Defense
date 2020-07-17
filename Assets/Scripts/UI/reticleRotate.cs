using UnityEngine;

public class reticleRotate : MonoBehaviour
{
    public bool rotate;
    public float speed;

    public void startRotate(float _speed)
    {
        speed = _speed;
        rotate = true;
    }

    public void stopRotate()
    {
        rotate = false;
    }

    private void Update()
    {
        if (rotate)
        {
            transform.Rotate(0, 0, speed * Time.deltaTime);
        }
    }
}