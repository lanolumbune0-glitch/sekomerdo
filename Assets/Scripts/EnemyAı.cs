using UnityEngine;
using UnityEngine.AI;

// Düşman Kişilikleri
public enum EnemyType 
{ 
    Red_Blinky,   // Dümdüz üstüne koşar (Agresif)
    Pink_Pinky,   // Önünü kesmeye çalışır (Zeki)
    Blue_Inky     // Etrafında dolanır, şaşırtır (Rastgele)
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
    public float blueRandomness = 8f;     // Mavi oyuncunun ne kadar uzağına gitsin?

    [Header("Referanslar")]
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;
    
    // Sistem Değişkenleri
    private NavMeshAgent agent;
    private Transform player;
    private PlayerHealth playerHealth;
    
    // Durumlar
    private bool playerInSight;
    private bool playerInAttackRange;
    private bool alreadyAttacked;
    private Vector3 walkPoint;
    private bool walkPointSet;
    
    // Debug (Görselleştirme)
    private Vector3 debugTarget;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player").transform;
        
        // PlayerHealth scripti varsa al, yoksa hata verme
        if(player != null) playerHealth = player.GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (player == null) return;

        // 1. GÖRÜŞ KONTROLÜ (Gerçekçi Görüş)
        playerInSight = CanSeePlayer();
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // 2. DURUM MAKİNESİ (State Machine)
        if (playerInSight && playerInAttackRange)
        {
            Attacking(); // Hem görüyor hem yakında -> SALDIR
        }
        else if (playerInSight)
        {
            Chasing();   // Görüyor ama uzakta -> KOVALA
        }
        else
        {
            Patroling(); // Görmüyor -> DEVRİYE AT
        }
    }

    // --- DURUM 1: KOVALAMA (Kişiliğe Göre Değişir) ---
    private void Chasing()
    {
        switch (enemyType)
        {
            case EnemyType.Red_Blinky:
                // Zombi modu: Direkt oyuncunun olduğu yere git
                agent.SetDestination(player.position);
                debugTarget = player.position;
                break;

            case EnemyType.Pink_Pinky:
                // Zeki mod: Oyuncunun gittiği yönün önüne kır (Interception)
                Vector3 interceptPoint = player.position + (player.forward * pinkyPredict);
                NavMeshHit hit;
                // Eğer tahmin edilen nokta duvar içindeyse normal kovalamaya dön
                if (NavMesh.SamplePosition(interceptPoint, out hit, 5f, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);
                else
                    agent.SetDestination(player.position);
                
                debugTarget = interceptPoint;
                break;

            case EnemyType.Blue_Inky:
                // Kaotik mod: Oyuncunun tam üstüne değil, etrafındaki rastgele bir noktaya git
                // Bu sayede seni sıkıştırabilir veya arkana dolanabilir
                if (!agent.pathPending && agent.remainingDistance < 2f)
                {
                    Vector3 randomPoint = Random.insideUnitSphere * blueRandomness;
                    agent.SetDestination(player.position + randomPoint);
                }
                debugTarget = agent.destination;
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
            
            // Can azaltma
            if (playerHealth != null) playerHealth.TakeDamage(damage);

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

        // Hedefe vardık mı?
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

    // --- ÖZELLİK: GERÇEKÇİ GÖRÜŞ (Duvar Arkasını Göremez) ---
    bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position, player.position) < sightRange)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToPlayer) < 110f) // 110 derece görüş açısı
            {
                // Kendi vücuduna çarpmasın diye layer mask
                int layerMask = ~LayerMask.GetMask("Enemy"); 
                
                RaycastHit hit;
                if (Physics.Raycast(transform.position, dirToPlayer, out hit, sightRange, layerMask))
                {
                    if (hit.transform == player) return true;
                }
            }
        }
        return false;
    }

    // --- ÖZELLİK: SES DUYMA (Silah Scripti Çağıracak) ---
    public void HearSound(Vector3 soundPos)
    {
        // Sesi duyunca oraya gitmek için walkPoint'i orası yapıyoruz
        walkPoint = soundPos;
        walkPointSet = true;
        agent.SetDestination(soundPos);
    }

    // GÖRSEL AYIKLAMA
    private void OnDrawGizmos()
    {
        // Hedefi göster (Mavi Top)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(debugTarget, 0.5f);
        
        // Menzilleri göster
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}