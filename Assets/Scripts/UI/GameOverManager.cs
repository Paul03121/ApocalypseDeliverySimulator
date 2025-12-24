using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public PlayerHealth playerHealth;

    private void Start()
    {
        // Subscribe to the player death event
        playerHealth.OnPlayerDeath += ShowGameOver;
    }

    private void ShowGameOver()
    {
        // Switch game to Game Over state
        GameStateManager.Instance.SetState(GameState.GameOver);
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