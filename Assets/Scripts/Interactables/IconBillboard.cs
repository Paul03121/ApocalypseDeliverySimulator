using UnityEngine;

public class IconBillboard : MonoBehaviour
{
    public Vector3 worldOffset = new Vector3(0f, 0.5f, 0f);

    private Transform parent; 
    private Camera mainCamera;

    private void Start()
    {
        parent = transform.parent; 
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        ApplyOffsetAndRotation();
        ApplyBillboard();
    }

    private void ApplyOffsetAndRotation()
    {
        if (parent == null)
            return;

        // Keep world-space offset (ignore parent's local rotation)
        transform.position = parent.position + worldOffset;

        // Force fixed world rotation
        transform.rotation = Quaternion.identity;
    }

    private void ApplyBillboard()
    {
        if (mainCamera == null)
            return;

        // Align forward vector with the camera
        transform.forward = mainCamera.transform.forward;
    }
}