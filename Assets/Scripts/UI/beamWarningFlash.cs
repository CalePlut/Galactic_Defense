using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class beamWarningFlash : MonoBehaviour
{
    private Image warning;

    public bool isFlashing;

    private void Awake()
    {
        warning = GetComponent<Image>();
    }

    public void flashWarning()
    {
        isFlashing = true;
        StartCoroutine(flash());
    }

    public void enemyDie()
    {
        clearFlash();
        gameObject.SetActive(false);
    }

    public void clearFlash()
    {
        isFlashing = false;
        StopCoroutine(flash());
        warning.enabled = false;
    }

    private IEnumerator flash()
    {
        warning.enabled = true;
        yield return new WaitForSeconds(0.1f);
        warning.enabled = false;
        yield return new WaitForSeconds(0.1f);
        warning.enabled = true;
        yield return new WaitForSeconds(0.1f);
        warning.enabled = false;
        yield return new WaitForSeconds(0.1f);
        warning.enabled = true;
    }
}