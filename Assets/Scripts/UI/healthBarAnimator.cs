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
    public Color fullColor;
    public Color emptyColor;

    public bool textDisplay = false;

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
        SetHealthColor(_value);
    }

    public void Refresh(float _max, float _value)
    {
        max = Mathf.RoundToInt(_max);
        health = Mathf.RoundToInt(_value);
        healthBar.maxValue = _max;
        healthBar.value = _value;

        setHealthText(health);
        SetHealthColor(health);
    }

    public void deActivate()
    {
        gameObject.SetActive(false);
    }

    public void SetValue(float _value)
    {
        var value = Mathf.RoundToInt(_value);
        health = value;
    }

    protected void SetHealthColor(int _value)
    {
        var healthPercentage = (float)_value / (float)max;
        var col = Color.Lerp(emptyColor, fullColor, healthPercentage);

        fill.color = col;
    }

    /// <summary>
    /// Called by shield bar to override the color of the shield with red while recharging
    /// </summary>
    /// <param name="col"></param>
    protected void OverrideHealthColor(Color col)
    {
        fill.color = col;
    }

    private void setHealthText(int _value)
    {
        if (textDisplay)
        {
            healthText.text = _value + "/" + max;
        }
        else { healthText.text = ""; }
    }

    public void TakeDamage(int _dam)
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
        Evaluate();
    }

    /// <summary>
    /// Evaluates health
    /// </summary>
    protected virtual void Evaluate()
    {
        if (health > 0)
        {
            if (!gameObject.activeInHierarchy) { gameObject.SetActive(true); }
            if (healthBar.value - health < -1)
            {
                healthBar.value += adjustSpeed * Time.deltaTime;
                setHealthText(Mathf.RoundToInt(healthBar.value));
                SetHealthColor(Mathf.RoundToInt(healthBar.value));
            }
            else if (healthBar.value - health > 1)
            {
                healthBar.value -= adjustSpeed * Time.deltaTime;
                setHealthText(Mathf.RoundToInt(healthBar.value));
                SetHealthColor(Mathf.RoundToInt(healthBar.value));
            }
            else { }
        }
        else
        {
            if (healthBar.value > 0)
            {
                healthBar.value -= adjustSpeed * 10.0f * Time.deltaTime;
                setHealthText(Mathf.RoundToInt(healthBar.value));
                SetHealthColor(Mathf.RoundToInt(healthBar.value));
            }
        }
    }
}