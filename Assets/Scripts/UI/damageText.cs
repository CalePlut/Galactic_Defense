using UnityEngine;

public class damageText : MonoBehaviour
{
    public GameObject textFloat;

    private float damageSum;
    private float sumTimer;
    public float sumTime = 1.0f;
    private textFloat sumText;

    public void TakeDamage(int damage, float damagePercent, bool shield)
    {
        var col = Color.red;
        if (shield) { col = Color.blue; }
        //If we aren't currently summing, create new text
        if (sumTimer <= 0.0f)
        {
            damageSum = damage;
            sumTimer = 1.0f;

            var floatObject = GameObject.Instantiate(textFloat, Vector3.zero, Quaternion.identity, this.transform);
            sumText = floatObject.GetComponent<textFloat>();
            sumText.floatText("-", damage, damagePercent, col);
        }
        else
        {
            sumTimer += 0.5f;
            damageSum += damage;
            sumText.changeText("-", damageSum.ToString());
        }
    }

    public void receiveHealing(int healing, float healPercent)
    {
        var floatObject = GameObject.Instantiate(textFloat, this.transform);
        var floatScript = floatObject.GetComponent<textFloat>();

        floatScript.floatText("+", healing, healPercent, Color.green);
    }

    private void Update()
    {
        sumTimer -= Time.deltaTime;
    }
}