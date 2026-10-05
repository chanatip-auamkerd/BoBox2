using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    [Header("Setup")]
    public GameObject projectilePrefab;
    public Transform shootPoint;

    [Header("Settings")]
    public float minForce = 5f;
    public float maxForce = 25f;
    public float chargeSpeed = 15f;
    public Vector2 baseDirection = new Vector2(1, 1).normalized; // องศาการปาเริ่มต้น (45 องศา)

    private float currentForce;
    private bool isCharging = false;

    void Update()
    {
        // 1. เริ่มกดค้างเพื่อชาร์จแรง
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            currentForce = minForce;
        }

        // 2. สะสมหลอดพลัง
        if (isCharging && Input.GetMouseButton(0))
        {
            currentForce += chargeSpeed * Time.deltaTime;
            currentForce = Mathf.Clamp(currentForce, minForce, maxForce);
            // อัปเดต UI Power Gauge ได้ตรงนี้
        }

        // 3. ปล่อยเพื่อยิง
        if (isCharging && Input.GetMouseButtonUp(0))
        {
            Shoot();
            isCharging = false;
        }
    }

    void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        // กำหนดทิศทางตามฝั่งที่หัน (เช่น ฝั่งซ้ายปาไปขวา, ฝั่งขวาปาไปซ้าย)
        Vector2 shootVector = baseDirection * currentForce;
        rb.linearVelocity = shootVector;
    }
}
