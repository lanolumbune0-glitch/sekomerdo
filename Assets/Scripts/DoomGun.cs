using UnityEngine;
using System.Collections; // Coroutine (Zamanlayıcı) için gerekli

public class DoomGun : MonoBehaviour
{
    [Header("Pompalı Şarjör Sistemi")]
    public int maxAmmo = 8;          // Toplam kapasite
    public float shellInsertTime = 0.5f; // Mermi başına doldurma süresi
    private int currentAmmo;         
    private bool isReloading = false;
    private Coroutine reloadCoroutine;

    [Header("Animasyon (Tepme)")]
    public WeaponMovement recoilScript; // SİLAH OBJENİ BURAYA SÜRÜKLE!

    [Header("Shotgun Atış Ayarları")]
    public int pellets = 10;        // Kaç saçma çıksın?
    public float spreadAngle = 5f;  // Dağılma açısı
    public float fireRate = 1f;     // Ateş hızı
    
    [Header("Güç ve Mesafe")]
    public float range = 50f;
    public float closeDamage = 10f; // Yakın hasar
    public float farDamage = 2f;    // Uzak hasar
    public float closeKnockback = 50f; // Yakın itme
    public float farKnockback = 5f;    // Uzak itme

    [Header("Görsel Efektler")]
    public Transform attackPoint;      // Namlu ucu (Boş obje)
    public GameObject bulletTrailPrefab; // Mermi izi prefabı
    public ParticleSystem muzzleFlash;   // Ateş efekti
    public GameObject impactEffect;      // Vuruş efekti (Kan/Duvar izi)

    [Header("Referanslar")]
    public Camera fpsCamera;
    public LayerMask vurulabilirKatmanlar;

    private float nextTimeToFire = 0f;

    void Start()
    {
        currentAmmo = maxAmmo;
    }

    void OnEnable()
    {
        isReloading = false;
    }

    void Update()
    {
        // 1. ATEŞ ETME KONTROLÜ
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            if (currentAmmo > 0)
            {
                // Doldururken ateş edersek doldurmayı kes
                if (isReloading) StopReload();
                
                nextTimeToFire = Time.time + 1f / fireRate;
                ShootShotgun();
            }
            else if (!isReloading)
            {
                // Mermi yoksa otomatik doldur
                StartReload();
            }
        }

        // 2. MANUEL RELOAD (R TUŞU)
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo && !isReloading)
        {
            StartReload();
        }
    }

    // --- YENİLEME FONKSİYONLARI ---
    void StartReload()
    {
        if (!isReloading) reloadCoroutine = StartCoroutine(ReloadSequence());
    }

    void StopReload()
    {
        if (reloadCoroutine != null) StopCoroutine(reloadCoroutine);
        isReloading = false;
    }

    IEnumerator ReloadSequence()
    {
        isReloading = true;
        // Şarjör dolana kadar tek tek mermi ekle
        while (currentAmmo < maxAmmo)
        {
            yield return new WaitForSeconds(shellInsertTime);
            currentAmmo++;
        }
        isReloading = false;
    }

    // --- ASIL ATEŞ ETME FONKSİYONU ---
    void ShootShotgun()
    {
        // Mermiyi azalt
        currentAmmo--;

        // SİLAH TEPME (RECOIL) EFEKTİ
        if (recoilScript != null)
        {
            recoilScript.RecoilFire();
        }

        // Namlu ateşi
        if (muzzleFlash != null) muzzleFlash.Play();

        // Başlangıç noktaları
        Vector3 eyePosition = fpsCamera.transform.position; // Hesaplama gözden
        Vector3 visualStartPoint = (attackPoint != null) ? attackPoint.position : eyePosition; // İz namludan

        // Saçma döngüsü
        for (int i = 0; i < pellets; i++)
        {
            // Dağılma (Spread) hesabı
            Vector3 deviation = Random.insideUnitCircle * spreadAngle;
            Quaternion rot = Quaternion.LookRotation(fpsCamera.transform.forward);
            Vector3 shotDirection = rot * Quaternion.Euler(deviation.x, deviation.y, 0) * Vector3.forward;

            Vector3 endPoint = eyePosition + (shotDirection * range);

            RaycastHit hit;
            if (Physics.Raycast(eyePosition, shotDirection, out hit, range, vurulabilirKatmanlar))
            {
                endPoint = hit.point;

                // Hasar ve İtme hesapları (Mesafeye göre)
                float distanceRatio = hit.distance / range;
                float currentDamage = Mathf.Lerp(closeDamage, farDamage, distanceRatio);
                float currentForce = Mathf.Lerp(closeKnockback, farKnockback, distanceRatio);

                // Hasar ver
                Target target = hit.transform.GetComponent<Target>();
                if (target != null) target.TakeDamage(currentDamage);

                // Düşmanı it
                SmartEnemy enemyAI = hit.transform.GetComponent<SmartEnemy>();
                if (enemyAI != null) enemyAI.AddKnockback(shotDirection, currentForce);

                // Vuruş efekti
                if (impactEffect != null)
                {
                    GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactGO, 1f);
                }
            }

            // Mermi İzi Çizimi
            if (bulletTrailPrefab != null)
            {
                GameObject trail = Instantiate(bulletTrailPrefab);
                LineRenderer lr = trail.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    lr.SetPosition(0, visualStartPoint);
                    lr.SetPosition(1, endPoint);
                }
            }
        }
    }
}