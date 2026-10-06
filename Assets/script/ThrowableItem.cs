using System.Collections;
using UnityEngine;

public class ThrowableItem : MonoBehaviour
{
    [Header("Damage Settings")]
    public float baseDamage = 20f;
    public float bonusDamagePerBounce = 5f;

    [Header("Bounce Settings")]
    public int maxGroundHits = 2;
    public float maxLifeTime = 8f;

    [Header("Audio Settings")]
    [Tooltip("ใส่ไฟล์เสียง BouncingBall1")]
    public AudioClip bounceSound;
    public AudioClip hitTargetSound;

    [Header("First Bounce Sound Settings (ตัดเอาเฉพาะเสียงเด้งแรก)")]
    [Tooltip("จุดเริ่มเสียงเด้งแรก (วินาที)")]
    public float bounceStartTime = 0.0f;
    [Tooltip("ความยาวของเสียงเด้งแรกเท่านั้น (ไม่เอาเสียงดึ๋งหลัง)")]
    public float bounceDuration = 0.28f;

    [Range(0.85f, 1.15f)]
    public float minPitch = 0.95f;
    [Range(0.85f, 1.15f)]
    public float maxPitch = 1.05f;

    private AudioSource audioSource;
    private int currentTotalBounces = 0;
    private int groundHitCount = 0;
    private bool isDestroyed = false;
    private Coroutine cutOffCoroutine;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; 
    }

    void Start()
    {
        Destroy(gameObject, maxLifeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDestroyed) return;

        CharacterHealth health = collision.gameObject.GetComponent<CharacterHealth>();
        if (health != null)
        {
            float totalDamage = baseDamage + (currentTotalBounces * bonusDamagePerBounce);
            health.TakeDamage(totalDamage);

            if (hitTargetSound != null)
            {
                AudioSource.PlayClipAtPoint(hitTargetSound, transform.position);
            }

            Debug.Log($"💥 โดนเป้าหมาย! เด้ง {currentTotalBounces} ครั้ง | ดาเมจรวม: {totalDamage}");
            DestroyProjectile();
            return;
        }

        bool isProp = collision.gameObject.CompareTag("Prop") ||
                      (collision.transform.parent != null && collision.transform.parent.CompareTag("Prop"));
        bool isWall = collision.gameObject.name.ToLower().Contains("wall");
        bool isGround = collision.gameObject.CompareTag("Ground") || collision.gameObject.name.ToLower().Contains("ground");

        if (isProp || isWall)
        {
            currentTotalBounces++;
            PlayInstantFirstBounceSound(); 
            ComboUI.Instance?.ShowCombo(currentTotalBounces);
        }
        else if (isGround)
        {
            groundHitCount++;
            currentTotalBounces++;
            PlayInstantFirstBounceSound(); 
            ComboUI.Instance?.ShowCombo(currentTotalBounces);

            if (groundHitCount >= maxGroundHits)
            {
                DestroyProjectile();
                return;
            }
        }
    }

    private void PlayInstantFirstBounceSound()
    {
        if (bounceSound == null || audioSource == null) return;

        if (cutOffCoroutine != null)
        {
            StopCoroutine(cutOffCoroutine);
        }
        audioSource.Stop();

        audioSource.clip = bounceSound;
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.time = bounceStartTime; 
        audioSource.Play();
        cutOffCoroutine = StartCoroutine(StopSoundAfterDuration(bounceDuration));
    }

    private IEnumerator StopSoundAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        audioSource.Stop();
    }

    private void DestroyProjectile()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        NotifyTurnManager();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            NotifyTurnManager();
        }
    }

    private void NotifyTurnManager()
    {
        var pvpManager = FindAnyObjectByType<TurnManagerPvP>();
        if (pvpManager != null)
        {
            pvpManager.OnProjectileDestroyed();
            return;
        }

        var soloManager = FindAnyObjectByType<TurnManager>();
        if (soloManager != null)
        {
            soloManager.OnProjectileDestroyed();
        }
    }
}