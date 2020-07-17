using UnityEngine;

public class environmentAudio : MonoBehaviour
{
    private AudioSource audioSource;

    public AudioClip hyperSpaceMain;
    public AudioClip hyperSpaceExit;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void enterHyperspace()
    {
        audioSource.loop = false;
        audioSource.Stop();
        audioSource.clip = hyperSpaceMain;
        audioSource.Play();
    }

    public void exitHyperspace()
    {
        audioSource.loop = false;
        audioSource.Stop();
        audioSource.clip = hyperSpaceExit;
        audioSource.Play();
    }
}