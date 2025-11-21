using UnityEngine;

// Makes the icon always face the player's camera
public class IconBillboard : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        // Cache the reference to the main camera for performance
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
            return;

        // Align this icon's forward direction with the camera's forward vector
        transform.forward = mainCamera.transform.forward;
    }
}