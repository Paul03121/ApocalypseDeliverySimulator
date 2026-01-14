using UnityEngine;

public class ThirdPersonFrontCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float sensitivity = 100f;
    public Transform playerBody;
    public float distance = 5f;
    public float height = 1.5f;
    public float maxRotationAngle = 45f;

    [Header("Collision Settings")]
    public LayerMask collisionLayers;
    public float collisionOffset = 0.3f;
    public float smoothSpeed = 10f;
    public float sphereCastRadius = 0.3f;

    [Header("Death Settings")]
    [SerializeField] private float deathMaxAngle = 40f;
    [SerializeField] private float deathInputSpeed = 0.2f;
    [SerializeField] private float deathDistance = 3.5f;
    private bool isDeathView = false;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private const float sensitivityMultiplier = 0.02f;

    private Vector3 currentCamPosition;

    void Start()
    {
        ResetCameraPosition();
    }

    void LateUpdate()
    {
        // Stop working if game is not in Gameplay mode
        if (!GameStateManager.Instance.IsGameplay)
            return;

        if (!isDeathView)
        {
            // Read mouse input
            float mouseX = Input.GetAxis("Mouse X") * sensitivity * sensitivityMultiplier;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity * sensitivityMultiplier;

            UpdateCameraWithInput(mouseX, mouseY, maxRotationAngle, distance);
        }
        else
        {
            // Force camera movement to see the player's death animation
            UpdateCameraWithInput(0, deathInputSpeed, deathMaxAngle, deathDistance);
        }
    }

    public void UpdateCameraWithInput(float inputX, float inputY, float maxAngle, float distance)
    {
        yRotation += inputX;
        xRotation -= inputY;
        xRotation = Mathf.Clamp(xRotation, -maxAngle, maxAngle);

        // Calculate desired rotation and position
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        Vector3 desiredOffset = rotation * new Vector3(0f, height, distance);
        Vector3 desiredPosition = playerBody.position + desiredOffset;

        // Origin for horizontal collision checks
        Vector3 rayOrigin = playerBody.position + Vector3.up * height;
        Vector3 direction = (desiredPosition - rayOrigin).normalized;
        float maxDistance = Vector3.Distance(rayOrigin, desiredPosition);

        // Horizontal collision detection
        if (Physics.SphereCast(rayOrigin, sphereCastRadius, direction, out RaycastHit hit, maxDistance, collisionLayers))
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

        // Allow camera movement if time is frozen only for death view
        float deltaTime = isDeathView ? Time.unscaledDeltaTime : Time.deltaTime;

        // Smooth camera movement
        currentCamPosition = Vector3.Lerp(currentCamPosition, desiredPosition, smoothSpeed * deltaTime);

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

    public void EnterDeathView()
    {
        isDeathView = true;
    }

    public void ExitDeathView()
    {
        isDeathView = false;
    }
}