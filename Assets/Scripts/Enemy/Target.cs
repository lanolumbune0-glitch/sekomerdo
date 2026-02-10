using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Target : MonoBehaviour
{
    [Header("Can Ayarları")]
    public float health = 100f;
    private float maxHealth;

    // --- BU DEĞİŞKEN YENİ EKLENDİ ---
    private bool hasExploded = false; // "Patladım mı?" kontrolü
    // --------------------------------

    [Header("Ölümsüzlük (Nemesis) Modu")]
    public bool isInvincible = false;
    public float stunDuration = 5f;
    public Color stunColor = Color.blue;
    private bool isStunned = false;

    [Header("--- PATLAYICI VARİL MODU ---")]
    public bool isExplosive = false;
    public GameObject destroyedVersion;
    public float areaDamage = 80f;
    public float explosionRadius = 10f;
    public float explosionForce = 700f;
    public GameObject explosionEffect;
    public AudioClip explosionSound;

    [Header("Görsel (Flash) Efekti")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.1f;
    public Renderer modelRenderer;

    private Color originalColor;
    private bool isFlashing = false;

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
        // Eğer zaten patladıysak veya baygınsak daha fazla hasar alma
        if (isStunned || hasExploded) return;

        health -= amount;

        if (modelRenderer != null && !isFlashing) StartCoroutine(FlashRoutine());

        if (health <= 0f)
        {
            if (isExplosive)
            {
                // BURASI ÇOK ÖNEMLİ: Sadece patlamadıysa patlat!
                if (!hasExploded) 
                {
                    Explode();
                }
            }
            else if (isInvincible)
            {
                StartCoroutine(StunRoutine());
            }
            else
            {
                Die();
            }
        }
    }

    void Explode()
    {
        // 1. Bayrağı dik: "Ben artık patladım, bir daha bu fonksiyonu çağırma!"
        hasExploded = true;

        // Kırık versiyonu yarat
        if (destroyedVersion != null) Instantiate(destroyedVersion, transform.position, transform.rotation);

        // Efekt ve Ses
        if (explosionEffect != null) Instantiate(explosionEffect, transform.position, Quaternion.identity);
        if (explosionSound != null) AudioSource.PlayClipAtPoint(explosionSound, transform.position);

        // Hasar Alanı
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        List<GameObject> damagedObjects = new List<GameObject>();

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null) rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);

            PlayerHealth player = nearbyObject.GetComponent<PlayerHealth>();
            if (player != null && !damagedObjects.Contains(player.gameObject))
            {
                player.TakeDamage(areaDamage);
                damagedObjects.Add(player.gameObject);
            }

            Target target = nearbyObject.GetComponent<Target>();
            if (target != null && target != this && !damagedObjects.Contains(target.gameObject))
            {
                target.TakeDamage(areaDamage);
                damagedObjects.Add(target.gameObject);
            }
        }

        // Sarsıntı
        if (Camera.main != null && CameraShake.Instance != null)
        {
            float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
            if (distance < 15f)
            {
                float shakeStrength = 1f - (distance / 15f); 
                CameraShake.Instance.Shake(0.5f, shakeStrength * 1.5f);
            }
        }

        Destroy(gameObject);
    }

    // ... (Diğer fonksiyonlar StunRoutine, FlashRoutine aynı kalıyor) ...
    IEnumerator StunRoutine()
    {
        if (isStunned) yield break; 
        isStunned = true; 
        health = 0; 
        SmartEnemy ai = GetComponent<SmartEnemy>();
        if (ai != null) ai.enabled = false; 
        if (modelRenderer != null) modelRenderer.material.color = stunColor;
        yield return new WaitForSeconds(stunDuration);
        health = maxHealth;
        if (modelRenderer != null) modelRenderer.material.color = originalColor;
        if (ai != null) ai.enabled = true; 
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
}