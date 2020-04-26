using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameAudioHandler : MonoBehaviour
{
    public AudioClip sizzle;
    public AudioClip slosh;
    public AudioClip jump;

    private AudioSource audioSource_Sizzle;
    private AudioSource audioSource_Slosh;
    private AudioSource audioSource_Jump;

    public void PlaySound_Sizzle()
    {
        audioSource_Sizzle = gameObject.AddComponent<AudioSource>();
        audioSource_Sizzle.clip = sizzle;
        audioSource_Sizzle.Play();
        StartCoroutine(PlaySound(audioSource_Sizzle));
    }

    public void PlaySound_Slosh()
    {
        audioSource_Slosh = gameObject.AddComponent<AudioSource>();
        audioSource_Slosh.clip = slosh;
        audioSource_Slosh.Play();
        StartCoroutine(PlaySound(audioSource_Slosh));
    }

    public void PlaySound_Jump()
    {
        audioSource_Jump = gameObject.AddComponent<AudioSource>();
        audioSource_Jump.clip = jump;
        audioSource_Jump.Play();
        StartCoroutine(PlaySound(audioSource_Jump));
    }

    private IEnumerator PlaySound(AudioSource source)
    {
        while(source.isPlaying)
        {
            yield return new WaitForSeconds(0.1f);
        };

        Destroy(source);
    }
}
