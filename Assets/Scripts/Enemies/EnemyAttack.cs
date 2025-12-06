using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float attackCooldown = 2f;
    private float cooldownTimer = 0f;

    private EnemyBase enemy;

    private void Awake()
    {
        // Cache EnemyBase reference
        enemy = GetComponent<EnemyBase>();
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
            PlayerHealth ph = target.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(enemy.baseDamage);
            }

            // Reset cooldown after performing an attack
            cooldownTimer = attackCooldown;
        }
    }
}