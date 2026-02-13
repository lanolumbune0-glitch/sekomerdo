using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public int totalGraves = 3;      // Toplam mezar sayısı
    private int completedGraves = 0; // Şu an kaçı bitti?

    [Header("Ödül")]
    public GameObject rewardKey;     // 3 görev bitince açılacak olan anahtar objesi

    public void CheckAllGraves()
    {
        completedGraves++;
        Debug.Log("Tamamlanan Mezar: " + completedGraves + "/" + totalGraves);

        // Hepsi bitti mi?
        if (completedGraves >= totalGraves)
        {
            SpawnKey();
        }
    }

    void SpawnKey()
    {
        Debug.Log("TÜM MEZARLAR TAMAM! ÖDÜL ANAHTARI ORTAYA ÇIKTI!");
        if (rewardKey != null)
        {
            rewardKey.SetActive(true);
            // Burada "Görev Bitti" sesi çalabilirsin
        }
    }
}