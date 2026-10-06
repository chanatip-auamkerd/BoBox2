using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TurnManager : MonoBehaviour
{
    [Header("Participants")]
    public PlayerController player;
    public BotController bot;

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI turnText;

    [Header("Phase Settings")]
    public float repositionDuration = 10f;
    public float turnDuration = 15f;

    [Header("Game Over & Scene Settings")]
    [Tooltip("ชื่อ Scene เมื่อผู้เล่นชนะบอท")]
    public string winSceneName = "WinScene";

    [Tooltip("ชื่อ Scene เมื่อผู้เล่นแพ้บอท")]
    public string loseSceneName = "LoseScene";

    [Tooltip("ระยะเวลาหน่วงก่อนโหลดฉากจบ (วินาที) เพื่อให้เสียงหรือเอฟเฟกต์เล่นจบ")]
    public float delayBeforeGameOverScene = 1.0f;

    [Tooltip("ลาก GameObject หรือ AudioSource ของเพลง BGM มาใส่ (ถ้าไม่ใส่ระบบจะหาให้อัตโนมัติ)")]
    public AudioSource battleBgmAudioSource;

    public enum TurnState { RepositionPhase, PlayerTurn, BotTurn, WaitingForProjectile, GameOver }
    public TurnState currentState;

    private float currentTimer;
    private bool isTimerRunning = false;
    private bool wasPlayerLastThrow = false;
    private bool isGameOver = false;

    void Start()
    {
        if (battleBgmAudioSource == null)
        {
            GameObject bgmObj = GameObject.Find("BGM_Battle");
            if (bgmObj != null)
            {
                battleBgmAudioSource = bgmObj.GetComponent<AudioSource>();
            }
        }

        StartPlayerRepositionPhase();
    }

    void Update()
    {
        if (isGameOver || !isTimerRunning) return;

        currentTimer -= Time.deltaTime;

        if (timerText != null)
        {
            int totalSeconds = Mathf.CeilToInt(Mathf.Max(0, currentTimer));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            timerText.color = totalSeconds <= 3 ? Color.red : Color.white;
        }

        if (currentTimer <= 0)
        {
            OnTimeOut();
        }
    }

    public void StartPlayerRepositionPhase()
    {
        if (isGameOver) return;

        currentState = TurnState.RepositionPhase;
        if (turnText != null)
        {
            turnText.text = "MOVE POSITION";
            turnText.color = Color.yellow;
        }

        player.EnableRepositionMode(true);
        currentTimer = repositionDuration;
        isTimerRunning = true;
    }

    public void ConfirmRepositionEarly()
    {
        if (currentState == TurnState.RepositionPhase && !isGameOver)
        {
            StartPlayerTurn();
        }
    }

    public void StartPlayerTurn()
    {
        if (isGameOver) return;

        currentState = TurnState.PlayerTurn;
        if (turnText != null)
        {
            turnText.text = "YOUR TURN";
            turnText.color = Color.cyan;
        }

        player.EnableRepositionMode(false);
        player.StartPlayerTurn();

        currentTimer = turnDuration;
        isTimerRunning = true;
    }

    public void StartBotTurn()
    {
        if (isGameOver) return;

        currentState = TurnState.BotTurn;
        if (turnText != null)
        {
            turnText.text = "ENEMY'S TURN";
            turnText.color = Color.red;
        }

        bot.StartBotTurn();
        currentTimer = turnDuration;
        isTimerRunning = true;
    }

    private void OnTimeOut()
    {
        if (isGameOver) return;
        isTimerRunning = false;

        if (currentState == TurnState.RepositionPhase)
        {
            StartPlayerTurn();
        }
        else if (currentState == TurnState.PlayerTurn)
        {
            player.isMyTurn = false;
            player.isAimLocked = false;
            if (player.trajectory != null) player.trajectory.HideLine();
            StartBotTurn();
        }
        else if (currentState == TurnState.BotTurn)
        {
            PropSpawner.Instance?.RespawnAllProps();
            StartPlayerRepositionPhase();
        }
    }

    public void OnItemThrown(bool isPlayer)
    {
        if (isGameOver) return;

        isTimerRunning = false;
        wasPlayerLastThrow = isPlayer;
        currentState = TurnState.WaitingForProjectile;

        if (turnText != null)
        {
            turnText.text = "ATTACKING...";
            turnText.color = Color.yellow;
        }
    }

    public void OnProjectileDestroyed()
    {
        if (isGameOver) return;
        StartCoroutine(SwitchTurnRoutine());
    }

    private IEnumerator SwitchTurnRoutine()
    {
        yield return new WaitForSeconds(0.6f);

        if (isGameOver) yield break;

        if (wasPlayerLastThrow)
        {
            StartBotTurn();
        }
        else
        {
            PropSpawner.Instance?.RespawnAllProps();
            StartPlayerRepositionPhase();
        }
    }
    public void OnPlayerWin()
    {
        if (isGameOver) return;
        isGameOver = true;
        isTimerRunning = false;
        currentState = TurnState.GameOver;

        if (turnText != null)
        {
            turnText.text = "VICTORY!";
            turnText.color = Color.green;
        }

        StopBattleBGM();
        StartCoroutine(LoadGameOverSceneRoutine(winSceneName));
    }
    public void OnPlayerLose()
    {
        if (isGameOver) return;
        isGameOver = true;
        isTimerRunning = false;
        currentState = TurnState.GameOver;

        if (turnText != null)
        {
            turnText.text = "DEFEAT!";
            turnText.color = Color.red;
        }

        StopBattleBGM();
        StartCoroutine(LoadGameOverSceneRoutine(loseSceneName));
    }

    private void StopBattleBGM()
    {
        if (battleBgmAudioSource != null && battleBgmAudioSource.isPlaying)
        {
            battleBgmAudioSource.Stop();
        }
    }

    private IEnumerator LoadGameOverSceneRoutine(string sceneName)
    {
        yield return new WaitForSeconds(delayBeforeGameOverScene);
        SceneManager.LoadScene(sceneName);
    }
}