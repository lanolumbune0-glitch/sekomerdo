using UnityEngine;
using UnityEngine.UI; 
using System.Collections;

public class DoomGun : MonoBehaviour
{
    [Header("UI Bağlantıları")]
    public Text ammoText;        
    public CrosshairHUD crosshairScript; 

    [Header("Ses Efektleri")]
    public AudioSource audioSource;
    public AudioClip shootSound;
    [Range(0f, 1f)] public float shootVolume = 1f; 
    
    public AudioClip shellInsertSound;
    [Range(0f, 1f)] public float reloadVolume = 0.8f; 

    public AudioClip pumpSound;
    [Range(0f, 1f)] public float pumpVolume = 1f;
    public float pumpDelay = 0.5f;

    [Header("Mermi Sistemi (YENİ)")]
    public int maxMagazineSize = 8;    // Şarjör kapasitesi (Pompalı haznesi)
    public int currentMagazineAmmo;    // Şu an namludaki mermi
    
    public int maxReserveAmmo = 32;    // Cebe en fazla kaç mermi sığar?
    public int currentReserveAmmo = 16;// Oyuna başlarken cepte kaç mermi var?
    
    public float shellInsertTime = 0.5f;
    private bool isReloading = false;
    private Coroutine reloadCoroutine;

    [Header("Animasyon & Fizik")]
    public WeaponMovement weaponMovement;

    [Header("Shotgun Ayarları")]
    public int pellets = 10;
    public float spreadAngle = 5f;
    public float fireRate = 1f;
    public float range = 50f;
    public float closeDamage = 10f;
    public float farDamage = 2f;
    public float closeKnockback = 50f;
    public float farKnockback = 5f;

    [Header("Görsel Efektler")]
    public Transform attackPoint;
    public GameObject bulletTrailPrefab;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;

    [Header("Referanslar")]
    public Camera fpsCamera;
    public LayerMask vurulabilirKatmanlar;

    private float nextTimeToFire = 0f;

    void Start()
    {
        currentMagazineAmmo = maxMagazineSize; // Oyuna dolu silahla başla
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        UpdateAmmoUI(); 
        if (crosshairScript != null) crosshairScript.HideReloadRing();
    }

    void OnEnable()
    {
        isReloading = false;
        if (weaponMovement != null) weaponMovement.SetReloading(false);
        if (crosshairScript != null) crosshairScript.HideReloadRing();
        UpdateAmmoUI();
    }

    void Update()
    {
        // 1. RELOAD KESİCİ
        if ((Input.GetButton("Fire1") || Input.GetButton("Fire2")) && isReloading)
        {
            StopReload();
        }

        // 2. ATEŞ ETME
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            // Şarjörde mermi varsa sık
            if (currentMagazineAmmo > 0)
            {
                if (isReloading) StopReload();
                nextTimeToFire = Time.time + 1f / fireRate;
                ShootShotgun();
            }
            // Şarjör boş ama cepte mermi varsa reload yap
            else if (!isReloading && currentReserveAmmo > 0)
            {
                StartReload();
            }
        }

