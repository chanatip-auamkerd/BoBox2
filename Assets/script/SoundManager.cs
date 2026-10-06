using UnityEngine;
using System;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Default Volumes")]
    [Range(0f, 1f)] public float defaultBGMVolume = 0.7f;
    [Range(0f, 1f)] public float defaultSFXVolume = 1.0f;

    public float BGMVolume { get; private set; }
    public float SFXVolume { get; private set; }

    public static event Action<float> OnBGMVolumeChanged;
    public static event Action<float> OnSFXVolumeChanged;

    private const string BGM_KEY = "Sound_BGM_Volume";
    private const string SFX_KEY = "Sound_SFX_Volume";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolumeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadVolumeSettings()
    {
        BGMVolume = PlayerPrefs.GetFloat(BGM_KEY, defaultBGMVolume);
        SFXVolume = PlayerPrefs.GetFloat(SFX_KEY, defaultSFXVolume);
    }

    public void SetBGMVolume(float volume)
    {
        BGMVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(BGM_KEY, BGMVolume);
        PlayerPrefs.Save();
        OnBGMVolumeChanged?.Invoke(BGMVolume);
    }

    public void SetSFXVolume(float volume)
    {
        SFXVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_KEY, SFXVolume);
        PlayerPrefs.Save();
        OnSFXVolumeChanged?.Invoke(SFXVolume);
    }
}