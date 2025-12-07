using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseUI;
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Do nothing if game is in Game Over
            if (GameStateManager.Instance.IsGameOver)
                return;

            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        // Return game to gameplay state
        GameStateManager.Instance.SetState(GameState.Gameplay);

        pauseUI.SetActive(false);
        isPaused = false;
    }

    private void Pause()
    {
        // Switch game to paused state
        GameStateManager.Instance.SetState(GameState.Paused);

        pauseUI.SetActive(true);
        isPaused = true;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}