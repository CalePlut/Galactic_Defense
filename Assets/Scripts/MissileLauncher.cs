using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLauncher : MonoBehaviour
{
    public GameObject missile;
    public List<GameObject> missileTubes;

    public void launchMissiles(int _missiles, GameObject target)
    {
        StartCoroutine(missileStagger(_missiles, target));
    }

    public void missileFail()
    {
        StartCoroutine(failStagger());
    }

    private IEnumerator failStagger()
    {
        foreach (GameObject tube in missileTubes)
        {
            var newMissile = Instantiate(missile, tube.transform.position, Quaternion.identity, this.transform);
            newMissile.GetComponent<MissileControl>().Miss();
            yield return new WaitForSeconds(0.25f);
        }
    }

    private IEnumerator missileStagger(int toFire, GameObject target)
    {
        toFire *= toFire;

        for (int i = 0; i < toFire; i++)
        {
            var tube = missileTubes[i % 3];
            var newMissile = Instantiate(missile, tube.transform.position, Quaternion.identity, this.transform);
            newMissile.GetComponent<MissileControl>().Launch(target);
            yield return new WaitForSeconds(0.05f);
        }
    }
}