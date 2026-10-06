using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WinSceneController : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("ลาก TextMeshPro ของช่อง Subtext มาใส่ที่นี่")]
    public TextMeshProUGUI subText;

    [Header("Subtext Content")]
    [TextArea(2, 4)]
    [Tooltip("ข้อความที่จะแสดงในช่องว่าง")]
    public string winMessage = "YOU SAVED ALL THE CANDIES!\nTHE GHOST HAS BEEN DEFEATED!";

    [Header("Scene Settings")]
    [Tooltip("ใส่ชื่อไฟล์ Scene ของด่านที่เล่นสู้กับบอท")]
    public string botGameSceneName = "1";

    [Tooltip("ใส่ชื่อไฟล์ Scene ของหน้าแรก")]
    public string mainMenuSceneName = "MainMenu_Scene";

    void Start()
    {
        if (subText != null)
        {
            subText.text = winMessage;
        }
    }
    public void PlayAgainVsBot()
    {
        SceneManager.LoadScene(botGameSceneName);
    }
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}