using UnityEngine;

[CreateAssetMenu(
    fileName = "BasicArmorVest",
    menuName = "Inventory/Equipment/Basic Armor Vest"
)]
public class BasicArmorVest : EquipmentItem
{
    [SerializeField] private int damageReduction = 5;

    // Applies the damage reduction effect to the player
    public override void ApplyEffect(GameObject player)
    {
        player.GetComponent<PlayerHealth>()
              .AddDamageReduction(damageReduction);
    }

    // Removes the damage reduction effect from the player
    public override void RemoveEffect(GameObject player)
    {
        player.GetComponent<PlayerHealth>()
              .RemoveDamageReduction(damageReduction);
    }
}