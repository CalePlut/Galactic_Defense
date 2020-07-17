using System.Collections.Generic;
using UnityEngine;

public class combatButtonManager : MonoBehaviour
{
    public float globalCoolDown;

    public List<CombatButton> allButtons;

    //public void triggerGlobalCooldown()
    //{
    //    foreach (CombatButton button in allButtons) {
    //        button.triggerGlobalCooldown();
    //    }
    //}

    //private void Awake()
    //{
    //    foreach (CombatButton button in allButtons)
    //    {
    //        button.setGlobalCooldown(globalCoolDown);
    //    }
    //}

    //public void pauseGame()
    //{
    //    foreach(CombatButton button in allButtons)
    //    {
    //        button.pauseGame();
    //    }
    //}

    //public void resumeGame()
    //{
    //    foreach(CombatButton button in allButtons)
    //    {
    //        button.resumeGame();
    //    }
    //}
}