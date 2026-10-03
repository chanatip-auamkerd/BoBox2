using UnityEngine;
using UnityEngine.UI;

public class CharacterHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthSlider; // ลาก Slider UI เลือดมาใส่ (Optional)

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " พ่ายแพ้แล้ว!");
        // เพิ่มอนิเมชันตาย หรือสั่งจบเกมตรงนี้ได้
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // เมื่อโดนของที่ปาเข้ามาชน
        if (collision.gameObject.CompareTag("Projectile"))
        {
            TakeDamage(25f);
            Destroy(collision.gameObject); // ทำลายของปา
        }
    }
}
