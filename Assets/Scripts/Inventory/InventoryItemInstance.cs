[System.Serializable]
public class InventoryItemInstance
{
    // Reference to the base item definition
    public InventoryItem data;

    // Constructor that creates a new inventory instance
    public InventoryItemInstance(InventoryItem data)
    {
        this.data = data;
    }
}