using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100;

    private float currentHealth;

    public float MaxHealth => maxHealth;              // Public read-only access to max health
    public float CurrentHealth => currentHealth;      // Public read-only access to current health

    // Event fired whenever health changes
    public event Action<float, float> OnHealthChanged;

    // Event fired when the player dies
    public event Action OnPlayerDeath;

    private void Awake()
    {
        // Initialize health to full at start
        currentHealth = maxHealth;
    }

    // Applies damage to the player
    public void TakeDamage(float amount)
    {
        if (amount <= 0)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Notify listeners that health has changed
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"[PlayerHealth] Player received {amount} damage. Remaining HP: {currentHealth}");

        // If health reaches zero, handle death
        if (currentHealth <= 0)
        {
            Die();
            return;
        }
    }

    // Heals the player
    public void Heal(float amount)
    {
        if (amount <= 0)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Notify UI or other systems
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // Restores the player's HP to full
    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;

        // Notify UI or other systems of the update
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        Debug.Log("Player has died.");

        // TODO: Trigger death animation

        // Notify any listeners (GameManager, UI, etc.)
        OnPlayerDeath?.Invoke();
    }
}