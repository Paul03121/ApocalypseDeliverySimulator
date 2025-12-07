using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverUI;
    public PlayerHealth playerHealth;

    private void Start()
    {
        gameOverUI.SetActive(false);

        // Subscribe to the player death event
        playerHealth.OnPlayerDeath += ShowGameOver;
    }

    private void ShowGameOver()
    {
        GameStateManager.Instance.SetState(GameState.GameOver);
        gameOverUI.SetActive(true);
    }

    public void Retry()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Application.Quit();
    }
}