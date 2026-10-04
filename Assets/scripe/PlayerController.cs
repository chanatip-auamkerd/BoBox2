using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform throwPoint;
    public GameObject projectilePrefab;
    public TrajectoryLine trajectory;

    [Header("UI Buttons")]
    public Button lockButton;
    public TextMeshProUGUI lockButtonText;
    public Button throwButton;

    [Header("Status")]
    public bool isMyTurn = false;
    public bool isAimLocked = false;
    public bool isRepositionMode = false;

    [Header("Reposition Settings (ขอบเขตและการเดิน)")]
    public float keyboardMoveSpeed = 6f; 
    public float minX = -8.5f;        
    public float maxX = -1.2f;         
    private float groundFixedY;
    private bool isDragging = false;

    [Header("Trajectory Arc")]
    public float arcHeight = 3.5f;

    private Vector2 currentVelocity;
    private float flightTime;

    void Awake()
    {
        groundFixedY = transform.position.y;
    }

    void Start()
    {
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

        HandleAimInput();

        if (!isAimLocked)
        {
            UpdateAim();
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
            if (trajectory != null) trajectory.HideLine();
        }

        UpdateUIState();
    }

    private void HandleRepositionInput()
    {
        float moveAxis = 0f;
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) moveAxis -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) moveAxis += 1f;
        }
#else
        moveAxis = Input.GetAxisRaw("Horizontal");
#endif

        if (Mathf.Abs(moveAxis) > 0.01f)
        {
            isDragging = false; 
            float newX = transform.position.x + (moveAxis * keyboardMoveSpeed * Time.deltaTime);
            newX = Mathf.Clamp(newX, minX, maxX);
            transform.position = new Vector3(newX, groundFixedY, transform.position.z);
        }

        HandleMouseDragging();

        bool confirmKey = false;
#if ENABLE_INPUT_SYSTEM
        if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)) confirmKey = true;
#else
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) confirmKey = true;
#endif
        if (confirmKey)
        {
            ConfirmPosition();
        }
    }

    private void HandleMouseDragging()
    {
        Vector3 mouseWorld = GetMouseWorldPosition();
        bool isMouseDown = false;
        bool isMouseUp = false;

#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        if (mouse != null)
        {
            isMouseDown = mouse.leftButton.wasPressedThisFrame;
            isMouseUp = mouse.leftButton.wasReleasedThisFrame;
        }
#else
        isMouseDown = Input.GetMouseButtonDown(0);
        isMouseUp = Input.GetMouseButtonUp(0);
#endif

        if (isMouseDown)
        {
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Collider2D col = GetComponent<Collider2D>();
            if (col != null && col.OverlapPoint(mouseWorld))
            {
                isDragging = true;
            }
        }

        if (isMouseUp)
        {
            isDragging = false;
        }

        if (isDragging)
        {
            float clampedX = Mathf.Clamp(mouseWorld.x, minX, maxX);
            transform.position = new Vector3(clampedX, groundFixedY, transform.position.z);
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
        FindAnyObjectByType<TurnManager>()?.ConfirmRepositionEarly();
    }

    private void HandleAimInput()
    {
        bool lockTogglePressed = false;
        bool spacePressed = false;
        bool cancelUnlockPressed = false;

#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        var keyboard = Keyboard.current;

        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame) spacePressed = true;
        if (mouse != null && mouse.rightButton.wasPressedThisFrame) lockTogglePressed = true;

        if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.deleteKey.wasPressedThisFrame))
        {
            cancelUnlockPressed = true;
        }
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            cancelUnlockPressed = true;
        }
#else
        if (Input.GetKeyDown(KeyCode.Space)) spacePressed = true;
        if (Input.GetMouseButtonDown(1)) lockTogglePressed = true;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Delete) || Input.GetMouseButtonDown(0))
        {
            cancelUnlockPressed = true;
        }
#endif

        if (isAimLocked && cancelUnlockPressed)
        {
            if (UnityEngine.EventSystems.EventSystem.current == null ||
                !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                ToggleLockAim();
                return;
            }
        }

        if (lockTogglePressed)
        {
            ToggleLockAim();
        }

        if (spacePressed)
        {
            if (!isAimLocked) ToggleLockAim();
            else Throw();
        }
    }

    public void ToggleLockAim()
    {
        if (!isMyTurn || isRepositionMode) return;

        isAimLocked = !isAimLocked;
        UpdateUIState();
    }

    private void UpdateAim()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        if (mouseWorldPos.x < transform.position.x)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        Vector2 startPos = throwPoint.position;
        Vector2 targetPos = mouseWorldPos;

        currentVelocity = CalculateVelocityToTarget(startPos, targetPos, arcHeight, out flightTime);

        if (trajectory != null)
        {
            trajectory.DrawTrajectoryToTarget(startPos, currentVelocity, flightTime);
        }
    }

    private Vector2 CalculateVelocityToTarget(Vector2 start, Vector2 target, float extraHeight, out float totalTime)
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y);
        float apexY = Mathf.Max(start.y, target.y) + extraHeight;

        float vy = Mathf.Sqrt(2f * gravity * Mathf.Max(0.1f, apexY - start.y));
        float timeUp = vy / gravity;
        float timeDown = Mathf.Sqrt(2f * Mathf.Max(0.01f, apexY - target.y) / gravity);
        totalTime = Mathf.Max(0.05f, timeUp + timeDown);

        float vx = (target.x - start.x) / totalTime;
        return new Vector2(vx, vy);
    }

    public void StartPlayerTurn()
    {
        isMyTurn = true;
        isAimLocked = false;
        UpdateUIState();

        if (trajectory != null)
        {
            trajectory.ShowLine();
        }
    }

    public void Throw()
    {
        if (!isMyTurn || !isAimLocked) return;

        isMyTurn = false;
        isAimLocked = false;
        UpdateUIState();

        if (trajectory != null)
        {
            trajectory.HideLine();
        }

        GameObject obj = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
        obj.tag = "Projectile";

        Collider2D playerCol = GetComponent<Collider2D>();
        Collider2D projCol = obj.GetComponent<Collider2D>();
        if (playerCol != null && projCol != null)
        {
            Physics2D.IgnoreCollision(playerCol, projCol);
        }

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = currentVelocity;
        }

        FindAnyObjectByType<TurnManager>()?.OnItemThrown(true);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreen;
#if ENABLE_INPUT_SYSTEM
        mouseScreen = Mouse.current != null ? (Vector3)Mouse.current.position.ReadValue() : Input.mousePosition;
#else
        mouseScreen = Input.mousePosition;
#endif
        Vector3 world = Camera.main.ScreenToWorldPoint(mouseScreen);
        world.z = 0f;
        return world;
    }

    private void UpdateUIState()
    {
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
}