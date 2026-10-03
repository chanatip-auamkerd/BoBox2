using UnityEngine;
using UnityEngine.UI;

public class AimAndThrowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform throwPoint;       // จุดปล่อยของ
    [SerializeField] private GameObject projectilePrefab; // ของที่จะปา
    [SerializeField] private TrajectoryLine trajectory;   // อ้างอิงสคริปต์เส้นประ

    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 12f;      // แรงปาพื้นฐาน
    [SerializeField] private Slider powerSlider;          // หลอดปรับแรง (Optional: ใส่ Slider หรือไม่ก็ได้)

    private Vector2 aimDirection;
    private float currentPower;

    void Start()
    {
        currentPower = throwForce;
        if (powerSlider != null)
        {
            powerSlider.minValue = 5f;
            powerSlider.maxValue = 25f;
            powerSlider.value = throwForce;
            powerSlider.onValueChanged.AddListener((val) => currentPower = val);
        }
    }

    void Update()
    {
        UpdateAiming();
    }

    private void UpdateAiming()
    {
        // 1. หาตำแหน่งเมาส์ใน World Space
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // 2. คำนวณเวกเตอร์ทิศทางจากจุดปล่อยไปยังเมาส์
        aimDirection = (mouseWorldPos - throwPoint.position).normalized;

        // 3. ความเร็วต้น = ทิศทาง * แรง
        Vector2 launchVelocity = aimDirection * currentPower;

        // 4. ส่งค่าไปวาดเส้นแบบ Real-time
        if (trajectory != null)
        {
            trajectory.DrawTrajectory(throwPoint.position, launchVelocity);
        }
    }

    // เรียกฟังก์ชันนี้จากปุ่ม UI (Button OnClick) หรือกด Spacebar
    public void ThrowItem()
    {
        if (projectilePrefab == null || throwPoint == null) return;

        // เสกของออกมา
        GameObject obj = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // ส่งแรงปาตามทิศทางและความแรงที่เล็งไว้
            rb.linearVelocity = aimDirection * currentPower;
        }

        // เสริม: หากปาแล้วต้องการซ่อนเส้นชั่วคราว หรือสลับเทิร์น
        // trajectory.HideLine();
    }
}