using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    void Update()
    {
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