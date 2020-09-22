using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class comboTracker : MonoBehaviour
{
    public Sprite combo1, combo2, combo3, combo4;
    public Image combo;

    public void SetCombo(int comboCount)
    {
        if (comboCount == 1)
        {
            combo.sprite = combo1;
        }
        else if (comboCount == 2)
        {
            combo.sprite = combo2;
        }
        else if (comboCount == 3)
        {
            combo.sprite = combo3;
        }
        else
        {
            combo.sprite = combo4;
        }
    }
}