using UnityEngine;
using UnityEngine.AI;

// Düşman Kişilikleri
public enum EnemyType 
{ 
    Red_Blinky,   // Dümdüz üstüne koşar (Agresif - Zombi gibi)
    Pink_Pinky,   // Önünü kesmeye çalışır (Zeki - Taktiksel)
    Blue_Inky     // Etrafında dolanır, şaşırtır (Rastgele - Kaotik)
}

public class SmartEnemy : MonoBehaviour
{
    [Header("Düşman Kimliği")]
    public EnemyType enemyType = EnemyType.Red_Blinky;

    [Header("Temel Zeka Ayarları")]
    public float sightRange = 20f;        // Görme Menzili
    public float attackRange = 2.5f;      // Saldırı Menzili
    public float damage = 10f;            // Vuruş Gücü
    public float timeBetweenAttacks = 1.5f; // Saldırı Hızı

    [Header("Kişilik Ayarları")]
    public float pinkyPredict = 5f;       // Pembe ne kadar öne kırsın?
    public float blueRandomness = 8f;     // Mavi ne kadar sapıtsın?

    [Header("Referanslar")]
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;
    
    // Sistem Değişkenleri
    private NavMeshAgent agent;
    private Transform player;
    private Target myStats; // Kendi can scriptimiz (Ölümsüzlük kontrolü için)

    // Fizik ve Durum Değişkenleri
    private Vector3 impactVelocity = Vector3.zero; // Geri tepme kuvveti
    private bool isStunned = false; // Sersemledi mi?
    
    // AI Durumları
    private bool playerInSight;
    private bool playerInAttackRange;
    private bool alreadyAttacked;
    private Vector3 walkPoint;
    private bool walkPointSet;

    [Header("Fizik Ayarları")]
    [Range(0f, 1f)] public float knockbackMultiplier = 1f;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player").transform;
        myStats = GetComponent<Target>(); // Kendi üzerindeki Target scriptini al
    }

    private void Update()
    {
        // 1. ÖNCELİK: SERSEMLEME KONTROLÜ
        // Eğer sersemlediyse (Nemesis modu) hiçbir şey yapma
        if (isStunned) return;

        // 2. ÖNCELİK: GERİ TEPME FİZİĞİ (KNOCKBACK)
        // Eğer bir darbe aldıysa geriye doğru kay
        if (impactVelocity.magnitude > 0.2f)
        {
            agent.Move(impactVelocity * Time.deltaTime);
            // Sürtünme etkisiyle yavaşla
            impactVelocity = Vector3.Lerp(impactVelocity, Vector3.zero, 5f * Time.deltaTime);
        }

        if (player == null) return;

        // 3. GÖRÜŞ KONTROLÜ
        // Oyuncuyu görüyor muyum? (Menzil + Duvar Arkası Kontrolü)
        playerInSight = CanSeePlayer();
        // Saldırı mesafesinde miyim?
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // 4. DURUM MAKİNESİ (State Machine)
        if (playerInSight && playerInAttackRange)
        {
            Attacking(); // Görüyor + Yakın = SALDIR
        }
        else if (playerInSight)
        {
            Chasing();   // Görüyor + Uzak = KOVALA (Kişiliğe göre)
        }
        else
        {
            Patroling(); // Görmüyor = DEVRİYE
        }
    }

    // --- DURUM 1: KOVALAMA (Kişiliğe Göre) ---
    private void Chasing()
    {
        switch (enemyType)
        {
            case EnemyType.Red_Blinky:
                // Zombi modu: Direkt oyuncuya koş
                agent.SetDestination(player.position);
                break;

            case EnemyType.Pink_Pinky:
                // Zeki mod: Oyuncunun gittiği yönün önüne kır
                Vector3 interceptPoint = player.position + (player.forward * pinkyPredict);
                NavMeshHit hit;
                // Eğer tahmin edilen nokta duvarın içindeyse normale dön
                if (NavMesh.SamplePosition(interceptPoint, out hit, 5f, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);
                else
                    agent.SetDestination(player.position);
                break;

            case EnemyType.Blue_Inky:
                // Kaotik mod: Oyuncunun etrafında rastgele dolan
                if (!agent.pathPending && agent.remainingDistance < 2f)
                {
                    Vector3 randomPoint = Random.insideUnitSphere * blueRandomness;
                    agent.SetDestination(player.position + randomPoint);
                }
                break;
        }
    }

    // --- DURUM 2: SALDIRI ---
    private void Attacking()
    {
        agent.SetDestination(transform.position); // Dur
        transform.LookAt(player); // Dön

        if (!alreadyAttacked)
        {
            Debug.Log(enemyType + " sana vurdu!");
            
            // Buraya oyuncunun canını azaltma kodu gelecek
            // Örn: player.GetComponent<PlayerHealth>().TakeDamage(damage);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    // --- DURUM 3: DEVRİYE ---
    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();
        if (walkPointSet) agent.SetDestination(walkPoint);

        if (Vector3.Distance(transform.position, walkPoint) < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float range = 15f;
        float randomZ = Random.Range(-range, range);
        float randomX = Random.Range(-range, range);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    // --- ÖZEL YETENEK 1: GERİ TEPME (KNOCKBACK) ---
    public void AddKnockback(Vector3 direction, float force)
    {
        direction.Normalize();
        if (direction.y < 0) direction.y = -direction.y; 

        // İŞTE SİHİR BURADA:
        // Gelen gücü, düşmanın kendi direnciyle çarpıyoruz.
        // Eğer multiplier 0.1 ise, gücün %90'ı yok olur.
        impactVelocity += direction * (force * knockbackMultiplier);
    }

    // --- ÖZEL YETENEK 2: SERSEMLEME (NEMESIS MODU) ---
    public void SetStunnedState(bool state)
    {
        isStunned = state;

        if (isStunned)
        {
            agent.isStopped = true;       // Yürümeyi durdur
            agent.velocity = Vector3.zero; // Kaymayı durdur
        }
        else
        {
            if(agent.isActiveAndEnabled) agent.isStopped = false; // Devam et
        }
    }

    // --- ÖZEL YETENEK 3: SES DUYMA ---
    public void HearSound(Vector3 soundPos)
    {
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh && !isStunned) 
        {
            walkPoint = soundPos;
            walkPointSet = true;
            agent.SetDestination(soundPos);
        }
    }

    // --- GÖRÜŞ SİSTEMİ (Gerçekçi - Duvar Arkası Görmez) ---
    bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position, player.position) < sightRange)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToPlayer) < 110f) // 110 derece görüş açısı
            {
                // Raycast atarken "Enemy" layerını görmezden gel ki kendine çarpmasın
                // (LayerMask ayarlarını Unity'den yapman gerekebilir)
                RaycastHit hit;
                if (Physics.Raycast(transform.position, dirToPlayer, out hit, sightRange))
                {
                    if (hit.transform == player) return true;
                }
            }
        }
        return false;
    }

    // EDİTÖRDE GÖRMEK İÇİN
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}