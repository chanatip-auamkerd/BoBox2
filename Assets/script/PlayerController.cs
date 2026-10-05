using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("1 = Player 1 / 2 = Player 2")]
    public int playerIndex = 1;

    public bool isMyTurn = false;
    public bool isRepositionMode = false;
    public bool isAimLocked = false;

    // =========================================================
    // Animation Bridge
    // =========================================================

    /// <summary>
    /// แจ้ง PlayerAnimationController ว่าผู้เล่นกด THROW แล้ว
    /// </summary>
    public event Action OnThrowAnimationRequested;

    // =========================================================
    // Movement
    // =========================================================

    [Header("Movement & Boundaries")]
    public float moveSpeed = 5f;
    public float minX = -7.5f;
    public float maxX = -1.5f;

    private float groundFixedY;
    private bool isDragging = false;

    // =========================================================
    // Aim / Throw
    // =========================================================

    [Header("Aiming & Throw Settings")]
    public Transform throwPoint;
    public GameObject projectilePrefab;
    public TrajectoryLine trajectory;

    public float minLaunchForce = 6f;
    public float maxLaunchForce = 22f;
    public float forceSensitivity = 2.5f;

    [Tooltip("เวลาหน่วงก่อนสร้างลูกบอล เพื่อให้ท่า THROW ได้เริ่มเล่นก่อน")]
    public float throwDelay = 0.18f;

    // =========================================================
    // UI
    // =========================================================

    [Header("UI Buttons")]
    public Button readyButton;
    public Button lockButton;
    public TextMeshProUGUI lockButtonText;
    public Button throwButton;

    // =========================================================
    // Runtime
    // =========================================================

    private Vector2 currentLaunchVelocity;
    private Collider2D playerCollider;
    private bool throwInProgress = false;

    // =========================================================
    // Unity
    // =========================================================

    private void Awake()
    {
        groundFixedY = transform.position.y;
        playerCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (readyButton != null)
            readyButton.onClick.AddListener(ConfirmPosition);

        if (lockButton != null)
            lockButton.onClick.AddListener(OnActionButtonClicked);

        if (throwButton != null)
            throwButton.onClick.AddListener(Throw);

        UpdateUIState();
    }

    private void Update()
    {
        if (throwInProgress)
            return;

        if (isRepositionMode)
        {
            HandleRepositionInput();
            return;
        }

        if (!isMyTurn)
            return;

        HandleAimingInput();
    }

    // =========================================================
    // TurnManager / TurnManagerPvP API
    // =========================================================

    public void EnableRepositionMode(bool enable)
    {
        isRepositionMode = enable;
        isDragging = false;

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minX, maxX),
            groundFixedY,
            transform.position.z
        );

        if (enable)
        {
            isMyTurn = false;
            isAimLocked = false;

            if (trajectory != null)
                trajectory.HideLine();
        }

        UpdateUIState();
    }

    public void StartPlayerTurn()
    {
        isMyTurn = true;
        isAimLocked = false;
        isRepositionMode = false;
        throwInProgress = false;

        UpdateUIState();

        if (trajectory != null)
            trajectory.ShowLine();
    }

    public void UpdateUIState()
    {
        if (readyButton != null)
            readyButton.interactable = isRepositionMode;

        if (lockButton != null)
            lockButton.interactable = isRepositionMode || isMyTurn;

        if (lockButtonText != null)
        {
            lockButtonText.color = Color.black;
            lockButtonText.text = isRepositionMode
                ? "READY"
                : (isAimLocked ? "UNLOCK" : "LOCK");
        }

        if (throwButton != null)
        {
            throwButton.interactable =
                isMyTurn &&
                isAimLocked &&
                !isRepositionMode &&
                !throwInProgress;
        }
    }

    // =========================================================
    // Reposition
    // =========================================================

    private void HandleRepositionInput()
    {
        transform.position = new Vector3(
            transform.position.x,
            groundFixedY,
            transform.position.z
        );

        float horizontal = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(horizontal) > 0.05f)
        {
            float newX = transform.position.x +
                         horizontal * moveSpeed * Time.deltaTime;

            newX = Mathf.Clamp(newX, minX, maxX);

            transform.position = new Vector3(
                newX,
                groundFixedY,
                transform.position.z
            );

            FlipFromInput(horizontal);
        }

        if (Camera.main == null)
            return;

        Vector3 mouseWorldPos =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (playerCollider != null &&
                playerCollider.OverlapPoint(mouseWorldPos))
            {
                isDragging = true;
            }
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            float clampedX = Mathf.Clamp(mouseWorldPos.x, minX, maxX);

            transform.position = new Vector3(
                clampedX,
                groundFixedY,
                transform.position.z
            );
        }

        if (Input.GetMouseButtonUp(0))
            isDragging = false;

        if (Input.GetKeyDown(KeyCode.Space))
            ConfirmPosition();
    }

    private void FlipFromInput(float horizontal)
    {
        if (horizontal > 0f)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
        else if (horizontal < 0f)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    // =========================================================
    // READY / LOCK
    // =========================================================

    public void OnActionButtonClicked()
    {
        if (isRepositionMode)
        {
            ConfirmPosition();
        }
        else if (isMyTurn)
        {
            ToggleLockAim();
        }
    }

    public void ConfirmPosition()
    {
        isDragging = false;

        TurnManager soloManager =
            FindAnyObjectByType<TurnManager>();

        if (soloManager != null)
        {
            soloManager.ConfirmRepositionEarly();
            return;
        }

        TurnManagerPvP pvpManager =
            FindAnyObjectByType<TurnManagerPvP>();

        if (pvpManager != null)
            pvpManager.ConfirmRepositionEarly();
    }

    // =========================================================
    // Aiming
    // =========================================================

    private void HandleAimingInput()
    {
        if (!isAimLocked)
        {
            if (Camera.main == null || throwPoint == null)
                return;

            Vector3 mousePos =
                Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 aimVector =
                (Vector2)(mousePos - throwPoint.position);

            float dist = aimVector.magnitude;

            Vector2 launchDirection = dist > 0.001f
                ? aimVector.normalized
                : Vector2.right;

            float speed = Mathf.Clamp(
                dist * forceSensitivity,
                minLaunchForce,
                maxLaunchForce
            );

            currentLaunchVelocity = launchDirection * speed;

            if (trajectory != null)
            {
                trajectory.ShowLine();
                trajectory.DrawBounceTrajectory(
                    throwPoint.position,
                    currentLaunchVelocity,
                    playerCollider
                );
            }

            if (Input.GetKeyDown(KeyCode.Space) ||
                Input.GetMouseButtonDown(1))
            {
                ToggleLockAim();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space) ||
                Input.GetMouseButtonDown(0))
            {
                Throw();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                ToggleLockAim();
            }
        }
    }

    public void ToggleLockAim()
    {
        if (!isMyTurn || isRepositionMode || throwInProgress)
            return;

        isAimLocked = !isAimLocked;
        UpdateUIState();

        if (trajectory != null)
        {
            if (isAimLocked)
            {
                trajectory.DrawBounceTrajectory(
                    throwPoint.position,
                    currentLaunchVelocity,
                    playerCollider
                );
            }
            else
            {
                trajectory.ShowLine();
            }
        }
    }

    // =========================================================
    // THROW
    // =========================================================

    public void Throw()
    {
        if (!isMyTurn || isRepositionMode || throwInProgress)
            return;

        if (projectilePrefab == null || throwPoint == null)
        {
            Debug.LogWarning(
                "PlayerController: ยังไม่ได้ใส่ Projectile Prefab หรือ ThrowPoint!"
            );
            return;
        }

        throwInProgress = true;
        isMyTurn = false;
        isAimLocked = false;

        UpdateUIState();

        if (trajectory != null)
            trajectory.HideLine();

        // บอก Animation Script ให้เปลี่ยนเป็นท่า THROW
        OnThrowAnimationRequested?.Invoke();

        StartCoroutine(ReleaseProjectileRoutine());
    }

    private IEnumerator ReleaseProjectileRoutine()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, throwDelay));

        ReleaseProjectileFromAnimation();
    }

    /// <summary>
    /// จุดที่สร้าง Projectile จริง
    /// Animation Controller ไม่ได้สร้างบอลเอง เพื่อรักษา separation
    /// ระหว่าง Visual กับ Gameplay
    /// </summary>
    public void ReleaseProjectileFromAnimation()
    {
        if (!throwInProgress)
            return;

        GameObject proj = Instantiate(
            projectilePrefab,
            throwPoint.position,
            Quaternion.identity
        );

        proj.tag = "Projectile";

        Collider2D projCol = proj.GetComponent<Collider2D>();

        if (playerCollider != null && projCol != null)
        {
            Physics2D.IgnoreCollision(
                playerCollider,
                projCol
            );
        }

        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.linearVelocity = currentLaunchVelocity;

        throwInProgress = false;

        TurnManager soloManager =
            FindAnyObjectByType<TurnManager>();

        if (soloManager != null)
        {
            soloManager.OnItemThrown(true);
            return;
        }

        TurnManagerPvP pvpManager =
            FindAnyObjectByType<TurnManagerPvP>();

        if (pvpManager != null)
            pvpManager.OnItemThrown(playerIndex);
    }
}
