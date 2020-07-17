using UnityEngine;
using UnityEngine.UI;

public class targettedCombatButton : CombatButton
{
    public GameObject targeting;
    private GameObject target;

    private void Start()
    {
        targetted = true;

        hotKey.Enable();

        button = GetComponent<Button>();
        queueIndicator = GetComponent<Outline>();
    }

    //public void setTarget(GameObject tar)
    //{
    //    target = tar;
    //}

    //public override void click()
    //{
    //    //base.click();
    //    if (!onCooldown || canQueue())
    //    {
    //        targeting.SetActive(true);
    //    }
    //}

    //protected override void queueAbility()
    //{
    //    base.queueAbility();
    //    targeting.SetActive(true);
    //}

    //public void doneTargetting()
    //{
    //    base.click();
    //    clearTargetting();
    //}

    //public override void sendToButton(float cooldown)
    //{
    //    if (target != null) //This should be impossible to avoid triggering, but let's be safe
    //    {
    //        base.sendToButton(cooldown);
    //    }

    //}

    public override void targetsUp()
    {
        //Debug.Log("Showing targets");
        base.targetsUp();
        targeting.SetActive(true);
    }

    public override void clearTargetting()
    {
        targeting.SetActive(false);
    }
}