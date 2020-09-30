using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class comboTracker : MonoBehaviour
{
    public Sprite basicAttack, bothFireAttack;
    public Image combo;

    public void SetCombo(bool fullFire)
    {
        if (!fullFire)
        {
            combo.sprite = basicAttack;
        }
        else
        {
            combo.sprite = bothFireAttack;
        }
    }
}