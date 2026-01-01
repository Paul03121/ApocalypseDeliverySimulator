using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // Load the main game scene
        SceneManager.LoadScene("PostApocalypticCity");
    }

    public void QuitGame()
    {
        // Quit the game
        Application.Quit();
    }
}
