using UnityEngine;

public class DoomGun : MonoBehaviour
{
    [Header("Silah Özellikleri")]
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 5f;

    [Header("ÖNEMLİ: Kimleri Vurabilirim?")]
    // Burası inspector'da açılır menü olacak
    public LayerMask vurulabilirKatmanlar; 
    
    [Header("Referanslar")]
    public Camera fpsCamera;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;

    private float nextTimeToFire = 0f;

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        if (muzzleFlash != null) muzzleFlash.Play();

        RaycastHit hit;

        // Kod kısmını basitleştirdik. Sadece senin menüden seçtiklerine çarpar.
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range, vurulabilirKatmanlar))
        {
            Debug.Log(hit.transform.name + " vuruldu!");

            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            if (impactEffect != null)
            {
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }
        }
        float noiseRadius = 30f; // Sesin duyulma mesafesi
    Collider[] enemies = Physics.OverlapSphere(transform.position, noiseRadius);
    
    foreach (Collider col in enemies)
    {
        // Eğer menzildeki objede SmartEnemy scripti varsa "HearSound" fonksiyonunu çalıştır
        SmartEnemy enemy = col.GetComponent<SmartEnemy>();
        if (enemy != null)
        {
            enemy.HearSound(transform.position);
        }
    }
    }
}