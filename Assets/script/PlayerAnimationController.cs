using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Animation Controller สำหรับตัวละครที่มี Sprite เพียง 2 รูป
///
/// Sprite 1 = Idle / ยืนปกติ
/// Sprite 2 = Action / เล็ง + โยน
///
/// ไม่ใช้ Animator และไม่ต้องสร้าง Animation Clip
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    public enum AnimationState
    {
        Idle,
        Aim,
        Throw
    }

    [Header("References")]
    public PlayerController playerController;
    public SpriteRenderer spriteRenderer;

    [Header("ONLY 2 SPRITES")]
    [Tooltip("รูปที่ 1: ยืนปกติ")]
    public Sprite idleSprite;

    [Tooltip("รูปที่ 2: ท่าการกระทำ ใช้ตอนเล็งและตอนโยน")]
    public Sprite actionSprite;

    [Header("Behaviour")]
    [Tooltip("ถ้าเป็น false ตอนเล็งจะยังใช้รูป Idle และจะเปลี่ยนเป็นรูป Action เฉพาะตอน THROW")]
    public bool useActionSpriteWhileAiming = true;

    [Tooltip("เวลาที่คงรูป Action ตอน THROW")]
    public float throwVisualDuration = 0.35f;

    public AnimationState CurrentState { get; private set; } = AnimationState.Idle;

    private Coroutine throwCoroutine;
    private bool initialized;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        initialized = true;
    }

    private void OnEnable()
    {
        if (playerController != null)
            playerController.OnThrowAnimationRequested += PlayThrow;
    }

    private void OnDisable()
    {
        if (playerController != null)
            playerController.OnThrowAnimationRequested -= PlayThrow;

        if (throwCoroutine != null)
        {
            StopCoroutine(throwCoroutine);
            throwCoroutine = null;
        }
    }

    private void Start()
    {
        // กันกรณี Reference ถูกตั้งช้ากว่า Awake
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // ให้ Event ถูกผูกแน่นอนในกรณี Script ถูก Enable/Disable ระหว่างเกม
        if (playerController != null)
        {
            playerController.OnThrowAnimationRequested -= PlayThrow;
            playerController.OnThrowAnimationRequested += PlayThrow;
        }

        initialized = true;
        PlayIdle();
    }

    private void Update()
    {
        if (!initialized || playerController == null || spriteRenderer == null)
            return;

        if (CurrentState == AnimationState.Throw)
            return;

        if (playerController.isRepositionMode)
        {
            PlayIdle();
            return;
        }

        if (!playerController.isMyTurn)
        {
            PlayIdle();
            return;
        }

        // ถึง Turn = กำลังเล็ง
        PlayAim();
    }

    // =========================================================
    // IDLE
    // =========================================================

    public void PlayIdle()
    {
        if (spriteRenderer == null)
            return;

        CurrentState = AnimationState.Idle;

        if (idleSprite != null)
            spriteRenderer.sprite = idleSprite;
    }

    // =========================================================
    // AIM
    // =========================================================

    public void PlayAim()
    {
        if (spriteRenderer == null)
            return;

        CurrentState = AnimationState.Aim;

        if (useActionSpriteWhileAiming && actionSprite != null)
        {
            spriteRenderer.sprite = actionSprite;
        }
        else if (idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
        }
    }

    // =========================================================
    // THROW
    // =========================================================

    public void PlayThrow()
    {
        if (spriteRenderer == null)
            return;

        if (throwCoroutine != null)
            StopCoroutine(throwCoroutine);

        throwCoroutine = StartCoroutine(ThrowRoutine());
    }

    private IEnumerator ThrowRoutine()
    {
        CurrentState = AnimationState.Throw;

        if (actionSprite != null)
            spriteRenderer.sprite = actionSprite;

        // คงท่าโยนตามเวลาที่กำหนด
        yield return new WaitForSeconds(
            Mathf.Max(0.01f, throwVisualDuration)
        );

        throwCoroutine = null;

        // PlayerController จะเป็นคนกำหนดว่าได้เทิร์นใหม่หรือไม่
        // ปกติหลังโยนจะกลับ Idle
        PlayIdle();
    }
}
