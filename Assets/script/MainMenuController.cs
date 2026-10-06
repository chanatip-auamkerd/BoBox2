using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("ชื่อ Scene สำหรับเลือกโหมดเล่นเกม")]
    public string playModeSceneName = "Playmodescene";

    [Tooltip("ชื่อ Scene ตั้งค่า")]
    public string settingSceneName = "settingscene";

    public void OpenPlayMode()
    {
        SceneManager.LoadScene(playModeSceneName);
    }

    public void OpenSetting()
    {
        SceneManager.LoadScene(settingSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game!");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#endif
    }
}