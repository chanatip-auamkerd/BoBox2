using System.Collections;
using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("References")]
    public Transform throwPoint;
    public GameObject projectilePrefab;
    public Transform targetPlayer;

    [Header("Bot Playstyle & Physics")]
    [Range(0f, 1f)] public float trickshotChance = 0.8f;
    public float maxArcHeight = 2.8f;
    public float maxLaunchSpeed = 16f;
    public float aimOffsetSpread = 0.6f;

    public void StartBotTurn()
    {
        StartCoroutine(BotThinkAndThrowRoutine());
    }

    private IEnumerator BotThinkAndThrowRoutine()
    {
        // ยืนคิดและเล็ง
        yield return new WaitForSeconds(Random.Range(1.2f, 1.8f));

        ThrowWithTrickshot();
    }

    private void ThrowWithTrickshot()
    {
        if (targetPlayer == null || projectilePrefab == null || throwPoint == null)
        {
            Debug.LogWarning("บอทยังใส่ References ไม่ครบ!");
            return;
        }

        Vector2 startPos = throwPoint.position;
        Vector2 targetAimPoint;

        GameObject[] props = GameObject.FindGameObjectsWithTag("Prop");
        bool tryBounce = (props.Length > 0) && (Random.value < trickshotChance);

        if (tryBounce)
        {
            GameObject chosenProp = props[Random.Range(0, props.Length)];
            targetAimPoint = chosenProp.transform.position;
            targetAimPoint += new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(0.1f, 0.4f));
        }
        else
        {
            targetAimPoint = targetPlayer.position;
            targetAimPoint.x += Random.Range(-1.5f, 1.5f);
        }

        targetAimPoint += new Vector2(
            Random.Range(-aimOffsetSpread, aimOffsetSpread),
            Random.Range(-aimOffsetSpread, aimOffsetSpread)
        );

        float selectedArc = Random.Range(1.5f, maxArcHeight);
        Vector2 velocity = CalculateVelocity(startPos, targetAimPoint, selectedArc);

        if (velocity.magnitude > maxLaunchSpeed)
        {
            velocity = velocity.normalized * maxLaunchSpeed;
        }

        GameObject obj = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
        obj.tag = "Projectile";

        Collider2D botCol = GetComponent<Collider2D>();
        Collider2D projCol = obj.GetComponent<Collider2D>();
        if (botCol != null && projCol != null)
        {
            Physics2D.IgnoreCollision(botCol, projCol);
        }

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }

        // แจ้ง TurnManager ว่าบอทปาแล้ว
        FindAnyObjectByType<TurnManager>()?.OnItemThrown(false);
    }

    private Vector2 CalculateVelocity(Vector2 start, Vector2 target, float extraHeight)
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y);
        float apexY = Mathf.Max(start.y, target.y) + extraHeight;

        float vy = Mathf.Sqrt(2f * gravity * Mathf.Max(0.1f, apexY - start.y));
        float timeUp = vy / gravity;
        float timeDown = Mathf.Sqrt(2f * Mathf.Max(0.01f, apexY - target.y) / gravity);
        float totalTime = Mathf.Max(0.1f, timeUp + timeDown);

        float vx = (target.x - start.x) / totalTime;
        return new Vector2(vx, vy);
    }
}