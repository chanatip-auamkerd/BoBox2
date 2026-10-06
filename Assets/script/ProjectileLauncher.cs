using System.Collections;
using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    [Header("Setup")]
    public GameObject projectilePrefab;
    public Transform shootPoint;

    [Header("Settings")]
    public float minForce = 5f;
    public float maxForce = 25f;
    public float chargeSpeed = 15f;
    public Vector2 baseDirection = new Vector2(1, 1).normalized; 

    private float currentForce;
    private bool isCharging = false;
    private Collider2D launcherCollider;

    void Awake()
    {
        launcherCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            currentForce = minForce;
        }

        if (isCharging && Input.GetMouseButton(0))
        {
            currentForce += chargeSpeed * Time.deltaTime;
            currentForce = Mathf.Clamp(currentForce, minForce, maxForce);
        }

        if (isCharging && Input.GetMouseButtonUp(0))
        {
            Shoot();
            isCharging = false;
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null || shootPoint == null) return;

        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

        ThrowableItem itemScript = projectile.GetComponent<ThrowableItem>();
        if (itemScript != null)
        {
            itemScript.SetThrower(this.gameObject);
        }

        Collider2D projCol = projectile.GetComponent<Collider2D>();
        if (launcherCollider != null && projCol != null)
        {
            Physics2D.IgnoreCollision(launcherCollider, projCol, true);
            StartCoroutine(ReEnableSelfCollisionRoutine(launcherCollider, projCol, 0.15f));
        }

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 shootVector = baseDirection * currentForce;
            rb.linearVelocity = shootVector;
        }
    }

    private IEnumerator ReEnableSelfCollisionRoutine(Collider2D colA, Collider2D colB, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (colA != null && colB != null)
        {
            Physics2D.IgnoreCollision(colA, colB, false);
        }
    }
}