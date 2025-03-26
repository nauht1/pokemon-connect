using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource musicSource;
    public AudioSource SFXSouce;

    [Header("Audio Clip")]
    public AudioClip background;
    public AudioClip connect;
    public AudioClip wrongConnect;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSouce.PlayOneShot(clip);
    }
}
