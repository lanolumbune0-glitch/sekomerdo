using UnityEngine;
using UnityEngine.UI; // UI işlemleri için şart

public class GraveQuest : MonoBehaviour
{
    [Header("Görev Ayarları")]
    public string requiredItemName;   // Envanterdeki tam isim (Örn: "Ayicik")
    
    [Header("Atmosfer & Hikaye")]
    [TextArea] public string ghostMessage; // Örn: "Sarılacak arkadaşımı kaybettim..."
    public string thankYouMessage = "Teşekkür ederim...";
    
    [Header("Görsel Bağlantılar")]
    public GameObject soulParticle;    // Mezarın üzerindeki ışık (Başta Kırmızı)
    public GameObject completionEffect; // Bitince çıkacak efekt (Mavi patlama vb.)
    public GameObject infoCanvas;      // Mezarın üstündeki yazı paneli
    public Text infoText;              // Yazının kendisi

    [Header("Yönetici")]
    public QuestManager manager;

    private bool isCompleted = false;
    private bool playerIsClose = false;

    void Start()
    {
        // Başlangıçta yazıyı gizle, ruh ışığını aç
        if (infoCanvas != null) infoCanvas.SetActive(false);
        if (soulParticle != null) soulParticle.SetActive(true);
        
        // Işığı kırmızı yap (Huzursuzluk rengi)
        if (soulParticle != null) 
        {
            var main = soulParticle.GetComponent<ParticleSystem>().main;
            main.startColor = Color.red;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. ADIM: Fizik çalışıyor mu?
        Debug.Log("BİRİSİ MEZARA GİRDİ: " + other.name);

        if (isCompleted) return;

        // 2. ADIM: Giren kişi Oyuncu mu?
        if (!other.CompareTag("Player"))
        {
            Debug.LogWarning("Giren kişi Player değil! Girenin Tag'i: " + other.tag);
            return;
        }

        Debug.Log("Evet, giren kişi Player!");

        playerIsClose = true;
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();

        // 3. ADIM: Canvas var mı?
        if (infoCanvas != null)
        {
            Debug.Log("Canvas açılıyor...");
            infoCanvas.SetActive(true);
            
            // ... (Kodun geri kalanı aynı) ...
        }
        else
        {
            Debug.LogError("HATA: Info Canvas bu mezara atanmamış! Inspector'dan sürükle!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = false;
            if (infoCanvas != null) infoCanvas.SetActive(false); // Yazıyı kapat
        }
    }

    private void Update()
    {
        // Oyuncu yakınsa, görev bitmediyse ve 'E' tuşuna bastıysa
        if (playerIsClose && !isCompleted && Input.GetKeyDown(KeyCode.E))
        {
            TryCompleteQuest();
        }
    }

    void TryCompleteQuest()
    {
        // Oyuncuyu bul (Performans için Cache yapılabilir ama şimdilik Find güvenli)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        PlayerInventory inventory = player.GetComponent<PlayerInventory>();

        if (inventory != null && inventory.HasQuestItem(requiredItemName))
        {
            // --- GÖREV BAŞARILI ---
            inventory.RemoveQuestItem(requiredItemName);
            
            isCompleted = true;
            
            // 1. Yazıyı güncelle
            if (infoText != null)
            {
                infoText.text = thankYouMessage;
                infoText.color = Color.cyan;
            }

            // 2. Görsel Efektler
            if (soulParticle != null) Destroy(soulParticle); // Kırmızı ışığı yok et
            if (completionEffect != null) Instantiate(completionEffect, transform.position, Quaternion.identity); // Mavi ışık patlat

            // 3. UI'ı 2 saniye sonra tamamen kapat
            Invoke("HideCanvas", 2f);

            // 4. Yöneticiye haber ver
            if (manager != null) manager.CheckAllGraves();
        }
        else
        {
            // Eşya yoksa ses çalınabilir (Hata sesi)
            Debug.Log("Bu eşyaya sahip değilsin!");
        }
    }

    void HideCanvas()
    {
        if (infoCanvas != null) infoCanvas.SetActive(false);
    }
    
}