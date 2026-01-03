using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    private EnemyAttack enemyAttack;

    void Awake()
    {
        // Search components in parent
        enemyAttack = GetComponentInParent<EnemyAttack>();
        if (enemyAttack == null)
            Debug.LogError("EnemyAttack not found in parent");
    }

    public void ExecuteDamage()
    {
        enemyAttack.ExecuteDamage();
    }
}
