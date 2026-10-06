using UnityEngine;
using UnityEngine.SceneManagement;

public class WinSceneController : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("ใส่ชื่อไฟล์ Scene ของด่านที่เล่นสู้กับบอท")]
    public string botGameSceneName = "GameScene";

    [Tooltip("ใส่ชื่อไฟล์ Scene ของหน้าแรก")]
    public string mainMenuSceneName = "MainMenu_Scene";

    public void PlayAgainVsBot()
    {
        SceneManager.LoadScene(botGameSceneName);
    }
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}