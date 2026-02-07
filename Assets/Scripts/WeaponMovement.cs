using UnityEngine;

public class WeaponMovement : MonoBehaviour
{
    [Header("Sallanma (Sway) Ayarları")]
    public float swayAmount = 0.02f;    // Sağa sola ne kadar kaysın?
    public float maxSwayAmount = 0.06f; // Maksimum kayma sınırı
    public float swaySmooth = 4f;       // Ne kadar yumuşak gelsin?

    [Header("Tepme (Recoil) Ayarları")]
    public float recoilX = -10f;     // Yukarı tepme açısı (Rotasyon)
    public float recoilY = 5f;       // Sağa sola rastgele tepme (Rotasyon)
    public float recoilZ = -0.2f;    // Geriye tepme mesafesi (Pozisyon) - Omuza vuruş
    
    public float snapiness = 6f;     // Tepme hızı (Ne kadar sert?)
    public float returnSpeed = 2f;   // Yerine dönme hızı

    // Private Değişkenler
    private Vector3 initialPosition;     // Başlangıç pozisyonu
    private Quaternion initialRotation;  // Başlangıç açısı
    
    private Vector3 currentRecoilPos;    // Anlık tepme pozisyonu
    private Vector3 targetRecoilPos;     // Hedef tepme pozisyonu
    
    private Vector3 currentRecoilRot;    // Anlık tepme açısı
    private Vector3 targetRecoilRot;     // Hedef tepme açısı

    void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        HandleSway();
        HandleRecoil();
    }

    // --- 1. SALLANMA (SWAY) ---
    // Fareyi çevirdiğinde silahın peşinden gelmesi (Ağırlık hissi)
    void HandleSway()
    {
        float movementX = -Input.GetAxis("Mouse X") * swayAmount;
        float movementY = -Input.GetAxis("Mouse Y") * swayAmount;

        // Sınırla (Çok fazla kaymasın)
        movementX = Mathf.Clamp(movementX, -maxSwayAmount, maxSwayAmount);
        movementY = Mathf.Clamp(movementY, -maxSwayAmount, maxSwayAmount);

        Vector3 finalPosition = new Vector3(movementX, movementY, 0);

        // Silahı o yöne doğru yumuşakça kaydır
        // NOT: Recoil etkisiyle karışmaması için burada sadece Sway hesaplıyoruz, 
        // asıl atama Recoil fonksiyonunda birleşerek yapılacak.
        // Ama basitlik için Sway'i direkt local position'a etkileyebiliriz, 
        // fakat en temizi Recoil ile çakışmamasıdır. 
        // Şimdilik Sway'i doğrudan uygulayalım, Recoil üzerine eklenecek.
        
        transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition + finalPosition + currentRecoilPos, Time.deltaTime * swaySmooth);
    }

    // --- 2. TEPME (RECOIL) MANTIĞI ---
    void HandleRecoil()
    {
        // Hedef tepme pozisyonundan sıfıra (orijinale) doğru yavaşça dön
        targetRecoilPos = Vector3.Lerp(targetRecoilPos, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRecoilPos = Vector3.Lerp(currentRecoilPos, targetRecoilPos, snapiness * Time.deltaTime);

        // Hedef rotasyondan sıfıra dön
        targetRecoilRot = Vector3.Lerp(targetRecoilRot, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRecoilRot = Vector3.Lerp(currentRecoilRot, targetRecoilRot, snapiness * Time.deltaTime);

        // Rotasyonu uygula (Sway rotasyonu etkilemez, sadece pozisyonu etkiler)
        transform.localRotation = Quaternion.Euler(currentRecoilRot) * initialRotation;
    }

    // --- BU FONKSİYONU SİLAH SCRİPTİ ÇAĞIRACAK ---
    public void RecoilFire()
    {
        // Rastgele sağa/sola tepme
        float randomY = Random.Range(-recoilY, recoilY);

        // Hedef pozisyonu geriye (Z ekseni) it
        targetRecoilPos += new Vector3(0, 0, recoilZ);

        // Hedef rotasyonu yukarı (X ekseni) kaldır
        targetRecoilRot += new Vector3(recoilX, randomY, 0);
    }
}