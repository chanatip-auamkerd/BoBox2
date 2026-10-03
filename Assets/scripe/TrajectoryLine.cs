using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryLine : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [Header("Trajectory Settings")]
    [SerializeField] private int resolution = 30; // จำนวนจุดบนเส้น (ยิ่งเยอะยิ่งเนียน)
    [SerializeField] private float timeStep = 0.05f; // ช่องว่างเวลาในแต่ละจุด

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // ฟังก์ชันคำนวณและวาดเส้น
    public void DrawTrajectory(Vector2 startPos, Vector2 initialVelocity)
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = resolution;

        Vector2 currentPosition = startPos;
        Vector2 gravity = Physics2D.gravity; // ดึงค่า Gravity ของ Unity 2D มาใช้

        for (int i = 0; i < resolution; i++)
        {
            float t = i * timeStep;

            // สูตรคำนวณวิถีโค้ง: Position = start + (v * t) + (0.5 * g * t^2)
            Vector2 point = startPos + (initialVelocity * t) + (0.5f * gravity * t * t);

            lineRenderer.SetPosition(i, new Vector3(point.x, point.y, 0f));
        }
    }

    public void HideLine()
    {
        lineRenderer.enabled = false;
    }
}