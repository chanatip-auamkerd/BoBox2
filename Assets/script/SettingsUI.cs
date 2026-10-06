using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Back Button")]
    public Button backButton;
    public string menuSceneName = "MainMenu";

    [Header("Button Sound Settings")]
    public AudioClip clickSound;
    [Tooltip("วินาทีที่เสียงคลิกเริ่มดังจริง (ข้ามช่วงเงียบด้านหน้า)")]
    public float clickStartTime = 0.48f;
    [Range(0f, 1f)]
    public float clickVolume = 1.0f;
    [Tooltip("หน่วงเวลารอให้เสียงคลิกดังจบก่อนเปลี่ยนฉาก")]
    public float sceneLoadDelay = 0.25f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    void Start()
    {
        if (SoundManager.Instance == null)
        {
            GameObject soundManagerObj = new GameObject("SoundManager");
            soundManagerObj.AddComponent<SoundManager>();
        }

        if (bgmSlider != null)
        {
            bgmSlider.minValue = 0f;
            bgmSlider.maxValue = 1f;
            bgmSlider.value = SoundManager.Instance.BGMVolume;
            bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.value = SoundManager.Instance.SFXVolume;
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }

    public void OnBGMChanged(float val)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetBGMVolume(val);
        }
    }

    public void OnSFXChanged(float val)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(val);
        }
    }

    public void OnBackButtonClicked()
    {
        PlayClickSound();
        StartCoroutine(LoadSceneRoutine());
    }

    public void PlayClickSound()
    {
        if (clickSound == null || audioSource == null) return;

        float sfxScale = (SoundManager.Instance != null) ? SoundManager.Instance.SFXVolume : 1.0f;
        audioSource.clip = clickSound;
        audioSource.volume = clickVolume * sfxScale;

        float validStart = Mathf.Clamp(clickStartTime, 0f, clickSound.length - 0.05f);
        audioSource.time = validStart;
        audioSource.Play();
    }

    private IEnumerator LoadSceneRoutine()
    {
        yield return new WaitForSeconds(sceneLoadDelay);

        if (!string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }
}