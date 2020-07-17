using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    private AudioSource audioSource;

    public AudioClip attack;
    public AudioClip altAttack;
    public AudioClip ability1;
    public AudioClip ability2;

    public AudioClip destroyed;
    public AudioClip lowHP;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void attackSound()
    {
        audioSource.PlayOneShot(attack);
    }

    public void alternateAttack()
    {
        audioSource.PlayOneShot(altAttack);
        //audioSource.Stop();
        //audioSource.clip = altAttack;
        //audioSource.Play();
    }

    public void ability1Sound()
    {
        audioSource.PlayOneShot(ability1);
        //audioSource.Stop();
        //audioSource.clip = ability1 ;
        //audioSource.Play();
    }

    public void ability2Sound()
    {
        audioSource.PlayOneShot(ability2);
        //audioSource.Stop();
        //audioSource.clip = ability2;
        //audioSource.Play();
    }

    public void dieSound()
    {
        audioSource.PlayOneShot(destroyed);
        //audioSource.Stop();
        //audioSource.clip = destroyed;
        //audioSource.Play();
    }

    public void lowHealth()
    {
        audioSource.PlayOneShot(lowHP);
        //audioSource.Stop();
        //audioSource.clip = lowHP;
        //audioSource.Play();
    }
}