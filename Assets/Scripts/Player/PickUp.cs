using UnityEngine;

// Kutu tiplerini belirliyoruz
public enum PickupType
{
    HealthPack, // Can Paketi
    AmmoBox     // Mermi Kutusu
}

public class Pickup : MonoBehaviour
{
    [Header("Kutu Ayarları")]
    public PickupType type;   // Inspector'dan seç: Can mı Mermi mi?
    public float amount = 25f;// Ne kadar versin?
    
    [Header("Animasyon Ayarları")]
    public float rotateSpeed = 50f; // Dönme Hızı
    public float bobSpeed = 2f;     // Yaylanma Hızı
    public float bobHeight = 0.3f;  // Yaylanma Yüksekliği

    [Header("Ses Efekti")]
    public AudioClip pickupSound; // Alınınca çıkacak ses
    [Range(0f, 1f)] public float soundVolume = 1f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // 1. DÖNME
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        // 2. SÜZÜLME (Yaylanma)
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bool wasPickedUp = false;

            // CAN ALMA
            if (type == PickupType.HealthPack)
            {
                PlayerHealth healthScript = other.GetComponent<PlayerHealth>();
                if (healthScript != null)
                {
                    healthScript.Heal(amount);
                    wasPickedUp = true;
                }
            }
            // MERMİ ALMA
            else if (type == PickupType.AmmoBox)
            {
                DoomGun gunScript = other.GetComponentInChildren<DoomGun>();
                if (gunScript != null)
                {
                    gunScript.AddAmmo((int)amount);
                    wasPickedUp = true;
                }
            }

            // ALINDI!
            if (wasPickedUp)
            {
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
                }
                Destroy(gameObject);
            }
        }
    }
}