using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Events;

public class basicButton : MonoBehaviour
{
    [Header("Inputs and References")]
    public InputAction hotKey;

    public Slider cooldownSlider;
    protected Button button;
    protected Outline queueIndicator;
    public List<basicButton> otherButtons;

    [Header("Events for Custom behaviour")]
    public UnityEvent initialCall;

    public UnityEvent abilityActivate;

    [Header("Mechanics and Values")]
    protected bool onCooldown = false;

    public float cooldown { get; protected set; } = 0.0f;
    public float myCD;
    public float haste = 1.0f;
    public bool queued { get; protected set; } = false;
    public bool targetted { get; protected set; } = false;

    // Start is called before the first frame update
    private void Start()
    {
        if (!GameManager.tutorial)
        {
            hotKey.Enable();
        }

        button = GetComponent<Button>();
        queueIndicator = GetComponent<Outline>();
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
            startCooldown(_cooldown);
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
            cooldownSlider.value = cooldown;
        }
        else //If the cooldown is expired and the ability is queued, fire the ability.
        {
            if (queued)
            {
                clearQueue();
                triggerAbility();
                startCooldown(myCD);
            }
        }
    }

    //implemented by targetted
    public virtual void targetsUp()
    {
    }

    public virtual void clearTargetting()
    {
    }

    #endregion Activation and Behaviour

    #region Cooldown and Timing

    /// <summary>
    /// Starts cooldown. If current cooldown is running, set to whichever is higher.
    /// </summary>
    /// <param name="time"></param>
    //Cooldown functions are called by the player ship and check whether the button is activatable
    public void startCooldown(float time)
    {
        if (cooldown < time)
        {
            cooldownSlider.maxValue = time;
            cooldown = time;
        }
    }

    public void alterCooldown(float newCD)
    {
        cooldown = newCD;
        cooldownSlider.maxValue = newCD;
        cooldownSlider.value = cooldown;
    }

    public void clearCooldown()
    {
        cooldown = 0.0f;
        cooldownSlider.value = cooldown;
    }

    public bool canActivate()
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