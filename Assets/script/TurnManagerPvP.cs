using System.Collections;
using UnityEngine;
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

    public enum TurnState { RepositionPhase, AimPhase, WaitingForProjectile }
    public TurnState currentState;

    public int activePlayerIndex = 1;
    private float currentTimer;
    private bool isTimerRunning = false;
    private int lastThrowPlayerIndex = 1;

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
        StartCoroutine(SwitchTurnRoutine());
    }

    private IEnumerator SwitchTurnRoutine()
    {
        yield return new WaitForSeconds(0.6f);
        SwitchToNextPlayer();
    }

    private void SwitchToNextPlayer()
    {
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

    public PlayerController GetActivePlayer() => activePlayerIndex == 1 ? player1 : player2;
    public PlayerController GetInactivePlayer() => activePlayerIndex == 1 ? player2 : player1;
}