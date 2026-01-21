using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 50f;

    // Hasar alma fonksiyonu
    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        // Şimdilik objeyi yok edelim. İleride buraya patlama efekti ekleriz.
        Destroy(gameObject);
    }
}