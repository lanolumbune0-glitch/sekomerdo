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
    private bool isStunned = false; // Şu an baygın mı?

    void Start()
    {
        maxHealth = health; // Başlangıç canını hafızaya al

        if (modelRenderer == null)
        {
            modelRenderer = GetComponent<Renderer>();
            if (modelRenderer == null) modelRenderer = GetComponentInChildren<Renderer>();
        }

        if (modelRenderer != null) originalColor = modelRenderer.material.color;
    }

    public void TakeDamage(float amount)
    {
        // Eğer zaten baygınsa hasar almasın (Veya istersen alsın ama süresi uzasın)
        if (isStunned) return;

        health -= amount;

        // --- FLASH EFEKTİ ---
        if (modelRenderer != null && !isFlashing) StartCoroutine(FlashRoutine());

        // --- ÖLÜM VEYA BAYILMA KONTROLÜ ---
        if (health <= 0f)
        {
            if (isInvincible)
            {
                StartCoroutine(StunRoutine()); // Ölümsüzse bayılt
            }
            else
            {
                Die(); // Normal düşmansa öldür
            }
        }
    }

    // --- SERSEMLEME (STUN) DÖNGÜSÜ ---
    IEnumerator StunRoutine()
    {
        isStunned = true;
        
        // 1. Yapay Zekayı Durdur
        SmartEnemy ai = GetComponent<SmartEnemy>();
        if (ai != null) ai.SetStunnedState(true);

        // 2. Rengi Değiştir (Görsel Geri Bildirim)
        if (modelRenderer != null) modelRenderer.material.color = stunColor;

        Debug.Log("Düşman Sersemledi! Kaçmak için " + stunDuration + " saniyen var!");

        // 3. Bekle
        yield return new WaitForSeconds(stunDuration);

        // 4. Canını Fullee ve Uyandır
        health = maxHealth;
        isStunned = false;

        if (modelRenderer != null) modelRenderer.material.color = originalColor;
        
        if (ai != null) ai.SetStunnedState(false);
    }

    IEnumerator FlashRoutine()
    {
        isFlashing = true;
        if (modelRenderer != null) modelRenderer.material.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        
        // Eğer o sırada sersemlediyse (rengi mavi olduysa), orijinal renge dönme!
        if (!isStunned && modelRenderer != null) 
            modelRenderer.material.color = originalColor;
            
        isFlashing = false;
    }

    void Die()
    {
        Destroy(gameObject);
    }
}