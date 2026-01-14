using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4.75f;
    public float runSpeed = 6.5f;
    public float crouchSpeed = 2f;
    public float jumpHeight = 0.8f;
    public float gravity = -18f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1.4f;
    public float crouchTransitionSpeed = 20f;

    [Header("Movement Abilities Controllers")]
    public bool runBlocked = false;
    public bool jumpBlocked = false;
    public bool crouchBlocked = false;

    [Header("Death Model Drop")]
    [SerializeField] private float deathYPosition = -0.85f;

    [Header("Speed Modifiers")]
    private float speedBonus = 0f;

    [Header("References")]
    private PlayerHealth playerHealth;
    private FirstPersonCameraController firstPersonCamera;
    private CharacterController controller;
    [SerializeField] private Animator fpAnimator;
    [SerializeField] private Animator tpAnimator;

    private Vector3 velocity;
    private float originalHeight;
    private float originalCenterY;
    private bool isMoving;
    private bool isCrouching = false;
    private bool wasGroundedLastFrame = false;

    [Header("Attack Rotation")]
    private bool isAttackRotationActive = false;
    [SerializeField] private float attackAngle = -120f;

    private float currentSpeed;

    private bool isBlocked = false;

    public float WalkSpeed => walkSpeed + speedBonus;
    public float RunSpeed => runSpeed + speedBonus;
    public float CrouchSpeed => crouchSpeed + speedBonus;

    public bool IsMoving => isMoving;

    void Awake()
    {
        // Search components needed
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
            Debug.LogError("PlayerHealth component not found on Player");

        firstPersonCamera = GetComponentInChildren<FirstPersonCameraController>();
        if (firstPersonCamera == null)
            Debug.LogError("FirstPersonCameraController component not found on Player");

        controller = GetComponent<CharacterController>();
        if (controller == null)
            Debug.LogError("CharacterController component not found on Player");

        originalHeight = controller.height;
        originalCenterY = controller.center.y;
    }

    void Start()
    {
        MapIconManager.Instance.RegisterIcon(this, MapIconType.Player, transform);
    }

    void Update()
    {
        // Block if player is dead
        if (isBlocked) return;

        // Stop working if game is paused or if player died
        if (GameStateManager.Instance.IsPaused || GameStateManager.Instance.IsGameOver)
            return;

        HandleCrouch();
        MovementAndJump();

        // Rotate model to match animation
        HandleAttackRotation();
    }

    void OnEnable()
    {
        playerHealth.OnPlayerDeathStarted += HandlePlayerDeathStarted;
        playerHealth.OnPlayerDeathEnded += HandlePlayerDeathEnded;
    }

    void OnDisable()
    {
        playerHealth.OnPlayerDeathStarted -= HandlePlayerDeathStarted;
        playerHealth.OnPlayerDeathEnded -= HandlePlayerDeathEnded;
    }

    private void HandlePlayerDeathStarted()
    {
        isBlocked = true;
    }

    private void HandlePlayerDeathEnded()
    {
        isBlocked = false;
    }

    void HandleCrouch()
    {
        // Toggle crouch
        if (!crouchBlocked && Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;

            // Notify the crouching state to the camera
            firstPersonCamera.SetCrouchingState(isCrouching);

            // Notify animator
            fpAnimator.SetBool("isCrouching", isCrouching);
            tpAnimator.SetBool("isCrouching", isCrouching);
        }

        // Set target height and center based on crouch state
        float targetHeight = isCrouching ? crouchHeight : originalHeight;
        float targetCenterY = isCrouching ? targetHeight / 2f : originalCenterY;

        // Smoothly interpolate controller height
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        // Smoothly interpolate controller center Y
        Vector3 center = controller.center;
        center.y = Mathf.Lerp(controller.center.y, targetCenterY, Time.deltaTime * crouchTransitionSpeed);
        controller.center = center;

        // Handle model rotation while crouching to match animation
        HandleCrouchRotation();
    }

    void MovementAndJump()
    {
        // Read input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Move direction
        Vector3 move = transform.right * x + transform.forward * z;

        // Check movement input
        float inputMagnitude = new Vector3(x, 0f, z).magnitude;
        isMoving = inputMagnitude > 0.01f;

        // Determine speed
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && isMoving && !isCrouching && !runBlocked;
        currentSpeed = isRunning ? RunSpeed : (isCrouching ? CrouchSpeed : WalkSpeed);

        // Horizontal velocity
        Vector3 horizontalVelocity = move * currentSpeed;

        // Notify the running state to the camera
        firstPersonCamera.SetRunningState(isRunning);

        // Notify animator
        float animSpeed = 0f;

        if (isMoving)
        {
            if (isRunning)
                animSpeed = 1f;     // Run
            else
                animSpeed = 0.5f;   // Walk
        }
        fpAnimator.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);
        tpAnimator.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);

        // Jump logic
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);
        bool jumpHeld = Input.GetKey(KeyCode.Space);
        bool justLanded = controller.isGrounded && !wasGroundedLastFrame;

        // Normal jump (GetKeyDown) OR auto-jump when landing while holding Space
        if ((!jumpBlocked && controller.isGrounded && jumpPressed && !isCrouching) ||
            (!jumpBlocked && justLanded && jumpHeld && !isCrouching))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // Notify animator
            fpAnimator.SetTrigger("JumpTrigger");
            tpAnimator.SetTrigger("JumpTrigger");

            // Force animation reboot for multiple jumps
            fpAnimator.Play("Jump", -1, 0f);
            tpAnimator.Play("Jump", -1, 0f);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Combine horizontal and vertical movement
        controller.Move((horizontalVelocity + Vector3.up * velocity.y) * Time.deltaTime);

        // Store grounded state for next frame
        wasGroundedLastFrame = controller.isGrounded;
    }

    public void AddSpeedBonus(float amount)
    {
        speedBonus += amount;
    }

    public void RemoveSpeedBonus(float amount)
    {
        speedBonus -= amount;
        speedBonus = Mathf.Max(0, speedBonus);
    }

    private void HandleCrouchRotation()
    {
        if (!isCrouching)
        {
            // Reset rotation when not crouching
            fpAnimator.transform.localRotation = Quaternion.Lerp(fpAnimator.transform.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            return;
        }

        // Get current speed from animator
        float speedParam = fpAnimator.GetFloat("Speed");

        float normalizedSpeed = Mathf.Clamp01(speedParam * 2f);
        float targetYRotation = Mathf.Lerp(15f, 35f, normalizedSpeed);
        Quaternion targetRotation = Quaternion.Euler(0f, targetYRotation, 0f);

        // Rotate model
        fpAnimator.transform.localRotation = Quaternion.Lerp(fpAnimator.transform.localRotation, targetRotation, Time.deltaTime * 10f);
    }

    private void HandleAttackRotation()
    {
        // Set target rotation
        Quaternion targetRotation = isAttackRotationActive ? Quaternion.Euler(0f, attackAngle, 0f) : Quaternion.identity;

        // Rotate model
        fpAnimator.transform.localRotation = Quaternion.Lerp(fpAnimator.transform.localRotation, targetRotation, Time.deltaTime * 10f);
    }

    public void StartAttackRotation()
    {
        isAttackRotationActive = true;
    }

    public void StopAttackRotation()
    {
        isAttackRotationActive = false;
    }

    public void StartDeathDrop()
    {
        StartCoroutine(DeathModelDrop());
    }

    IEnumerator DeathModelDrop()
    {
        Transform model = tpAnimator.transform;

        Vector3 startPos = model.localPosition;
        Vector3 endPos = new(startPos.x, deathYPosition, startPos.z);

        float duration = 1.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            model.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        model.localPosition = endPos;
    }
}