using UnityEngine;
using UnityEngine.UI; // UI Yazısı için

public class WeaponPickupEvent : MonoBehaviour
{
    [Header("Bağlantılar")]
    public GameObject realWeapon;      // Oyuncunun elindeki (gizli) silah
    public GameObject enemySpawner;    // Düşman spawner objesi
    public Text infoText;              // Ekranda çıkacak yazı (Örn: "Hayatta Kal!")

    [Header("Ayarlar")]
    public string startMessage = "SİLAH BULUNDU! DÜŞMANLAR GELİYOR!";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Oyuncunun elindeki gerçek silahı aç
            realWeapon.SetActive(true);

            // 2. Düşman spawner'ı çalıştır
            if (enemySpawner != null) enemySpawner.SetActive(true);

            // 3. Ekrana gaza getirici yazıyı yaz
            if (infoText != null)
            {
                infoText.text = startMessage;
                infoText.gameObject.SetActive(true);
                Destroy(infoText.gameObject, 5f); // 5 saniye sonra yazıyı sil
            }

            // 4. Bu yerdeki silahı yok et
            Destroy(gameObject);
        }
    }
}