using UnityEngine;

public class QuestItem : MonoBehaviour
{
    [Header("Eşya Ayarları")]
    public string itemName; // Buraya Inspector'dan isim yaz (Örn: "Oyuncak Ayı")

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            
            if (inventory != null)
            {
                // Yeni fonksiyona göre güncellendi:
                inventory.AddQuestItem(itemName);
                
                // Efekt veya ses buraya eklenebilir
                Destroy(gameObject); // Eşyayı sahneden sil
            }
        }
    }
}