using UnityEngine;

public class damageText : MonoBehaviour
{
    public GameObject textFloat;

    public void takeDamage(int damage, float damagePercent)
    {
        var floatObject = GameObject.Instantiate(textFloat, Vector3.zero, Quaternion.identity, this.transform);
        var floatScript = floatObject.GetComponent<textFloat>();

        floatScript.floatText(damage, damagePercent, Color.red, "-");
    }

    public void receiveHealing(int healing, float healPercent)
    {
        var floatObject = GameObject.Instantiate(textFloat, this.transform);
        var floatScript = floatObject.GetComponent<textFloat>();

        floatScript.floatText(healing, healPercent, Color.green, "+");
    }
}