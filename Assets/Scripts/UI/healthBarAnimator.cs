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

    public float max { get; private set; }
    public float health { get; private set; }

    private void Awake()
    {
        healthBar = GetComponent<Slider>();
        fillCol = fill.color;
    }

    public void Refresh(float _max, float _value)
    {
        max = _max;
        health = _value;
        healthBar.maxValue = _max;

        SetHealthColor(_value);
    }

    public void deActivate()
    {
        gameObject.SetActive(false);
    }

    public void SetValue(float _value)
    {
        if (health != _value)
        {
            health = _value;
        }
    }

    protected void SetHealthColor(float _value)
    {
        var healthPercentage = _value / max;
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

    private void Update()
    {
        Evaluate();
    }

    /// <summary>
    /// Evaluates health
    /// </summary>
    protected virtual void Evaluate()
    {
        if (health > 0.0f)
        {
            if (!gameObject.activeInHierarchy) { gameObject.SetActive(true); }
            healthBar.value = Mathf.MoveTowards(healthBar.value, health, adjustSpeed);
            SetHealthColor(healthBar.value);
        }
        else
        {
            if (healthBar.value > 0f)
            {
                healthBar.value = Mathf.MoveTowards(healthBar.value, health, adjustSpeed * 10);
                SetHealthColor(healthBar.value);
            }
        }
    }
}