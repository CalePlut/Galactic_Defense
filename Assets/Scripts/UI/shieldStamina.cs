using UnityEngine;
using UnityEngine.UI;

public class shieldStamina : MonoBehaviour
{
    public float maxStamina;
    public float stamina { get; private set; }

    public float rechargeSpeed, recoverySpeed;

    public bool shielded { get; private set; }
    public bool recovery { get; private set; }

    public bool absorbing { get; private set; }

    public Slider display;
    public Image fill;
    public Color mainColor, recoveryColor;

    private void Start()
    {
        shielded = false;
        recovery = false;

        fill.color = mainColor;
    }

    public void ShieldsUp()
    {
        shielded = true;
    }

    public void ShieldDown()
    {
        shielded = false;
        absorbing = false;
    }

    public void AbsorbAttack()
    {
        absorbing = true;
    }

    /// <summary>
    /// Sets initial stamina
    /// </summary>
    /// <param name="_stamina"></param>
    public void SetStamina(float _stamina)
    {
        maxStamina = _stamina;
        stamina = _stamina;

        display.maxValue = maxStamina;
        display.value = stamina;
    }

    /// <summary>
    /// Chunks down the stamina by an amount
    /// </summary>
    /// <param name="_toChunk"></param>
    public void StaminaChunk(float _toChunk)
    {
        stamina -= _toChunk;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!recovery) //If we're not recovering, we fill and drain as usual
        {
            if (shielded) //While we're shielded, we lose stamina at a default rate
            {
                if (absorbing)
                {
                    stamina += Time.deltaTime * 2.0f;
                    if (stamina >= maxStamina) { absorbing = false; }
                }
                else
                {
                    stamina -= Time.deltaTime;
                }
                display.value = stamina;
                if (stamina <= 0)
                {
                    shielded = false;
                    recovery = true;
                    fill.color = recoveryColor;
                }
            }
            else //If we're not shielded and didn't run out of stamina, we recover at a quick rate
            {
                if (stamina < maxStamina)
                {
                    stamina += Time.deltaTime * rechargeSpeed;
                    display.value = stamina;
                }
            }
        }
        else //If we recover, it refills slowly before we're ready again.
        {
            if (stamina < maxStamina)
            {
                stamina += Time.deltaTime * recoverySpeed;
                display.value = stamina;
            }
            else
            {
                recovery = false;
                fill.color = mainColor;
            }
        }
    }
}