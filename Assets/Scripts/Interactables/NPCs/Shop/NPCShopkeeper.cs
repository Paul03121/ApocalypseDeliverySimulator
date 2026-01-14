using System.Collections.Generic;
using UnityEngine;

public class NPCShopkeeper : Interactable
{
    [Header("Shop Items")]
    public List<InventoryItem> itemsForSale;

    protected override bool WaitForMessage => true;

    private void Start()
    {
        MapIconManager.Instance.RegisterIcon(this, MapIconType.Shop, transform);
    }

    protected override void OnInteract()
    {
        // Open the shop UI and pass this shopkeeper as context
        ShopUIManager.Instance.OpenShop(this);
    }
}