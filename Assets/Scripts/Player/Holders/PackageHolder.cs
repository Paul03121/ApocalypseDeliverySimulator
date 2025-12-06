using UnityEngine;

public class PackageHolder : MonoBehaviour
{
    [Header("References")]
    public Transform holdPoint;             // Position where the carried package will be attached
    public Camera playerCamera;             // Used to determine forward direction when dropping a package

    // Currently held package (read-only from outside)
    public PackageInteractable currentPackage { get; private set; }

    // Convenience property to check if the player is holding a package
    public bool IsHoldingPackage => currentPackage != null;

    // Attaches a package to the player's hold point and disables physics
    public void PickUp(PackageInteractable package)
    {
        if (currentPackage != null) return;  // Already holding a package

        currentPackage = package;

        // Re-parent package to the hold point
        package.transform.SetParent(holdPoint);
        package.transform.localPosition = Vector3.zero;
        package.transform.localRotation = Quaternion.identity;

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
        if (currentPackage == null) return;

        // Detach package from player
        currentPackage.transform.SetParent(null);

        // Re-enable physics
        var rb = currentPackage.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;

            // Apply a small forward force when dropping
            Vector3 throwDirection = playerCamera.transform.forward;
            float throwForce = 8f;
            rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }

        currentPackage = null;
    }

    // Delivers the package and destroys it from the scene
    public void Deliver()
    {
        if (currentPackage == null) return;

        var pkg = currentPackage;
        currentPackage = null;

        // Destroy the delivered package
        GameObject.Destroy(pkg.gameObject);
    }
}