using UnityEngine;
using UnityEngine.UI; // UI işlemleri için şart
using UnityEngine.SceneManagement; // Ölünce bölümü yeniden başlatmak için

public class PlayerHealth : MonoBehaviour
{
    [Header("Can Ayarları")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI Bağlantıları")]
    public Text healthText;      // Ekrana yazılacak yazı
    public Image damageOverlay;  // (Opsiyonel) Hasar alınca çıkan kanlı ekran görseli

    [Header("Sesler")]
    public AudioSource audioSource;
    public AudioClip hurtSound; // Can yanma sesi (Ah!)
    public AudioClip deathSound; // Ölme sesi

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    // --- DÜŞMANIN ÇAĞIRDIĞI FONKSİYON ---
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        // Can yanma sesi
        if (hurtSound != null && audioSource != null) 
            audioSource.PlayOneShot(hurtSound);

        // UI Güncelle
        UpdateHealthUI();

        // Kan Efekti (Varsa)
        if (damageOverlay != null) 
            StartCoroutine(ShowDamageEffect());

        // Ölüm Kontrolü
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            // Canı tam sayıya yuvarla
            healthText.text = "HP: " + Mathf.Ceil(currentHealth).ToString();
            
            // Can azaldıkça rengi soluklaşsın veya değişsin istersen buraya eklenir
        }
    }

    void Die()
    {
        Debug.Log("ÖLDÜN!");
        
        // Ölüm sesi
        if (deathSound != null && audioSource != null) 
            AudioSource.PlayClipAtPoint(deathSound, transform.position);

        // Sahneyi Yeniden Başlat (Basit Ölüm)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Ekranın kısa süreliğine kızarması için
    System.Collections.IEnumerator ShowDamageEffect()
    {
        damageOverlay.color = new Color(1, 0, 0, 0.5f); // Yarım şeffaf kırmızı
        yield return new WaitForSeconds(0.1f);
        damageOverlay.color = Color.clear; // Görünmez
    }
}