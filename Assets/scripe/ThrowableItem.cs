using UnityEngine;

public class ThrowableItem : MonoBehaviour
{
    public int maxBounces = 2; // จำนวนครั้งสูงสุดที่จะให้เด้งก่อนแตก
    private int currentBounces = 0;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ถ้าโดน Prop หรือพื้น
        if (collision.gameObject.CompareTag("Prop") || collision.gameObject.CompareTag("Ground"))
        {
            currentBounces++;

            // เล่นเสียงกระดอน / Effect ฝุ่น
            if (currentBounces > maxBounces)
            {
                Destroy(gameObject); // เด้งครบโควตาแล้วทำลายตัวเอง
            }
        }
        // ถ้าโดนตัวละครฝั่งตรงข้าม
        else if (collision.gameObject.CompareTag("Player"))
        {
            // ทำดาเมจ แล้วทำลาย
            Destroy(gameObject);
        }
    }
} 

