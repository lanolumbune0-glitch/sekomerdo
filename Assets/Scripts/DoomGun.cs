using UnityEngine;
using UnityEngine.UI; 
using System.Collections;

public class DoomGun : MonoBehaviour
{
    [Header("UI Bağlantıları")]
    public Text ammoText;        
    public CrosshairHUD crosshairScript; 

    [Header("Ses Efektleri & Mikser")]
    public AudioSource audioSource;
    
    [Header("Ateş & Reload Sesleri")]
    public AudioClip shootSound;
    [Range(0f, 1f)] public float shootVolume = 1f; 
    
    public AudioClip shellInsertSound; // Mermi tek tek girme sesi
    [Range(0f, 1f)] public float reloadVolume = 0.8f; 

    [Header("Pompa (Kurma) Sesi - YENİ")]
    public AudioClip pumpSound;      // "Şak-Şak" sesi (Pump Action)
    [Range(0f, 1f)] public float pumpVolume = 1f;
    public float pumpDelay = 0.5f;   // Ateş ettikten kaç sn sonra sesi çalsın?

    [Header("Pompalı Şarjör Sistemi")]
    public int maxAmmo = 8;
    public float shellInsertTime = 0.5f;
    private int currentAmmo;         
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
        currentAmmo = maxAmmo;
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
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            if (currentAmmo > 0)
            {
                // Eğer reload yaparken ateş edersek durdur
                if (isReloading) StopReload();
                
                nextTimeToFire = Time.time + 1f / fireRate;
                ShootShotgun();
            }
            else if (!isReloading)
            {
                StartReload();
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo && !isReloading)
        {
            StartReload();
        }
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + maxAmmo;
        }
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
        
        // Reload yarıda kesilirse pompa sesini iptal et (İsteğe bağlı)
        // CancelInvoke(nameof(PlayPumpSound)); 
    }

    IEnumerator ReloadSequence()
    {
        isReloading = true;
        if (weaponMovement != null) weaponMovement.SetReloading(true);

        while (currentAmmo < maxAmmo)
        {
            float timer = 0f;
            while (timer < shellInsertTime)
            {
                timer += Time.deltaTime;
                float progress = timer / shellInsertTime;
                if (crosshairScript != null) crosshairScript.UpdateReloadProgress(progress);
                yield return null;
            }

            currentAmmo++;
            UpdateAmmoUI(); 

            if (shellInsertSound != null && audioSource != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(shellInsertSound, reloadVolume);
            }

            if (weaponMovement != null) weaponMovement.ReloadBump();
        }

        // --- YENİ KISIM: RELOAD TAMAMLANIRSA POMPA SESİ ---
        // Buraya geldiysek reload hiç kesilmemiş demektir.
        PlayPumpSound();
        // --------------------------------------------------

        isReloading = false;
        if (weaponMovement != null) weaponMovement.SetReloading(false);
        if (crosshairScript != null) crosshairScript.HideReloadRing();
    }

    void ShootShotgun()
    {
        currentAmmo--;
        UpdateAmmoUI(); 

        // Ateş Sesi
        if (shootSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(shootSound, shootVolume);
        }

        // --- YENİ KISIM: ATEŞ SONRASI POMPA SESİ ---
        // Belirlenen süre (pumpDelay) kadar bekle ve sesi çal
        Invoke(nameof(PlayPumpSound), pumpDelay);
        // -------------------------------------------

        if (weaponMovement != null) weaponMovement.RecoilFire();
        if (muzzleFlash != null) muzzleFlash.Play();

        Vector3 eyePosition = fpsCamera.transform.position;
        Vector3 visualStartPoint = (attackPoint != null) ? attackPoint.position : eyePosition;

        for (int i = 0; i < pellets; i++)
        {
            Vector3 deviation = Random.insideUnitCircle * spreadAngle;
            Quaternion rot = Quaternion.LookRotation(fpsCamera.transform.forward);
            Vector3 shotDirection = rot * Quaternion.Euler(deviation.x, deviation.y, 0) * Vector3.forward;
            Vector3 endPoint = eyePosition + (shotDirection * range);

            RaycastHit hit;
            if (Physics.Raycast(eyePosition, shotDirection, out hit, range, vurulabilirKatmanlar))
            {
                endPoint = hit.point;
                float distanceRatio = hit.distance / range;
                float currentDamage = Mathf.Lerp(closeDamage, farDamage, distanceRatio);
                float currentForce = Mathf.Lerp(closeKnockback, farKnockback, distanceRatio);

                Target target = hit.transform.GetComponent<Target>();
                if (target != null) target.TakeDamage(currentDamage);

                SmartEnemy enemyAI = hit.transform.GetComponent<SmartEnemy>();
                if (enemyAI != null) enemyAI.AddKnockback(shotDirection, currentForce);

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
                    lr.SetPosition(0, visualStartPoint); 
                    lr.SetPosition(1, endPoint);
                }
            }
        }
    }

    // --- YENİ FONKSİYON: POMPA SESİNİ ÇAL ---
    void PlayPumpSound()
    {
        if (pumpSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f); // Robotik olmasın diye hafif ton değişimi
            audioSource.PlayOneShot(pumpSound, pumpVolume);
            
            // İstersen burada silahı tekrar hafifçe zıplatabilirsin:
            if (weaponMovement != null) weaponMovement.ReloadBump(); 
        }
    }
}