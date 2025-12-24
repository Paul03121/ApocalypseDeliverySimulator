using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Items")]
    public List<InventoryItemInstance> ownedEquipment = new();
    public List<InventoryItemInstance> ownedConsumables = new();

    [Header("Equipped Items")]
    private InventoryItemInstance equippedVest;
    private InventoryItemInstance equippedBoots;

    private GameObject player;

    public InventoryItemInstance EquippedVest => equippedVest;
    public InventoryItemInstance EquippedBoots => equippedBoots;

    private void Awake()
    {
        player = gameObject;
    }

    public void AddItem(InventoryItem item)
    {
        if (item == null)
            return;

        // Create a new inventory instance from the item data
        InventoryItemInstance instance = new(item);

        // Decide which inventory type the item belongs to
        if (item is EquipmentItem)
            ownedEquipment.Add(instance);
        else if (item is ConsumableItem)
            ownedConsumables.Add(instance);
    }

    public void EquipEquipmentItem(InventoryItemInstance itemInstance)
    {
        if (itemInstance == null || itemInstance.data is not EquipmentItem equipment)
            return;

        // Prevent equipping items not owned by the player
        if (!ownedEquipment.Contains(itemInstance))
        {
            Debug.LogWarning($"Trying to equip non-owned item: {equipment.itemName}");
            return;
        }

        // Handle equipment based on slot type
        switch (equipment.slot)
        {
            case EquipmentSlot.Vest:
                UnequipVest();
                equippedVest = itemInstance;
                break;

            case EquipmentSlot.Boots:
                UnequipBoots();
                equippedBoots = itemInstance;
                break;

            default:
                Debug.LogWarning("Unsupported equipment slot.");
                return;
        }

        // Apply item effect to the player
        equipment.ApplyEffect(player);
    }

    public void UnequipEquipmentItem(InventoryItemInstance itemInstance)
    {
        if (itemInstance == null)
            return;

        // Unequips a specific equipment item
        if (equippedVest == itemInstance)
            UnequipVest();
        else if (equippedBoots == itemInstance)
            UnequipBoots();
    }

    private void UnequipVest()
    {
        if (equippedVest == null)
            return;

        // Remove item effect from the player
        if (equippedVest.data is EquipmentItem equipment)
            equipment.RemoveEffect(player);

        equippedVest = null;
    }

    private void UnequipBoots()
    {
        if (equippedBoots == null)
            return;

        // Remove item effect from the player
        if (equippedBoots.data is EquipmentItem equipment)
            equipment.RemoveEffect(player);

        equippedBoots = null;
    }

    public void UseConsumable(InventoryItemInstance itemInstance)
    {
        if (itemInstance == null || itemInstance.data is not ConsumableItem consumable)
            return;

        if (!ownedConsumables.Contains(itemInstance))
            return;

        // Apply consumable effect
        consumable.Use(player);

        // Remove consumable after use
        ownedConsumables.Remove(itemInstance);
    }

    public void DiscardItem(InventoryItemInstance itemInstance)
    {
        if (itemInstance == null)
            return;

        // Prevent discarding equipped items
        if (itemInstance == equippedVest || itemInstance == equippedBoots)
            return;

        if (ownedEquipment.Contains(itemInstance))
            ownedEquipment.Remove(itemInstance);
        else if (ownedConsumables.Contains(itemInstance))
            ownedConsumables.Remove(itemInstance);
    }
}