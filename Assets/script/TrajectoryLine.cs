using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryLine : MonoBehaviour
{
    private LineRenderer lr;

    [Header("Simulation Settings")]
    [Tooltip("จำนวนครั้งการสะท้อนที่จะแสดงในเส้นเล็ง (เช่น 1 = สะท้อนครั้งแรก, 2 = ชิ่ง 2 ทอด)")]
    public int maxBouncesToPreview = 1;

    [Tooltip("ความละเอียดของเส้น (ค่ายิ่งน้อย เส้นยิ่งโค้งเนียน)")]
    public float timeStep = 0.03f;

    [Tooltip("ระยะเวลาการบินจำลองสูงสุด")]
    public float maxSimulationDuration = 2.5f;

    [Header("Collision Layers")]
    [Tooltip("Layer ของวัตถุที่ให้เส้นเล็งสะท้อน (ปกติเลือก Default)")]
    public LayerMask collisionMask = ~0; 

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }
    public void DrawBounceTrajectory(Vector2 startPos, Vector2 initialVelocity, Collider2D shooterCollider = null)
    {
        if (lr == null) lr = GetComponent<LineRenderer>();

        List<Vector3> points = new List<Vector3>();
        points.Add(startPos);

        Vector2 currentPos = startPos;
        Vector2 currentVel = initialVelocity;
        Vector2 gravity = Physics2D.gravity;

        int bounceCount = 0;
        float elapsed = 0f;

        while (elapsed < maxSimulationDuration)
        {
            elapsed += timeStep;

            Vector2 nextPos = currentPos + (currentVel * timeStep) + (0.5f * gravity * timeStep * timeStep);
            currentVel += gravity * timeStep;

            RaycastHit2D hit = Physics2D.Linecast(currentPos, nextPos, collisionMask);

            if (hit.collider != null && hit.collider != shooterCollider)
            {
                points.Add(hit.point);

                if (hit.collider.CompareTag("Prop") && bounceCount < maxBouncesToPreview)
                {
                    bounceCount++;

                    currentVel = Vector2.Reflect(currentVel, hit.normal) * 0.95f;

                    currentPos = hit.point + (hit.normal * 0.05f);
                    points.Add(currentPos);
                    continue;
                }
                else
                {
                    break;
                }
            }

            points.Add(nextPos);
            currentPos = nextPos;
        }

        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());
    }
    public void DrawTrajectoryToTarget(Vector2 startPos, Vector2 velocity, float totalTime)
    {
        DrawBounceTrajectory(startPos, velocity);
    }

    public void ShowLine()
    {
        if (lr != null) lr.enabled = true;
    }

    public void HideLine()
    {
        if (lr != null)
        {
            lr.positionCount = 0;
            lr.enabled = false;
        }
    }
}