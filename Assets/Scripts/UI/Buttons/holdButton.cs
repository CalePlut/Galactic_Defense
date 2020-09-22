﻿using UnityEngine.InputSystem;

public class holdButton : basicButton
{
    public PlayerShip ship;
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
            ship.ShieldsDown();
            //StartCooldown(myCD);
        }

        base.Behaviour();
    }
}