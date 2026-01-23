using UnityEngine;

public class BulletTrailEffect : MonoBehaviour
{
    private LineRenderer lineRenderer;
    
    [Header("Ayarlar")]
    public float fadeSpeed = 2f;   // SİLİNME HIZI: Ne kadar düşükse o kadar yavaş silinir. (Örn: 2)
    public float hangTime = 0.2f;  // ASILI KALMA: Silinmeye başlamadan önce havada kaç saniye beklesin?
    
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        // Kendini 3 saniye sonra her türlü yok et (Garanti olsun, süre uzadığı için artırdık)
        Destroy(gameObject, 3f); 
    }

    void Update()
    {
        if (lineRenderer != null)
        {
            // Önce bekleme süresinden düş (Geri sayım)
            hangTime -= Time.deltaTime;

            // Eğer bekleme süresi bittiyse (sıfırın altına indiyse) silinmeye başla
            if (hangTime <= 0)
            {
                Color startColor = lineRenderer.startColor;
                Color endColor = lineRenderer.endColor;

                // Alphasını yavaşça azalt
                float fadeAmount = Time.deltaTime * fadeSpeed;
                startColor.a -= fadeAmount;
                endColor.a -= fadeAmount;

                lineRenderer.startColor = startColor;
                lineRenderer.endColor = endColor;

                // Tamamen görünmez olduysa objeyi sil
                if (startColor.a <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}