using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShieldStamina : MonoBehaviour
{
    #region Attribute variables

    public float maxStamina;

    public float stamina { get; private set; }

    public float rechargeSpeed, depletedRecoverySpeed;

    #endregion Attribute variables

    #region Mechanic variables

    public bool shielded { get; private set; }
    public bool recovery { get; private set; }

    public bool absorbing { get; private set; }

    private bool staminaTrack = true;

    #endregion Mechanic variables

    #region References

    public Slider display;
    public Image fill;
    public Color mainColor, recoveryColor;

    #endregion References

    #region setup and bookkeeping

    private void StaminaSetup()
    {
        shielded = false;
        recovery = false;
        absorbing = false;

        fill.color = mainColor;

        StartCoroutine(StaminaEvaluate());
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

    #endregion setup and bookkeeping

    #region Calls from Player

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
    /// Chunks down the stamina by an amount
    /// </summary>
    /// <param name="_toChunk"></param>
    public void StaminaChunk(float _toChunk)
    {
        stamina -= _toChunk;
    }

    #endregion Calls from Player

    private void UpdateUI()
    {
        display.value = stamina;
    }

    private void OnEnable()
    {
        StaminaSetup();
    }

    private IEnumerator StaminaEvaluate()
    {
        //Main loop, tracks and decides on stamina use.
        while (staminaTrack)
        {
            if (!recovery) //If we're not recovering, we fill and drain as usual
            {
                if (shielded) //While we're shielded, we lose stamina at a default rate
                {
                    ShieldUpkeep(); //Drains stamina if we're not absorbing an attack, fills if we are
                }
                else //If we're not shielded and didn't run out of stamina, we recover at a quick rate
                {
                    FillStamina(Time.deltaTime, rechargeSpeed);
                }
            }
            else //If we recover, it refills slowly before we're ready again.
            {
                FillStamina(Time.deltaTime, depletedRecoverySpeed);
            }
            //Update display value each frame
            UpdateUI();

            yield return null;
        }

        //Local functions
        //Evaluates whether we're filling or draining
        void ShieldUpkeep()
        {
            if (!absorbing) //Normal operation is to simply drain
            {
                DrainStamina(Time.deltaTime);
            }
            else  //If we're absorbing an enemy attack, we fill instead of drain
            {
                FillStamina(Time.deltaTime, 1f);
            }
        }

        //Drains stamina by amount
        //Also checks if we're out
        void DrainStamina(float amount)
        {
            stamina -= amount;
            if (stamina <= 0)
            {
                stamina = 0; //Just in case we but out and get like -50 stamina somehow
                shielded = false;
                recovery = true;
                fill.color = recoveryColor;
            }
        }

        //Fills stamina by amount * multiplier
        //Also checks for changing recovery if we need it
        void FillStamina(float amount, float multiplier)
        {
            stamina += amount * multiplier;
            if (stamina >= maxStamina)
            {
                stamina = maxStamina;
                recovery = false;
                fill.color = mainColor;
            }
        }
    }
}