using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance; // Her yerden ulaşmak için anahtar

    private Vector3 originalPos;
    private float shakeTimer;
    private float shakeAmount;
    private float shakeFadeTime;

    void Awake()
    {
        // Tek ve yetkili script olduğunu onayla
        Instance = this;
    }

    void Start()
    {
        // Başlangıç pozisyonunu kaydet (Sarsıntı bitince buraya dönecek)
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            // Rastgele bir noktaya titreşim ver (Sphere içinde rastgele nokta)
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

            // Süreyi azalt
            shakeTimer -= Time.deltaTime;

            // Sarsıntı gücünü yavaşça sönümle (Fade Out)
            if (shakeFadeTime > 0)
            {
                shakeAmount = Mathf.Lerp(shakeAmount, 0f, Time.deltaTime / shakeFadeTime);
            }
        }
        else
        {
            // Süre bitti, kamerayı orijinal yerine sabitle
            shakeTimer = 0f;
            transform.localPosition = originalPos;
        }
    }

    // --- BU FONKSİYONU ÇAĞIRACAĞIZ ---
    public void Shake(float duration, float amount)
    {
        shakeTimer = duration;  // Ne kadar sürsün?
        shakeAmount = amount;   // Ne kadar şiddetli olsun?
        shakeFadeTime = duration / 2f; // Sonlara doğru yavaşlasın
    }
}