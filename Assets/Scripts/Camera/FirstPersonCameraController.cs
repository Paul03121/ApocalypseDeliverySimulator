using UnityEngine;

public class FirstPersonCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float sensitivity = 100f;     // User-adjustable sensitivity
    public Transform playerBody;         // Player transform

    private float xRotation = 0f;
    private const float sensitivityMultiplier = 0.02f; // Internal scale factor

    void Update()
    {
        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * sensitivityMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * sensitivityMultiplier;

        // Vertical rotation
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontal rotation
        playerBody.Rotate(Vector3.up * mouseX);
    }
}