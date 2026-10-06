using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MenuBGMController : MonoBehaviour
{
    public static MenuBGMController Instance { get; private set; }

    [Header("Scene ที่อนุญาตให้เพลงนี้เล่นต่อได้")]
    public string mainMenuSceneName = "MainMenu";
    public string playModeSceneName = "PlaymodeScene";
    public string settingSceneName = "Setting";

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentScene = scene.name;

        if (currentScene == mainMenuSceneName ||
            currentScene == playModeSceneName ||
            currentScene == settingSceneName)
        {
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}