using System.Collections;
using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("References")]
    public Transform throwPoint;
    public GameObject projectilePrefab;
    public Transform targetPlayer; // พิกัดของผู้เล่น

    [Header("AI Balance")]
    [Range(0f, 1f)] public float accuracy = 0.85f; // ค่าความแม่น (0 = มั่ว, 1 = เข้าเป้า 100%)
    public float launchAngleDegree = 55f; // มุมยิงวิถีโด่งขึ้นฟ้าของบอท

    public void StartBotTurn()
    {
        StartCoroutine(BotThinkAndThrowRoutine());
    }

    private IEnumerator BotThinkAndThrowRoutine()
    {
        // หน่วงเวลาจำลองว่าบอทกำลัง "คิดและเล็ง"
        yield return new WaitForSeconds(1.5f);

        ThrowAtTarget();

        // รอของตกแล้วค่อยสลับเทิร์นกลับ
        yield return new WaitForSeconds(2.5f);
        FindAnyObjectByType<TurnManager>()?.OnBotFinishedTurn();
    }

    private void ThrowAtTarget()
    {
        if (targetPlayer == null || projectilePrefab == null) return;

        Vector2 start = throwPoint.position;
        Vector2 target = targetPlayer.position;

        // คำนวณความเร็วต้นที่ต้องใช้ตามมุมที่กำหนด (launchAngleDegree)
        Vector2 velocity = CalculateLaunchVelocity(start, target, launchAngleDegree);

        // ใส่ Error สุ่มระยะตกตามค่า Accuracy
        float errorOffset = (1f - accuracy) * Random.Range(-3f, 3f);
        velocity.x += errorOffset;

        // ปล่อยของ
        GameObject obj = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
        obj.tag = "Projectile";
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        rb.linearVelocity = velocity;
    }

    // สูตรฟิสิกส์คำนวณ Velocity จากจุด A ไป B ด้วยมุมคงที่
    private Vector2 CalculateLaunchVelocity(Vector2 start, Vector2 target, float angleDeg)
    {
        Vector2 dir = target - start;
        float h = dir.y;
        dir.y = 0;
        float dist = dir.magnitude;
        float a = angleDeg * Mathf.Deg2Rad;
        dir.y = dist * Mathf.Tan(a);
        dist += h / Mathf.Tan(a);

        // คำนวณความเร็วต้น v = sqrt( (g * dist^2) / (2 * (dist * tan(a) - h) * cos^2(a)) )
        float g = Mathf.Abs(Physics2D.gravity.y);
        float velocityMag = Mathf.Sqrt(dist * g / Mathf.Sin(2 * a));

        if (float.IsNaN(velocityMag)) velocityMag = 12f; // Fallback หากคำนวณไม่ได้

        Vector2 launchDir = new Vector2(target.x > start.x ? Mathf.Cos(a) : -Mathf.Cos(a), Mathf.Sin(a));
        return launchDir * velocityMag;
    }
}