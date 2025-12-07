using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("References")]
    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public CrosshairController crosshair;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Enable or disable the Pause Menu
    public void ShowPauseMenu(bool value)
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(value);
    }

    // Enable or disable the Game Over Menu
    public void ShowGameOverMenu(bool value)
    {
        if (gameOverMenu != null)
            gameOverMenu.SetActive(value);
    }

    // Enable or disable UI elements
    public void SetUIElementsVisible(bool value)
    {
        SetCrosshairVisible(value);
    }

    // Enable or disable the Crosshair
    public void SetCrosshairVisible(bool value)
    {
        if (crosshair != null)
            crosshair.SetVisible(value);
    }
}
