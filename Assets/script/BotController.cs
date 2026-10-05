using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("References")]
    public Transform throwPoint;
    public GameObject projectilePrefab;
    public Transform targetPlayer;
    public TrajectoryLine botTrajectory;

    [Header("Reposition Zone")]
    public float minX = 2.5f;
    public float maxX = 7.5f;
    public float moveSpeed = 4.5f;

    private float groundFixedY;

    [Header("Aiming Settings")]
    public float aimPreviewDuration = 1.2f;
    public float minLaunchForce = 8f;
    public float maxLaunchForce = 22f;

    // =========================================================
    // Animation Event
    // =========================================================

    public event Action OnThrowAnimation;

    // =========================================================
    // Animation State
    // =========================================================

    [HideInInspector]
    public bool isAiming = false;

    // =========================================================
    // Pending Shot
    // =========================================================

    private Vector2 pendingThrowVelocity;
    private bool projectileAlreadyReleased = false;

    private Coroutine throwSafetyCoroutine;

    // =========================================================
    // Smart Shot
    // =========================================================

    private struct SmartShot
    {
        public float standingX;
        public Vector2 velocity;
        public float score;
        public float flightTime;
    }

    // =========================================================
    // Awake
    // =========================================================

    void Awake()
    {
        groundFixedY =
            transform.position.y;
    }

    // =========================================================
    // Start Bot Turn
    // =========================================================

    public void StartBotTurn()
    {
        StartCoroutine(
            BotTurnRoutine()
        );
    }

    // =========================================================
    // BOT TURN
    // =========================================================

    private IEnumerator BotTurnRoutine()
    {
        if (
            targetPlayer == null ||
            projectilePrefab == null ||
            throwPoint == null
        )
        {
            Debug.LogWarning(
                "⚠️ Bot ยังใส่ References ไม่ครบใน Inspector!"
            );

            yield break;
        }

        SmartShot chosenShot =
            FindBestPossibleShot();

        // =====================================================
        // Move Bot
        // =====================================================

        while (
            Mathf.Abs(
                transform.position.x -
                chosenShot.standingX
            ) > 0.05f
        )
        {
            float step =
                moveSpeed *
                Time.deltaTime;

            float newX =
                Mathf.MoveTowards(
                    transform.position.x,
                    chosenShot.standingX,
                    step
                );

            transform.position =
                new Vector3(
                    newX,
                    groundFixedY,
                    transform.position.z
                );

            yield return null;
        }

        transform.position =
            new Vector3(
                chosenShot.standingX,
                groundFixedY,
                transform.position.z
            );

        // =====================================================
        // Start Aiming
        // =====================================================

        isAiming = true;

        if (botTrajectory != null)
        {
            botTrajectory.ShowLine();

            botTrajectory.DrawTrajectoryToTarget(
                throwPoint.position,
                chosenShot.velocity,
                chosenShot.flightTime
            );
        }

        // =====================================================
        // Aim Preview
        // =====================================================

        yield return new WaitForSeconds(
            aimPreviewDuration
        );

        // =====================================================
        // Stop Aiming
        // =====================================================

        isAiming = false;

        if (botTrajectory != null)
        {
            botTrajectory.HideLine();
        }

        // =====================================================
        // Save Shot
        // =====================================================

        pendingThrowVelocity =
            chosenShot.velocity;

        projectileAlreadyReleased = false;

        // =====================================================
        // Tell Animation
        // =====================================================

        if (OnThrowAnimation != null)
        {
            OnThrowAnimation.Invoke();
        }
        else
        {
            // ไม่มี Animation Controller
            // ยิงทันที
            ReleaseProjectileFromAnimation();
        }

        // =====================================================
        // Safety
        // =====================================================

        if (throwSafetyCoroutine != null)
        {
            StopCoroutine(
                throwSafetyCoroutine
            );
        }

        throwSafetyCoroutine =
            StartCoroutine(
                ThrowSafetyRoutine()
            );
    }

    // =========================================================
    // Safety Routine
    // =========================================================

    private IEnumerator ThrowSafetyRoutine()
    {
        // ให้ Animation มีเวลาทำงาน
        yield return new WaitForSeconds(1.0f);

        if (!projectileAlreadyReleased)
        {
            ReleaseProjectileFromAnimation();
        }

        throwSafetyCoroutine = null;
    }

    // =========================================================
    // FIND BEST SHOT
    // =========================================================

    private SmartShot FindBestPossibleShot()
    {
        SmartShot best =
            new SmartShot
            {
                score = -999999f,
                standingX =
                    transform.position.x
            };

        Collider2D botCol =
            GetComponent<Collider2D>();

        float[] testPositions =
        {
            3.0f,
            4.2f,
            5.4f,
            6.5f,
            7.2f
        };

        foreach (
            float testX
            in testPositions
        )
        {
            Vector2 origin =
                new Vector2(
                    testX - 0.35f,
                    groundFixedY + 0.75f
                );

            for (
                float angle = 110f;
                angle <= 165f;
                angle += 4f
            )
            {
                float rad =
                    angle *
                    Mathf.Deg2Rad;

                Vector2 dir =
                    new Vector2(
                        Mathf.Cos(rad),
                        Mathf.Sin(rad)
                    );

                for (
                    float speed =
                        minLaunchForce;
                    speed <=
                        maxLaunchForce;
                    speed += 2.0f
                )
                {
                    Vector2 testVelocity =
                        dir * speed;

                    int bounces;
                    float flightTime;

                    float score =
                        SimulateTrajectory(
                            origin,
                            testVelocity,
                            botCol,
                            out bounces,
                            out flightTime
                        );

                    if (score > best.score)
                    {
                        best.score = score;

                        best.standingX =
                            testX;

                        best.velocity =
                            testVelocity;

                        best.flightTime =
                            flightTime;
                    }
                }
            }
        }

        // =====================================================
        // Fallback
        // =====================================================

        if (best.score <= -500f)
        {
            best.standingX = 5.8f;

            Vector2 safeOrigin =
                new Vector2(
                    best.standingX - 0.35f,
                    groundFixedY + 0.75f
                );

            float[] fallbackArcs =
            {
                5.5f,
                6.8f,
                8.0f,
                9.5f,
                11.0f
            };

            foreach (
                float arc
                in fallbackArcs
            )
            {
                float time;

                Vector2 vel =
                    CalculateHighArc(
                        safeOrigin,
                        targetPlayer.position,
                        arc,
                        out time
                    );

                int b;

                float simScore =
                    SimulateTrajectory(
                        safeOrigin,
                        vel,
                        botCol,
                        out b,
                        out time
                    );

                if (simScore > -500f)
                {
                    best.score =
                        simScore;

                    best.velocity =
                        vel;

                    best.flightTime =
                        time;

                    break;
                }
            }

            if (
                best.velocity ==
                Vector2.zero
            )
            {
                float safeTime;

                best.velocity =
                    CalculateHighArc(
                        safeOrigin,
                        targetPlayer.position,
                        8.5f,
                        out safeTime
                    );

                best.flightTime =
                    safeTime;
            }
        }

        return best;
    }

    // =========================================================
    // SIMULATE TRAJECTORY
    // =========================================================

    private float SimulateTrajectory(
        Vector2 startPos,
        Vector2 initialVelocity,
        Collider2D botCol,
        out int bounces,
        out float totalTime
    )
    {
        bounces = 0;
        totalTime = 0f;

        Vector2 currentPos =
            startPos;

        Vector2 currentVel =
            initialVelocity;

        float gravScale =
            projectilePrefab
                .GetComponent<Rigidbody2D>()?
                .gravityScale ?? 1f;

        Vector2 gravity =
            Physics2D.gravity *
            gravScale;

        float timeStep = 0.035f;

        float minDistanceToPlayer =
            999f;

        bool landedInPlayerTerritory =
            false;

        for (
            float t = 0;
            t < 3.2f;
            t += timeStep
        )
        {
            totalTime = t;

            Vector2 nextPos =
                currentPos +
                (
                    currentVel *
                    timeStep
                ) +
                (
                    0.5f *
                    gravity *
                    timeStep *
                    timeStep
                );

            currentVel +=
                gravity *
                timeStep;

            RaycastHit2D hit =
                Physics2D.Linecast(
                    currentPos,
                    nextPos
                );

            if (
                hit.collider != null &&
                hit.collider != botCol
            )
            {
                // -------------------------------------------------
                // Middle Wall
                // -------------------------------------------------

                if (
                    hit.collider.gameObject
                        .name
                        .Contains(
                            "MiddleWall"
                        ) ||
                    hit.collider.CompareTag(
                        "MiddleWall"
                    )
                )
                {
                    return -999999f;
                }

                // -------------------------------------------------
                // Player Hit
                // -------------------------------------------------

                if (
                    hit.collider.transform ==
                    targetPlayer
                )
                {
                    return
                        5000f +
                        (
                            bounces *
                            1500f
                        );
                }

                // -------------------------------------------------
                // Ground
                // -------------------------------------------------

                if (
                    hit.collider.CompareTag(
                        "Ground"
                    )
                )
                {
                    if (
                        hit.point.x <
                        -0.5f
                    )
                    {
                        landedInPlayerTerritory =
                            true;
                    }

                    break;
                }

                // -------------------------------------------------
                // Prop Bounce
                // -------------------------------------------------

                if (
                    hit.collider.CompareTag(
                        "Prop"
                    )
                )
                {
                    bounces++;

                    if (bounces > 3)
                    {
                        break;
                    }

                    currentVel =
                        Vector2.Reflect(
                            currentVel,
                            hit.normal
                        ) *
                        0.92f;

                    nextPos =
                        hit.point +
                        (
                            hit.normal *
                            0.06f
                        );

                    if (
                        currentVel.x >
                            0.3f &&
                        nextPos.x >
                            0.5f
                    )
                    {
                        return -999999f;
                    }
                }
                else
                {
                    break;
                }
            }

            // -----------------------------------------------------
            // Player Territory
            // -----------------------------------------------------

            if (
                nextPos.x <
                -0.5f
            )
            {
                landedInPlayerTerritory =
                    true;
            }

            float dist =
                Vector2.Distance(
                    nextPos,
                    targetPlayer.position
                );

            if (
                dist <
                minDistanceToPlayer
            )
            {
                minDistanceToPlayer =
                    dist;
            }

            currentPos =
                nextPos;
        }

        if (!landedInPlayerTerritory)
        {
            return -999999f;
        }

        return
            (
                600f -
                minDistanceToPlayer *
                45f
            ) +
            (
                bounces *
                350f
            );
    }

    // =========================================================
    // HIGH ARC
    // =========================================================

    private Vector2 CalculateHighArc(
        Vector2 start,
        Vector2 target,
        float extraHeight,
        out float totalTime
    )
    {
        float gravScale =
            projectilePrefab
                .GetComponent<Rigidbody2D>()?
                .gravityScale ?? 1f;

        float gravity =
            Mathf.Abs(
                Physics2D.gravity.y *
                gravScale
            );

        float apexY =
            Mathf.Max(
                start.y,
                target.y
            ) +
            extraHeight;

        float vy =
            Mathf.Sqrt(
                2f *
                gravity *
                Mathf.Max(
                    0.1f,
                    apexY - start.y
                )
            );

        float timeUp =
            vy / gravity;

        float timeDown =
            Mathf.Sqrt(
                2f *
                Mathf.Max(
                    0.01f,
                    apexY - target.y
                ) /
                gravity
            );

        totalTime =
            Mathf.Max(
                0.1f,
                timeUp + timeDown
            );

        float vx =
            (
                target.x -
                start.x
            ) /
            totalTime;

        return new Vector2(
            vx,
            vy
        );
    }

    // =========================================================
    // RELEASE PROJECTILE
    // =========================================================

    public void ReleaseProjectileFromAnimation()
    {
        if (projectileAlreadyReleased)
        {
            return;
        }

        projectileAlreadyReleased = true;

        Fire(
            pendingThrowVelocity
        );
    }

    // =========================================================
    // FIRE
    // =========================================================

    private void Fire(Vector2 velocity)
    {
        GameObject obj =
            Instantiate(
                projectilePrefab,
                throwPoint.position,
                Quaternion.identity
            );

        obj.tag =
            "Projectile";

        // -----------------------------------------------------
        // Ignore Bot Collision
        // -----------------------------------------------------

        Collider2D botCol =
            GetComponent<Collider2D>();

        Collider2D projCol =
            obj.GetComponent<Collider2D>();

        if (
            botCol != null &&
            projCol != null
        )
        {
            Physics2D.IgnoreCollision(
                botCol,
                projCol
            );
        }

        // -----------------------------------------------------
        // Velocity
        // -----------------------------------------------------

        Rigidbody2D rb =
            obj.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity =
                velocity;
        }

        // -----------------------------------------------------
        // Tell TurnManager
        // -----------------------------------------------------

        FindAnyObjectByType<TurnManager>()?
            .OnItemThrown(false);
    }
}