using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonKeyImageManager : MonoBehaviour
{
    public InputAction KBAM;
    public InputAction Controller;

    public Sprite kbamPrompt;
    public Sprite controllerPrompt;

    private void Start()
    {
        KBAM.Enable();
        Controller.Enable();
    }

    private void OnEnable()
    {
        KBAM.Enable();
        Controller.Enable();
    }

    private void OnDisable()
    {
        KBAM.Disable();
        Controller.Disable();
    }

    private void Update()
    {
        if (KBAM.triggered)
        {
            GetComponent<Image>().sprite = kbamPrompt;
        }
        if (Controller.triggered)
        {
            GetComponent<Image>().sprite = controllerPrompt;
        }
    }
}