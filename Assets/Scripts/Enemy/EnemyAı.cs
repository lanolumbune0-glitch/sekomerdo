using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum EnemyType { Red_Blinky, Pink_Pinky, Blue_Inky }

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Target))]
public class SmartEnemy : MonoBehaviour
{
    [Header("Düşman Kimliği")]
    public EnemyType enemyType = EnemyType.Red_Blinky;

    [Header("Savaş Ayarları")]
    public float sightRange = 20f;        // Ne kadar uzağı görebilir?
    public float attackRange = 2.5f;      // Saldırı mesafesi
    public float damage = 10f;            // Hasar
    public float timeBetweenAttacks = 1.5f;

    [Header("--- GELİŞMİŞ DUYU AYARLARI (YENİ) ---")]
    [Tooltip("Düşman bu mesafedeki oyuncuyu arkası dönük olsa bile hisseder (Altıncı His).")]
    public float proximityRange = 8f;     // Eskiden 8f idi, şimdi buradan ayarla
    
    [Tooltip("Düşmanın görüş açısı (90 = Önündeki 180 dereceyi görür).")]
    public float viewAngle = 90f;         // Eskiden 90f idi (Geniş Görüş)

    [Header("Kişilik & Zeka")]
    public float pinkyPredict = 5f;       // Tahmin süresi
    public float blueRandomness = 8f;     // Rastgelelik
    public float stuckCheckInterval = 0.5f; // Sıkışma kontrolü

    [Header("Hareket Hızları")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 6f;

    [Header("--- NAVMESH AYARLARI (YENİ) ---")]
    [Tooltip("Düşmanın hızlanma ivmesi. Yüksek olursa 'zınk' diye durur kalkar.")]
    public float agentAcceleration = 25f; // Eskiden kodda 25f sabitti
    
    [Tooltip("Dönüş hızı. 360 idealdir.")]
    public float agentAngularSpeed = 360f; // Eskiden kodda 360f sabitti

    [Header("Fizik & Hareket")]
    [Range(0f, 1f)] public float knockbackResistance = 0.5f; // 0 = Uçar, 1 = Kıpırdamaz
    
    [Header("Referanslar")]
    public LayerMask whatIsPlayer;
    public LayerMask whatIsObstacle;

    // Sistem Bileşenleri
    private NavMeshAgent agent;
    private Transform player;
    private PlayerHealth playerHealthScript;
    private Animator anim;

    // Durumlar
    private bool isStunned = false;
    private bool isAttacking = false;
    private Vector3 impactVelocity;
    
    // Devriye
    private Vector3 walkPoint;
    private bool walkPointSet;
    
    // Sıkışma Kontrolü
    private float stuckTimer;
    private Vector3 lastPosition;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        
        // --- AYARLARI ARTIK EDİTÖRDEN ALIYORUZ ---
        agent.stoppingDistance = attackRange - 0.5f; 
        agent.autoBraking = false; 
        
        // Editörden girdiğin değerleri NavMesh'e uygula
        agent.acceleration = agentAcceleration;
        agent.angularSpeed = agentAngularSpeed;
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealthScript = playerObj.GetComponent<PlayerHealth>();
        }

