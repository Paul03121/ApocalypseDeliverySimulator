using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Death System Block")]
    private PlayerHealth playerHealth;
    private bool isBlocked = false;

    void Awake()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth == null)
            Debug.LogError("PlayerHealth not found in scene");
    }

    void Update()
    {
        // Block if player is dead
        if (isBlocked) return;

        // Prevents multiple state changes in the same frame
        if (!GameStateManager.Instance.CanChangeState)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Pause game only during gameplay
            if (GameStateManager.Instance.IsGameplay)
                Pause();

            // Resume game if it is already paused
            else if (GameStateManager.Instance.IsPaused)
                Resume();
        }
    }

    void OnEnable()
    {
        playerHealth.OnPlayerDeathStarted += HandlePlayerDeathStarted;
        playerHealth.OnPlayerDeathEnded += HandlePlayerDeathEnded;
    }

    void OnDisable()
    {
        playerHealth.OnPlayerDeathStarted -= HandlePlayerDeathStarted;
        playerHealth.OnPlayerDeathEnded -= HandlePlayerDeathEnded;
    }

    private void HandlePlayerDeathStarted()
    {
        isBlocked = true;
    }

    private void HandlePlayerDeathEnded()
    {
        isBlocked = false;
    }

    public void Resume()
    {
        // Return game to gameplay state
        GameStateManager.Instance.SetState(GameState.Gameplay);
    }

    private void Pause()
    {
        // Switch game to paused state
        GameStateManager.Instance.SetState(GameState.Paused);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}