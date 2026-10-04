using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterHealth : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("ติ๊กถูกถ้าคอมโพเนนต์นี้แปะอยู่ที่ตัว Player (ถ้าเป็น Bot ให้ติ๊กออก)")]
    public bool isPlayer = true;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthSlider;

    [Header("Game Over Scene Names")]
    public string winSceneName = "WinScene";
    public string loseSceneName = "LoseScene";
    public float sceneLoadDelay = 1.0f; 

    private bool isDead = false;

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

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        FindAnyObjectByType<TurnManager>()?.gameObject.SetActive(false);

        StartCoroutine(LoadGameOverSceneRoutine());
    }

    private IEnumerator LoadGameOverSceneRoutine()
    {
        yield return new WaitForSeconds(sceneLoadDelay);

        if (isPlayer)
        {
            SceneManager.LoadScene(loseSceneName);
        }
        else
        {
            SceneManager.LoadScene(winSceneName);
        }
    }
}