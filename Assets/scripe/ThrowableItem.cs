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

            Debug.Log($"💥 โดนเป้าหมาย! เด้งไป {currentTotalBounces} ครั้ง ทำดาเมจรวม: {totalDamage}");
            DestroyProjectile();
            return;
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            groundHitCount++;
            currentTotalBounces++;

            ComboUI.Instance?.ShowCombo(currentTotalBounces);

            if (groundHitCount >= maxGroundHits)
            {
                DestroyProjectile();
                return;
            }
        }
        else if (collision.gameObject.CompareTag("Prop"))
        {
            currentTotalBounces++;

            ComboUI.Instance?.ShowCombo(currentTotalBounces);

            if (currentTotalBounces >= maxPropBounces)
            {
                DestroyProjectile();
                return;
            }
        }
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