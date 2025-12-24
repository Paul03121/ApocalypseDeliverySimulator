using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float baseMaxHealth = 100f;

    [Header("Modifiers")]
    private int damageReduction = 0;
    private float bonusMaxHealth = 0f;

    private float currentHealth;

    public float MaxHealth => baseMaxHealth + bonusMaxHealth;
    public float CurrentHealth => currentHealth;

    // Event fired whenever health changes
    public event Action<float, float> OnHealthChanged;

    // Event fired when the player dies
    public event Action OnPlayerDeath;

    private void Awake()
    {
        // Initialize health to full at start
        currentHealth = MaxHealth;
    }

    // Applies damage to the player
    public void TakeDamage(float amount)
    {
        if (amount <= 0)
            return;

        // Apply damage reduction
        float finalDamage = amount - damageReduction;
        currentHealth = Mathf.Clamp(currentHealth - finalDamage, 0, MaxHealth);

        // Notify listeners
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);

        // If health reaches zero, handle death
        if (currentHealth <= 0)
            Die();
    }

    // Heals the player
    public void Heal(float amount)
    {
        if (amount <= 0)
            return;

        // Increase health without exceeding max
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, MaxHealth);

        // Notify listeners
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    // Restores the player's HP to full
    public void RestoreFullHealth()
    {
        currentHealth = MaxHealth;

        // Notify listeners
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    private void Die()
    {
        // TODO: Trigger death animation

        // Notify listeners
        OnPlayerDeath?.Invoke();
    }

    public void AddMaxHealthBonus(float amount)
    {
        bonusMaxHealth += amount;
        currentHealth += amount;

        // Clamp current health to new max
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        // Notify listeners
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    public void RemoveMaxHealthBonus(float amount)
    {
        bonusMaxHealth -= amount;
        bonusMaxHealth = Mathf.Max(0, bonusMaxHealth);

        // Clamp current health to new max
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        // Notify listeners
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    public void AddDamageReduction(int amount)
    {
        damageReduction += amount;
    }

    public void RemoveDamageReduction(int amount)
    {
        damageReduction -= amount;
        damageReduction = Mathf.Max(damageReduction, 0);
    }
}