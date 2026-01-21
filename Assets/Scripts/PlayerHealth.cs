using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float health = 100f;

    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log("Canın azaldı! Kalan Can: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("ÖLDÜN! Game Over.");
        // İleride buraya oyun yeniden başlatma kodu gelecek
        Time.timeScale = 0; // Oyunu dondur
    }
}