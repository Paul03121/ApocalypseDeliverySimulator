using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State { Wander, Chase, Attack }
    public State currentState = State.Wander;

    private Vector3 currentWanderDestination;
    private bool hasDestination = false;
    private float nextDestinationCooldown = 0f;

    [Header("References")]
    private Transform player;
    private NavMeshAgent agent;
    private EnemyAttack attack;
    private EnemyBase enemy;

    private void Awake()
    {
        // Cache core component references
        agent = GetComponent<NavMeshAgent>();
        attack = GetComponent<EnemyAttack>();
        enemy = GetComponent<EnemyBase>();

        // Locate player transform
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Start()
    {
        // Begin in wandering state
        ChangeState(State.Wander);
    }

    private void Update()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        // Main state switch
        switch (currentState)
        {
            case State.Wander:
                bool seesPlayer = PlayerInVision();
                if (seesPlayer || dist <= enemy.detectionRange)
                {
                    ChangeState(State.Chase);
                    break;
                }
                WanderBehaviour();
                break;

            case State.Chase:
                ChaseBehaviour();

                if (dist <= enemy.attackRange)
                    ChangeState(State.Attack);
                else if (dist > enemy.viewRange * 1.3f)
                    ChangeState(State.Wander);
                break;

            case State.Attack:
                AttackBehaviour(dist);

                if (dist > enemy.attackRange)
                    ChangeState(State.Chase);
                break;
        }

        // Lock enemy to terrain height
        ApplyGrounding();
    }

    private void ChangeState(State newState)
    {
        currentState = newState;

        switch (newState)
        {
            case State.Wander:
                agent.isStopped = false;
                agent.speed = enemy.wanderSpeed;
                break;

            case State.Chase:
                agent.isStopped = false;
                agent.speed = enemy.chaseSpeed;
                break;

            case State.Attack:
                agent.isStopped = false;
                break;
        }
    }

    private void WanderBehaviour()
    {
        // Pick a new random destination
        if (!hasDestination || Time.time >= nextDestinationCooldown)
        {
            currentWanderDestination = RandomNavSphere(transform.position, enemy.wanderRange);
            agent.SetDestination(currentWanderDestination);

            hasDestination = true;
            nextDestinationCooldown = Time.time + enemy.wanderCooldown;
        }

        // Drop the current path when near the wander point
        float distanceToPoint = Vector3.Distance(transform.position, currentWanderDestination);

        if (distanceToPoint < 1f)
            agent.ResetPath();
    }

    private void ChaseBehaviour()
    {
        // Continuously update the chase target
        agent.SetDestination(player.position);
    }

    private void AttackBehaviour(float distanceToPlayer)
    {
        // Stop only when extremely close to avoid sliding into the player
        bool shouldStop = distanceToPlayer <= enemy.attackRange * 0.5f;

        if (shouldStop)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
        else
        {
            // If not too close, allow subtle repositioning
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        // Rotate smoothly toward the player
        Vector3 lookDir = (player.position - transform.position).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lookDir),
                Time.deltaTime * 8f
            );
        }

        // Perform attack logic
        attack.TryAttack(player, distanceToPlayer);
    }


    private Vector3 RandomNavSphere(Vector3 origin, float dist)
    {
        // Generate a random point
        Vector3 randomDirection = Random.insideUnitSphere * dist;
        randomDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, dist, -1);

        return navHit.position;
    }

    private bool PlayerInVision()
    {
        if (player == null)
            return false;

        // Raycast from enemy to player
        Vector3 origin = transform.position + Vector3.up * (agent.height * 0.9f);
        Vector3 targetPos = player.position + Vector3.up * 0.7f;
        Vector3 dirToPlayer = (targetPos - origin).normalized;

        // Check if in vision radius
        float dist = Vector3.Distance(origin, targetPos);

        if (dist > enemy.viewRange)
            return false;

        // Check if within vision cone
        float angleBetween = Vector3.Angle(transform.forward, dirToPlayer);

        if (angleBetween > enemy.viewAngle / 2f)
            return false;

        // Check if something solid blocks the view
        int solidMask = LayerMask.GetMask("Solid");

        if (Physics.Raycast(origin, dirToPlayer, out RaycastHit hit, dist, solidMask))
            return false;

        return true;
    }

    private void ApplyGrounding()
    {
        // Current position
        Vector3 position = transform.position;

        // Raycast slightly above to avoid clipping issues
        Vector3 rayOrigin = position + Vector3.up * 0.5f;

        // Force the enemy to stick to the ground if there's solid terrain below
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 2f, LayerMask.GetMask("Solid")))
        {
            transform.position = new Vector3(
                position.x,
                hit.point.y,
                position.z
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && enemy != null)
        {
            // Attack radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemy.attackRange);

            // Detection radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemy.detectionRange);

            // Wander radius
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, enemy.wanderRange);

            // Vision radius
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, enemy.viewRange);

            // Vision cone
            Vector3 leftLimit = Quaternion.Euler(0, -enemy.viewAngle / 2f, 0) * transform.forward;
            Vector3 rightLimit = Quaternion.Euler(0, enemy.viewAngle / 2f, 0) * transform.forward;

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + leftLimit * enemy.viewRange);
            Gizmos.DrawLine(transform.position, transform.position + rightLimit * enemy.viewRange);
        }

        // Vision ray
        if (player == null || agent == null || enemy == null)
            return;

        Vector3 origin = transform.position + Vector3.up * (agent.height * 0.9f);
        Vector3 targetPos = player.position + Vector3.up * 0.7f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, targetPos);
    }
}