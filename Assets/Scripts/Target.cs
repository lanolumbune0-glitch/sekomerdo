using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour
{
    [Header("Can Ayarları")]
    public float health = 100f;
    private float maxHealth; // Canı dolarken kaça dolacak?

    [Header("Ölümsüzlük (Nemesis) Modu")]
    public bool isInvincible = false;   // Bu kutuyu işaretlersen ölmez, bayılır!
    public float stunDuration = 5f;     // Kaç saniye baygın kalsın?
    public Color stunColor = Color.blue; // Bayılınca ne renk olsun?

    [Header("Flash Efekti")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.1f;
    public Renderer modelRenderer; 

    private Color originalColor;
    private bool isFlashing = false;
    
    // YENİ: Bu değişkeni public yapalım ki Inspector'dan takılı kalıp kalmadığını görebil
    [Header("Durum (Debug)")]
    public bool isStunned = false; 

    void Start()
    {
        maxHealth = health;

        // Renderer'ı otomatik bul
        if (modelRenderer == null)
        {
            modelRenderer = GetComponent<Renderer>();
            if (modelRenderer == null) modelRenderer = GetComponentInChildren<Renderer>();
        }

        if (modelRenderer != null) originalColor = modelRenderer.material.color;
    }

    public void TakeDamage(float amount)
    {
        // 1. KORUMA: Eğer zaten sersemlemişse hasar alma!
        if (isStunned) return;

        health -= amount;

        // Flash efekti
        if (modelRenderer != null && !isFlashing) StartCoroutine(FlashRoutine());

        // Ölüm veya Bayılma Kontrolü
        if (health <= 0f)
        {
            if (isInvincible)
            {
                // ÇİFTE KORUMA: Zaten bayılma süreci başlamışsa tekrar başlatma
                if (!isStunned) StartCoroutine(StunRoutine());
            }
            else
            {
                Die();
            }
        }
    }

    IEnumerator StunRoutine()
    {
        isStunned = true; // Bayılma modu AÇIK
        health = 0; // Canı 0'da sabitle

        // 1. Yapay Zekayı Durdur
        SmartEnemy ai = GetComponent<SmartEnemy>();
        if (ai != null) ai.SetStunnedState(true);

        // 2. Rengi Değiştir
        if (modelRenderer != null) modelRenderer.material.color = stunColor;

        Debug.Log("NEMESIS BAYILDI! " + stunDuration + " saniye bekliyor...");

        // 3. Bekle (Bu sırada hasar alamaz)
        yield return new WaitForSeconds(stunDuration);

        // --- UYANIŞ ---

        // 4. Canı Fullee
        health = maxHealth;

        // 5. Rengi Düzelt
        if (modelRenderer != null) modelRenderer.material.color = originalColor;
        
        // 6. Yapay Zekayı Başlat
        if (ai != null) ai.SetStunnedState(false);

        // 7. EN ÖNEMLİSİ: Bayılma modunu KAPAT
        isStunned = false;
        
        Debug.Log("NEMESIS UYANDI!");
    }

    IEnumerator FlashRoutine()
    {
        isFlashing = true;
        // Eğer o sırada baygın değilse kırmızı yap (Baygınsa mavi kalsın)
        if (modelRenderer != null && !isStunned) modelRenderer.material.color = flashColor;
        
        yield return new WaitForSeconds(flashDuration);
        
        // Sadece baygın DEĞİLSE eski rengine dön
        if (modelRenderer != null && !isStunned) 
            modelRenderer.material.color = originalColor;
            
        isFlashing = false;
    }

    void Die()
    {
        Destroy(gameObject);
    }
    
    // Eğer obje kapanıp açılırsa bug'a girmemesi için resetle
    void OnDisable()
    {
        isStunned = false;
        isFlashing = false;
    }
}