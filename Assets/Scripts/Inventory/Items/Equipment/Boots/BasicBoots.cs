using UnityEngine;

[CreateAssetMenu(
    fileName = "BasicBoots",
    menuName = "Inventory/Equipment/Basic Boots"
)]
public class BasicBoots : EquipmentItem
{
    [Header("Boots Stats")]
    public float speedBonus = 2f;

    // Applies the speed bonus effect to the player
    public override void ApplyEffect(GameObject user)
    {
        PlayerMovement playerMovement = user.GetComponent<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogWarning("BasicBoots equipped on object without PlayerMovement.");
            return;
        }

        playerMovement.AddSpeedBonus(speedBonus);
    }

    // Removes the speed bonus effect from the player
    public override void RemoveEffect(GameObject user)
    {
        PlayerMovement playerMovement = user.GetComponent<PlayerMovement>();

        if (playerMovement == null)
            return;

        playerMovement.RemoveSpeedBonus(speedBonus);
    }
}