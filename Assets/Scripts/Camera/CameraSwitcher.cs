using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cameras to switch between")]
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;

    [Header("Player Model (mesh root)")]
    public GameObject playerBodyModel;

    [Header("Input Settings")]
    public KeyCode switchKey = KeyCode.V;

    private bool isFirstPersonActive = true;
    private ThirdPersonFrontCameraController thirdPersonController;

    void Start()
    {
        // Find the third person camera controller
        thirdPersonController = thirdPersonCamera.GetComponent<ThirdPersonFrontCameraController>();

        // Initialize view state
        ActivateFirstPerson(isFirstPersonActive);
    }

    void Update()
    {
        // Stop working if game is paused or if player died
        if (GameStateManager.Instance.IsPaused || GameStateManager.Instance.IsGameOver)
            return;

        if (Input.GetKeyDown(switchKey))
        {
            isFirstPersonActive = !isFirstPersonActive;
            ActivateFirstPerson(isFirstPersonActive);
        }
    }

    private void ActivateFirstPerson(bool activate)
    {
        // Enable/disable cameras
        firstPersonCamera.gameObject.SetActive(activate);
        thirdPersonCamera.gameObject.SetActive(!activate);

        // Hide player model in first-person view
        if (playerBodyModel != null)
            playerBodyModel.SetActive(!activate);

        // Reset third-person camera position when switching to third person
        if (!activate && thirdPersonController != null)
        {
            thirdPersonController.ResetCameraPosition();
        }
    }
}