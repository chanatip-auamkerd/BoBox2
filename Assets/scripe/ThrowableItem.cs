using UnityEngine;

public class ThrowableItem : MonoBehaviour
{
    [Header("Damage Settings")]
    public float baseDamage = 20f;
    public float bonusDamagePerBounce = 5f;

    [Header("Bounce Settings")]
    public int maxGroundHits = 2;  
    public int maxPropBounces = 4;  
    public float maxLifeTime = 6f; 

    private int currentTotalBounces = 0; 
    private int groundHitCount = 0;     
    private bool isDestroyed = false;

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

            Debug.Log($"💥 โดนเป้าหมาย! เด้งรวม {currentTotalBounces} ครั้ง ดาเมจสุทธิ: {totalDamage}");
            DestroyProjectile();
            return;
        }
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundHitCount++;
            currentTotalBounces++;
            if (groundHitCount >= maxGroundHits)
            {
                DestroyProjectile();
                return;
            }
        }
        else if (collision.gameObject.CompareTag("Prop"))
        {
            currentTotalBounces++;
            if (currentTotalBounces >= maxPropBounces)
            {
                DestroyProjectile();
            }
        }
    }

    private void DestroyProjectile()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        FindAnyObjectByType<TurnManager>()?.OnProjectileDestroyed();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            FindAnyObjectByType<TurnManager>()?.OnProjectileDestroyed();
        }
    }
}