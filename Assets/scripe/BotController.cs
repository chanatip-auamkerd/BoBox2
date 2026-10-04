using System.Collections;
using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("References")]
    public Transform throwPoint;
    public GameObject projectilePrefab;
    public Transform targetPlayer;
    public TrajectoryLine botTrajectory;

    [Header("Reposition (เขตพื้นที่เดินของ Bot ฝั่งขวา)")]
    public float minX = 1.5f;
    public float maxX = 7.5f;
    public float moveSpeed = 4f;
    private float groundFixedY;

    [Header("Aiming & Delay Settings")]
    [Tooltip("เวลาที่บอทจะโชว์เส้นวิถียิงค้างไว้ให้ผู้เล่นลุ้น (วินาที)")]
    public float aimPreviewDuration = 1.2f;

    [Header("Bot Playstyle & Physics")]
    [Range(0f, 1f)] public float trickshotChance = 0.8f;
    public float maxArcHeight = 2.8f;
    public float maxLaunchSpeed = 16f;
    public float aimOffsetSpread = 0.6f;

    void Awake()
    {
        groundFixedY = transform.position.y;
    }

    public void StartBotTurn()
    {
        StartCoroutine(BotTurnRoutine());
    }

    private IEnumerator BotTurnRoutine()
    {
        float targetDestinationX = Random.Range(minX, maxX);

        while (Mathf.Abs(transform.position.x - targetDestinationX) > 0.05f)
        {
            float step = moveSpeed * Time.deltaTime;
            float newX = Mathf.MoveTowards(transform.position.x, targetDestinationX, step);
            transform.position = new Vector3(newX, groundFixedY, transform.position.z);

            if (targetDestinationX < transform.position.x)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }

            yield return null;
        }

        transform.position = new Vector3(targetDestinationX, groundFixedY, transform.position.z);
        transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(AimAndThrowRoutine());
    }

    private IEnumerator AimAndThrowRoutine()
    {
        if (targetPlayer == null || projectilePrefab == null || throwPoint == null)
        {
            Debug.LogWarning("บอทยังใส่ References ไม่ครบ!");
            yield break;
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
        float flightTime;
        Vector2 launchVelocity = CalculateVelocity(startPos, targetAimPoint, selectedArc, out flightTime);

        if (launchVelocity.magnitude > maxLaunchSpeed)
        {
            launchVelocity = launchVelocity.normalized * maxLaunchSpeed;
        }

        if (botTrajectory != null)
        {
            botTrajectory.ShowLine();
            botTrajectory.DrawTrajectoryToTarget(startPos, launchVelocity, flightTime);
        }
        yield return new WaitForSeconds(aimPreviewDuration);

        if (botTrajectory != null)
        {
            botTrajectory.HideLine();
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
            rb.linearVelocity = launchVelocity;
        }

        FindAnyObjectByType<TurnManager>()?.OnItemThrown(false);
    }

    private Vector2 CalculateVelocity(Vector2 start, Vector2 target, float extraHeight, out float totalTime)
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y);
        float apexY = Mathf.Max(start.y, target.y) + extraHeight;

        float vy = Mathf.Sqrt(2f * gravity * Mathf.Max(0.1f, apexY - start.y));
        float timeUp = vy / gravity;
        float timeDown = Mathf.Sqrt(2f * Mathf.Max(0.01f, apexY - target.y) / gravity);
        totalTime = Mathf.Max(0.1f, timeUp + timeDown);

        float vx = (target.x - start.x) / totalTime;
        return new Vector2(vx, vy);
    }
}