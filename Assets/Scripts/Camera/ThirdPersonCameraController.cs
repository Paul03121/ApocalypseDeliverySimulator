using UnityEngine;

public class ThirdPersonFrontCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float sensitivity = 100f;
    public Transform playerBody;
    public float distance = 5f;
    public float height = 0.5f;

    [Header("Collision Settings")]
    public LayerMask collisionLayers;
    public float collisionOffset = 0.3f;
    public float smoothSpeed = 10f;
    public float sphereCastRadius = 0.3f;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private const float sensitivityMultiplier = 0.02f;

    private Vector3 currentCamPosition;

    void Start()
    {
        // Align camera rotation with player's forward direction at startup
        Vector3 playerForward = playerBody.forward;
        yRotation = Quaternion.LookRotation(playerForward).eulerAngles.y;

        // Apply same logic as ResetCameraPosition to ensure correct starting position
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        Vector3 offset = rotation * new Vector3(0f, height, distance);
        transform.position = playerBody.position + offset;
        transform.LookAt(playerBody.position + Vector3.up * height);

        currentCamPosition = transform.position;
    }

    void LateUpdate()
    {
        // Read mouse input
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * sensitivityMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * sensitivityMultiplier;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -45f, 45f);

        // Calculate desired rotation and position
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        Vector3 desiredOffset = rotation * new Vector3(0f, height, distance);
        Vector3 desiredPosition = playerBody.position + desiredOffset;

        // Origin for horizontal collision checks
        Vector3 rayOrigin = playerBody.position + Vector3.up * height;
        Vector3 direction = (desiredPosition - rayOrigin).normalized;
        float maxDistance = Vector3.Distance(rayOrigin, desiredPosition);

        RaycastHit hit;

        // Horizontal collision detection
        if (Physics.SphereCast(rayOrigin, sphereCastRadius, direction, out hit, maxDistance, collisionLayers))
        {
            Vector3 hitPosition = hit.point + hit.normal * collisionOffset;
            hitPosition.y = desiredPosition.y;  // Keep height temporarily
            desiredPosition = hitPosition;
        }

        // Vertical collision detection (ceilings and floors)
        Vector3 verticalCastOrigin = playerBody.position + Vector3.up * (height * 0.5f);
        float verticalDistance = desiredPosition.y - verticalCastOrigin.y;

        if (verticalDistance > 0)
        {
            // Check ceiling above
            if (Physics.SphereCast(verticalCastOrigin, sphereCastRadius, Vector3.up, out hit, verticalDistance, collisionLayers))
            {
                desiredPosition.y = hit.point.y - collisionOffset;
            }
        }
        else if (verticalDistance < 0)
        {
            // Check floor below
            if (Physics.SphereCast(verticalCastOrigin, sphereCastRadius, Vector3.down, out hit, -verticalDistance, collisionLayers))
            {
                desiredPosition.y = hit.point.y + collisionOffset;
            }
        }

        // Smooth camera movement
        currentCamPosition = Vector3.Lerp(currentCamPosition, desiredPosition, smoothSpeed * Time.deltaTime);

        // Apply position and rotation
        transform.position = currentCamPosition;
        transform.LookAt(playerBody.position + Vector3.up * height);

        // Rotate player to face camera horizontally
        Vector3 lookDir = transform.position - playerBody.position;
        lookDir.y = 0f;
        playerBody.rotation = Quaternion.LookRotation(lookDir);
    }

    // Reset camera rotation to align with player forward
    public void ResetCameraPosition()
    {
        Vector3 playerForward = playerBody.forward;
        yRotation = Quaternion.LookRotation(playerForward).eulerAngles.y;

        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        Vector3 offset = rotation * new Vector3(0f, height, distance);
        transform.position = playerBody.position + offset;
        transform.LookAt(playerBody.position + Vector3.up * height);

        currentCamPosition = transform.position;
    }
}