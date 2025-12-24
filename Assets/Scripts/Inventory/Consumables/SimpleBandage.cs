using UnityEngine;

[CreateAssetMenu(
    fileName = "SimpleBandage",
    menuName = "Inventory/Consumables/Simple Bandage"
)]
public class SimpleBandage : ConsumableItem
{
    [Header("Bandage Settings")]
    public float healAmount = 20f;

    // Restore health to the player when used
    public override void Use(GameObject user)
    {
        PlayerHealth playerHealth = user.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogWarning("SimpleBandage used on object without PlayerHealth.");
            return;
        }

        playerHealth.Heal(healAmount);
    }
}