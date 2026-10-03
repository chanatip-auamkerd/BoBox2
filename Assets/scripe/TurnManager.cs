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

    [Header("Turn Timer Settings")]
    public float turnDuration = 15f;
    private float currentTurnTime;
    private bool isTimerRunning = false;

    public enum TurnState { PlayerTurn, BotTurn, WaitingForProjectile }
    public TurnState currentState;

    private bool wasPlayerLastThrow = false;

    void Start()
    {
        StartPlayerTurn();
    }

    void Update()
    {
        if (isTimerRunning)
        {
            currentTurnTime -= Time.deltaTime;

            if (timerText != null)
            {
                int totalSeconds = Mathf.CeilToInt(Mathf.Max(0, currentTurnTime));
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;

                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

                timerText.color = totalSeconds <= 5 ? Color.red : Color.white;
            }

            if (currentTurnTime <= 0)
            {
                OnTimeOut();
            }
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

        player.StartPlayerTurn();
        ResetAndStartTimer();
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
        ResetAndStartTimer();
    }

    private void ResetAndStartTimer()
    {
        currentTurnTime = turnDuration;
        isTimerRunning = true;
    }

    private void StopTimer()
    {
        isTimerRunning = false;
    }
    private void OnTimeOut()
    {
        StopTimer();
        Debug.Log("⏰ เวลาหมด! สลับเทิร์น");

        if (currentState == TurnState.PlayerTurn)
        {
            player.isMyTurn = false;
            player.isAimLocked = false;
            if (player.trajectory != null) player.trajectory.HideLine();

            StartBotTurn();
        }
        else if (currentState == TurnState.BotTurn)
        {
            StartPlayerTurn();
        }
    }
    public void OnItemThrown(bool isPlayer)
    {
        StopTimer(); 
        wasPlayerLastThrow = isPlayer;
        currentState = TurnState.WaitingForProjectile;

        if (turnText != null)
        {
            turnText.text = "ATTACKING...!"; 
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
            StartPlayerTurn();
        }
    }
}
