using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public GameObject cameraObj;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (cameraObj == null)
        {
            cameraObj = GameObject.Find("MainCamera");
        }
        transform.LookAt(cameraObj.transform.position);
    }
}