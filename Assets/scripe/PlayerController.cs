using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform throwPoint;
    public GameObject projectilePrefab;
    public TrajectoryLine trajectory;

    [Header("Status")]
    public bool isMyTurn = true;

    [Header("Trajectory Arc")]
    [Tooltip("ความสูงส่วนโค้งเหนือจุดที่เล็ง (ยิ่งเยอะ เส้นยิ่งย้อยโด่งขึ้นฟ้า)")]
    public float arcHeight = 3.5f;

    private Vector2 currentVelocity;
    private float flightTime;

    void Update()
    {
        if (!isMyTurn) return;

        UpdateAim();
    }

    private void UpdateAim()
    {
        // 1. หาตำแหน่งเมาส์ใน World Space
        Vector3 mouseScreenPos;
#if ENABLE_INPUT_SYSTEM
        mouseScreenPos = Mouse.current != null ? (Vector3)Mouse.current.position.ReadValue() : Input.mousePosition;
#else
        mouseScreenPos = Input.mousePosition;
#endif

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        // ไม่ให้เล็งไปด้านหลังตัวละคร (บังคับปาไปข้างหน้า/ขวา)
        if (mouseWorldPos.x <= throwPoint.position.x + 0.5f)
        {
            mouseWorldPos.x = throwPoint.position.x + 0.5f;
        }

        Vector2 startPos = throwPoint.position;
        Vector2 targetPos = mouseWorldPos;

        // 2. คำนวณ Velocity และเวลาในการเดินทาง (flightTime) เพื่อให้ตกที่เป้าหมาย
        currentVelocity = CalculateVelocityToTarget(startPos, targetPos, arcHeight, out flightTime);

        // 3. วาดเส้นให้หยุดตรงตำแหน่งเมาส์พอดี
        if (trajectory != null)
        {
            trajectory.DrawTrajectoryToTarget(startPos, currentVelocity, flightTime);
        }
    }

    // สูตรฟิสิกส์คำนวณความเร็วต้นแยกแกน X และ Y
    private Vector2 CalculateVelocityToTarget(Vector2 start, Vector2 target, float extraHeight, out float totalTime)
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y);

        // หาจุดสูงสุดของการปา (Apex) ให้อยู่สูงกว่าจุดเริ่มต้นและเป้าหมายเสมอ
        float apexY = Mathf.Max(start.y, target.y) + extraHeight;

        // ความเร็วต้นแกน Y: Vy = sqrt(2 * g * h)
        float vy = Mathf.Sqrt(2f * gravity * (apexY - start.y));

        // เวลาขาขึ้นและเวลาขาลง
        float timeUp = vy / gravity;
        float timeDown = Mathf.Sqrt(2f * Mathf.Max(0.01f, apexY - target.y) / gravity);
        totalTime = timeUp + timeDown;

        // ความเร็วต้นแกน X: Vx = ระยะทางแกน X / เวลาทั้งหมด
        float vx = (target.x - start.x) / totalTime;

        return new Vector2(vx, vy);
    }

    public void Throw()
    {
        if (!isMyTurn) return;

        GameObject obj = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
        obj.tag = "Projectile";

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = currentVelocity;
        }

        if (trajectory != null) trajectory.HideLine();
        isMyTurn = false;

        FindAnyObjectByType<TurnManager>()?.OnPlayerFinishedTurn();
    }
}