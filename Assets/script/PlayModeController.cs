using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayModeController : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("ชื่อ Scene เล่นกับเพื่อน (PVP)")]
    public string pvpSceneName = "PvP_Scene";

    [Tooltip("ชื่อ Scene เล่นกับบอท")]
    public string botSceneName = "1";

    [Tooltip("ชื่อ Scene หน้าเมนูหลัก")]
    public string mainMenuSceneName = "MainMenu_Scene";

    public void PlayWithPlayer()
    {
        SceneManager.LoadScene(pvpSceneName);
    }
    public void PlayWithBot()
    {
        SceneManager.LoadScene(botSceneName);
    }
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}