using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("1 สำหรับ Player 1 (ฟ้า), 2 สำหรับ Player 2 (แดง)")]
    public int playerIndex = 1;
    public bool isMyTurn = false;
    public bool isRepositionMode = false;
    public bool isAimLocked = false;

    [Header("Movement & Boundaries")]
    public float moveSpeed = 5f;
    public float minX = -7.5f;
    public float maxX = -1.5f;
    private float groundFixedY;
    private bool isDragging = false;

    [Header("Aiming & Throw Settings")]
    public Transform throwPoint;
    public GameObject projectilePrefab;
    public TrajectoryLine trajectory;

    public float minLaunchForce = 6f;
    public float maxLaunchForce = 22f;
    public float forceSensitivity = 2.5f;

    [Header("UI Buttons")]
    public Button readyButton;
    public Button lockButton;
    public TextMeshProUGUI lockButtonText;
    public Button throwButton;

    private Vector2 currentLaunchVelocity;
    private Collider2D playerCollider;

    void Awake()
    {
        groundFixedY = transform.position.y;
        playerCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        if (readyButton != null) readyButton.onClick.AddListener(ConfirmPosition);
        if (lockButton != null) lockButton.onClick.AddListener(OnActionButtonClicked);
        if (throwButton != null) throwButton.onClick.AddListener(Throw);

        UpdateUIState();
    }

    void Update()
    {
        if (isRepositionMode)
        {
            HandleRepositionInput();
            return;
        }

        if (!isMyTurn) return;

        HandleAimingInput();
    }

    // =========================================================================
    // ฟังก์ชันที่ TurnManager และ TurnManagerPvP เรียกใช้งาน
    // =========================================================================
    public void EnableRepositionMode(bool enable)
    {
        isRepositionMode = enable;
        isDragging = false;

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, minX, maxX), groundFixedY, transform.position.z);

        if (enable)
        {
            isMyTurn = false;
            isAimLocked = false;
            if (trajectory != null) trajectory.HideLine();
        }

        UpdateUIState();
    }

    public void StartPlayerTurn()
    {
        isMyTurn = true;
        isAimLocked = false;
        isRepositionMode = false;
        UpdateUIState();

        if (trajectory != null)
        {
            trajectory.ShowLine();
        }
    }

    public void UpdateUIState()
    {
        if (readyButton != null)
        {
            readyButton.interactable = isRepositionMode;
        }

        if (lockButton != null)
        {
            lockButton.interactable = isRepositionMode || isMyTurn;
        }

        if (lockButtonText != null)
        {
            lockButtonText.color = Color.black;
            if (isRepositionMode)
            {
                lockButtonText.text = "READY";
            }
            else
            {
                lockButtonText.text = isAimLocked ? "UNLOCK" : "LOCK";
            }
        }

        if (throwButton != null)
        {
            throwButton.interactable = isMyTurn && isAimLocked && !isRepositionMode;
        }
    }

    // =========================================================================
    // การเคลื่อนที่ / วางตำแหน่ง
    // =========================================================================
    private void HandleRepositionInput()
    {
        transform.position = new Vector3(transform.position.x, groundFixedY, transform.position.z);

        float horizontal = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(horizontal) > 0.05f)
        {
            float newX = transform.position.x + (horizontal * moveSpeed * Time.deltaTime);
            newX = Mathf.Clamp(newX, minX, maxX);
            transform.position = new Vector3(newX, groundFixedY, transform.position.z);

            if (horizontal > 0)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (horizontal < 0)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            if (playerCollider != null && playerCollider.OverlapPoint(mouseWorldPos))
            {
                isDragging = true;
            }
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            float clampedX = Mathf.Clamp(mouseWorldPos.x, minX, maxX);
            transform.position = new Vector3(clampedX, groundFixedY, transform.position.z);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmPosition();
        }
    }

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

        var soloManager = FindAnyObjectByType<TurnManager>();
        if (soloManager != null)
        {
            soloManager.ConfirmRepositionEarly();
            return;
        }

        var pvpManager = FindAnyObjectByType<TurnManagerPvP>();
        if (pvpManager != null)
        {
            pvpManager.ConfirmRepositionEarly();
        }
    }

    // =========================================================================
    // การเล็งและยิง (พร้อมแสดงเส้นชิ่งเด้ง Trajectory Preview)
    // =========================================================================
    private void HandleAimingInput()
    {
        if (!isAimLocked)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 aimVector = (Vector2)(mousePos - throwPoint.position);
            float dist = aimVector.magnitude;
            Vector2 launchDirection = aimVector.normalized;

            float speed = Mathf.Clamp(dist * forceSensitivity, minLaunchForce, maxLaunchForce);
            currentLaunchVelocity = launchDirection * speed;

            // วาดเส้นสะท้อนด้วย TrajectoryLine
            if (trajectory != null)
            {
                trajectory.ShowLine();
                trajectory.DrawBounceTrajectory(throwPoint.position, currentLaunchVelocity, playerCollider);
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1))
            {
                ToggleLockAim();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
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
        if (!isMyTurn || isRepositionMode) return;

        isAimLocked = !isAimLocked;
        UpdateUIState();

        if (trajectory != null)
        {
            if (isAimLocked)
            {
                trajectory.DrawBounceTrajectory(throwPoint.position, currentLaunchVelocity, playerCollider);
            }
            else
            {
                trajectory.ShowLine();
            }
        }
    }

    public void Throw()
    {
        if (!isMyTurn || isRepositionMode) return;

        if (projectilePrefab == null || throwPoint == null)
        {
            Debug.LogWarning("ยังไม่ได้ใส่ Projectile Prefab หรือ ThrowPoint!");
            return;
        }

        isMyTurn = false;
        isAimLocked = false;
        UpdateUIState();

        if (trajectory != null)
        {
            trajectory.HideLine();
        }

        GameObject proj = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
        proj.tag = "Projectile";

        Collider2D projCol = proj.GetComponent<Collider2D>();
        if (playerCollider != null && projCol != null)
        {
            Physics2D.IgnoreCollision(playerCollider, projCol);
        }

        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = currentLaunchVelocity;
        }

        var soloManager = FindAnyObjectByType<TurnManager>();
        if (soloManager != null)
        {
            soloManager.OnItemThrown(true);
            return;
        }

        var pvpManager = FindAnyObjectByType<TurnManagerPvP>();
        if (pvpManager != null)
        {
            pvpManager.OnItemThrown(playerIndex);
        }
    }
}