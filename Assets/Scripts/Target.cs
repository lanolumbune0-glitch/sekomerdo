using UnityEngine;
using System.Collections; // Coroutine (Zamanlayıcı) için gerekli

public class Target : MonoBehaviour
{
    [Header("Can Ayarları")]
    public float health = 50f;

    [Header("Flash Efekti Ayarları")]
    public Color flashColor = Color.red;    // Hangi renk yansın? (Genelde kırmızı)
    public float flashDuration = 0.1f;      // Kaç saniye sürsün? (Çok kısa olmalı)
    
    // Düşmanın dış görünüşünü (Materyalini) tutan bileşen
    public Renderer modelRenderer; 

    private Color originalColor; // Düşmanın orijinal rengini hafızada tutacağız
    private bool isFlashing = false; // Aynı anda üst üste flash patlamasın diye kontrol

    void Start()
    {
        // EĞER İNSPECTOR'DAN RENDERER ATAMADIYSAN OTOMATİK BULMAYA ÇALIŞALIM:
        if (modelRenderer == null)
        {
            // Önce kendi üzerimde ara
            modelRenderer = GetComponent<Renderer>();
            
            // Bulamazsan alt objelerimde (Çocuklarımda) ara (Karmaşık modeller için)
            if (modelRenderer == null)
            {
                modelRenderer = GetComponentInChildren<Renderer>();
            }
        }

        // Eğer bir renderer bulduysak orijinal rengini hafızaya alalım
        if (modelRenderer != null)
        {
            // NOT: 'material.color' kullanıyoruz, 'sharedMaterial' değil.
            // Böylece sadece vurulan düşman renk değiştirir, hepsi değil.
            originalColor = modelRenderer.material.color;
        }
        else
        {
            Debug.LogWarning(name + " objesinde RENDERER bulunamadı! Flash efekti çalışmayacak.");
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            Die();
        }

        // --- FLASH EFEKTİNİ BAŞLAT ---
        // Renderer varsa ve şu an zaten yanıp sönmüyorsa başlat
        if (modelRenderer != null && !isFlashing)
        {
            StartCoroutine(FlashRoutine());
        }
    }

    // Zaman ayarlı yanıp sönme işlemi (Coroutine)
    IEnumerator FlashRoutine()
    {
        isFlashing = true;
        // Rengi kırmızı yap
        modelRenderer.material.color = flashColor;
        
        // Belirlenen süre kadar bekle (0.1 saniye)
        yield return new WaitForSeconds(flashDuration);
        
        // Rengi orijinal haline döndür
        modelRenderer.material.color = originalColor;
        isFlashing = false;
    }

    void Die()
    {
        Destroy(gameObject);
    }
}