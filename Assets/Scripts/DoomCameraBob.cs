using UnityEngine;

public class DoomCameraBob : MonoBehaviour
{
    [Header("Sallanma Ayarları")]
    public float bobFrequency = 14f; // Sallanma Hızı (Adım sıklığı)
    public float bobHeight = 0.05f;  // Sallanma Miktarı (Ne kadar inip çıkacak)
    public bool smoothReturn = true; // Durunca yumuşakça merkeze dönsün mü?

    private float defaultPosY; // Kameranın başlangıç yüksekliği
    private float timer = 0;

    void Start()
    {
        // Başlangıçtaki Y pozisyonunu kaydet (Merkez noktası)
        defaultPosY = transform.localPosition.y;
    }

    void Update()
    {
        // Karakter hareket ediyor mu kontrol et (W,A,S,D'ye basılıyor mu?)
        float inputMagnitude = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude;

        if (inputMagnitude > 0.1f)
        {
            // Hareket halindeyiz: Sinüs dalgası ile zamanı ilerlet
            timer += Time.deltaTime * bobFrequency;

            // Sinüs dalgası -1 ile 1 arasında gider gelir, bunu yükseklikle çarp
            float newY = defaultPosY + Mathf.Sin(timer) * bobHeight;

            // Kameranın pozisyonunu güncelle (Sadece Y ekseni değişir)
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
        }
        else
        {
            // Durduk: Kamerayı orijinal yüksekliğine (defaultPosY) geri getir
            timer = 0; // Zamanlayıcıyı sıfırla ki tekrar başlayınca adım ortasından başlamasın
            
            if (smoothReturn)
            {
                // Lerp ile yumuşak geçiş
                float newY = Mathf.Lerp(transform.localPosition.y, defaultPosY, Time.deltaTime * bobFrequency);
                transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
            }
            else
            {
                // Direkt geçiş (Daha robotik)
                transform.localPosition = new Vector3(transform.localPosition.x, defaultPosY, transform.localPosition.z);
            }
        }
    }
}