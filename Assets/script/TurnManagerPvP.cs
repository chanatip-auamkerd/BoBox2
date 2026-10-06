using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TurnManagerPvP : MonoBehaviour
{
    [Header("Participants")]
    public PlayerController player1;
    public PlayerController player2;

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI turnText;

    [Header("Phase Settings")]
    public float repositionDuration = 10f;
    public float turnDuration = 15f;

    [Header("Game Over Settings")]
    [Tooltip("ชื่อ Scene จบเกมของโหมด PvP")]
    public string pvpGameOverSceneName = "PvP_GameOverScene";

    public enum TurnState { RepositionPhase, AimPhase, WaitingForProjectile, GameOver }
    public TurnState currentState;

    public int activePlayerIndex = 1;
    private float currentTimer;
    private bool isTimerRunning = false;
    private int lastThrowPlayerIndex = 1;

    public static string winnerName = "Human";

    void Start()
    {
        activePlayerIndex = 1;

        if (player1 != null)
        {
            player1.isMyTurn = false;
            player1.isAimLocked = false;
            player1.isRepositionMode = false;
            if (player1.trajectory != null) player1.trajectory.HideLine();
        }

        if (player2 != null)
        {
            player2.isMyTurn = false;
            player2.isAimLocked = false;
            player2.isRepositionMode = false;
            if (player2.trajectory != null) player2.trajectory.HideLine();
        }

        StartRepositionPhase(player1);
    }

    void Update()
    {
        if (!isTimerRunning) return;

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

    public void StartRepositionPhase(PlayerController player)
    {
        if (currentState == TurnState.GameOver) return;

        currentState = TurnState.RepositionPhase;
        activePlayerIndex = player.playerIndex;

        if (turnText != null)
        {
            turnText.text = $"P{player.playerIndex}: MOVE POSITION";
            turnText.color = player.playerIndex == 1 ? Color.cyan : Color.red;
        }
        GetInactivePlayer().EnableRepositionMode(false);
        player.EnableRepositionMode(true);

        currentTimer = repositionDuration;
        isTimerRunning = true;
    }

    public void ConfirmRepositionEarly()
    {
        if (currentState == TurnState.RepositionPhase)
        {
            StartAimPhase(GetActivePlayer());
        }
    }

    public void StartAimPhase(PlayerController player)
    {
        if (currentState == TurnState.GameOver) return;

        currentState = TurnState.AimPhase;

        if (turnText != null)
        {
            turnText.text = $"P{player.playerIndex}'S TURN";
            turnText.color = player.playerIndex == 1 ? Color.cyan : Color.red;
        }

        player.EnableRepositionMode(false);
        player.StartPlayerTurn();

        currentTimer = turnDuration;
        isTimerRunning = true;
    }

    private void OnTimeOut()
    {
        isTimerRunning = false;
        PlayerController currentP = GetActivePlayer();

        if (currentState == TurnState.RepositionPhase)
        {
            StartAimPhase(currentP);
        }
        else if (currentState == TurnState.AimPhase)
        {
            currentP.isMyTurn = false;
            currentP.isAimLocked = false;
            if (currentP.trajectory != null) currentP.trajectory.HideLine();
            currentP.UpdateUIState();

            SwitchToNextPlayer();
        }
    }

    public void OnItemThrown(int thrownByPlayerIndex)
    {
        isTimerRunning = false;
        lastThrowPlayerIndex = thrownByPlayerIndex;
        currentState = TurnState.WaitingForProjectile;

        if (turnText != null)
        {
            turnText.text = $"P{thrownByPlayerIndex} ATTACKING...";
            turnText.color = Color.yellow;
        }
    }

    public void OnProjectileDestroyed()
    {
        if (currentState == TurnState.GameOver) return;
        StartCoroutine(SwitchTurnRoutine());
    }

    private IEnumerator SwitchTurnRoutine()
    {
        yield return new WaitForSeconds(0.6f);

        if (CheckGameOver()) yield break;

        SwitchToNextPlayer();
    }

    private void SwitchToNextPlayer()
    {
        if (currentState == TurnState.GameOver) return;

        if (lastThrowPlayerIndex == 1)
        {
            StartRepositionPhase(player2);
        }
        else
        {
            PropSpawner.Instance?.RespawnAllProps();
            StartRepositionPhase(player1);
        }
    }
    private bool CheckGameOver()
    {
        if (player1 != null && player1.GetComponent<CharacterHealth>() != null && player1.GetComponent<CharacterHealth>().currentHealth <= 0)
        {
            TriggerGameOver("Ghost");
            return true;
        }

        if (player2 != null && player2.GetComponent<CharacterHealth>() != null && player2.GetComponent<CharacterHealth>().currentHealth <= 0)
        {
            TriggerGameOver("Human");
            return true;
        }

        return false;
    }
    public void TriggerGameOver(string winner)
    {
        currentState = TurnState.GameOver;
        isTimerRunning = false;
        winnerName = winner; 

        if (turnText != null)
        {
            turnText.text = $"{winner.ToUpper()} WINS!";
            turnText.color = winner == "Human" ? Color.cyan : Color.red;
        }

        StartCoroutine(LoadGameOverSceneRoutine());
    }

    private IEnumerator LoadGameOverSceneRoutine()
    {
        yield return new WaitForSeconds(1.5f); 
        SceneManager.LoadScene(pvpGameOverSceneName);
    }

    public PlayerController GetActivePlayer() => activePlayerIndex == 1 ? player1 : player2;
    public PlayerController GetInactivePlayer() => activePlayerIndex == 1 ? player2 : player1;
}