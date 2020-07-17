using UnityEngine;
using UnityEngine.UI;

public class pauseButton : MonoBehaviour
{
    private Image image;

    public Sprite play, pause;

    // Start is called before the first frame update
    private void Start()
    {
        image = GetComponent<Image>();
        image.sprite = pause;
    }

    public void resume()
    {
        image.sprite = pause;
    }

    public void pauseG()
    {
        image.sprite = play;
    }
}