using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoseSceneController : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("ลาก TextMeshPro ของข้อความแพ้มาใส่ที่นี่")]
    public TextMeshProUGUI loseText;

    [Header("Subtext Message")]
    [TextArea(2, 4)]
    [Tooltip("ข้อความที่แสดงในกรอบสีม่วง")]
    public string defaultLoseMessage = "NO CANDY LEFT FOR ME...\nTHE GHOST TOOK THEM ALL!";

    [Header("Scene Settings")]
    [Tooltip("ชื่อ Scene ด่านเล่นกับบอท")]
    public string botGameSceneName = "1";

    [Tooltip("ชื่อ Scene หน้าแรก")]
    public string mainMenuSceneName = "MainMenu_Scene";

    void Start()
    {
        if (loseText != null)
        {
            loseText.text = defaultLoseMessage;
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