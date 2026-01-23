using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Düşman Ayarları")]
    public GameObject enemyPrefab;   
    public int maxEnemies = 5;       
    public float spawnCheckTime = 2f;

    [Header("Doğuş (Spawn) Alanı - Yeşil Halka")]
    public float minSpawnDistance = 10f;  // En yakın (Burnunun dibinde doğmasın)
    public float maxSpawnDistance = 25f;  // En uzak (Çok uzağa doğmasın)

    [Header("Yok Olma (Despawn) Alanı - Kırmızı Çizgi")]
    public float despawnDistance = 40f;   // BU MESAFEYİ GEÇEN SİLİNİR!

    [Header("Referanslar")]
    public Transform player;
    public Camera playerCamera;

    void Start()
    {
        if (player == null) player = GameObject.Find("Player").transform;
        if (playerCamera == null) playerCamera = Camera.main;

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // 1. ÖNCE TEMİZLİK: Çok uzaktakileri sil ki yer açılsın
            RemoveDistantEnemies();

            // 2. SAYIM: Kaç düşman kaldı?
            int currentCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

            // 3. ÜRETİM: Eksik varsa tamamla
            if (currentCount < maxEnemies)
            {
                TrySpawnEnemy();
            }

            yield return new WaitForSeconds(spawnCheckTime);
        }
    }

    // --- YENİ EKLENEN FONKSİYON: UZAKLARI SİL ---
    void RemoveDistantEnemies()
    {
        // Sahnedeki tüm düşmanları bul
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            // Düşman ile oyuncu arasındaki mesafeyi ölç
            float distance = Vector3.Distance(player.position, enemy.transform.position);

            // Eğer mesafe sınırını aştıysa
            if (distance > despawnDistance)
            {
                Destroy(enemy); // Yok et
                // Not: Burası çalışınca "currentCount" azalacak ve yukarıdaki döngü 
                // otomatik olarak oyuncunun yakınına yenisini doğuracak.
            }
        }
    }

    void TrySpawnEnemy()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            // Spawn mesafesini kullanıyoruz
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance); 
            Vector3 randomPoint = player.position + new Vector3(randomDir.x, 0, randomDir.y) * distance;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas))
            {
                if (!IsVisible(hit.position))
                {
                    Instantiate(enemyPrefab, hit.position, Quaternion.identity);
                    return; 
                }
            }
        }
    }

    bool IsVisible(Vector3 position)
    {
        Vector3 viewportPoint = playerCamera.WorldToViewportPoint(position);
        bool onScreen = viewportPoint.x > 0 && viewportPoint.x < 1 && 
                        viewportPoint.y > 0 && viewportPoint.y < 1 &&
                        viewportPoint.z > 0;
        return onScreen;
    }
}