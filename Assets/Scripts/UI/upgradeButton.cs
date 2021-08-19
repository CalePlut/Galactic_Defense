using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class upgradeButton : MonoBehaviour
{
    public TextMeshProUGUI Title, Description;

    public void SetInfo(Upgrade upgrade)
    {
        Title.text = upgrade.Name();
        Description.text = upgrade.Description();
    }
}
