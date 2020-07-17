using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum shipType { frigate, intel, support }

public class CombatButton : MonoBehaviour
{
    public InputAction hotKey;
    protected bool onCooldown = false;
    public Slider cooldownSlider;
    protected Button button;
    public float cooldown { get; private set; } = 0.0f;
    public float myCD;

    //public buttonManager buttonManager;
    //public float abilityCooldown = 1.5f;
    //float gcd = 10.0f;
    //  float remainingCooldown;
    protected Outline queueIndicator;

    public UnityEvent initialCall; //myAbility
    public UnityEvent abilityActivate;

    public float haste = 1.0f;

    // bool paused;
    public bool queued { get; private set; } = false;

    public bool targetted { get; protected set; } = false;
    //bool canQueue = false;

    public List<CombatButton> otherButtons;

    // Start is called before the first frame update
    private void Start()
    {
        //cooldownSlider = GetComponentInChildren<Slider>();
        if (!GameManager.tutorial)
        {
            hotKey.Enable();
        }

        button = GetComponent<Button>();
        queueIndicator = GetComponent<Outline>();
    }

    #region newLogic

    public virtual void activateButton()
    {
        initialCall.Invoke();
    }

    public virtual void triggerAbility()
    {
        abilityActivate.Invoke();
    }

    public virtual void sendToButton(float _cooldown)
    {
        // Debug.Log("Final step - setting ability. Cooldown = " + cooldown);
        //This is the last step - we have, theoretically, checked to make sure that we can be activated/queued and assigned a target
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
        if (!GameManager.tutorial)
        {
            //This checks whether the hotkey has been pressed, and acts as though clicked.
            if (hotKey.triggered)
            {
                activateButton();
            }

            //This continuously reduces the cooldown
            if (cooldown > 0.0f)
            {
                cooldown -= Time.deltaTime * haste;
                cooldownSlider.value = cooldown;
            }
            else //If the cooldown is expired and the ability is queued, fire the ability.
            {
                //if (GameManager.autoPause)
                //{
                //    buttonManager.cooldownPause(this);
                //}
                if (queued)
                {
                    clearQueue();
                    triggerAbility();
                    startCooldown(myCD);
                }
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

    //Cooldown functions are called by the player hsip and check whether the button is activatable
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
        foreach (CombatButton other in otherButtons)
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

    #endregion newLogic

    #region oldLogic

    //public virtual void hotkeyLogic()
    //{
    //}

    //public void triggerCooldown()
    //{
    //    if (!onCooldown)
    //    {
    //        StartCoroutine(cooldownTimer(abilityCooldown));
    //    }
    //}

    //public void triggerGlobalCooldown()
    //{
    //    if (!onCooldown)
    //    {
    //        StartCoroutine(cooldownTimer(gcd));
    //    }
    //    else if (remainingCooldown < gcd) { remainingCooldown = gcd; }
    //}

    //public virtual void click()
    //{
    //    if (!onCooldown)
    //    {
    //        triggerCooldown();
    //        foreach (CombatButton other in otherButtons)
    //        {
    //            if (other != null)
    //            {
    //                other.triggerGlobalCooldown();
    //            }

    //        }
    //    }
    //    else if (canQueue())
    //    {
    //        queueAbility();
    //       // Debug.Log("cued ability");
    //    }

    //}

    //public void clearQueue()
    //{
    //    queueIndicator.enabled = false;
    //    queued = false;
    //}

    //protected void clearOthers()
    //{
    //    foreach(CombatButton other in otherButtons)
    //    {
    //        clearQueue();
    //    }
    //}

    //protected virtual void queueAbility()
    //{
    //    clearOthers();
    //    queued = true;
    //    queueIndicator.enabled = true;
    //}

    //protected bool canQueue()
    //{
    //    //Debug.Log("Checking to see if we can queue ability");
    //    var cueCheck = true;
    //    if (!onCooldown)
    //    {
    //        cueCheck = false;
    //        //   Debug.Log("Cannot queue because ability is not on cooldown");
    //    }
    //    if (queued)
    //    {
    //        cueCheck = false;
    //        // Debug.Log("Cannot queue because already queued");
    //    }
    //    //foreach (CombatButton other in otherButtons) //This one shouldn't be necessary as queueing now removes other queues
    //    //{
    //    //    if (other.queued)
    //    //    {
    //    //        cueCheck = false;
    //    //        // Debug.Log("Cannot queue because other ability queued");
    //    //    }
    //    //}
    //    if (remainingCooldown > 1.5f)
    //    {
    //        cueCheck = false;
    //        // Debug.Log("Cannot queue because remaining cooldown too high");
    //    }

    //    return cueCheck;
    //}

    //public void pauseGame()
    //{
    //    paused = true;
    //}
    //public void resumeGame()
    //{
    //    paused = false;
    //}

    //public void setGlobalCooldown(float _cooldown)
    //{
    //    gcd = _cooldown;
    //    if (_cooldown > abilityCooldown)
    //    {
    //        abilityCooldown = _cooldown;
    //    }
    //}

    //IEnumerator cooldownTimer(float duration)
    //{
    //    //Debug.Log("Starting cooldown of " + duration);
    //    cooldown.maxValue = duration;
    //    cooldown.value = duration;
    //    button.enabled = false;
    //    onCooldown = true;

    //    remainingCooldown = duration;
    //    while (remainingCooldown > 0.0f)
    //    {
    //        remainingCooldown -= Time.deltaTime * haste;
    //        cooldown.value = remainingCooldown;
    //        yield return null;
    //    }
    //    button.enabled = true;
    //    onCooldown = false;
    //    if (queued)
    //    {
    //        //click();
    //        button.onClick.Invoke();
    //        clearQueue();
    //    }
    //}

    #endregion oldLogic
}