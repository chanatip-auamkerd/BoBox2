using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BGMVolumeListener : MonoBehaviour
{
    private AudioSource audioSource;
    private float initialVolume = 1f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        initialVolume = audioSource.volume;
    }

    void OnEnable()
    {
        SoundManager.OnBGMVolumeChanged += UpdateVolume;
        if (SoundManager.Instance != null)
        {
            UpdateVolume(SoundManager.Instance.BGMVolume);
        }
    }

    void OnDisable()
    {
        SoundManager.OnBGMVolumeChanged -= UpdateVolume;
    }

    private void UpdateVolume(float globalBGMVolume)
    {
        if (audioSource != null)
        {
            audioSource.volume = initialVolume * globalBGMVolume;
        }
    }
}