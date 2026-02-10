using UnityEngine;

public class DoorLockSystem : MonoBehaviour
{
    [Header("Görsel Ayarlar")]
    public GameObject[] visualKeysOnDoor; // Kapıdaki 3 anahtar
    
    [Header("Kapı Hareketi")]
    public float openHeight = 3f;
    public float openSpeed = 2f;
    
    [Header("Sesler")]
    public AudioClip insertSound; 
    public AudioClip openSound;   
    public AudioClip lockedSound; 

    private int insertedKeyCount = 0;
    private bool isOpen = false;
    private Vector3 targetPos;
    private AudioSource audioSource;
    
    // YENİ: Oyuncu şu an kapının yanında mı?
    private PlayerInventory nearbyInventory; 

    void Start()
    {
        targetPos = transform.position + new Vector3(0, openHeight, 0);
        audioSource = GetComponent<AudioSource>();

        foreach (GameObject keyObj in visualKeysOnDoor)
        {
            if(keyObj != null) keyObj.SetActive(false);
        }
    }

    void Update()
    {
        // 1. KAPI HAREKETİ
        if (isOpen)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * openSpeed);
        }

        // 2. ETKİLEŞİM (Artık Update içinde!)
        // Eğer oyuncu yanımızdaysa (nearbyInventory doluysa) ve E'ye bastıysa
        if (nearbyInventory != null && Input.GetKeyDown(KeyCode.E) && !isOpen)
        {
            AttemptToPlaceKey();
        }
    }

    // Alana girince oyuncuyu tanı
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            nearbyInventory = other.GetComponent<PlayerInventory>();
        }
    }

    // Alandan çıkınca oyuncuyu unut
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            nearbyInventory = null;
        }
    }

    void AttemptToPlaceKey()
    {
        // Anahtarımız var mı?
        if (nearbyInventory.HasKey())
        {
            // Daha yer var mı?
            if (insertedKeyCount < visualKeysOnDoor.Length)
            {
                // --- İŞLEM ---
                nearbyInventory.UseKey(); // Cepten düş
                visualKeysOnDoor[insertedKeyCount].SetActive(true); // Kapıda göster
                insertedKeyCount++; // Sayacı artır
                
                if (insertSound) audioSource.PlayOneShot(insertSound);
                Debug.Log("Anahtar Takıldı: " + insertedKeyCount);

                // Hepsini tamamladık mı?
                if (insertedKeyCount >= visualKeysOnDoor.Length)
                {
                    OpenDoor();
                }
            }
        }
        else
        {
            // Anahtar yoksa
            if (lockedSound) audioSource.PlayOneShot(lockedSound);
            Debug.Log("Anahtarın yok!");
        }
    }

    void OpenDoor()
    {
        isOpen = true;
        if (openSound) audioSource.PlayOneShot(openSound);
    }
}