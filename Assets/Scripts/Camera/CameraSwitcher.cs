using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cameras to switch between")]
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;

    [Header("Input Settings")]
    public KeyCode switchKey = KeyCode.V;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    private bool isFirstPersonActive = true;
    private ThirdPersonFrontCameraController thirdPersonController;

    private bool isBlocked = false;

    void Start()
    {
        // Find the third person camera controller
        thirdPersonController = thirdPersonCamera.GetComponent<ThirdPersonFrontCameraController>();

        // Initialize view state
        ActivateFirstPerson(isFirstPersonActive);
    }

    void Update()
    {
        // Block if player is dead
        if (isBlocked) return;

        // Stop working if game is not in Gameplay mode
        if (!GameStateManager.Instance.IsGameplay)
            return;

        if (Input.GetKeyDown(switchKey))
        {
            isFirstPersonActive = !isFirstPersonActive;
            ActivateFirstPerson(isFirstPersonActive);
        }
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
        // Block switcher
        isBlocked = true;

        // Force third person camera for death view
        thirdPersonController.EnterDeathView();
        ActivateFirstPerson(false);
    }

    private void HandlePlayerDeathEnded()
    {
        // Unblock switcher
        isBlocked = false;

        // Exit from death view for third person camera
        thirdPersonController.ExitDeathView();
    }

    private void ActivateFirstPerson(bool activate)
    {
        // Enable/disable cameras
        firstPersonCamera.gameObject.SetActive(activate);
        thirdPersonCamera.gameObject.SetActive(!activate);

        // Reset third-person camera position when switching to third person
        if (!activate && thirdPersonController != null)
            thirdPersonController.ResetCameraPosition();
    }
}