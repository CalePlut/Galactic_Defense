using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AffectManager : MonoBehaviour
{
    private float valence, arousal, tension;
    private float baseValence, baseArousal, baseTension;
    private float frigateHP, intelHP, supportHP;

    private bool intelLow, frigateLow, supportLow;
    private bool leftEnemyLow, centreEnemyLow, rightEnemyLow, parryFrame;  //Booleans track states that increase player tension.
    private bool intelRespawning, supportRespawning;

    private float intelTension, frigateTension, supportTension;
    private float leftEnemyHPTension, centreEnemyHPTension, rightEnemyHPTension;
    private float parryTension;

    private List<ArousalModify> arousalModifiers;

    public AffectVariables affectVariables;

    private float weakValence, moderateValence, strongValence;
    private float weakTension, moderateTension, strongTension;
    private float arousalScale;

    private int valenceLevel = 2, arousalLevel = 2, tensionLevel = 2;

    private bool valenceLockout = false, arousalLockout = false, tensionLockout = false; //These booleans remove problems of music adapting too fast and sending conflicting messages - only one change per level per 5 seconds is allowed

    private float resetSpeed;

    public bool musicControl = false;

    //private int actions;

    // public TextMeshProUGUI apmText;

    public AffectBarText valenceDisplay, arousalDisplay, tensionDisplay;
    private EliasPlayer music;

    public InputAction anything;

    private void Start()
    {
        baseValence = affectVariables.V11;
        baseArousal = affectVariables.A11;
        baseTension = affectVariables.T11;

        valence = baseValence;
        arousal = baseArousal;
        tension = baseTension;

        adjustValence(valence);
        adjustTension(tension);

        weakValence = affectVariables.weakValence;
        moderateValence = affectVariables.moderateValence;
        strongValence = affectVariables.strongValence;

        weakTension = affectVariables.weakTension;
        moderateTension = affectVariables.moderateTension;
        strongTension = affectVariables.strongTension;

        resetSpeed = affectVariables.resetSpeed;

        arousalScale = affectVariables.arousalScalar;

        arousalModifiers = new List<ArousalModify>();

        music = GetComponent<EliasPlayer>();
    }

    #region valenceEvents

    //First, positive events
    public void enemyHit(float damage) //Dealing damage adds valence scaling on damage
    {
        var valenceAdd = weakValence + damage;
        adjustValence(valenceAdd);

        arousalModifiers.Add(new ArousalModify(damage * arousalScale));
    }

    public void enemydie(position pos)
    {
        //adjustTension(-10.0f);

        if (pos == position.left)
        {
            adjustValence(weakValence);
            leftEnemyLow = false;
            leftEnemyHPTension = 0.0f;
        }
        if (pos == position.centre)
        {
            adjustValence(strongValence);
            centreEnemyLow = false;
            centreEnemyHPTension = 0.0f;

            parryFrame = false;
            parryTension = 0.0f;
        }
        if (pos == position.right)
        {
            adjustValence(weakValence);
            rightEnemyLow = false;
            rightEnemyHPTension = 0.0f;
        }
    }

    public void interruptFusionCannon()
    {
        adjustValence(weakValence);
    }

    public void healShip(bool upgrade) //Disabled
    {
        var toChange = moderateValence;
        if (upgrade) { toChange = strongValence; }
        //  adjustValence(toChange);
    }

    public void respawnShip() //Disabled
    {
        //adjustValence(moderateValence);
    }

    public void ultimateAbility()
    {
        adjustValence(strongValence);
    }

    public void parry()
    {
        adjustValence(weakValence);
    }

    //Second, negative valence events
    public void playerHit(float damage)
    {
        var toAdjust = weakValence + damage;
        toAdjust = -toAdjust;
        adjustValence(toAdjust);
    }

    public void loseShip(PlayerShip ship)
    {
        adjustValence(strongValence * (-1));
        if (ship.gameObject.name == "Support")
        {
            supportRespawning = true;
        }
        if (ship.gameObject.name == "Intel")
        {
            intelRespawning = true;
        }
        //adjustTension(+moderateTension);
    }

    public void enemyAbility()
    {
        adjustValence(-moderateValence);
    }

    #endregion valenceEvents

    #region tensionStates

    public void updatePlayerTension()
    {
        //Evaluate tension levels of various player ships
        if (intelLow) { intelTension += weakTension * Time.deltaTime; }
        else if (intelRespawning) { intelTension += moderateTension * Time.deltaTime; }
        else { intelTension = 0; }
        if (frigateLow) { frigateTension += moderateTension * Time.deltaTime; }
        else { frigateTension = 0; }
        if (supportLow) { supportTension += weakTension * Time.deltaTime; }
        else if (supportRespawning) { supportTension += moderateTension * Time.deltaTime; }
        else { supportTension = 0; }

        var playerValues = intelTension + frigateTension + supportTension;

        if (leftEnemyLow) { leftEnemyHPTension += weakTension * Time.deltaTime; }
        else { leftEnemyHPTension = 0; }
        if (centreEnemyLow) { centreEnemyHPTension += moderateTension * Time.deltaTime; }
        else { centreEnemyHPTension = 0; }
        if (rightEnemyLow) { rightEnemyHPTension += weakTension * Time.deltaTime; }
        else { rightEnemyHPTension = 0; }

        if (parryFrame) { parryTension += strongTension * Time.deltaTime; }
        else { parryTension = 0; }

        var enemyValues = leftEnemyHPTension + centreEnemyHPTension + rightEnemyHPTension + parryTension;

        var tensionTotal = baseTension + playerValues + enemyValues;
        adjustTension(tensionTotal);
    }

    public void respawn(PlayerShip ship)
    {
        if (ship.gameObject.name == "Support")
        {
            supportRespawning = false;
            respawnShip();
        }
        if (ship.gameObject.name == "Intel")
        {
            respawnShip();
            intelRespawning = false;
        }
    }

    public void setParryFrame(bool frame)
    {
        if (frame) { parryFrame = true; }
        else
        {
            parryFrame = false;
            parryTension = 0.0f;
        }
    }

    #endregion tensionStates

    #region moodVariables

    public void setProgess(int stage, int encounter)
    {
        switch (stage)
        {
            case 1:
                switch (encounter)
                {
                    case 1:
                        setBase(affectVariables.V11, affectVariables.A11, affectVariables.T11);
                        break;

                    case 2:
                        setBase(affectVariables.V12, affectVariables.A12, affectVariables.T12);
                        break;
                }
                break;

            case 2:
                switch (encounter)
                {
                    case 1:
                        setBase(affectVariables.V21, affectVariables.A21, affectVariables.T21);
                        break;

                    case 2:
                        setBase(affectVariables.V22, affectVariables.A22, affectVariables.T22);
                        break;

                    case 3:
                        setBase(affectVariables.V23, affectVariables.A23, affectVariables.T23);
                        break;
                }
                break;

            case 3:
                switch (encounter)
                {
                    case 1:
                        setBase(affectVariables.V31, affectVariables.A31, affectVariables.T31);
                        break;

                    case 2:
                        setBase(affectVariables.V32, affectVariables.A32, affectVariables.T32);
                        break;

                    case 3:
                        setBase(affectVariables.V33, affectVariables.A33, affectVariables.T33);
                        break;

                    case 4:
                        setBase(affectVariables.V34, affectVariables.A34, affectVariables.T34);
                        break;
                }
                break;
        }
    } //Sets stage progress from affectvariables levels

    private void setBase(float V, float A, float T)
    {
        baseValence = V;
        baseArousal = A;
        baseTension = T;
    }

    //Trying a thing where tension creating situations also control valence - layered model, remember
    public void updateEnemyHealth(float percent, position pos)
    {
        if (pos == position.left)
        {
            if (percent < 0.4f)
            {
                leftEnemyLow = true;
                adjustValence(weakValence);
                // Debug.Log("Updating valence rom enemy health");
            }
            else { leftEnemyLow = false; }
        }
        if (pos == position.centre)
        {
            if (percent < 0.4f)
            {
                centreEnemyLow = true;
                adjustValence(weakValence);

                // Debug.Log("Updating valence rom enemy health");
            }
            else { centreEnemyLow = false; }
        }
        if (pos == position.right)
        {
            if (percent < 0.4f)
            {
                rightEnemyLow = true;
                adjustValence(weakValence);

                //  Debug.Log("Updating valence rom enemy health");
            }
            else { rightEnemyLow = false; }
        }
    }

    public void updateFrigateHealth(float percent)
    {
        frigateHP = percent;
        if (frigateHP < 0.2)
        {
            adjustValence(-weakValence);

            // Debug.Log("Updating valence from player health");
            frigateLow = true;
        }
        else
        {
            frigateLow = false;
            frigateTension = 0.0f;
        }
    }

    public void updateIntelHealth(float percent)
    {
        intelHP = percent;
        if (intelHP < 0.2)
        {
            adjustValence(-weakValence);
            intelLow = true;

            // Debug.Log("Updating valence from artillery health");
        }
        else
        {
            intelLow = false;
            intelTension = 0.0f;
        }
    }

    public void updateSupportHealth(float percent)
    {
        supportHP = percent;

        if (supportHP < 0.2)
        {
            adjustValence(-weakValence);
            supportLow = true;
        }
        else
        {
            supportLow = false;
            supportTension = 0.0f;
        }
    }

    #endregion moodVariables

    #region AffectImplementation

    private void adjustValence(float amount)
    {
        valence += amount;
        updateValence();
    }

    private void updateValence()
    {
        if (valence > 100.0f) { valence = 100.0f; }
        if (valence < -100.0f) { valence = -100.0f; }
        if (!valenceLockout)
        {
            if (valence < 33.0f)
            {
                if (valenceLevel != 1)
                {
                    valenceDisplay.Affect = 1;
                    if (musicControl)
                    {
                        if (arousalLevel == 1) { music.RunActionPreset("LowVLowA"); }
                        else if (arousalLevel == 3) { music.RunActionPreset("LowVHighA"); }
                        else { music.RunActionPreset("LowVMidA"); }
                    }
                    //music.RunActionPreset("ValenceLow");
                    valenceLevel = 1;
                    StartCoroutine(valenceLock());
                }
            }
            else if (valence < 66.0f)
            {
                if (valenceLevel != 2)
                {
                    valenceDisplay.Affect = 2;
                    if (musicControl)
                    {
                        if (arousalLevel == 1) { music.RunActionPreset("MidVLowA"); }
                        else if (arousalLevel == 3) { music.RunActionPreset("MidVHighA"); }
                        else { music.RunActionPreset("MidVMidA"); }
                    }
                    //music.RunActionPreset("ValenceMid");
                    valenceLevel = 2;
                    StartCoroutine(valenceLock());
                }
            }
            else
            {
                if (valenceLevel != 3)
                {
                    valenceDisplay.Affect = 3;
                    if (musicControl)
                    {
                        if (arousalLevel == 1) { music.RunActionPreset("HighVLowA"); }
                        else if (arousalLevel == 3) { music.RunActionPreset("HighVHighA"); }
                        else { music.RunActionPreset("HighVMidA"); }
                    }
                    // music.RunActionPreset("ValenceHigh");
                    valenceLevel = 3;
                    StartCoroutine(valenceLock());
                }
            }
        }
    }

    private void adjustArousal()
    {
        arousal = baseArousal;
        if (arousalModifiers == null) { arousalModifiers = new List<ArousalModify>(); }
        if (arousalModifiers.Count > 0)
        {
            var toRemove = new List<ArousalModify>(); //This will be used to clear the arousalmodifiers when they reach zero hopefully

            foreach (ArousalModify modifier in arousalModifiers)
            {
                modifier.tick(Time.deltaTime);
                if (modifier.zeroWeight()) { toRemove.Add(modifier); }
                else { arousal += modifier.queryArousal(); }
            }

            foreach (ArousalModify removal in toRemove) //Removes any arousalModifiers that were below zero
            {
                if (arousalModifiers.Contains(removal))
                {
                    arousalModifiers.Remove(removal);
                }
            }
        }

        //Here's the hook-up to everything else, once we have an arousal value
        if (arousal > 100.0f) { arousal = 100.0f; }
        if (arousal < -100.0f) { arousal = -100.0f; }
        //arousalDisplay.updateValue(Mathf.RoundToInt(arousal));
        if (!arousalLockout)
        {
            if (arousal < 33.0f)
            {
                if (arousalLevel != 1)
                {
                    arousalDisplay.Affect = 1;
                    if (musicControl)
                    {
                        if (valenceLevel == 1) { music.RunActionPreset("LowVLowA"); }
                        else if (valenceLevel == 3) { music.RunActionPreset("HighVLowA"); }
                        else { music.RunActionPreset("MidVLowA"); }
                    }
                    //music.RunActionPreset("ArousalLow");
                    arousalLevel = 1;
                    StartCoroutine(arousalLock());
                }
            }
            else if (arousal < 66.0f)
            {
                if (arousalLevel != 2)
                {
                    arousalDisplay.Affect = 2;
                    if (musicControl)
                    {
                        if (valenceLevel == 1) { music.RunActionPreset("LowVMidA"); }
                        else if (valenceLevel == 3) { music.RunActionPreset("HighVMidA"); }
                        else { music.RunActionPreset("MidVMidA"); }
                    }
                    //music.RunActionPreset("ArousalMid");
                    arousalLevel = 2;
                    StartCoroutine(arousalLock());
                }
            }
            else
            {
                if (arousalLevel != 3)
                {
                    arousalDisplay.Affect = 3;
                    if (musicControl)
                    {
                        if (valenceLevel == 1) { music.RunActionPreset("LowVHighA"); }
                        else if (valenceLevel == 3) { music.RunActionPreset("HighVHighA"); }
                        else { music.RunActionPreset("MidVHighA"); }
                    }
                    // music.RunActionPreset("ArousalHigh");
                    arousalLevel = 3;
                    StartCoroutine(arousalLock());
                }
            }
        }
    }

    private void adjustTension(float amount)
    {
        tension = amount;
        if (tension > 100.0f) { tension = 100.0f; }
        if (tension < -100.0f) { tension = -100.0f; }
        //tensionDisplay.updateValue(Mathf.RoundToInt(tension));

        if (!tensionLockout)
        {
            if (tension < 33.0f)
            {
                if (tensionLevel != 1)
                {
                    tensionDisplay.Affect = 1;
                    if (musicControl)
                    {
                        music.RunActionPreset("TensionLow");
                        tensionLevel = 1;
                        StartCoroutine(tensionLock());
                    }
                }
            }
            else if (tension < 66.0f)
            {
                if (tensionLevel != 2)
                {
                    tensionDisplay.Affect = 2;
                    if (musicControl)
                    {
                        music.RunActionPreset("TensionMid");
                        tensionLevel = 2;
                        StartCoroutine(tensionLock());
                    }
                }
            }
            else
            {
                if (tensionLevel != 3)
                {
                    tensionDisplay.Affect = 3;
                    if (musicControl)
                    {
                        music.RunActionPreset("TensionHigh");
                        tensionLevel = 3;
                        StartCoroutine(tensionLock());
                    }
                }
            }
        }
    }

    private void autoReset()
    {
        valence = Mathf.Lerp(valence, baseValence, resetSpeed * Time.deltaTime);
        //arousal = Mathf.Lerp(arousal, baseArousal, resetSpeed * Time.deltaTime); //Trying this out where arousal doesn't auto-reset because the arousal modifiers already decay over time
        tension = Mathf.Lerp(tension, baseTension, resetSpeed * Time.deltaTime);

        adjustTension(tension);
        updateValence();
        //valenceDisplay.updateValue((int)valence);
        //arousalDisplay.updateValue((int)arousal);
        //tensionDisplay.updateValue((int)tension);
    }

    #endregion AffectImplementation

    private void Update()
    {
        updatePlayerTension();
        autoReset();
        adjustArousal();

        // apmText.text = "APM: " + actions;
    }

    private IEnumerator valenceLock()
    {
        valenceLockout = true;
        yield return new WaitForSeconds(1.5f);
        valenceLockout = false;
    }

    private IEnumerator arousalLock()
    {
        arousalLockout = true;
        yield return new WaitForSeconds(1.5f);
        arousalLockout = false;
    }

    private IEnumerator tensionLock()
    {
        tensionLockout = true;
        yield return new WaitForSeconds(1.5f);
        tensionLockout = false;
    }
}

public class ArousalModify
{
    //ArousalModify stores events and weights, and decreases the weight over 6 seconds.

    private float amount;
    private float weight;

    public ArousalModify(float _amount)
    {
        amount = _amount;
        weight = 1.0f;
    }

    public float queryArousal()
    {
        return amount * weight;
    }

    public void tick(float deltaTime)
    {
        weight -= deltaTime / 6.0f;
    }

    public bool zeroWeight()
    {
        if (weight <= 0.0f) { return true; }
        else return false;
    }
}