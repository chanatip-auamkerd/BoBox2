using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public PlayerController player;
    public BotController bot;

    public void OnPlayerFinishedTurn()
    {
        // ตาผู้เล่นจบ ส่งต่อไปรอบบอท
        bot.StartBotTurn();
    }

    public void OnBotFinishedTurn()
    {
        // ตาบอทจบ สลับกลับมาเปิดให้ผู้เล่นเล็งได้ใหม่
        player.isMyTurn = true;
    }
}
