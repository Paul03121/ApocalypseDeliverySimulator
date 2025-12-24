using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("References")]
    public GameObject gameplayUI;
    public GameObject pauseUI;
    public GameObject inventoryUI;
    public GameObject shopUI;
    public GameObject gameOverUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Enable or disable Gameplay UI
    public void ShowGameplayUI(bool value)
    {
        if (gameplayUI != null)
            gameplayUI.SetActive(value);
    }

    // Enable or disable the Pause UI
    public void ShowPauseUI(bool value)
    {
        if (pauseUI != null)
            pauseUI.SetActive(value);
    }

    // Enable or disable the Inventory UI
    public void ShowInventoryUI(bool value)
    {
        if (inventoryUI != null)
            inventoryUI.SetActive(value);
    }

    // Enable or disable the Shop UI
    public void ShowShopUI(bool value)
    {
        if (shopUI != null)
            shopUI.SetActive(value);
    }

    // Enable or disable the Game Over UI
    public void ShowGameOverUI(bool value)
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(value);
    }
}