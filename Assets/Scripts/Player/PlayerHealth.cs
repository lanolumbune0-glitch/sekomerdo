using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Can Ayarları")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI Bağlantıları")]
    public Text healthText;       // Can yazısı
    public Image damageOverlay;   // YENİ: Kırmızı ekran görseli

    [Header("Efekt Ayarları")]
    public float overlayDuration = 0.5f; // Kırmızılık ne kadar sürede kaybolsun?
    public float overlayIntensity = 0.4f; // Kırmızılık ne kadar koyu olsun? (0-1 arası)

    [Header("Sesler")]
    public AudioSource audioSource;
    public AudioClip hurtSound;   // YENİ: Hasar alma sesi (Ah!)
    public AudioClip deathSound;  // Ölme sesi

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        
        // Başlangıçta kırmızılığı tamamen gizle
        if (damageOverlay != null)
        {
            damageOverlay.color = new Color(1, 0, 0, 0);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
if (CameraShake.Instance != null) 
            CameraShake.Instance.Shake(0.2f, 0.2f);
        // --- YENİ: HASAR SESİ ---
        if (hurtSound != null && audioSource != null) 
        {
            // Ses üst üste binmesin diye pitch ile oynayabiliriz
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(hurtSound);
        }

        // UI Güncelle
        UpdateHealthUI();

        // --- YENİ: KIRMIZI EKRAN EFEKTİ ---
        if (damageOverlay != null) 
        {
            StopCoroutine(FadeOverlay()); // Önceki efekt hala sürüyorsa durdur
            StartCoroutine(FadeOverlay()); // Yenisini başlat
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Ekranı önce kırmızı yapıp sonra yavaşça şeffaflaştıran fonksiyon
    IEnumerator FadeOverlay()
    {
        // 1. Anında kırmızı yap
        damageOverlay.color = new Color(1, 0, 0, overlayIntensity);

        // 2. Yavaşça yok et
        float timer = 0f;
        while (timer < overlayDuration)
        {
            timer += Time.deltaTime;
            // Zaman geçtikçe alpha değerini düşür
            float newAlpha = Mathf.Lerp(overlayIntensity, 0f, timer / overlayDuration);
            damageOverlay.color = new Color(1, 0, 0, newAlpha);
            yield return null;
        }

        // 3. Garanti olsun diye tamamen temizle
        damageOverlay.color = new Color(1, 0, 0, 0);
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "HP: " + Mathf.Ceil(currentHealth).ToString();
            
            // Can azaldıkça yazının rengini de kırmızıya çekebiliriz (Opsiyonel)
            if (currentHealth <= 30) healthText.color = Color.red;
            else healthText.color = Color.white; // Veya eski rengi neyse
        }
    }

    void Die()
    {
        Debug.Log("ÖLDÜN!");
        
        if (deathSound != null) 
            AudioSource.PlayClipAtPoint(deathSound, transform.position);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        
        // Canımız 100'ü geçmesin
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        // Ekrandaki yazıyı güncelle
        UpdateHealthUI();
        
        // Kırmızılık varsa temizle
        if (damageOverlay != null) 
            damageOverlay.color = Color.clear;
    }
}