using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseSceneController : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("ใส่ชื่อ Scene ด่านเล่นกับบอท")]
    public string botGameSceneName = "GameScene";

    [Tooltip("ใส่ชื่อ Scene หน้าแรก")]
    public string mainMenuSceneName = "MainMenu_Scene";

    public void RetryGame()
    {
        SceneManager.LoadScene(botGameSceneName);
    }
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}