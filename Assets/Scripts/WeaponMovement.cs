using UnityEngine;

public class WeaponMovement : MonoBehaviour
{
    [Header("Nişan Alma (ADS) Ayarları - YENİ")]
    public Vector3 aimPosition;      // Nişan alırken silah nerede dursun?
    public float adsSpeed = 8f;      // Nişan alma hızı
    public Camera mainCamera;        // Zoom için kamera
    public float defaultFov = 60f;   // Normal görüş açısı
    public float adsFov = 40f;       // Nişan alınca görüş açısı (Daha düşük = Daha zoom)
    private bool isAiming = false;

    [Header("Sallanma (Sway)")]
    public float swayAmount = 0.02f;
    public float maxSwayAmount = 0.06f;
    public float swaySmooth = 4f;

    [Header("Tepme (Recoil)")]
    public float recoilX = -10f;     
    public float recoilY = 5f;       
    public float recoilZ = -0.2f;    
    public float snapiness = 6f;     
    public float returnSpeed = 2f;   

    [Header("Reload Animasyonu")]
    public Vector3 reloadPosOffset = new Vector3(0, -0.2f, 0); 
    public Vector3 reloadRotOffset = new Vector3(30f, 0, -10f); 
    public float reloadSmooth = 3f; 
    private bool isReloading = false;

    // Private Değişkenler
    private Vector3 initialPosition; // Kalça hizası (Hip Fire)
    private Quaternion initialRotation;
    
    private Vector3 currentRecoilPos;
    private Vector3 targetRecoilPos;
    private Vector3 currentRecoilRot;
    private Vector3 targetRecoilRot;

    void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        
        // Eğer kamera atanmamışsa otomatik bul
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null) defaultFov = mainCamera.fieldOfView;
    }

    void Update()
    {
        HandleInput();  // Sağ tık kontrolü
        HandleSway();
        HandleRecoil();
        HandlePositionAndRotation(); // Tüm hareketleri birleştir
    }

    void HandleInput()
    {
        // ESKİSİ: if (Input.GetButton("Fire2") && !isReloading)
        // YENİSİ: Şartı kaldırdık. Sağ tık varsa nişan moduna geçmeye çalış.
        if (Input.GetButton("Fire2"))
        {
            isAiming = true;
        }
        else
        {
            isAiming = false;
        }
    }

    void HandleSway()
    {
        // Nişan alırken sallanma daha az olsun (Daha stabil atış)
        float currentSwayAmount = isAiming ? swayAmount * 0.2f : swayAmount;
        float currentMaxSway = isAiming ? maxSwayAmount * 0.2f : maxSwayAmount;

        float movementX = -Input.GetAxis("Mouse X") * currentSwayAmount;
        float movementY = -Input.GetAxis("Mouse Y") * currentSwayAmount;
        movementX = Mathf.Clamp(movementX, -currentMaxSway, currentMaxSway);
        movementY = Mathf.Clamp(movementY, -currentMaxSway, currentMaxSway);

        // Sway hesabı burada yapılır ama uygulaması HandlePositionAndRotation'da olur
        // Basitlik için burada direkt localPosition'a eklemiyoruz, aşağıda target ile birleştireceğiz.
    }

    void HandleRecoil()
    {
        targetRecoilPos = Vector3.Lerp(targetRecoilPos, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRecoilPos = Vector3.Lerp(currentRecoilPos, targetRecoilPos, snapiness * Time.deltaTime);

        targetRecoilRot = Vector3.Lerp(targetRecoilRot, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRecoilRot = Vector3.Lerp(currentRecoilRot, targetRecoilRot, snapiness * Time.deltaTime);
    }

    // --- TÜM POZİSYONLARI YÖNETEN FONKSİYON ---
    void HandlePositionAndRotation()
    {
        Vector3 targetPos = initialPosition;
        Quaternion targetRot = initialRotation;
        float targetFov = defaultFov;

        // 1. Durum Kontrolü (Reload > Aim > Normal)
        if (isReloading)
        {
            // Reload pozisyonu
            targetPos += reloadPosOffset;
            targetRot *= Quaternion.Euler(reloadRotOffset);
        }
        else if (isAiming)
        {
            // Nişan alma pozisyonu
            targetPos = aimPosition; 
            targetFov = adsFov;
        }

        // 2. Sway (Mouse Sallantısı) Hesabı
        float currentSwayAmount = isAiming ? swayAmount * 0.1f : swayAmount; // Nişandayken az salla
        float moveX = -Input.GetAxis("Mouse X") * currentSwayAmount;
        float moveY = -Input.GetAxis("Mouse Y") * currentSwayAmount;
        Vector3 finalSwayPos = new Vector3(moveX, moveY, 0);

        // 3. Pozisyonları Birleştir ve Uygula (Smooth)
        // Hedef = (Baz Pozisyon) + (Recoil) + (Sway)
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos + currentRecoilPos + finalSwayPos, Time.deltaTime * adsSpeed);
        
        // Rotasyonu Uygula
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot * Quaternion.Euler(currentRecoilRot), Time.deltaTime * adsSpeed);

        // 4. Kamera Zoom (FOV)
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * adsSpeed);
        }
    }

    public void RecoilFire()
    {
        // Nişan alırken tepme biraz daha az olsun mu? (Tercih meselesi, şimdilik aynı)
        float randomY = Random.Range(-recoilY, recoilY);
        targetRecoilPos += new Vector3(0, 0, recoilZ);
        targetRecoilRot += new Vector3(recoilX, randomY, 0);
    }

    public void SetReloading(bool state)
    {
        isReloading = state;
    }

    public void ReloadBump()
    {
        targetRecoilRot += new Vector3(-5f, 0, 0); 
        targetRecoilPos += new Vector3(0, 0, -0.05f); 
    }
}