        lastPosition = transform.position;
    }

    private void Update()
    {
        if (isStunned || player == null) return;

        // Knockback (Fizik)
        if (impactVelocity.magnitude > 0.2f)
        {
            agent.Move(impactVelocity * Time.deltaTime);
            impactVelocity = Vector3.Lerp(impactVelocity, Vector3.zero, 5f * Time.deltaTime);
        }

        // Zeka
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        bool playerInSight = CheckSight(distanceToPlayer);
        bool playerInAttackRange = distanceToPlayer <= attackRange;

        if (playerInSight && playerInAttackRange)
        {
            AttackState();
        }
        else if (playerInSight)
        {
            ChaseState();
        }
        else
        {
            PatrolState();
        }

        if(anim != null) anim.SetFloat("Speed", agent.velocity.magnitude);
    }

    // --- DEĞİŞKENLİ GÖRÜŞ FONKSİYONU ---
    bool CheckSight(float dist)
    {
        // 1. KURAL: HİSSETME (Editörden ayarlanabilir: proximityRange)
        if (dist < proximityRange) return true;

        // 2. KURAL: GÖRME
        if (dist < sightRange)
        {
            Vector3 targetPos = player.position + Vector3.up * 1.5f;
            Vector3 eyePos = transform.position + Vector3.up * 1.5f;
            Vector3 direction = (targetPos - eyePos).normalized;

            // Görüş Açısı (Editörden ayarlanabilir: viewAngle)
            if (Vector3.Angle(transform.forward, direction) < viewAngle)
            {
                RaycastHit hit;
                if (Physics.Raycast(eyePos, direction, out hit, sightRange))
                {
                    if (hit.transform == player || hit.transform.CompareTag("Player"))
                    {
                        return true; 
                    }
                }
            }
        }
        return false;
    }

    void ChaseState()
    {
        if (isAttacking) return;
        
        // Eski rotayı unut, yeni hedefe kilitlen
        if (agent.hasPath && agent.destination != player.position)
        {
            agent.ResetPath();
        }

        agent.speed = chaseSpeed;
        agent.isStopped = false;
        lastPosition = transform.position;

        switch (enemyType)
        {
            case EnemyType.Red_Blinky:
                SetDestinationSmart(player.position);
                break;

            case EnemyType.Pink_Pinky:
                Vector3 targetPos = player.position + (player.forward * pinkyPredict);
                SetDestinationSmart(targetPos);
                break;

            case EnemyType.Blue_Inky:
                if (!agent.pathPending && agent.remainingDistance < 2f)
                {
                    Vector3 randomDir = Random.insideUnitSphere * blueRandomness;
                    randomDir += player.position;
                    SetDestinationSmart(randomDir);
                }
                break;
        }
    }

    void AttackState()
    {
        agent.isStopped = true;
        
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        Quaternion lookRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);

        if (!isAttacking) StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        if(anim != null) anim.SetTrigger("Attack");

        yield return new WaitForSeconds(0.4f);

        if (Vector3.Distance(transform.position, player.position) <= attackRange + 1f)
        {
            if (playerHealthScript != null) playerHealthScript.TakeDamage(damage);
        }

        yield return new WaitForSeconds(timeBetweenAttacks - 0.4f);
        isAttacking = false;
    }

    void PatrolState()
    {
        agent.speed = patrolSpeed;
        agent.isStopped = false;

        if (!walkPointSet) SearchWalkPoint();
        if (walkPointSet) agent.SetDestination(walkPoint);

        if ((transform.position - walkPoint).magnitude < 2f)
            walkPointSet = false;

        // Sıkışma Kontrolü
        stuckTimer += Time.deltaTime;
        if (stuckTimer > stuckCheckInterval)
        {
            if (Vector3.Distance(transform.position, lastPosition) < 0.1f)
            {
                walkPointSet = false; 
            }
            lastPosition = transform.position;
            stuckTimer = 0;
        }
    }

    void SearchWalkPoint()
    {
        float range = 15f;
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 4f, NavMesh.AllAreas))
        {
            walkPoint = hit.position;
            walkPointSet = true;
        }
    }

    void SetDestinationSmart(Vector3 target)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            agent.SetDestination(player.position);
        }
    }

    public void AddKnockback(Vector3 direction, float force)
    {
        direction.y = 0; 
        float finalForce = force * (1f - knockbackResistance); 
        impactVelocity += direction.normalized * finalForce;
    }

    public void SetStunnedState(bool state)
    {
        isStunned = state;
        agent.isStopped = state;
        if(anim != null) anim.SetBool("IsStunned", state);
    }

    public void HearSound(Vector3 soundPos)
    {
        if (!isStunned)
        {
            walkPoint = soundPos;
            walkPointSet = true;
            agent.SetDestination(soundPos);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Görüş Açısını ve Hissiyat Mesafesini Görselleştir
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, proximityRange); // Altıncı his alanı
    }
}