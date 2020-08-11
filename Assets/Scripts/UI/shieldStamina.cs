using UnityEngine;
using UnityEngine.UI;

public class shieldStamina : MonoBehaviour
{
    public float maxStamina;
    public float stamina { get; private set; }

    public float rechargeSpeed, recoverySpeed;

    public bool shielded { get; private set; }
    public bool recovery { get; private set; }

    public Slider display;
    public Image fill;
    public Color mainColor, recoveryColor;

    private void Start()
    {
        stamina = maxStamina;
        shielded = false;
        recovery = false;

        display.maxValue = maxStamina;
        display.value = stamina;
        fill.color = mainColor;
    }

    public void shieldsUp()
    {
        shielded = true;
    }

    public void shieldsDown()
    {
        shielded = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!recovery) //If we're not recovering, we fill and drain as usual
        {
            if (shielded) //While we're shielded, we lose stamina at a default rate
            {
                stamina -= Time.deltaTime;
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