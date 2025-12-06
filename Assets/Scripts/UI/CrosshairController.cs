using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair UI")]
    public GameObject crosshairUI;

    void Start()
    {
        // Lock and hide the system cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Enable the crosshair
        if (crosshairUI != null)
            crosshairUI.SetActive(true);
        else
            Debug.LogWarning("CrosshairController: No crosshair UI assigned.");
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // Relock cursor when returning to the game window
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (crosshairUI != null)
            crosshairUI.SetActive(false);
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshairUI != null)
            crosshairUI.SetActive(true);
    }
}