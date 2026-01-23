using UnityEngine;

public class DoomGun : MonoBehaviour
{
    [Header("Shotgun Ayarları")]
    public int pellets = 10;
    public float spreadAngle = 5f;
    public float fireRate = 1f;
    
    [Header("Güç ve Mesafe")]
    public float range = 50f;
    public float closeDamage = 10f;
    public float farDamage = 2f;
    public float closeKnockback = 50f;
    public float farKnockback = 5f;

    [Header("Görsel Efektler")]
    public Transform attackPoint; // YENİ: Namlu ucunu buraya atacağız
    public GameObject bulletTrailPrefab;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;

    [Header("Referanslar")]
    public Camera fpsCamera;
    public LayerMask vurulabilirKatmanlar;

    private float nextTimeToFire = 0f;

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            ShootShotgun();
        }
    }

    void ShootShotgun()
    {
        // Ateş efektini namlu ucunda oynat
        if (muzzleFlash != null) muzzleFlash.Play();

        // Matematiksel Başlangıç (Gözümüz)
        Vector3 eyePosition = fpsCamera.transform.position;
        
        // Görsel Başlangıç (Namlu Ucu) - Eğer atamayı unuttuysan kamerayı kullan
        Vector3 visualStartPoint = (attackPoint != null) ? attackPoint.position : eyePosition;

        for (int i = 0; i < pellets; i++)
        {
            // --- 1. HESAPLAMA (KAMERADAN) ---
            // Dağılma ve nişan alma işlemleri hala GÖZ hizasından yapılmalı
            Vector3 deviation = Random.insideUnitCircle * spreadAngle;
            Quaternion rot = Quaternion.LookRotation(fpsCamera.transform.forward);
            Vector3 shotDirection = rot * Quaternion.Euler(deviation.x, deviation.y, 0) * Vector3.forward;

            // Varsayılan bitiş noktası (Menzil sonu)
            Vector3 endPoint = eyePosition + (shotDirection * range);

            RaycastHit hit;
            if (Physics.Raycast(eyePosition, shotDirection, out hit, range, vurulabilirKatmanlar))
            {
                endPoint = hit.point; // Bir şeye çarptıysa iz orada biter

                // Hasar işlemleri...
                float distanceRatio = hit.distance / range;
                float currentDamage = Mathf.Lerp(closeDamage, farDamage, distanceRatio);
                float currentForce = Mathf.Lerp(closeKnockback, farKnockback, distanceRatio);

                Target target = hit.transform.GetComponent<Target>();
                if (target != null) target.TakeDamage(currentDamage);

                SmartEnemy enemyAI = hit.transform.GetComponent<SmartEnemy>();
                if (enemyAI != null) enemyAI.AddKnockback(shotDirection, currentForce); // İtme yönü mermi yönüdür

                if (impactEffect != null)
                {
                    GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactGO, 1f);
                }
            }

            // --- 2. GÖRSELLİK (NAMLUDAN) ---
            if (bulletTrailPrefab != null)
            {
                GameObject trail = Instantiate(bulletTrailPrefab);
                LineRenderer lr = trail.GetComponent<LineRenderer>();

                if (lr != null)
                {
                    // İŞTE SİHİR BURADA:
                    // Çizginin başı -> Namlu Ucu (attackPoint)
                    // Çizginin sonu -> Gözün gördüğü hedef (endPoint)
                    lr.SetPosition(0, visualStartPoint); 
                    lr.SetPosition(1, endPoint);
                }
                // Silme işini trail üzerindeki script yapıyor, buraya dokunma.
            }
        }
    }
}