        // 3. RELOAD BAŞLATMA
        // Şarjör tam dolu değilse VE cepte mermi varsa reload yapabiliriz
        if (Input.GetKeyDown(KeyCode.R) && currentMagazineAmmo < maxMagazineSize && currentReserveAmmo > 0 && !isReloading)
        {
            StartReload();
        }
    }

    // --- YENİ UI GÜNCELLEME ---
    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            // Ekranda "Şarjör / Cep" şeklinde yazar (Örn: 5 / 20)
            ammoText.text = currentMagazineAmmo + " / " + currentReserveAmmo;
        }
    }

    // --- YENİ MERMİ TOPLAMA FONKSİYONU ---
    public void AddAmmo(int amount)
    {
        // Mermiyi cebe ekle
        currentReserveAmmo += amount;
        
        // Cebi taşırma (Maksimum kapasite)
        if (currentReserveAmmo > maxReserveAmmo) currentReserveAmmo = maxReserveAmmo;

        Debug.Log("Mermi Alındı! Cepte: " + currentReserveAmmo);
        UpdateAmmoUI();
    }

    void StartReload()
    {
        if (!isReloading) reloadCoroutine = StartCoroutine(ReloadSequence());
    }

    void StopReload()
    {
        if (reloadCoroutine != null) StopCoroutine(reloadCoroutine);
        isReloading = false;
        if (weaponMovement != null) weaponMovement.SetReloading(false);
        if (crosshairScript != null) crosshairScript.HideReloadRing();
        UpdateAmmoUI();
    }

    IEnumerator ReloadSequence()
    {
        isReloading = true;
        if (weaponMovement != null) weaponMovement.SetReloading(true);

        // Döngü Şartı: Şarjör dolana kadar VE cepte mermi bitene kadar
        while (currentMagazineAmmo < maxMagazineSize && currentReserveAmmo > 0)
        {
            float timer = 0f;
            while (timer < shellInsertTime)
            {
                timer += Time.deltaTime;
                float progress = timer / shellInsertTime;
                if (crosshairScript != null) crosshairScript.UpdateReloadProgress(progress);
                yield return null;
            }

            // --- YENİ MANTIK: CEPTEN AL ŞARJÖRE KOY ---
            currentReserveAmmo--; // Cepten 1 düş
            currentMagazineAmmo++; // Şarjöre 1 ekle
            UpdateAmmoUI(); 

            if (shellInsertSound != null && audioSource != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(shellInsertSound, reloadVolume);
            }

            if (weaponMovement != null) weaponMovement.ReloadBump();
        }

        // Reload bitince pompa sesi
        PlayPumpSound();

        isReloading = false;
        if (weaponMovement != null) weaponMovement.SetReloading(false);
        if (crosshairScript != null) crosshairScript.HideReloadRing();
    }

    void ShootShotgun()
    {
        currentMagazineAmmo--; // Şarjörden düş
        UpdateAmmoUI(); 
        if (CameraShake.Instance != null) 
            CameraShake.Instance.Shake(0.08f, 0.08f);
        // ------------------------------------------

        if (shootSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(shootSound, shootVolume);
        }

        Invoke(nameof(PlayPumpSound), pumpDelay);

        if (weaponMovement != null) weaponMovement.RecoilFire();
        if (muzzleFlash != null) muzzleFlash.Play();

        // SPHERECAST (Kalın Mermi - Fixlenmiş Hali)
        Vector3 rayOrigin = fpsCamera.transform.position - (fpsCamera.transform.forward * 0.5f); 

        for (int i = 0; i < pellets; i++)
        {
            Vector3 deviation = Random.insideUnitCircle * spreadAngle;
            Quaternion rot = Quaternion.LookRotation(fpsCamera.transform.forward);
            Vector3 shotDirection = rot * Quaternion.Euler(deviation.x, deviation.y, 0) * Vector3.forward;

            RaycastHit hit;
            // 0.1f kalınlığında mermi
            if (Physics.SphereCast(rayOrigin, 0.1f, shotDirection, out hit, range, vurulabilirKatmanlar))
            {
                // Hasar kodları aynen
                Target target = hit.transform.GetComponent<Target>();
                if (target != null) target.TakeDamage(closeDamage); // Basitlik için yakın hasarını aldım

                SmartEnemy enemyAI = hit.transform.GetComponent<SmartEnemy>();
                if (enemyAI != null) enemyAI.AddKnockback(shotDirection, closeKnockback);

                if (impactEffect != null)
                {
                    GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactGO, 1f);
                }
            }

            if (bulletTrailPrefab != null)
            {
                GameObject trail = Instantiate(bulletTrailPrefab);
                LineRenderer lr = trail.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    lr.SetPosition(0, attackPoint.position); 
                    lr.SetPosition(1, hit.point != Vector3.zero ? hit.point : rayOrigin + shotDirection * range);
                }
            }
        }
    }

    void PlayPumpSound()
    {
        if (pumpSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(pumpSound, pumpVolume);
            if (weaponMovement != null) weaponMovement.ReloadBump(); 
        }
    }
}