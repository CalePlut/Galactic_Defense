using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class buttonPressMonitor : MonoBehaviour
{
    public InputAction press;

    private void OnEnable()
    {
        press.Enable();
    }

    private void OnDisable()
    {
        press.Disable();
    }

    private void Update()
    {
        if (press.triggered)
        {
            GetComponent<Button>().onClick.Invoke();
        }
    }
}