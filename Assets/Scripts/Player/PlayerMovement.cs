using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float crouchSpeed = 2f;
    public float jumpHeight = 2f;
    public float gravity = -24f;

    [Header("Crouch Settings")]
    public float crouchHeight = 0.99f;
    private float originalHeight;
    public float crouchTransitionSpeed = 20f;

    [Header("Ground & Ceiling Check Settings")]
    public float groundCheckDistance = 0.15f;
    public float ceilingCheckDistance = 0.1f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isCrouching = false;
    private bool isGrounded;
    private float currentSpeed;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        originalHeight = controller.height;
    }

    void Update()
    {
        HandleMovement();
        HandleCrouch();
        HandleJump();
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Calculate movement direction
        Vector3 move = transform.right * x + transform.forward * z;

        // Determine current speed
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching;
        currentSpeed = isRunning ? runSpeed : (isCrouching ? crouchSpeed : walkSpeed);

        controller.Move(currentSpeed * Time.deltaTime * move);
    }

    void HandleCrouch()
    {
        // Detect attempt to toggle crouch state
        if (Input.GetKeyDown(KeyCode.LeftControl))
            isCrouching = !isCrouching;

        // Prevent standing if ceiling above while grounded
        if (!isCrouching && isGrounded)
        {
            Vector3 rayOriginAbove = transform.position + Vector3.up * (controller.height / 2);
            bool isBlockedAbove = Physics.Raycast(rayOriginAbove, Vector3.up, ceilingCheckDistance);

            if (isBlockedAbove)
            {
                isCrouching = true;
            }
        }

        // Smooth height transition
        float targetHeight = isCrouching ? crouchHeight : originalHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
    }

    void HandleJump()
    {
        // Ground check
        Vector3 rayOrigin = transform.position + Vector3.down * (controller.height / 2 - 0.1f);
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance);

        // Ceiling check
        Vector3 rayOriginAbove = transform.position + Vector3.up * (controller.height / 2);
        bool isBlockedAbove = Physics.Raycast(rayOriginAbove, Vector3.up, ceilingCheckDistance);

        // Reset vertical velocity when grounded
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // Cancel upward velocity if hitting a ceiling
        if (isBlockedAbove && velocity.y > 0f)
            velocity.y = 0f;

        // Jump
        if (Input.GetKey(KeyCode.Space) && isGrounded && !isCrouching)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Vertical movement
        controller.Move(Time.deltaTime * velocity.y * Vector3.up);
    }
}