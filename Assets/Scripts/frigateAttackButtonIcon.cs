using UnityEngine;
using UnityEngine.UI;

public class frigateAttackButtonIcon : MonoBehaviour
{
    public Sprite cannon, laser;
    private Image image;

    // Start is called before the first frame update
    private void Start()
    {
        image = GetComponent<Image>();
    }

    public void setCannon()
    {
        image.sprite = cannon;
    }

    public void setLaser()
    {
        image.sprite = laser;
    }
}