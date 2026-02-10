using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public AudioClip pickupSound; // Ses dosyası

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                // 1. Anahtarı envantere ekle
                inventory.AddKey();
                
                // 2. Ses var mı kontrol et (HATA ÇÖZÜCÜ KISIM)
                if (pickupSound != null) 
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                // 3. Yerden sil
                Destroy(gameObject); 
            }
        }
    }
}