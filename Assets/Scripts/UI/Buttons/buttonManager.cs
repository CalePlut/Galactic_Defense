using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buttonManager : MonoBehaviour
{
    public float gcdValue = 2.5f;

    public List<basicButton> buttons;

    //public AffectManager affect;

    public bool ultimateFromStart = false;

    //public List<basicButton> standardAbilities;
    //public List<basicButton> ultimateAbilities;

    //public GameManager gm;

    //private bool onCooldown;
    //private float postCooldown;

    public void globalCooldown() //Starts global cooldowns and begins clock for internal cooldown.
    {
        foreach (basicButton button in buttons)
        {
            if (button.gameObject.activeSelf)
            {
                button.StartCooldown(gcdValue);
            }
        }

        //  gm.resumeGame();
        StartCoroutine(ActionUsed());
    }

    public void globalCooldown(float _customCD) //Starts global cooldowns and begins clock for internal cooldown.
    {
        foreach (basicButton button in buttons)
        {
            if (button.gameObject.activeSelf)
            {
                button.StartCooldown(_customCD);
            }
        }

        //  gm.resumeGame();
        StartCoroutine(ActionUsed());
    }

    public void ActionHappening()
    {
        foreach (basicButton button in buttons)
        {
            button.HoldButton();
        }
    }

    public void ReleaseAction()
    {
        foreach (basicButton button in buttons)
        {
            button.ReleaseButton();
        }
    }

    public void RefreshAllCooldowns()
    {
        //Debug.Log("Clearing Cooldowns");
        foreach (basicButton button in buttons)
        {
            button.ClearCooldown();
        }
    }

    private void Update()
    {
        var totalCooldown = 0.0f;
        foreach (basicButton button in buttons)
        {
            totalCooldown += button.cooldown / button.myCD;
        }

        //postCooldown += Time.deltaTime;
    }

    private IEnumerator ActionUsed()
    {
        //onCooldown = true;
        //affect.actionTrigger(postCooldown);
        yield return new WaitForSeconds(gcdValue);
        //onCooldown = false;
        //postCooldown = 0.0f;
    }
}