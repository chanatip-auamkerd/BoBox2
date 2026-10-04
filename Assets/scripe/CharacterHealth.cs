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
    [Tooltip("1 สำหรับ P1, 2 สำหรับ P2")]
    public int playerIndex = 1;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthSlider;

    [Header("Scene Settings")]
    [Tooltip("ชื่อ Scene หน้าจบเกมของโหมด PvP")]
    public string pvpGameOverScene = "PvP_GameOverScene";
    public string soloWinScene = "WinScene";
    public string soloLoseScene = "LoseScene";
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
        FindAnyObjectByType<TurnManagerPvP>()?.gameObject.SetActive(false);

        StartCoroutine(LoadGameOverSceneRoutine());
    }

    private IEnumerator LoadGameOverSceneRoutine()
    {
        yield return new WaitForSeconds(sceneLoadDelay);

        bool isPvP = FindAnyObjectByType<TurnManagerPvP>() != null || playerIndex == 2;

        if (isPvP)
        {
            if (playerIndex == 1)
            {
                winnerIndex = 2;
                winnerMessage = "PLAYER 2 WINS!";
            }
            else
            {
                winnerIndex = 1;
                winnerMessage = "PLAYER 1 WINS!";
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