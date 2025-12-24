using UnityEngine;

// Defines the available equipment slots
public enum EquipmentSlot
{
    Vest,
    Boots
}

[CreateAssetMenu(menuName = "Inventory/Equipment Item")]
public abstract class EquipmentItem : InventoryItem
{
    [Header("Equipment Settings")]
    public EquipmentSlot slot;

    // Called when the equipment effect is applied
    public abstract void ApplyEffect(GameObject user);

    // Called when the equipment effect is removed
    public abstract void RemoveEffect(GameObject user);
}