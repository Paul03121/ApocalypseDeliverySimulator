using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TMP_Text healthText;

    private void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            // Subscribe to health change event
            playerHealth.OnHealthChanged += UpdateHealthUI;

            // Initialize UI with current health
            UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
        else
        {
            Debug.LogError("HealthUI: PlayerHealth not found.");
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            // Unsubscribe to avoid memory leaks
            playerHealth.OnHealthChanged -= UpdateHealthUI;
        }
    }

    private void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        float normalizedHealth = currentHealth / maxHealth;

        // Update health bar
        healthFillImage.fillAmount = normalizedHealth;

        // Update numeric health text
        healthText.text = Mathf.CeilToInt(currentHealth).ToString();
    }
}
