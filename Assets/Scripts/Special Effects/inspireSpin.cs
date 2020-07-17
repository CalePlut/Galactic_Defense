using UnityEngine;

public class inspireSpin : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(Vector3.forward, 500.0f * Time.deltaTime);
    }
}