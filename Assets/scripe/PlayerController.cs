using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform throwPoint;
    public GameObject projectilePrefab;
    public TrajectoryLine trajectory;
    public Button throwButton;

    [Header("Status")]
    public bool isMyTurn = true;
    public bool isAimLocked = false;

    [Header("Trajectory Arc")]
    [Tooltip("ความสูงส่วนโค้งเหนือจุดที่เล็ง")]
    public float arcHeight = 3.5f;

    private Vector2 currentVelocity;
    private float flightTime;

    void Start()
    {
        UpdateThrowButtonState();
    }

    void Update()
    {
        if (!isMyTurn) return;

        HandleInput();

        if (!isAimLocked)
        {
            UpdateAim();
        }
    }

    private void HandleInput()
    {
        bool lockPressed = false;
        bool spacePressed = false;

#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        var keyboard = Keyboard.current;

        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame) spacePressed = true;
        if (mouse != null && mouse.rightButton.wasPressedThisFrame) lockPressed = true;
#else
        if (Input.GetKeyDown(KeyCode.Space)) spacePressed = true;
        if (Input.GetMouseButtonDown(1)) lockPressed = true;
#endif

        if (lockPressed)
        {
            ToggleLockAim();
        }

        if (spacePressed)
        {
            if (!isAimLocked)
            {
                ToggleLockAim();
            }
            else
            {
                Throw();
            }
        }
    }

    private void ToggleLockAim()
    {
        isAimLocked = !isAimLocked;
        UpdateThrowButtonState();
    }

    private void UpdateAim()
    {
        Vector3 mouseScreenPos;
#if ENABLE_INPUT_SYSTEM
        mouseScreenPos = Mouse.current != null ? (Vector3)Mouse.current.position.ReadValue() : Input.mousePosition;
#else
        mouseScreenPos = Input.mousePosition;
#endif

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

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
        UpdateThrowButtonState();

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
        UpdateThrowButtonState();
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

    private void UpdateThrowButtonState()
    {
        if (throwButton != null)
        {
            throwButton.interactable = isMyTurn && isAimLocked;
        }
    }
}