using System.Collections;
using UnityEngine;

public class BotAnimationController : MonoBehaviour
{
    [Header("References")]
    public BotController botController;
    public SpriteRenderer spriteRenderer;

    [Header("2 Sprite Animation")]
    [Tooltip("รูปผีตอนยืน")]
    public Sprite idleSprite;

    [Tooltip("รูปผีตอนเตรียมโยน / โยน")]
    public Sprite actionSprite;

    [Header("Sprite Facing")]
    public bool idleFlipX = false;
    public bool actionFlipX = false;

    [Header("Aiming Behaviour")]
    [Tooltip("เปิด = ตอนผีเล็งจะใช้ Action Sprite")]
    public bool useActionSpriteWhileAiming = true;

    [Header("Throw Timing")]
    [Tooltip("เวลาหลังเริ่มท่าโยนก่อนลูกบอลออก")]
    public float releaseDelay = 0.18f;

    [Tooltip("เวลารวมที่แสดง Action Sprite ตอนโยน")]
    public float throwVisualDuration = 0.35f;

    private bool isThrowing = false;
    private Coroutine throwCoroutine;

    // =========================================================
    // Awake
    // =========================================================

    private void Awake()
    {
        if (botController == null)
        {
            botController =
                GetComponent<BotController>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }
    }

    // =========================================================
    // Enable
    // =========================================================

    private void OnEnable()
    {
        if (botController != null)
        {
            botController.OnThrowAnimation +=
                PlayThrow;
        }
    }

    // =========================================================
    // Disable
    // =========================================================

    private void OnDisable()
    {
        if (botController != null)
        {
            botController.OnThrowAnimation -=
                PlayThrow;
        }

        if (throwCoroutine != null)
        {
            StopCoroutine(
                throwCoroutine
            );

            throwCoroutine = null;
        }

        isThrowing = false;
    }

    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        SetIdleSprite();
    }

    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        if (
            botController == null ||
            spriteRenderer == null
        )
        {
            return;
        }

        // ระหว่าง Throw ห้าม State ปกติทับ
        if (isThrowing)
        {
            return;
        }

        UpdateSpriteState();
    }

    // =========================================================
    // STATE
    // =========================================================

    private void UpdateSpriteState()
    {
        // -----------------------------------------------------
        // Bot กำลังเล็ง
        // -----------------------------------------------------

        if (botController.isAiming)
        {
            if (useActionSpriteWhileAiming)
            {
                SetActionSprite();
            }
            else
            {
                SetIdleSprite();
            }

            return;
        }

        // -----------------------------------------------------
        // ปกติ
        // -----------------------------------------------------

        SetIdleSprite();
    }

    // =========================================================
    // IDLE
    // =========================================================

    public void SetIdleSprite()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (idleSprite != null)
        {
            spriteRenderer.sprite =
                idleSprite;
        }

        spriteRenderer.flipX =
            idleFlipX;
    }

    // =========================================================
    // ACTION
    // =========================================================

    public void SetActionSprite()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (actionSprite != null)
        {
            spriteRenderer.sprite =
                actionSprite;
        }

        spriteRenderer.flipX =
            actionFlipX;
    }

    // =========================================================
    // THROW
    // =========================================================

    public void PlayThrow()
    {
        if (throwCoroutine != null)
        {
            StopCoroutine(
                throwCoroutine
            );
        }

        throwCoroutine =
            StartCoroutine(
                ThrowRoutine()
            );
    }

    // =========================================================
    // THROW ROUTINE
    // =========================================================

    private IEnumerator ThrowRoutine()
    {
        isThrowing = true;

        // เปลี่ยนเป็นภาพโยน
        SetActionSprite();

        float totalDuration =
            Mathf.Max(
                0.01f,
                throwVisualDuration
            );

        float actualReleaseDelay =
            Mathf.Clamp(
                releaseDelay,
                0f,
                totalDuration
            );

        // -----------------------------------------------------
        // รอจังหวะปล่อย
        // -----------------------------------------------------

        yield return new WaitForSeconds(
            actualReleaseDelay
        );

        // -----------------------------------------------------
        // ปล่อย Projectile
        // -----------------------------------------------------

        if (botController != null)
        {
            botController
                .ReleaseProjectileFromAnimation();
        }

        // -----------------------------------------------------
        // ค้างภาพ Action
        // -----------------------------------------------------

        float remainingTime =
            totalDuration -
            actualReleaseDelay;

        if (remainingTime > 0f)
        {
            yield return new WaitForSeconds(
                remainingTime
            );
        }

        // -----------------------------------------------------
        // กลับ Idle
        // -----------------------------------------------------

        isThrowing = false;

        throwCoroutine = null;

        SetIdleSprite();
    }
}