using System.Collections.Generic;
using UnityEngine;

public class NPCShopkeeper : Interactable
{
    [Header("Shop Items")]
    public List<InventoryItem> itemsForSale;

    protected override void OnInteract()
    {
        isInteracted = false;

        // Open the shop UI and pass this shopkeeper as context
        ShopUIManager.Instance.OpenShop(this);
    }
}