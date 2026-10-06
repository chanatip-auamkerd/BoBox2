using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("UI Audio Settings")]
    [Tooltip("ใส่ไฟล์เสียงคลิกปุ่ม Click1")]
    public AudioClip buttonClickSound;

    [Header("Timing Settings")]
    [Tooltip("จุดเริ่มเสียงคลิกจริง (วินาที)")]
    public float clickStartTime = 0.50f;

    private AudioSource audioSource;

    private void Awake()
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
            audioSource.spatialBlend = 0f;
        }
        else if (Instance != this)
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
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.Stop();
        audioSource.clip = buttonClickSound;
        audioSource.time = clickStartTime;
        audioSource.Play();
    }
}