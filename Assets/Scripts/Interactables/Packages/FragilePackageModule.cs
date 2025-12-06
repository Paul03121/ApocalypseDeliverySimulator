using UnityEngine;

public class FragilePackageModule : MonoBehaviour, IPackageModule
{
    private PackageInteractable package;
    private PlayerHealth playerHealth;
    private float lastPlayerHealth;
    private bool isSubscribed = false;

    public void Initialize(PackageInteractable pkg)
    {
        package = pkg;

        // Get reference to the player's health component.
        playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("[FragileModule] PlayerHealth not found.");
            return;
        }
    }

    public void OnPackagePickedUp(PackageInteractable package)
    {
        if (playerHealth == null || isSubscribed)
            return;

        // Store current health
        lastPlayerHealth = playerHealth.CurrentHealth;

        // Subscribe to the player's health change event
        playerHealth.OnHealthChanged += OnDamageReceived;
        isSubscribed = true;
    }

    public void OnPackageDropped(PackageInteractable package)
    {
        Unsubscribe();
    }

    public void OnPackageDelivered(PackageInteractable package)
    {
        Unsubscribe();
    }

    private void OnDamageReceived(float current, float max)
    {
        // Ignore updates if the package is not being carried
        if (!package.isBeingHeld)
            return;

        // Player received damage
        if (current < lastPlayerHealth)
        {
            Debug.LogWarning("[FragileModule] ¡Paquete frágil dañado!");

            // Destroy the fragile package
            Destroy(package.gameObject);

            // Stop listening for health changes
            Unsubscribe();
        }

        // Update previous health value
        lastPlayerHealth = current;
    }

    private void Unsubscribe()
    {
        // Remove event subscription
        if (playerHealth != null && isSubscribed)
        {
            playerHealth.OnHealthChanged -= OnDamageReceived;
            isSubscribed = false;
        }
    }
}