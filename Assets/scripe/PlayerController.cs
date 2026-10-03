using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform throwPoint;
    public GameObject projectilePrefab;
    public TrajectoryLine trajectory;

    [Header("Status")]
    public bool isMyTurn = true;
    public float throwPower = 14f;

    private Vector2 aimDirection;

    void Update()
    {
        if (!isMyTurn) return;

        UpdateAim();
    }

    private void UpdateAim()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        aimDirection = (mouseWorldPos - throwPoint.position).normalized;
        Vector2 velocity = aimDirection * throwPower;

        if (trajectory != null)
        {
            trajectory.DrawTrajectory(throwPoint.position, velocity);
        }
    }

    // ผูกฟังก์ชันนี้เข้ากับปุ่ม Throw Button
    public void Throw()
    {
        if (!isMyTurn) return;

        // ปาของ
        GameObject obj = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
        obj.tag = "Projectile";
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        rb.linearVelocity = aimDirection * throwPower;

        // ปิดเส้นและสลับเทิร์น
        if (trajectory != null) trajectory.HideLine();
        isMyTurn = false;

        // สั่งให้สลับไปเทิร์นบอท (ผ่าน TurnManager หรือส่งต่อตรงๆ)
        FindAnyObjectByType<TurnManager>()?.OnPlayerFinishedTurn();
    }
}