using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buttonManager : MonoBehaviour
{
    public float gcdValue = 2.5f;

    public List<basicButton> buttons;
    public basicButton ultimate;

    public AffectManager affect;

    //public List<basicButton> standardAbilities;
    //public List<basicButton> ultimateAbilities;

    //public GameManager gm;

    private bool onCooldown;
    private float postCooldown;

    // Start is called before the first frame update
    private void Start()
    {
        ultimate.gameObject.SetActive(false);
        //foreach(basicButton button in ultimateAbilities)
        //{
        //    button.gameObject.SetActive(false);
        //}
    }

    public void globalCooldown() //Starts global cooldowns and begins clock for internal cooldown.
    {
        foreach (basicButton button in buttons)
        {
            if (button.gameObject.activeSelf)
            {
                button.startCooldown(gcdValue);
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
                button.startCooldown(_customCD);
            }
        }

        //  gm.resumeGame();
        StartCoroutine(ActionUsed());
    }

    //public void setHaste(float multiplier, float time)
    //{
    //    foreach(basicButton button in standardAbilities)
    //    {
    //        button.setHaste(multiplier);
    //    }
    //    StartCoroutine(hasteTimer(time));
    //}

    public void unlockUltimate()
    {
        ultimate.gameObject.SetActive(true);
    }

    //IEnumerator hasteTimer(float _time)
    //{
    //    yield return new WaitForSeconds(_time);
    //    foreach(basicButton button in standardAbilities)
    //    {
    //        button.setHaste(1.0f);
    //    }
    //}

    public void refreshAllCooldowns()
    {
        foreach (basicButton button in buttons)
        {
            button.clearCooldown();
        }
    }

    private void Update()
    {
        var totalCooldown = 0.0f;
        foreach (basicButton button in buttons)
        {
            totalCooldown += button.cooldown / button.myCD;
        }
        var avgCooldown = totalCooldown / buttons.Count;
        //affect.updateCooldowns(avgCooldown);

        postCooldown += Time.deltaTime;
    }

    //public void cooldownPause(basicButton button)
    //{
    //    if (button.GetType() != typeof(targettedbasicButton))
    //    {
    //        gm.pauseGame();
    //    }
    //}

    private IEnumerator ActionUsed()
    {
        onCooldown = true;
        //affect.actionTrigger(postCooldown);
        yield return new WaitForSeconds(gcdValue);
        onCooldown = false;
        postCooldown = 0.0f;
    }
}