using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryLine : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [Header("Trajectory Settings")]
    [SerializeField] private int resolution = 30;
    [SerializeField] private float timeStep = 0.05f;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void DrawTrajectoryToTarget(Vector2 startPos, Vector2 initialVelocity, float totalTime)
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.enabled = true;
        lineRenderer.positionCount = resolution;

        Vector2 gravity = Physics2D.gravity;
        float step = totalTime / Mathf.Max(1, resolution - 1);

        for (int i = 0; i < resolution; i++)
        {
            float t = i * step;
            Vector2 point = startPos + (initialVelocity * t) + (0.5f * gravity * t * t);
            lineRenderer.SetPosition(i, new Vector3(point.x, point.y, 0f));
        }
    }

    public void DrawTrajectory(Vector2 startPos, Vector2 initialVelocity)
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.enabled = true;
        lineRenderer.positionCount = resolution;

        Vector2 gravity = Physics2D.gravity;

        for (int i = 0; i < resolution; i++)
        {
            float t = i * timeStep;
            Vector2 point = startPos + (initialVelocity * t) + (0.5f * gravity * t * t);
            lineRenderer.SetPosition(i, new Vector3(point.x, point.y, 0f));
        }
    }

    public void HideLine()
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;
        gameObject.SetActive(false); 
    }

    public void ShowLine()
    {
        gameObject.SetActive(true);
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = true;
    }
}