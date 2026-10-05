using System.Collections;
using UnityEngine;
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

    public enum TurnState { RepositionPhase, PlayerTurn, BotTurn, WaitingForProjectile }
    public TurnState currentState;

    private float currentTimer;
    private bool isTimerRunning = false;
    private bool wasPlayerLastThrow = false;

    void Start()
    {
        StartPlayerRepositionPhase();
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
    public void StartPlayerRepositionPhase()
    {
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
        if (currentState == TurnState.RepositionPhase)
        {
            StartPlayerTurn();
        }
    }
    public void StartPlayerTurn()
    {
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
        StartCoroutine(SwitchTurnRoutine());
    }

    private IEnumerator SwitchTurnRoutine()
    {
        yield return new WaitForSeconds(0.6f);

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
}