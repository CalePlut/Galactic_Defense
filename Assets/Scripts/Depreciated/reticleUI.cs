using UnityEngine;
using UnityEngine.UI;

public class reticleUI : MonoBehaviour
{
    public float rotateSpeed = 25.0f;

    public Sprite reticle2, reticle3;

    public GameObject reticleCore;
    public GameObject reticleRadial;
    private Image image;

    private void Awake()
    {
        image = reticleRadial.GetComponent<Image>();
    }

    public void setReticle(int reticle)
    {
        if (reticle <= 0)
        {
            reticleCore.SetActive(false);
            reticleRadial.SetActive(false);
        }
        if (reticle == 1)
        {
            reticleCore.SetActive(true);
            reticleRadial.SetActive(false);
        }
        if (reticle == 2)
        {
            reticleCore.SetActive(true);
            reticleRadial.SetActive(true);
            image.sprite = reticle2;
            reticleRadial.GetComponent<reticleRotate>().startRotate(rotateSpeed);
        }
        if (reticle == 3)
        {
            reticleCore.SetActive(true);
            reticleRadial.SetActive(true);
            reticleRadial.GetComponent<reticleRotate>().startRotate(rotateSpeed * 1.5f);
            reticleCore.GetComponent<reticleRotate>().startRotate(-rotateSpeed * 1.5f);
            image.sprite = reticle3;
        }
    }
}