using UnityEngine;

public class EnemyZombie : EnemyBase
{
    private void Reset()
    {
        wanderSpeed = 1f;
        chaseSpeed = 6f;

        attackRange = 2f;
        baseDamage = 10;

        detectionRange = 5f;
        wanderRange = 8f;
        viewRange = 15f;
        viewAngle = 100f;
        wanderCooldown = 8f;
    }
}