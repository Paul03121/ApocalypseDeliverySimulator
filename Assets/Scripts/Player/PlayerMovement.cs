using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float crouchSpeed = 2f;
    public float jumpHeight = 0.8f;
    public float gravity = -18f;

    [Header("Crouch Settings")]
    public float crouchHeight = 0.99f;
    private float originalHeight;
    public float crouchTransitionSpeed = 20f;

    [Header("Ground Check Settings")]
    public float groundCheckRadius = 0.75f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isCrouching = false;
    private bool isGrounded;
    private bool wasGroundedLastFrame = false;

    private float currentSpeed;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        originalHeight = controller.height;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        GroundCheck();
        HandleCrouch();
        MovementAndJump();
    }

    void GroundCheck()
    {
        // Ground check using a small sphere near the bottom of the capsule
        Vector3 origin = transform.position + Vector3.down * (controller.height / 2f);
        float sphereRadius = controller.radius * groundCheckRadius;
        isGrounded = Physics.CheckSphere(origin, sphereRadius, groundMask);

        // Reset vertical velocity when grounded
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;
    }

    void HandleCrouch()
    {
        // Toggle crouch
        if (Input.GetKeyDown(KeyCode.LeftControl))
            isCrouching = !isCrouching;

        // Smooth height transition
        float targetHeight = isCrouching ? crouchHeight : originalHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
    }

    void MovementAndJump()
    {
        // Read input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Move direction
        Vector3 move = transform.right * x + transform.forward * z;

        // Determine speed
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching;
        currentSpeed = isRunning ? runSpeed : (isCrouching ? crouchSpeed : walkSpeed);

        // Horizontal velocity
        Vector3 horizontalVelocity = move * currentSpeed;

        // Jump logic
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);
        bool jumpHeld = Input.GetKey(KeyCode.Space);
        bool justLanded = isGrounded && !wasGroundedLastFrame;

        // Normal jump (GetKeyDown) OR auto-jump when landing while holding Space
        if ((isGrounded && jumpPressed && !isCrouching) ||
            (justLanded && jumpHeld && !isCrouching))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Combine horizontal and vertical movement
        controller.Move((horizontalVelocity + Vector3.up * velocity.y) * Time.deltaTime);

        // Store grounded state for next frame
        wasGroundedLastFrame = isGrounded;
    }

    void OnDrawGizmosSelected()
    {
        if (!controller) return;

        // Visualize ground check sphere
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position + Vector3.down * (controller.height / 2f);
        Gizmos.DrawWireSphere(origin, controller.radius * groundCheckRadius);
    }
}