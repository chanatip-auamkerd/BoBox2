using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterHealth : MonoBehaviour
{
    public static string winnerMessage = "PLAYER 1 WINS!";
    public static int winnerIndex = 1;

    [Header("Identity")]
    public bool isPlayer = true;
    [Tooltip("1 สำหรับ P1 (คน), 2 สำหรับ P2 (ผี)")]
    public int playerIndex = 1;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthSlider;

    [Header("Hurt Sound Timing (ปรับแต่งช่วงเสียงเองได้)")]
    [Tooltip("ใส่ไฟล์เสียงเจ็บ SufferingDamage1")]
    public AudioClip hurtSoundClip;

    [Tooltip("วินาทีเริ่มต้นของท่อนเสียงที่ต้องการ (เช่น 2.5)")]
    public float hurtStartTime = 2.50f;

    [Tooltip("วินาทีสิ้นสุดของท่อนเสียง (ถ้าตั้งค่านี้ ระบบจะหยุดเล่นเมื่อถึงวินาทีนี้)")]
    public float hurtEndTime = 3.35f;

    [Range(0f, 1f)]
    public float hurtVolume = 1.0f;

    [Header("Scene Settings")]
    [Tooltip("ชื่อ Scene หน้าจบเกมของโหมด PvP")]
    public string pvpGameOverScene = "PvP_GameOverScene";
    public string soloWinScene = "WinScene";
    public string soloLoseScene = "LoseScene";
    public float sceneLoadDelay = 1.0f;

    private bool isDead = false;
    private AudioSource audioSource;
    private Coroutine stopHurtCoroutine;

    void Awake()
    {
        SetupAudioSource();
    }

    private void SetupAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        PlayHurtSound();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    [ContextMenu("Test Hurt Sound")]
    public void PlayHurtSound()
    {
        if (hurtSoundClip == null)
        {
            Debug.LogWarning($"[HurtSound] {gameObject.name}: ยังไม่ได้ใส่ไฟล์ AudioClip!");
            return;
        }

        SetupAudioSource();

        if (stopHurtCoroutine != null)
        {
            StopCoroutine(stopHurtCoroutine);
        }

        audioSource.Stop();
        audioSource.clip = hurtSoundClip;

        float sfxMaster = (SoundManager.Instance != null) ? SoundManager.Instance.SFXVolume : 1.0f;
        audioSource.volume = hurtVolume * sfxMaster;

        float validStart = Mathf.Clamp(hurtStartTime, 0f, hurtSoundClip.length - 0.05f);
        audioSource.Play();
        audioSource.time = validStart;

        float duration = Mathf.Max(0.05f, hurtEndTime - validStart);
        stopHurtCoroutine = StartCoroutine(StopHurtSoundRoutine(duration));
    }

    private IEnumerator StopHurtSoundRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        var pvpManager = FindAnyObjectByType<TurnManagerPvP>(FindObjectsInactive.Include);
        bool isPvP = (pvpManager != null);
        var soloManager = FindAnyObjectByType<TurnManager>(FindObjectsInactive.Include);
        if (soloManager != null) soloManager.enabled = false;
        if (pvpManager != null) pvpManager.enabled = false;

        StartCoroutine(LoadGameOverSceneRoutine(isPvP));
    }

    private IEnumerator LoadGameOverSceneRoutine(bool isPvP)
    {
        yield return new WaitForSeconds(sceneLoadDelay);

        if (isPvP)
        {
            if (playerIndex == 1)
            {
                winnerIndex = 2;
                winnerMessage = "GHOST WINS!";
                TurnManagerPvP.winnerName = "Ghost";
            }
            else
            {
                winnerIndex = 1;
                winnerMessage = "HUMAN WINS!";
                TurnManagerPvP.winnerName = "Human";
            }

            SceneManager.LoadScene(pvpGameOverScene);
        }
        else
        {
            if (isPlayer)
            {
                SceneManager.LoadScene(soloLoseScene);
            }
            else
            {
                SceneManager.LoadScene(soloWinScene);
            }
        }
    }
}