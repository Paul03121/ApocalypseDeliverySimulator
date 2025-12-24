using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    public static ShopUIManager Instance { get; private set; }

    [Header("Money")]
    public TextMeshProUGUI moneyText;

    [Header("Items Grid")]
    public Transform itemsGrid;

    [Header("Item Details")]
    public GameObject itemDetailsContent;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemEffectText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemPriceText;
    public Button buyButton;

    private NPCShopkeeper currentShop;
    private InventoryItem selectedItem;

    private Inventory inventory;
    private PlayerWallet wallet;

    private void Awake()
    {
        // Initialize instance
        Instance = this;

        // Cache required player components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        inventory = player.GetComponent<Inventory>();
        wallet = player.GetComponent<PlayerWallet>();

        if (inventory == null)
            Debug.LogError("Inventory not found on Player");

        if (wallet == null)
            Debug.LogError("PlayerWallet not found on Player");

        // Ensure buy button has a single listener
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(BuySelectedItem);
    }

    private void OnEnable()
    {
        // Subscribe to money change event
        if (wallet != null)
        {
            wallet.OnMoneyChanged += UpdateMoneyUI;
            UpdateMoneyUI(wallet.CurrentMoney);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from money change event
        if (wallet != null)
            wallet.OnMoneyChanged -= UpdateMoneyUI;
    }

    private void Update()
    {
        if (GameStateManager.Instance.IsShop && Input.GetKeyDown(KeyCode.Escape))
            CloseShop();
    }

    public void OpenShop(NPCShopkeeper shop)
    {
        // Switch game to shop state
        GameStateManager.Instance.SetState(GameState.Shop);

        // Cache current shop and reset selection
        currentShop = shop;
        selectedItem = null;

        // Refresh UI content
        RefreshShop();
        HideItemDetails();
    }

    public void CloseShop()
    {
        // Return game to gameplay state
        GameStateManager.Instance.SetState(GameState.Gameplay);

        HideItemDetails();
    }

    private void RefreshShop()
    {
        // Update money and refresh available items
        UpdateMoneyUI(wallet.CurrentMoney);
        PopulateItemsGrid();
    }

    private void UpdateMoneyUI(int amount)
    {
        // Update money display
        moneyText.text = $"$ {amount}";

        // Buying availability depends on current money
        UpdateBuyButtonState();
    }

    private void PopulateItemsGrid()
    {
        // Reset all slots before repopulating
        foreach (Transform slot in itemsGrid)
        {
            Image icon = slot.GetChild(0).GetComponent<Image>();
            icon.sprite = null;
            icon.enabled = false;

            Button btn = slot.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(HideItemDetails);
        }

        // Populate grid with items sold by the current shop
        foreach (var item in currentShop.itemsForSale)
            CreateSlot(item);
    }

    private void CreateSlot(InventoryItem item)
    {
        // Find the first empty slot and assign the item
        foreach (Transform slot in itemsGrid)
        {
            Image icon = slot.GetChild(0).GetComponent<Image>();

            if (!icon.enabled)
            {
                icon.sprite = item.icon;
                icon.enabled = true;

                Button btn = slot.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnItemSelected(item));

                return;
            }
        }
    }

    private void OnItemSelected(InventoryItem item)
    {
        // Store selected item
        selectedItem = item;

        // Update item details
        itemNameText.text = item.itemName;
        itemEffectText.text = item.effect;
        itemDescriptionText.text = item.description;
        itemPriceText.text = $"$ {item.price}";

        itemDetailsContent.SetActive(true);
        UpdateBuyButtonState();
    }

    private void UpdateBuyButtonState()
    {
        // Disable buy button if no item is selected
        if (selectedItem == null)
        {
            buyButton.interactable = false;
            return;
        }

        // Enable button only if the player has enough money
        buyButton.interactable = wallet.HasEnough(selectedItem.price);
    }

    public void BuySelectedItem()
    {
        if (selectedItem == null)
            return;

        // Attempt to spend money
        if (!wallet.SpendMoney(selectedItem.price))
            return;

        // Add purchased item to inventory
        inventory.AddItem(selectedItem);

        // Re-evaluate button state after purchase
        UpdateBuyButtonState();
    }

    private void HideItemDetails()
    {
        // Hide item details panel and clear selection
        itemDetailsContent.SetActive(false);
        selectedItem = null;
    }
}