using UnityEngine;

public abstract class InventoryItem : ScriptableObject
{
    [Header("General")]
    public string itemName;
    public Sprite icon;

    [Header("Effect")]
    public string effect;

    [Header("Description")]
    [TextArea(2, 4)]
    public string description;

    [Header("Economy")]
    public int price;
}