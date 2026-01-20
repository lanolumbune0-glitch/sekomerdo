using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class DoomMovement : MonoBehaviour
{
    [Header("Ayarlar")]
    public float speed = 15f;
    public float gravity = 30f;
    public float mouseSensitivity = 2f;

    private CharacterController controller;
    private Transform playerCamera; // Bunu private yaptık, kendi bulacak
    private float xRotation = 0f;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // 1. OTOMATİK KAMERA BULMA
        // Eğer sahnede "MainCamera" etiketli bir kamera varsa onu bulur.
        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
        else
        {
            // Bulamazsa hata verir ama oyunu çökertmemek için uyarı yazar
            Debug.LogError("Sahnede 'MainCamera' etiketli bir kamera bulunamadı! Kamerana 'MainCamera' tag'i ver.");
        }

        // Mouse'u gizle ve kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 2. KAMERA KONTROLÜ (Hata korumalı)
        if (playerCamera != null)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Gövdeyi döndür (Sağ/Sol)
            transform.Rotate(Vector3.up * mouseX);

            // Kamerayı döndür (Yukarı/Aşağı)
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        // 3. HAREKET
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // 4. YERÇEKİMİ
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}