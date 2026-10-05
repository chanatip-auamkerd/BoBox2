using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PvPGameOverUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI winnerText;
    public Button playAgainButton;
    public Button mainMenuButton;

    [Header("Scene Names")]
    public string pvpSceneName = "PvP_Scene";
    public string mainMenuSceneName = "MainMenu"; 

    void Start()
    {
        if (winnerText != null)
        {
            winnerText.text = CharacterHealth.winnerMessage;

            if (CharacterHealth.winnerIndex == 1)
            {
                winnerText.color = Color.cyan;
            }
            else
            {
                winnerText.color = new Color(1f, 0.35f, 0.35f);
            }
        }

        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(PlayAgain);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(pvpSceneName);
    }

    public void GoToMainMenu()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}