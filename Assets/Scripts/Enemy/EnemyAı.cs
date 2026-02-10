using UnityEngine;
using UnityEngine.AI;

// Düşman Kişilikleri
public enum EnemyType 
{ 
    Red_Blinky,   // Agresif
    Pink_Pinky,   // Taktiksel
    Blue_Inky     // Rastgele
}

public class SmartEnemy : MonoBehaviour
{
    [Header("Düşman Kimliği")]
    public EnemyType enemyType = EnemyType.Red_Blinky;

    [Header("Temel Zeka Ayarları")]
    public float sightRange = 20f;
    public float attackRange = 2.5f;
    public float damage = 10f;            // Vuruş Gücü
    public float timeBetweenAttacks = 1.5f; 

    [Header("Kişilik Ayarları")]
    public float pinkyPredict = 5f;
    public float blueRandomness = 8f;

    [Header("Fizik Ayarları")]
    [Range(0f, 1f)] public float knockbackMultiplier = 1f; // 1 = Normal, 0.1 = Tank

    [Header("Referanslar")]
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;
    
    // Sistem Değişkenleri
    private NavMeshAgent agent;
    private Transform player;
    private Target myStats; 
    
    // YENİ: Oyuncunun can scriptine ulaşmamız lazım
    private PlayerHealth playerHealthScript; 

    // Değişkenler
    private Vector3 impactVelocity = Vector3.zero;
    private bool isStunned = false;
    private bool playerInSight;
    private bool playerInAttackRange;
    private bool alreadyAttacked;
    private Vector3 walkPoint;
    private bool walkPointSet;
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Oyuncuyu bul
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            // YENİ: Oyuncunun üzerindeki Can scriptini al
            playerHealthScript = playerObj.GetComponent<PlayerHealth>();
        }
        
        myStats = GetComponent<Target>(); 
    }

    private void Update()
    {
        if (isStunned) return;

        // Geri Tepme
        if (impactVelocity.magnitude > 0.2f)
        {
            agent.Move(impactVelocity * Time.deltaTime);
            impactVelocity = Vector3.Lerp(impactVelocity, Vector3.zero, 5f * Time.deltaTime);
        }

        if (player == null) return;

        // Görüş Kontrolleri
        playerInSight = CanSeePlayer();
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // Durum Makinesi
        if (playerInSight && playerInAttackRange)
        {
            Attacking();
        }
        else if (playerInSight)
        {
            Chasing();
        }
        else
        {
            Patroling();
        }
    }

    // --- DURUM 1: KOVALAMA ---
    private void Chasing()
    {
        switch (enemyType)
        {
            case EnemyType.Red_Blinky:
                agent.SetDestination(player.position);
                break;

            case EnemyType.Pink_Pinky:
                Vector3 interceptPoint = player.position + (player.forward * pinkyPredict);
                NavMeshHit hit;
                if (NavMesh.SamplePosition(interceptPoint, out hit, 5f, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);
                else
                    agent.SetDestination(player.position);
                break;

            case EnemyType.Blue_Inky:
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
        agent.SetDestination(transform.position); 
        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            // YENİ: HASAR VERME KODU BURASI
            if (playerHealthScript != null)
            {
                playerHealthScript.TakeDamage(damage);
                Debug.Log(enemyType + " oyuncuya " + damage + " hasar verdi!");
            }

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

    public void AddKnockback(Vector3 direction, float force)
    {
        direction.Normalize();
        if (direction.y < 0) direction.y = -direction.y;
        impactVelocity += direction * (force * knockbackMultiplier);
    }

    public void SetStunnedState(bool state)
    {
        isStunned = state;
        if (isStunned)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
        else
        {
            if(agent.isActiveAndEnabled) agent.isStopped = false;
        }
    }

    public void HearSound(Vector3 soundPos)
    {
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh && !isStunned) 
        {
            walkPoint = soundPos;
            walkPointSet = true;
            agent.SetDestination(soundPos);
        }
    }

    bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position, player.position) < sightRange)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToPlayer) < 110f)
            {
                RaycastHit hit;
                // Layer mask kullanmıyoruz ki her şeye çarpsın, eğer Player'a çarparsa görsün
                if (Physics.Raycast(transform.position, dirToPlayer, out hit, sightRange))
                {
                    if (hit.transform == player) return true;
                }
            }
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}