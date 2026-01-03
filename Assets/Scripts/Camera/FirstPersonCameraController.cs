using UnityEngine;

public class FirstPersonCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float sensitivity = 100f;        // User-adjustable sensitivity
    public float xRotationAngle = 65f;      // Vertical rotation limit
    public Transform playerBody;            // Player transform

    [Header("Camera Walk Offset")]
    public float walkZOffset = 0f;          // Default Z offset
    public float walkYOffset = 0f;          // Default Y offset

    [Header("Camera Run Offset")]
    public float runZOffset = 0.375f;       // Forward offset when running
    public float runYOffset = -0.1f;        // Downward offset when running

    [Header("Camera Jump Offset")]
    public float jumpZOffset = 0.375f;      // Forward offset when jumping
    public float jumpYOffset = -0.1f;       // Downward offset when jumping

    [Header("Camera Crouch Offset")]
    public float crouchZOffset = 0.375f;    // Forward offset when crouching
    public float crouchYOffset = -0.6f;     // Downward offset when crouching

    [Header("Camera Attack Offset")]
    public float attackZOffset = 0.375f;    // Forward offset when attacking
    public float attackYOffset = 0f;        // Downward offset when attacking

    [Header("Camera Offset Speed")]
    public float zOffsetLerpSpeed = 8f;     // Smooth transition speed

    [Header("Camera Collision Settings")]
    public LayerMask collisionLayers;
    public float collisionRadius = 0.5f;

    private float xRotation = 0f;
    private const float sensitivityMultiplier = 0.02f;  // Internal scale factor

    private bool isRunning = false;
    private bool isJumping = false;
    private bool isCrouching = false;
    private bool isAttacking = false;
    private Vector3 initialLocalPosition;

    void Start()
    {
        // Store the initial local position of the camera
        initialLocalPosition = transform.localPosition;
    }

    void Update()
    {
        // Stop working if game is not in Gameplay mode
        if (!GameStateManager.Instance.IsGameplay)
            return;

        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * sensitivityMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * sensitivityMultiplier;

        // Vertical rotation
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -xRotationAngle, xRotationAngle);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontal rotation
        playerBody.Rotate(Vector3.up * mouseX);

        HandleCameraOffset();
    }

    private void HandleCameraOffset()
    {
        float targetZ;
        float targetY;

        if (isAttacking)
        {
            targetZ = attackZOffset;
            targetY = attackYOffset;
        }
        else if (isRunning)
        {
            targetZ = runZOffset;
            targetY = runYOffset;
        }
        else if (isJumping)
        {
            targetZ = jumpZOffset;
            targetY = jumpYOffset;
        }
        else if (isCrouching)
        {
            targetZ = crouchZOffset;
            targetY = crouchYOffset;
        }
        else
        {
            targetZ = walkZOffset;
            targetY = walkYOffset;
        }

        // World position of desired offset
        Vector3 desiredLocalPos = initialLocalPosition + new Vector3(0f, targetY, targetZ);
        Vector3 finalLocalPos = desiredLocalPos;

        // Check camera collision
        Transform parent = transform.parent;
        Vector3 desiredWorldPos = parent.TransformPoint(desiredLocalPos);

        // Detect camera collisions
        bool isColliding = Physics.CheckSphere(desiredWorldPos, collisionRadius, collisionLayers);

        // Cancel camera offset
        if (isColliding && (isRunning || isJumping || isCrouching))
            finalLocalPos = initialLocalPosition;

        // Move camera to the final position
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalLocalPos, Time.deltaTime * zOffsetLerpSpeed);
    }

    public void SetRunningState(bool running)
    {
        isRunning = running;
    }

    public void SetJumpingState(bool jumping)
    {
        isJumping = jumping;
    }

    public void SetCrouchingState(bool crouching)
    {
        isCrouching = crouching;
    }

    public void SetAttackingState(bool attacking)
    {
        isAttacking = attacking;
    }
}