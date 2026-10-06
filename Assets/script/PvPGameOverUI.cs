using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PvPGameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text สำหรับแสดงว่าใครชนะ (HUMAN WINS! / GHOST WINS!)")]
    public TextMeshProUGUI winnerText;

    [Tooltip("Text รองสำหรับคำแซว/คำขิง")]
    public TextMeshProUGUI subWinnerText;

    public Button playAgainButton;
    public Button mainMenuButton;

    [Header("Scene Names")]
    public string pvpSceneName = "PvP_Scene";
    public string mainMenuSceneName = "MainMenu_Scene";

    [Header("Colors")]
    public Color humanColor = Color.cyan;
    public Color ghostColor = new Color(1f, 0.35f, 0.35f);

    void Start()
    {
        SetupWinnerDisplay();

        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(PlayAgain);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
    }

    private void SetupWinnerDisplay()
    {
        bool isHumanWin = (CharacterHealth.winnerIndex == 1) || (TurnManagerPvP.winnerName == "Human");

        if (isHumanWin)
        {
            if (winnerText != null)
            {
                winnerText.text = "HUMAN WINS!";
                winnerText.color = humanColor;
            }

            if (subWinnerText != null)
            {
                subWinnerText.text = "THE GHOST GOT BUSTED!";
            }
        }
        else 
        {
            if (winnerText != null)
            {
                winnerText.text = "GHOST WINS!";
                winnerText.color = ghostColor;
            }

            if (subWinnerText != null)
            {
                subWinnerText.text = "YOU GOT SPOOKED, HUMAN!";
            }
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