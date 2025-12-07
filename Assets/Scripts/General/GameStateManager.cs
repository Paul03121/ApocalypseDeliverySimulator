using UnityEngine;

public enum GameState
{
    Gameplay,
    Paused,
    GameOver
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.Gameplay;

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
        SetState(GameState.Gameplay);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.Gameplay:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                UIManager.Instance.SetUIElementsVisible(true);
                UIManager.Instance.ShowPauseMenu(false);
                UIManager.Instance.ShowGameOverMenu(false);
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                UIManager.Instance.SetUIElementsVisible(false);
                UIManager.Instance.ShowPauseMenu(true);
                UIManager.Instance.ShowGameOverMenu(false);
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                UIManager.Instance.SetUIElementsVisible(false);
                UIManager.Instance.ShowPauseMenu(false);
                UIManager.Instance.ShowGameOverMenu(true);
                break;
        }
    }

    public bool IsGameplay => CurrentState == GameState.Gameplay;
    public bool IsPaused => CurrentState == GameState.Paused;
    public bool IsGameOver => CurrentState == GameState.GameOver;
}
