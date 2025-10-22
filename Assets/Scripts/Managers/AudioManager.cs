using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")] 
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Default Volumes")] 
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.5f;

    private Dictionary<string, AudioClip> sfxLibrary = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null)
        {
            var go = new GameObject("SFX Source");
            go.transform.SetParent(transform);
            sfxSource = go.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        if (musicSource == null)
        {
            var go = new GameObject("Music Source");
            go.transform.SetParent(transform);
            musicSource = go.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        
        sfxSource.volume = sfxVolume;
        musicSource.volume = musicVolume;
    }
    
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volumeScale * sfxVolume);
    }
    
    public void PlaySFX(string clipKey, float volumeScale = 1f)
    {
        if (sfxLibrary.TryGetValue(clipKey, out var clip))
            PlaySFX(clip, volumeScale);
    }
    
    public void RegisterClip(string key, AudioClip clip)
    {
        if (!sfxLibrary.ContainsKey(key))
            sfxLibrary.Add(key, clip);
    }
    
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.Play();
    }
    
    public void StopMusic() => musicSource.Stop();

    public void StopSFX()
    {
        if (sfxSource == null) return;
        sfxSource.Stop();
    }
}
