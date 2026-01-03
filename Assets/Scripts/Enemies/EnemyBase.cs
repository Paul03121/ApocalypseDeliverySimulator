using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Speed Settings")]
    public float wanderSpeed = 1f;
    public float chaseSpeed = 5f;

    [Header("Attack Settings")]
    public float attackRange = 1f;
    public int baseDamage = 10;

    [Header("AI Settings")]
    public float detectionRange = 5f;
    public float wanderRange = 10f;
    public float viewRange = 10f;
    public float viewAngle = 100f;
    public float wanderCooldown = 10f;

    public bool IsDead { get; private set; } = false;

    private Animator animator;
    protected Transform player;

    public event System.Action OnDeath;

    protected virtual void Awake()
    {
        // Animator reference
        animator = GetComponentInChildren<Animator>();

        // Player reference
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;
    }

    public virtual void Die()
    {
        if (IsDead) return;

        IsDead = true;

        // Notify animator
        animator.SetTrigger("isDefeated");

        OnDeath?.Invoke();

        // Deactivate IA
        var ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        // Stop NavMeshAgent
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Deactivate damage
        var damageDealer = GetComponent<EnemyAttack>();
        if (damageDealer != null)
            damageDealer.enabled = false;


        Destroy(gameObject, 6f);
    }
}