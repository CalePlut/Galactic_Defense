using UnityEngine.Events;
using UnityEngine.InputSystem;

public class holdButton : basicButton
{
    public PlayerShip ship;
    public InputAction release;
    public UnityEvent releaseEvent;

    protected override void Behaviour()
    {
        if (!release.enabled) { release.Enable(); }

        if (hotKey.triggered)
        {
            activateButton();
        }
        if (release.triggered)
        {
            releaseEvent.Invoke();
        }

        base.Behaviour();
    }
}