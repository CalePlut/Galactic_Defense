using System.Collections.Generic;
using UnityEngine;

public class basicButtonManager : MonoBehaviour
{
    public float globalCoolDown;

    public List<basicButton> allButtons;

    //public void triggerGlobalCooldown()
    //{
    //    foreach (basicButton button in allButtons) {
    //        button.triggerGlobalCooldown();
    //    }
    //}

    //private void Awake()
    //{
    //    foreach (basicButton button in allButtons)
    //    {
    //        button.setGlobalCooldown(globalCoolDown);
    //    }
    //}

    //public void pauseGame()
    //{
    //    foreach(basicButton button in allButtons)
    //    {
    //        button.pauseGame();
    //    }
    //}

    //public void resumeGame()
    //{
    //    foreach(basicButton button in allButtons)
    //    {
    //        button.resumeGame();
    //    }
    //}
}