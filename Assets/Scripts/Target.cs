using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour
{
    [Header("Can Ayarları")]
    public float health = 100f;
    private float maxHealth;

    [Header("Ölümsüzlük (Nemesis) Modu")]
    public bool isInvincible = false;   
    public float stunDuration = 5f;     
    public Color stunColor = Color.blue;

    [Header("--- PATLAYICI VARİL MODU (YENİ) ---")]
    public bool isExplosive = false;       // Bu kutuyu işaretlersen varil olur!
    public float explosionRadius = 10f;    // Patlama yarıçapı
    public float explosionForce = 700f;    // İtme gücü
    public float explosionDamage = 100f;   // Patlama hasarı
    public GameObject explosionEffect;     // Patlama efekti (Particle System)
    public AudioClip explosionSound;       // Patlama sesi

    [Header("Görsel (Flash) Efekti")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.1f;
    public Renderer modelRenderer; 

    private Color originalColor;
    private bool isFlashing = false;
    private bool isStunned = false; 

    void Start()
    {
        maxHealth = health;
        if (modelRenderer == null)
        {
            modelRenderer = GetComponent<Renderer>();
            if (modelRenderer == null) modelRenderer = GetComponentInChildren<Renderer>();
        }
        if (modelRenderer != null) originalColor = modelRenderer.material.color;
    }

    public void TakeDamage(float amount)
    {
        if (isStunned) return;

        health -= amount;

        if (modelRenderer != null && !isFlashing) StartCoroutine(FlashRoutine());

        if (health <= 0f)
        {
            if (isExplosive)
            {
                Explode(); // Varilse patlat
            }
            else if (isInvincible)
            {
                if (!isStunned) StartCoroutine(StunRoutine()); // Nemesis ise bayılt
            }
            else
            {
                Die(); // Normalse yok et
            }
        }
    }

    // --- YENİ: PATLAMA FONKSİYONU ---
    void Explode()
    {
        // 1. Görsel Efekt
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, transform.rotation);
        }

        // 2. Ses Efekti (AudioSource.PlayClipAtPoint geçici obje yaratır ve sesi çalar)
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // 3. ALAN HASARI (OverlapSphere)
        // Patlama merkezindeki tüm objeleri bul
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            // A. Düşmanlara Hasar Ver
            Target target = nearbyObject.GetComponent<Target>();
            if (target != null && target != this) // Kendini tekrar patlatmasın
            {
                // Mesafeye göre hasar düşsün (Yakınsa çok, uzaksa az)
                float distance = Vector3.Distance(transform.position, nearbyObject.transform.position);
                float damagePercent = 1 - (distance / explosionRadius); // 0 ile 1 arası
                if (damagePercent < 0) damagePercent = 0;
                
                target.TakeDamage(explosionDamage * damagePercent);
            }

            // B. Oyuncuya Hasar Ver (Dikkat et kendini de yakarsın!)
            PlayerHealth playerHealth = nearbyObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(explosionDamage * 0.5f); // Oyuncuya biraz daha az vursun
            }

            // C. Fiziksel Fırlatma (Rigidbody)
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            // D. Smart Enemy İtme (Bizim özel kodumuz)
            SmartEnemy enemyAI = nearbyObject.GetComponent<SmartEnemy>();
            if (enemyAI != null)
            {
                // Patlama merkezinden düşmana doğru bir vektör
                Vector3 knockbackDir = nearbyObject.transform.position - transform.position;
                enemyAI.AddKnockback(knockbackDir, explosionForce * 0.1f); // AI itme kuvveti
            }
        }

        // 4. Varili Yok Et
        Destroy(gameObject);
    }

    IEnumerator StunRoutine()
    {
        if(isStunned && health > 0) yield break; 
        isStunned = true; 
        health = 0; 
        
        SmartEnemy ai = GetComponent<SmartEnemy>();
        if (ai != null) ai.SetStunnedState(true);

        if (modelRenderer != null) modelRenderer.material.color = stunColor;

        float timer = 0f;
        while(timer < stunDuration)
        {
            timer += Time.deltaTime;
            if(this == null || gameObject == null) yield break; 
            yield return null;
        }

        health = maxHealth;
        if (modelRenderer != null) modelRenderer.material.color = originalColor;
        if (ai != null) ai.SetStunnedState(false);
        isStunned = false; 
    }

    IEnumerator FlashRoutine()
    {
        isFlashing = true;
        if (modelRenderer != null && !isStunned) modelRenderer.material.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        if (modelRenderer != null && !isStunned) modelRenderer.material.color = originalColor;
        isFlashing = false;
    }

    void Die()
    {
        Destroy(gameObject);
    }
    
    void OnDisable()
    {
        isStunned = false;
        isFlashing = false;
    }
}