using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("UI Audio Settings")]
    public AudioClip buttonClickSound;
    [Range(0f, 1f)]
    public float clickVolume = 1.0f;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f; 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayButtonClick()
    {
        if (buttonClickSound == null) return;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        float sfxScale = (SoundManager.Instance != null) ? SoundManager.Instance.SFXVolume : 1.0f;

        audioSource.PlayOneShot(buttonClickSound, clickVolume * sfxScale);
    }
}