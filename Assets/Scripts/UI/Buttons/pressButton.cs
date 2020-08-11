public class pressButton : basicButton
{
    protected override void Behaviour()
    {
        if (!GameManager.tutorial)
        {
            //This checks whether the hotkey has been pressed, and acts as though clicked.
            if (hotKey.triggered)
            {
                activateButton();
            }
        }
        base.Behaviour();
    }
}