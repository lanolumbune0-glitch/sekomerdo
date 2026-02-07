using UnityEngine;

public class WeaponMovement : MonoBehaviour
{
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

    [Header("Reload Animasyonu (YENİ)")]
    public Vector3 reloadPosOffset = new Vector3(0, -0.2f, 0); // Reload yaparken silah ne kadar aşağı insin?
    public Vector3 reloadRotOffset = new Vector3(30f, 0, -10f); // Reload yaparken silah nasıl dönsün? (Eğilme)
    public float reloadSmooth = 3f; // Pozisyona geçiş hızı
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
    }

    void Update()
    {
        HandleSway();
        HandleRecoil();
        HandleReloadState(); // YENİ: Reload pozisyonunu kontrol et
    }

    void HandleSway()
    {
        float movementX = -Input.GetAxis("Mouse X") * swayAmount;
        float movementY = -Input.GetAxis("Mouse Y") * swayAmount;
        movementX = Mathf.Clamp(movementX, -maxSwayAmount, maxSwayAmount);
        movementY = Mathf.Clamp(movementY, -maxSwayAmount, maxSwayAmount);

        Vector3 finalPosition = new Vector3(movementX, movementY, 0);

        // Sway hareketini mevcut pozisyona ekle (Reload pozisyonu hesaplandıktan sonra üzerine biner)
    }

    void HandleRecoil()
    {
        targetRecoilPos = Vector3.Lerp(targetRecoilPos, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRecoilPos = Vector3.Lerp(currentRecoilPos, targetRecoilPos, snapiness * Time.deltaTime);

        targetRecoilRot = Vector3.Lerp(targetRecoilRot, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRecoilRot = Vector3.Lerp(currentRecoilRot, targetRecoilRot, snapiness * Time.deltaTime);
    }

    // --- YENİ: RELOAD POZİSYONLAMA ---
    void HandleReloadState()
    {
        Vector3 targetPos = initialPosition;
        Quaternion targetRot = initialRotation;

        // Eğer reload yapıyorsak hedefimiz değişir
        if (isReloading)
        {
            targetPos += reloadPosOffset;
            targetRot *= Quaternion.Euler(reloadRotOffset);
        }

        // Sway + Recoil + Reload Pozisyonunu Birleştir
        // Pozisyonu yumuşakça değiştir
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos + currentRecoilPos, Time.deltaTime * reloadSmooth);
        
        // Rotasyonu yumuşakça değiştir
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot * Quaternion.Euler(currentRecoilRot), Time.deltaTime * reloadSmooth);
    }

    public void RecoilFire()
    {
        float randomY = Random.Range(-recoilY, recoilY);
        targetRecoilPos += new Vector3(0, 0, recoilZ);
        targetRecoilRot += new Vector3(recoilX, randomY, 0);
    }

    // --- YENİ FONKSİYONLAR (DoomGun Çağıracak) ---
    public void SetReloading(bool state)
    {
        isReloading = state;
    }

    public void ReloadBump()
    {
        // Mermi girince silah hafifçe zıplasın (Recoil'in minik versiyonu)
        targetRecoilRot += new Vector3(-5f, 0, 0); // Hafif yukarı kalkma
        targetRecoilPos += new Vector3(0, 0, -0.05f); // Hafif geri gelme
    }
}