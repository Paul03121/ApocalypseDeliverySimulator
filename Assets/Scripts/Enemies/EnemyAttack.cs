using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float attackCooldown = 3f;
    private float cooldownTimer = 0f;

    [Header("References")]
    private EnemyBase enemy;
    private Animator animator;
    private Transform currentTarget;

    private void Awake()
    {
        // Cache references
        enemy = GetComponent<EnemyBase>();
        animator = GetComponentInChildren<Animator>();
    }

    public void TryAttack(Transform target, float requiredDistance)
    {
        // Abort if no target or the enemy component is missing
        if (target == null || enemy == null) return;

        // Countdown internal attack cooldown
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer > 0f) return;

        // Measure distance to determine if attack is allowed
        float dist = Vector3.Distance(transform.position, target.position);

        // Only attack if the target is within range
        if (dist <= requiredDistance)
        {
            currentTarget = target;

            // Notify animator
            animator.SetTrigger("AttackTrigger");

            // Reset cooldown
            cooldownTimer = attackCooldown;
        }
    }

    public void ExecuteDamage()
    {
        if (currentTarget == null || enemy.IsDead) return;

        // Check distance with target
        float dist = Vector3.Distance(transform.position, currentTarget.position);
        if (dist <= enemy.attackRange * 1.2f)
        {
            PlayerHealth ph = currentTarget.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                // Apply damage
                ph.TakeDamage(enemy.baseDamage);
            }
        }
    }
}