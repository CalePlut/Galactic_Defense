using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class healthBarAnimator : MonoBehaviour
{
    private Slider healthBar;
    public TextMeshProUGUI healthText;
    public AnimationCurve valueChange;
    public Image fill;
    public float adjustSpeed = 5.0f;
    private Color fillCol;

    public bool textDisplay = true;

    public int max { get; private set; }
    public int health { get; private set; }

    private void Awake()
    {
        healthBar = GetComponent<Slider>();
        fillCol = fill.color;
    }

    public void Refresh(int _max, int _value)
    {
        max = _max;
        health = _value;
        healthBar.maxValue = _max;
        healthBar.value = _value;

        setHealthText(health);
        setHealthColor(_value);
    }

    public void deActivate()
    {
        gameObject.SetActive(false);
    }

    private void setValue(int _value)
    {
        healthBar.value = _value;
        setHealthText(_value);
        setHealthColor(_value);
    }

    private void setHealthColor(int _value)
    {
        var healthPercentage = (float)_value / (float)max;
        var r = 1.0f - healthPercentage;
        var g = healthPercentage;

        var textColor = new Color(r * 2.0f, g * 2.0f, 1.0f, 1.0f);
        healthText.color = textColor;

        fillCol = new Color(r, g, fillCol.b);
        fill.color = fillCol;
    }

    private void setHealthText(int _value)
    {
        if (textDisplay)
        {
            healthText.text = _value + "/" + max;
        }
        else { healthText.text = ""; }
    }

    public void takeDamage(int _dam)
    {
        health -= _dam;
    }

    public void addValue(int toAdd)
    {
        health += toAdd;
        if (health > max) { health = max; }
    }

    private void Update()
    {
        if (health > 0)
        {
            if (!gameObject.activeInHierarchy) { gameObject.SetActive(true); }
            if (healthBar.value - health < -1)
            {
                healthBar.value += adjustSpeed * Time.deltaTime;
                setHealthText(Mathf.RoundToInt(healthBar.value));
                setHealthColor(Mathf.RoundToInt(healthBar.value));
            }
            else if (healthBar.value - health > 1)
            {
                healthBar.value -= adjustSpeed * Time.deltaTime;
                setHealthText(Mathf.RoundToInt(healthBar.value));
                setHealthColor(Mathf.RoundToInt(healthBar.value));
            }
            else { }
        }
        else
        {
            if (healthBar.value > 0)
            {
                healthBar.value -= adjustSpeed * 10.0f * Time.deltaTime;
                setHealthText(Mathf.RoundToInt(healthBar.value));
                setHealthColor(Mathf.RoundToInt(healthBar.value));
            }
        }
    }
}