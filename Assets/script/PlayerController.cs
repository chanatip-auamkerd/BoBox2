using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public event Action OnThrowAnimationRequested;

    [Header("Player Settings")]
    [Tooltip("1 สำหรับ Player 1 (ฟ้า), 2 สำหรับ Player 2 (แดง/ผี)")]
    public int playerIndex = 1;
    public bool isMyTurn = false;
    public bool isRepositionMode = false;
    public bool isAimLocked = false;

    [Header("Visual Sprites & Facing")]
    [Tooltip("ใส่รูปท่ายืนนิ่ง/เดิน")]
    public Sprite idleSprite;
    [Tooltip("ใส่รูปท่ายกมือเตรียมปา")]
    public Sprite throwSprite;
    [Tooltip("ติ๊กถูกถ้าภาพวาดต้นฉบับหันหน้าไปทางซ้าย (เช่น ตัวผี Player2)")]
    public bool defaultFacingLeft = false;
    private SpriteRenderer spriteRenderer;

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

    [Header("Throw Point Offsets")]
    [Tooltip("ตำแหน่งปล่อยบอลตอนท่ายืนนิ่ง")]
    public Vector2 idleThrowOffset = new Vector2(0.4f, 0.4f);
    [Tooltip("ตำแหน่งปล่อยบอลตอนท่าเตรียมปา (ตรงมือที่ยื่นออกไป)")]
    public Vector2 actionThrowOffset = new Vector2(0.6f, 0.5f);

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
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (readyButton != null) readyButton.onClick.AddListener(ConfirmPosition);
        if (lockButton != null) lockButton.onClick.AddListener(OnActionButtonClicked);
        if (throwButton != null) throwButton.onClick.AddListener(Throw);
        SetVisualPose(false);
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
    public void SetVisualPose(bool isThrowPose)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (isThrowPose && throwSprite != null)
        {
            spriteRenderer.sprite = throwSprite;
            if (throwPoint != null) throwPoint.localPosition = actionThrowOffset;
        }
        else if (idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
            if (throwPoint != null) throwPoint.localPosition = idleThrowOffset;
        }
    }

    public void EnableRepositionMode(bool enable)
    {
        isRepositionMode = enable;
        isDragging = false;

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, minX, maxX), groundFixedY, transform.position.z);

        if (enable)
        {
            isMyTurn = false;
            isAimLocked = false;
            SetVisualPose(false);
            if (trajectory != null) trajectory.HideLine();
        }

        UpdateUIState();
    }

    public void StartPlayerTurn()
    {
        isMyTurn = true;
        isAimLocked = false;
        isRepositionMode = false;

        SetVisualPose(false);
        UpdateUIState();

        if (trajectory != null)
        {
            trajectory.ShowLine();
        }
    }

    public void UpdateUIState()
    {
        if (readyButton != null) readyButton.interactable = isRepositionMode;
        if (lockButton != null) lockButton.interactable = isRepositionMode || isMyTurn;

        if (lockButtonText != null)
        {
            lockButtonText.color = Color.black;
            if (isRepositionMode) lockButtonText.text = "READY";
            else lockButtonText.text = isAimLocked ? "UNLOCK" : "LOCK";
        }

        if (throwButton != null)
        {
            throwButton.interactable = isMyTurn && isAimLocked && !isRepositionMode;
        }
    }
    private void HandleRepositionInput()
    {
        transform.position = new Vector3(transform.position.x, groundFixedY, transform.position.z);

        float horizontal = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(horizontal) > 0.05f)
        {
            float newX = transform.position.x + (horizontal * moveSpeed * Time.deltaTime);
            newX = Mathf.Clamp(newX, minX, maxX);
            transform.position = new Vector3(newX, groundFixedY, transform.position.z);

            float facingSign = defaultFacingLeft ? -1f : 1f;
            if (horizontal > 0)
                transform.localScale = new Vector3(facingSign * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (horizontal < 0)
                transform.localScale = new Vector3(-facingSign * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
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
            float prevX = transform.position.x;
            transform.position = new Vector3(clampedX, groundFixedY, transform.position.z);

            float dragDir = clampedX - prevX;
            float facingSign = defaultFacingLeft ? -1f : 1f;
            if (dragDir > 0.01f)
                transform.localScale = new Vector3(facingSign * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (dragDir < -0.01f)
                transform.localScale = new Vector3(-facingSign * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
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
        if (isRepositionMode) ConfirmPosition();
        else if (isMyTurn) ToggleLockAim();
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
    private void HandleAimingInput()
    {
        if (!isAimLocked)
        {
            Vector3 mouseScreenPos = Input.mousePosition;
            if (Camera.main != null)
            {
                mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);
            }
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            mouseWorldPos.z = 0f; 

            Vector3 startPos = throwPoint != null ? throwPoint.position : transform.position;
            startPos.z = 0f;

            Vector2 aimVector = (Vector2)(mouseWorldPos - startPos);
            float dist = aimVector.magnitude;
            Vector2 launchDirection = aimVector.normalized;

            float speed = Mathf.Clamp(dist * forceSensitivity, minLaunchForce, maxLaunchForce);
            currentLaunchVelocity = launchDirection * speed;

            if (trajectory != null)
            {
                trajectory.ShowLine();
                trajectory.DrawBounceTrajectory(startPos, currentLaunchVelocity, playerCollider);
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
        SetVisualPose(isAimLocked);
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
            Debug.LogWarning("⚠️ ยังใส่ References ไม่ครบใน Inspector!");
            return;
        }

        OnThrowAnimationRequested?.Invoke();

        isMyTurn = false;
        isAimLocked = false;
        UpdateUIState();

        if (trajectory != null) trajectory.HideLine();

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

        SetVisualPose(false);

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