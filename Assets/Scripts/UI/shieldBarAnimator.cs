using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shieldBarAnimator : healthBarAnimator
{
    private bool fullRecharge = false; //Tracks fully depleted shield
    private bool jam = false; //Tacks jamming
    public Color broken;

    protected override void Evaluate()
    {
        base.Evaluate();
        if (fullRecharge) //If we are recharging, we need to change the colour back to red
        {
            OverrideHealthColor(Color.red);
        }
        if (jam)
        {
            OverrideHealthColor(broken);
        }
        else
        {
            SetHealthColor(health);
        }
    }

    public void Jam()
    {
        jam = true;
    }
    public void endJam()
    {
        jam = false;
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