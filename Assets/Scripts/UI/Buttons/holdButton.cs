using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class holdButton : basicButton
{
    public FrigateShip ship;
    public InputAction release;

    protected override void Behaviour()
    {
        if (!release.enabled) { release.Enable(); }

        if (hotKey.triggered)
        {
            activateButton();
        }
        if (release.triggered)
        {
            ship.shieldDown();
            startCooldown(myCD);
        }

        base.Behaviour();
    }
}