using UnityEngine;
using System.Collections;

public class DoomGun : MonoBehaviour
{
    [Header("Ses Efektleri & Mikser")]
    public AudioSource audioSource;
    public AudioClip shootSound;
    [Range(0f, 1f)] public float shootVolume = 1f; 
    public AudioClip shellInsertSound;
    [Range(0f, 1f)] public float reloadVolume = 0.8f; 

    [Header("Pompalı Şarjör Sistemi")]
    public int maxAmmo = 8;
    public float shellInsertTime = 0.5f;
    private int currentAmmo;         
    private bool isReloading = false;
    private Coroutine reloadCoroutine;

    [Header("Animasyon & Fizik")]
    public WeaponMovement weaponMovement; // İSİM DEĞİŞİKLİĞİ: Daha genel bir isim verdik

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
    }

    void OnEnable()
    {
        isReloading = false;
        // Eğer silah değiştirilirse animasyon takılı kalmasın
        if (weaponMovement != null) weaponMovement.SetReloading(false);
    }

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            if (currentAmmo > 0)
            {
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

    void StartReload()
    {
        if (!isReloading) reloadCoroutine = StartCoroutine(ReloadSequence());
    }

    void StopReload()
    {
        if (reloadCoroutine != null) StopCoroutine(reloadCoroutine);
        isReloading = false;
        
        // ANİMASYONU DURDUR
        if (weaponMovement != null) weaponMovement.SetReloading(false);
    }

    IEnumerator ReloadSequence()
    {
        isReloading = true;
        
        // 1. ANİMASYONU BAŞLAT (Silahı aşağı indir)
        if (weaponMovement != null) weaponMovement.SetReloading(true);

        while (currentAmmo < maxAmmo)
        {
            yield return new WaitForSeconds(shellInsertTime);
            currentAmmo++;

            // SES
            if (shellInsertSound != null && audioSource != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(shellInsertSound, reloadVolume);
            }

            // 2. MERMİ GİRME EFEKTİ (Silahı hafif zıplat)
            if (weaponMovement != null) weaponMovement.ReloadBump();
        }

        isReloading = false;
        
        // 3. ANİMASYONU BİTİR (Silahı kaldır)
        if (weaponMovement != null) weaponMovement.SetReloading(false);
    }

    void ShootShotgun()
    {
        currentAmmo--;

        if (shootSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(shootSound, shootVolume);
        }

        // TEPME EFEKTİ
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
}