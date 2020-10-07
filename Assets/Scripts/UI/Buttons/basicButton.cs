﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class basicButton : MonoBehaviour
{
    [Header("Inputs and References")]
    public InputAction hotKey;

    public Slider cooldownSlider;
    protected Button button;
    protected Outline queueIndicator;
    public List<basicButton> otherButtons;

    private bool buttonHeld = false; //Cooldowns don't advance when player shielding.

    [Header("Events for Custom behaviour")]
    public UnityEvent initialCall;

    public UnityEvent abilityActivate;

    [Header("Mechanics and Values")]
    protected bool onCooldown = false;

    public Image fill;
    private Color baseColor;

    public float cooldown { get; protected set; } = 0.0f;
    public float myCD;
    public float haste = 1.0f;
    public float shieldRecover = 0.231f;
    public bool queued { get; protected set; } = false;
    public bool targetted { get; protected set; } = false;

    // Start is called before the first frame update
    private void Start()
    {
        baseColor = fill.color;

        button = GetComponent<Button>();
        queueIndicator = GetComponent<Outline>();
    }

    public void StartCombat()
    {
        hotKey.Enable();
    }

    public void EndCombat()
    {
        hotKey.Disable();
    }

    #region Activation and Behaviour

    /// <summary>
    /// First step in button --- Invokes "Initial Call"
    /// </summary>
    public virtual void activateButton()
    {
        initialCall.Invoke();
    }

    /// <summary>
    /// Sends message that shield is up - sets boolean to not display cooldown
    /// </summary>
    public void HoldButton()
    {
        buttonHeld = true;
        button.image.color = Color.green;
    }

    /// <summary>
    /// Releases shield and resumes normal behaviour
    /// </summary>
    public void ReleaseButton()
    {
        buttonHeld = false;
        button.image.color = baseColor;
        //Debug.Log("Released button");
    }

    /// <summary>
    /// Final step in the button --- activates "Ability Activate"
    /// </summary>
    public virtual void triggerAbility()
    {
        abilityActivate.Invoke();
    }

    /// <summary>
    /// This either activates the ability or queues ability, depending on the remaining cooldown
    /// </summary>
    /// <param name="_cooldown"></param>
    public virtual void sendToButton(float _cooldown)
    {
        //If our cooldown is already 0, we trigger the ability and cooldown
        if (cooldown <= 0.0f)
        {
            // Debug.Log("No cooldown, firing!");
            triggerAbility();
            StartCooldown(_cooldown);
        }
        else if (cooldown < 1.0f)
        {
            // Debug.Log("Cooldown above 0, queueing ability and ability will fire after grace period");
            queueAbility();
        }
    }

    private void Update() //Update continuously checks if the hotkey is pressed, and
    {
        Behaviour();
    }

    /// <summary>
    /// This is where the core of the button behaviour lives - reduces cooldown. Overriden based on button behaviour
    /// </summary>
    protected virtual void Behaviour()
    {
        //This continuously reduces the cooldown
        if (cooldown > 0.0f)
        {
            cooldown -= Time.deltaTime * haste;
        }
        else //If the cooldown is expired and the ability is queued, fire the ability.
        {
            fill.color = baseColor;
            if (queued)
            {
                clearQueue();
                triggerAbility();
                StartCooldown(myCD);
            }
        }

        if (buttonHeld) //Regardless of logic, determine what to display
        {
            //    Debug.Log("Button held, not displaying cooldown");
            cooldownSlider.value = cooldownSlider.maxValue; //Set skill to full, regardless of cooldown, if shield is held.
        }
        else
        {
            cooldownSlider.value = cooldown;
        }
    }

    #endregion Activation and Behaviour

    #region Cooldown and Timing

    /// <summary>
    /// Starts cooldown. If current cooldown is running, set to whichever is higher.
    /// </summary>
    /// <param name="time"></param>
    //Cooldown functions are called by the player ship and check whether the button is activatable
    public void StartCooldown(float time)
    {
        if (cooldown < time)
        {
            cooldownSlider.maxValue = time;
            cooldown = time;
        }
    }

    public void StartCooldown(float time, Color col)
    {
        if (cooldown < time)
        {
            cooldownSlider.maxValue = time;
            cooldown = time;
            fill.color = col;
        }
    }

    public void alterCooldown(float newCD)
    {
        cooldown = newCD;
        cooldownSlider.maxValue = newCD;
        cooldownSlider.value = cooldown;
    }

    public void ClearCooldown()
    {
        cooldown = 0.0f;
        cooldownSlider.value = cooldown;
        fill.color = baseColor;
    }

    public bool CanActivate()
    {
        if (cooldown > 1.0f) { return false; }
        else { return true; }
    }

    //Queue functions clear the current queue as well as removing the queued status of other buttons if this button is queued
    public void clearQueue()
    {
        queueIndicator.enabled = false;
        queued = false;
    }

    protected void clearOthers()
    {
        foreach (basicButton other in otherButtons)
        {
            clearQueue();
        }
    }

    protected virtual void queueAbility()
    {
        clearOthers();
        queued = true;
        queueIndicator.enabled = true;
    }

    //Haste is multiplied by time.deltaTime during cooldown.
    public void setHaste(float _amt)
    {
        haste = _amt;
    }

    #endregion Cooldown and Timing
}