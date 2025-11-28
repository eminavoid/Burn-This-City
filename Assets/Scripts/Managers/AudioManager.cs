using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Scene Music")]
    public AudioClip mainMenuMusic;

    [Header("Audio Sources")] 
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    public AudioSource uiFeedbackSource;
    
    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    private Coroutine currentFade;

    [Header("Default Volumes")] 
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.5f;
    
    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;

    private Dictionary<string, AudioClip> sfxLibrary = new Dictionary<string, AudioClip>();

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main Menu")
        {
            if (musicSource.clip != mainMenuMusic)
            {
                PlayMusic(mainMenuMusic);
            }
        }
    }
    
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
        
        LoadSavedVolumes();
        
        sfxSource.volume = sfxVolume;
        musicSource.volume = musicVolume;
    }
    
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volumeScale * sfxVolume);
    }
    
    public void PlayUIFeedback(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        uiFeedbackSource.PlayOneShot(clip, volumeScale * sfxVolume);
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

        if (currentFade != null)
        {
            StopCoroutine(currentFade);
        }
        
        currentFade = StartCoroutine(FadeMusicRoutine(clip));
        // musicSource.clip = clip;
        // musicSource.Play();
    }
    
    public void StopMusic() => musicSource.Stop();

    public void StopSFX()
    {
        if (sfxSource == null) return;
        sfxSource.Stop();
    }
    
    private IEnumerator FadeMusicRoutine(AudioClip newClip)
    {
        float startVolume = musicSource.volume;

        // --- FADE OUT ---
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();

        // Cambiamos track
        musicSource.clip = newClip;
        musicSource.Play();

        // --- FADE IN ---
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = startVolume;
        currentFade = null;
    }
    
    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        musicSource.volume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        sfxSource.volume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
    
    private void LoadSavedVolumes()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
            musicVolume = PlayerPrefs.GetFloat("MusicVolume");

        if (PlayerPrefs.HasKey("SFXVolume"))
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume");
    }
}
