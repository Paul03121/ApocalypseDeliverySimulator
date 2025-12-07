using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair UI")]
    public GameObject crosshairUI;

    private void Start()
    {
        // Warn if no crosshair object was assigned
        if (crosshairUI == null)
        {
            Debug.LogWarning("CrosshairController: No crosshair UI assigned.");
            return;
        }

        // Show the crosshair when the game starts
        SetVisible(true);
    }

    // Enable or disable the crosshair
    public void SetVisible(bool value)
    {
        if (crosshairUI != null)
            crosshairUI.SetActive(value);
    }
}
