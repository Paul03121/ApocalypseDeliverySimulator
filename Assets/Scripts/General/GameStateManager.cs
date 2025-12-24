using UnityEngine;

public enum GameState
{
    Gameplay,
    Paused,
    Inventory,
    Shop,
    GameOver
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.Gameplay;

    private bool stateChangedThisFrame = false;

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep this object across scene loads
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Apply the initial state configuration at startup
        ApplyStateSettings(CurrentState);
    }

    private void LateUpdate()
    {
        // Reset state-change lock at the end of the frame
        stateChangedThisFrame = false;
    }

    // Used by other systems to check if a state change is allowed
    public bool CanChangeState => !stateChangedThisFrame;

    public void SetState(GameState newState)
    {
        // Prevent redundant or multiple state changes
        if (stateChangedThisFrame || CurrentState == newState)
            return;
        
        stateChangedThisFrame = true;
        CurrentState = newState;

        ApplyStateSettings(CurrentState);
    }

    private void ApplyStateSettings(GameState state)
    {
        switch (state)
        {
            case GameState.Gameplay:
                ApplyGameplaySettings();

                UIManager.Instance.ShowGameplayUI(true);
                UIManager.Instance.ShowPauseUI(false);
                UIManager.Instance.ShowInventoryUI(false);
                UIManager.Instance.ShowShopUI(false);
                UIManager.Instance.ShowGameOverUI(false);
                break;

            case GameState.Paused:
                ApplyNonGameplaySettings();

                UIManager.Instance.ShowGameplayUI(false);
                UIManager.Instance.ShowPauseUI(true);
                UIManager.Instance.ShowInventoryUI(false);
                UIManager.Instance.ShowShopUI(false);
                UIManager.Instance.ShowGameOverUI(false);
                break;

            case GameState.Inventory:
                ApplyNonGameplaySettings();

                UIManager.Instance.ShowGameplayUI(false);
                UIManager.Instance.ShowPauseUI(false);
                UIManager.Instance.ShowInventoryUI(true);
                UIManager.Instance.ShowShopUI(false);
                UIManager.Instance.ShowGameOverUI(false);
                break;

            case GameState.Shop:
                ApplyNonGameplaySettings();

                UIManager.Instance.ShowGameplayUI(false);
                UIManager.Instance.ShowPauseUI(false);
                UIManager.Instance.ShowInventoryUI(false);
                UIManager.Instance.ShowShopUI(true);
                UIManager.Instance.ShowGameOverUI(false);
                break;

            case GameState.GameOver:
                ApplyNonGameplaySettings();

                UIManager.Instance.ShowGameplayUI(false);
                UIManager.Instance.ShowPauseUI(false);
                UIManager.Instance.ShowInventoryUI(false);
                UIManager.Instance.ShowShopUI(false);
                UIManager.Instance.ShowGameOverUI(true);
                break;
        }
    }

    // Gameplay-specific settings
    private void ApplyGameplaySettings()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Shared  Non-Gameplay settings
    private void ApplyNonGameplaySettings()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // State checks
    public bool IsGameplay => CurrentState == GameState.Gameplay;
    public bool IsPaused => CurrentState == GameState.Paused;
    public bool IsInventory => CurrentState == GameState.Inventory;
    public bool IsShop => CurrentState == GameState.Shop;
    public bool IsGameOver => CurrentState == GameState.GameOver;
}
