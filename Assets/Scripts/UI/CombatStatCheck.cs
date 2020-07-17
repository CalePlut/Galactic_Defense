using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatStatCheck : MonoBehaviour
{
    public GameObject reactor, sensors, engine;
    private List<int> comboDamage = new List<int> { 0, 1, 2, 4, 6 };
    public TextMeshProUGUI alphaText, missileText;
    public Toggle shieldStatus;

    public void disableCheck(int _reactor, int _sensors, int _engine)
    {
        reactor.SetActive(true);
        sensors.SetActive(true);
        engine.SetActive(true);
        if (_reactor <= 0)
        {
            reactor.SetActive(false);
        }
        if (_sensors <= 0) { sensors.SetActive(false); }
        if (_engine <= 0) { engine.SetActive(false); }
    }

    public void setValues(int _reactor, int lockOn)
    {
        alphaText.text = damage(_reactor, lockOn, false) + "(<color=#AA9639>" + _reactor + "</color>*<color=#7D9F35>" + damageMultiplier(lockOn) + "</color>)";
        missileText.text = lockOn + "/3";
    }

    private int damageMultiplier(int lockOn)
    {
        var comboPoints = lockOn + 1;
        return comboDamage[comboPoints];
    }

    public void setOvershield(bool shield)
    {
        shieldStatus.isOn = shield;
    }

    private int damage(int baseDamage, int lockOn, bool shielded)
    {
        var comboPoints = lockOn + 1; //Base damage is 1, each point in lockOn increases the comboDamage multiplier
        if (shielded && lockOn < 4) { lockOn--; } //Shielded reduces the combo multiplier by one unless there is a full combo - lockOn of 3 (4 with previous step) will always result in 6x multiplier
        var damageMultiplier = comboDamage[comboPoints];
        return baseDamage * damageMultiplier;
    }
}