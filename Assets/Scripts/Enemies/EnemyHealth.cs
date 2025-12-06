using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 50f;
    private float currentHealth;

    private EnemyBase enemy;

    private void Awake()
    {
        // Cache reference to the EnemyBase component
        enemy = GetComponent<EnemyBase>();
    }

    private void Start()
    {
        // Initialize enemy HP at max value
        currentHealth = maxHealth;
    }

    // Getter
    public virtual float GetCurrentHealth() => currentHealth;

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        // Trigger death
        if (currentHealth <= 0f)
            enemy.Die();
    }
}