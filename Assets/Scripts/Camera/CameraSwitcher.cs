using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cameras to switch between")]
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;

    [Header("Input Settings")]
    public KeyCode switchKey = KeyCode.V;

    private bool isFirstPersonActive = true;
    private ThirdPersonFrontCameraController thirdPersonController;

    void Start()
    {
        // Find the third person camera controller
        thirdPersonController = thirdPersonCamera.GetComponent<ThirdPersonFrontCameraController>();
        ActivateFirstPerson(isFirstPersonActive);
    }

    void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            isFirstPersonActive = !isFirstPersonActive;
            ActivateFirstPerson(isFirstPersonActive);
        }
    }

    private void ActivateFirstPerson(bool activate)
    {
        firstPersonCamera.gameObject.SetActive(activate);
        thirdPersonCamera.gameObject.SetActive(!activate);

        // When switching to third person, reset its position
        if (!activate && thirdPersonController != null)
        {
            thirdPersonController.ResetCameraPosition();
        }
    }
}