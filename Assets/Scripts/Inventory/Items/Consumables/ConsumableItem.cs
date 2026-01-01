using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Consumable Item")]
public abstract class ConsumableItem : InventoryItem
{
    // Called when the item is used
    public abstract void Use(GameObject user);
}