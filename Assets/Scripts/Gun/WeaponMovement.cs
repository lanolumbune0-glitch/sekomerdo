using UnityEngine;

public class WeaponMovement : MonoBehaviour
{
    [Header("Nişan Alma (ADS) Ayarları")]
    public Vector3 aimPosition;      // Nişan alırken silah nerede dursun?
    public float adsSpeed = 8f;      // Nişan alma hızı
    public Camera mainCamera;        // Zoom için kamera
    public float defaultFov = 60f;   // Normal görüş açısı
    public float adsFov = 40f;       // Nişan alınca görüş açısı
    private bool isAiming = false;

    [Header("Sallanma (Sway)")]
    public float swayAmount = 0.02f;
    public float maxSwayAmount = 0.06f;
    public float swaySmooth = 4f;

    [Header("Yürüme Sallantısı (Bobbing) - YENİ")]
    public float bobSpeed = 14f;       // Adım atma hızı
    public float bobAmount = 0.05f;    // Sallanma miktarı
    private float bobTimer = 0;        // Zamanlayıcı

    [Header("Yatış (Tilt) - YENİ")]
    public float tiltAmount = 2f;      // A-D basınca kaç derece yatsın?
    public float tiltSmooth = 4f;      // Yatış hızı

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
    private Vector3 initialPosition; 
    private Quaternion initialRotation;
    
    private Vector3 currentRecoilPos;
    private Vector3 targetRecoilPos;
    private Vector3 currentRecoilRot;
    private Vector3 targetRecoilRot;

    void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null) defaultFov = mainCamera.fieldOfView;
    }

    void Update()
    {
        HandleInput();  
        HandleRecoil(); // Tepme hesaplamaları
        
        // Tüm hareketleri (Sway + Bob + Tilt + Recoil + ADS) burada birleştiriyoruz
        HandlePositionAndRotation(); 
    }

    void HandleInput()
    {
        if (Input.GetButton("Fire2"))
            isAiming = true;
        else
            isAiming = false;
    }

    void HandleRecoil()
    {
        targetRecoilPos = Vector3.Lerp(targetRecoilPos, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRecoilPos = Vector3.Lerp(currentRecoilPos, targetRecoilPos, snapiness * Time.deltaTime);

        targetRecoilRot = Vector3.Lerp(targetRecoilRot, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRecoilRot = Vector3.Lerp(currentRecoilRot, targetRecoilRot, snapiness * Time.deltaTime);
    }

    // --- YENİ EKLENEN: YÜRÜME SALLANTISI HESABI ---
    Vector3 CalculateBobbing()
    {
        Vector3 bobPos = Vector3.zero;
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Hareket yoksa zamanlayıcıyı sıfırla
        if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0)
        {
            bobTimer = 0.0f;
        }
        else
        {
            // Sinüs dalgası oluştur
            float waveSlice = Mathf.Sin(bobTimer);
            bobTimer += bobSpeed * Time.deltaTime;
            if (bobTimer > Mathf.PI * 2) bobTimer -= (Mathf.PI * 2);

            if (waveSlice != 0)
            {
                float translateChange = waveSlice * bobAmount;
                float totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
                totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);

                // Nişan alıyorsak sallantıyı %90 azalt (Daha rahat nişan almak için)
                float damper = isAiming ? 0.1f : 1f; 

                bobPos.y = translateChange * totalAxes * damper;
                bobPos.x = translateChange * totalAxes * damper * 0.5f; // X ekseni daha az oynasın
            }
        }
        return bobPos;
    }

    // --- YENİ EKLENEN: YATIŞ HESABI ---
    Quaternion CalculateTilt()
    {
        float moveX = Input.GetAxis("Horizontal");
        // Nişan alıyorsan yatış yapma (veya az yap)
        float currentTilt = isAiming ? tiltAmount * 0.2f : tiltAmount;
        
        // Z ekseninde döndür
        return Quaternion.Euler(0, 0, -moveX * currentTilt);
    }

    void HandlePositionAndRotation()
    {
        // 1. TEMEL HEDEF BELİRLEME (ADS mi, Kalça mı, Reload mu?)
        Vector3 targetPos = initialPosition;
        Quaternion targetRot = initialRotation;
        float targetFov = defaultFov;

        if (isReloading)
        {
            targetPos += reloadPosOffset;
            targetRot *= Quaternion.Euler(reloadRotOffset);
        }
        else if (isAiming)
        {
            targetPos = aimPosition; 
            targetFov = adsFov;
        }

        // 2. SWAY (MOUSE GECİKMESİ)
        float currentSwayAmount = isAiming ? swayAmount * 0.1f : swayAmount;
        float swayX = -Input.GetAxis("Mouse X") * currentSwayAmount;
        float swayY = -Input.GetAxis("Mouse Y") * currentSwayAmount;
        swayX = Mathf.Clamp(swayX, -maxSwayAmount, maxSwayAmount);
        swayY = Mathf.Clamp(swayY, -maxSwayAmount, maxSwayAmount);
        Vector3 finalSwayPos = new Vector3(swayX, swayY, 0);

        // 3. BOBBING (YÜRÜME) VE TILT (YATIŞ)
        Vector3 bobbingPos = CalculateBobbing();
        Quaternion tiltRot = CalculateTilt();

        // 4. HEPSİNİ BİRLEŞTİR VE UYGULA
        // Pozisyon: Hedef + Tepme + Sway + Yürüme
        transform.localPosition = Vector3.Lerp(transform.localPosition, 
            targetPos + currentRecoilPos + finalSwayPos + bobbingPos, 
            Time.deltaTime * adsSpeed);
        
        // Rotasyon: Hedef + Tepme + Yatış
        transform.localRotation = Quaternion.Slerp(transform.localRotation, 
            targetRot * Quaternion.Euler(currentRecoilRot) * tiltRot, 
            Time.deltaTime * adsSpeed);

        // 5. KAMERA ZOOM
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * adsSpeed);
        }
    }

    public void RecoilFire()
    {
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