using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("References")]
    private EnemyBase enemy;
    private Animator animator;

    private void Awake()
    {
        // Cache references
        enemy = GetComponent<EnemyBase>();
        animator = GetComponentInChildren<Animator>();
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

        if (currentHealth <= 0f)
        {
            // Trigger death
            enemy.Die();
        }
        else
        {
            // Notify animator
            animator.SetTrigger("isHitted");
        }
    }
}