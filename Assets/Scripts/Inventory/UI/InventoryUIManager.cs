using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    [Header("Player Section")]
    public TextMeshProUGUI moneyText;
    public Button equippedVestSlot;
    public Button equippedBootsSlot;

    [Header("Item Grids")]
    public Transform vestGrid;
    public Transform bootsGrid;
    public Transform consumablesGrid;

    [Header("Item Details")]
    public GameObject itemDetailsRoot;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemEffectText;
    public Button ActionButton;
    public TextMeshProUGUI ActionButtonText;
    public Button discardButton;
    public Button cancelButton;

    [Header("References")]
    [SerializeField] private GameObject player;

    [Header("Player Model Preview")]
    [SerializeField] private Camera inventoryCamera;
    [SerializeField] private Transform modelSpawnPoint;
    [SerializeField] private GameObject playerModelPrefab;

    private GameObject spawnedModel;

    private Inventory inventory;
    private PlayerWallet wallet;

    private InventoryItemInstance currentSelectedItem;
    private RectTransform currentSlotTransform;

    private Dictionary<EquipmentSlot, Transform> equipmentGrids;

    private void Awake()
    {
        // Cache required player components
        inventory = player.GetComponent<Inventory>();
        wallet = player.GetComponent<PlayerWallet>();

        if (inventory == null)
            Debug.LogError("Inventory not found on Player");

        if (wallet == null)
            Debug.LogError("PlayerWallet not found on Player");

        // Initialize equipment dictionary
        equipmentGrids = new Dictionary<EquipmentSlot, Transform>
        {
            {EquipmentSlot.Vest, vestGrid},
            {EquipmentSlot.Boots, bootsGrid}
        };
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
        // Prevents multiple state changes in the same frame
        if (!GameStateManager.Instance.CanChangeState)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Open inventory only during gameplay
            if (GameStateManager.Instance.IsGameplay)
                OpenInventory();

            // Close inventory with Tab if it is already open
            else if (GameStateManager.Instance.IsInventory)
                CloseInventory();
        }

        // Close inventory with Esc if it is already open
        if (GameStateManager.Instance.IsInventory && Input.GetKeyDown(KeyCode.Escape))
            CloseInventory();
    }

    private void OpenInventory()
    {
        // Switch game to inventory state
        GameStateManager.Instance.SetState(GameState.Inventory);

        RefreshUI();
        ShowPlayerModel();
        HideItemDetails();
    }

    private void CloseInventory()
    {
        // Return game to gameplay state
        GameStateManager.Instance.SetState(GameState.Gameplay);

        HidePlayerModel();
        HideItemDetails();
    }

    private void ShowPlayerModel()
    {
        if (spawnedModel != null)
            return;

        // Enable model root and spawn preview model
        modelSpawnPoint.gameObject.SetActive(true);

        spawnedModel = Instantiate(playerModelPrefab, modelSpawnPoint);
        spawnedModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        // TODO: Play IDLE animation
        // Animator anim = spawnedModel.GetComponent<Animator>();
        // anim.Play("Idle");

        inventoryCamera.gameObject.SetActive(true);
    }

    private void HidePlayerModel()
    {
        // Destroy preview model if it exists
        if (spawnedModel != null)
            Destroy(spawnedModel);

        modelSpawnPoint.gameObject.SetActive(false);
        inventoryCamera.gameObject.SetActive(false);
    }

    private void RefreshUI()
    {
        // Refresh equipped slots and item grids
        RefreshEquippedSlots();
        RefreshItemGrids();
    }

    private void RefreshEquippedSlots()
    {
        // Update equipped item slots
        SetupEquippedSlot(equippedVestSlot, inventory.EquippedVest);
        SetupEquippedSlot(equippedBootsSlot, inventory.EquippedBoots);
    }

    private void SetupEquippedSlot(Button slotButton, InventoryItemInstance itemInstance)
    {
        // Get icon image from button child
        Image icon = slotButton.transform.GetChild(0).GetComponent<Image>();

        slotButton.onClick.RemoveAllListeners();

        // Clear slot if empty
        if (itemInstance == null)
        {
            icon.sprite = null;
            icon.enabled = false;
            return;
        }

        // Show equipped item icon
        icon.sprite = itemInstance.data.icon;
        icon.enabled = true;

        // Allow clicking the equipped item
        slotButton.onClick.AddListener(() => OnItemSelected(itemInstance, slotButton.GetComponent<RectTransform>()));
    }

    private void RefreshItemGrids()
    {
        // Clear equipment grids
        foreach (var grid in equipmentGrids.Values)
            ClearGrid(grid);

        // Clear consumables grid
        ClearGrid(consumablesGrid);

        // Populate equipment grids
        foreach (var instance in inventory.ownedEquipment)
        {
            EquipmentItem item = instance.data as EquipmentItem;
            if (item == null)
                continue;

            if (equipmentGrids.TryGetValue(item.slot, out Transform grid))
                CreateSlot(instance, grid);
            else
                Debug.LogWarning($"No grid assigned for slot: {item.slot}");
        }

        // Populate consumables grid
        foreach (var instance in inventory.ownedConsumables)
        {
            ConsumableItem item = instance.data as ConsumableItem;
            if (item == null)
                continue;

            CreateSlot(instance, consumablesGrid);
        }
    }

    private void ClearGrid(Transform grid)
    {
        // Reset all slots in a grid
        foreach (Transform slot in grid)
        {
            Image icon = slot.GetChild(0).GetComponent<Image>();
            icon.sprite = null;
            icon.enabled = false;

            Button btn = slot.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(HideItemDetails);
        }
    }

    private void CreateSlot(InventoryItemInstance instance, Transform grid)
    {
        // Find the first empty slot and assign the item
        foreach (Transform slot in grid)
        {
            Image icon = slot.GetChild(0).GetComponent<Image>();

            if (!icon.enabled)
            {
                icon.sprite = instance.data.icon;
                icon.enabled = true;

                Button btn = slot.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnItemSelected(instance, slot.GetComponent<RectTransform>()));

                return;
            }
        }
    }

    private void OnItemSelected(InventoryItemInstance instance, RectTransform slotTransform)
    {
        // Store selected item and slot
        currentSelectedItem = instance;
        currentSlotTransform = slotTransform;

        // Update item details UI
        itemNameText.text = instance.data.itemName;
        itemEffectText.text = instance.data.effect;

        itemDetailsRoot.SetActive(true);
        itemDetailsRoot.GetComponent<RectTransform>().position =
            slotTransform.position + new Vector3(0f, 0f, 0f);

        ConfigureButtons(instance);
    }

    private void ConfigureButtons(InventoryItemInstance instance)
    {
        // Clear previous listeners
        ActionButton.onClick.RemoveAllListeners();
        discardButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        // Check if the item is currently equipped
        bool isEquipped = instance == inventory.EquippedVest || instance == inventory.EquippedBoots;

        if (instance.data is EquipmentItem equipment)
        {
            // Equip and unequip logic
            ActionButtonText.text = isEquipped ? "Desequipar" : "Equipar";

            ActionButton.onClick.AddListener(() =>
            {
                if (isEquipped)
                    inventory.UnequipEquipmentItem(instance);
                else
                    inventory.EquipEquipmentItem(instance);

                RefreshUI();
                HideItemDetails();
            });

            // Equipped items cannot be discarded
            discardButton.interactable = !isEquipped;
        }
        else if (instance.data is ConsumableItem consumable)
        {
            // Use consumable logic
            ActionButtonText.text = "Usar";

            ActionButton.onClick.AddListener(() =>
            {
                inventory.UseConsumable(instance);
                RefreshUI();
                HideItemDetails();
            });

            // Consumable items can always be discarded
            discardButton.interactable = true;
        }

        // Discard button logic
        discardButton.onClick.AddListener(() =>
        {
            inventory.DiscardItem(instance);
            RefreshUI();
            HideItemDetails();
        });

        // Cancel button
        cancelButton.onClick.AddListener(HideItemDetails);
    }

    private void HideItemDetails()
    {
        // Hide item details panel and clear selection
        itemDetailsRoot.SetActive(false);
        currentSelectedItem = null;
        currentSlotTransform = null;
    }

    private void UpdateMoneyUI(int amount)
    {
        // Update money display
        moneyText.text = $"$ {amount}";
    }
}