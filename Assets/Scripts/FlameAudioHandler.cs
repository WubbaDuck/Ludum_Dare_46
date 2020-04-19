using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameAudioHandler : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip sizzle;
    public AudioClip slosh;
    public AudioClip jump;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound_Sizzle()
    {
        audioSource.Stop();
        audioSource.clip = sizzle;
        audioSource.Play();
    }

    public void PlaySound_Slosh()
    {
        audioSource.Stop();
        audioSource.clip = slosh;
        audioSource.Play();
    }

    public void PlaySound_Jump()
    {
        audioSource.Stop();
        audioSource.clip = jump;
        audioSource.Play();
    }
}
