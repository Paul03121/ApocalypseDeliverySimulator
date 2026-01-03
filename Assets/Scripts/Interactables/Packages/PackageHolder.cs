using UnityEngine;

public class PackageHolder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform holdPoint;           // Position where the carried package will be attached
    [SerializeField] private Transform rightHandTransform;  // For tracking right hand position
    [SerializeField] private Camera playerCamera;           // Used to determine forward direction when dropping a package

    [Header("Hand Follow Settings")]
    [SerializeField] private Vector3 positionOffset;        // Position adjustment
    [SerializeField] private float followSpeed = 25f;       // Speed for smoother tracking

    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 8f;
    
    // Currently held package (read-only from outside)
    public PackageInteractable CurrentPackage { get; private set; }

    // Convenience property to check if the player is holding a package
    public bool IsHoldingPackage => CurrentPackage != null;

    private void LateUpdate()
    {
        if (!IsHoldingPackage || rightHandTransform == null)
            return;

        // Calculate target position
        Vector3 targetPosition = rightHandTransform.position +
            transform.right * positionOffset.x +
            transform.up * positionOffset.y +
            transform.forward * positionOffset.z;

        // Move to target position
        holdPoint.position = Vector3.Lerp(holdPoint.position, targetPosition, Time.deltaTime * followSpeed);
    }

    // Attaches a package to the player's hold point and disables physics
    public void PickUp(PackageInteractable package)
    {
        if (CurrentPackage != null) return;  // Already holding a package

        CurrentPackage = package;

        // Re-parent package to the hold point
        package.transform.SetParent(holdPoint);
        package.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        // Disable physics on the held package
        var rb = package.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Freeze motion entirely while still allowing collisions
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    // Releases the package and applies a forward impulse to simulate a throw/drop
    public void Drop()
    {
        if (CurrentPackage == null) return;

        // Detach package from player
        CurrentPackage.transform.SetParent(null);

        // Re-enable physics
        var rb = CurrentPackage.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;

            // Apply a small forward force when dropping
            Vector3 throwDirection = playerCamera.transform.forward;
            rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }

        CurrentPackage = null;
    }

    // Delivers the package and destroys it from the scene
    public void Deliver()
    {
        if (CurrentPackage == null) return;

        var pkg = CurrentPackage;
        CurrentPackage = null;

        // Destroy the delivered package
        GameObject.Destroy(pkg.gameObject);
    }
}