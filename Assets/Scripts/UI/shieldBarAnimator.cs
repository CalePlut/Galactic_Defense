using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shieldBarAnimator : healthBarAnimator
{
    private bool fullRecharge = false; //Tracks

    protected override void Evaluate()
    {
        base.Evaluate();
        if (fullRecharge) //If we are recharging, we need to change the colour back to red
        {
            OverrideHealthColor(Color.red);
        }
    }

    public void ShieldBreak()
    {
        fullRecharge = true;
    }

    public void ShieldRestore()
    {
        fullRecharge = false;
    }
}