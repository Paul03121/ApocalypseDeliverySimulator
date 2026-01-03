using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [Header("References")]
    public Transform equippedHoldPoint;
    public Transform unequippedHoldPoint;
    public Transform rightHandTransform;
    public Camera playerCamera;

    [Header("Hand Follow Settings")]
    [SerializeField] private float followSpeed = 25f;

    public WeaponInteractable CurrentWeapon { get; private set; }
    public WeaponInteractable EquippedWeapon { get; private set; }

    public bool IsHoldingWeapon => CurrentWeapon != null;
    public bool IsWeaponEquipped => EquippedWeapon != null;

    private void LateUpdate()
    {
        if (IsWeaponEquipped && EquippedWeapon != null && rightHandTransform != null)
        {
            // Get weapon's equipped offsets
            Vector3 positionOffset = EquippedWeapon.equippedPositionOffset;
            Vector3 rotationOffset = EquippedWeapon.equippedRotationOffset;

            // Calculate target position
            Vector3 targetPosition = rightHandTransform.position +
                transform.right * positionOffset.x +
                transform.up * positionOffset.y +
                transform.forward * positionOffset.z;

            // Move weapon to target position
            EquippedWeapon.transform.position = Vector3.Lerp(EquippedWeapon.transform.position, targetPosition, Time.deltaTime * followSpeed);

            // Calculate target rotation
            Quaternion offsetRotation = Quaternion.Euler(rotationOffset);
            Quaternion targetRotation = rightHandTransform.rotation * offsetRotation;

            // Rotate weapon to target position
            EquippedWeapon.transform.rotation = Quaternion.Slerp(EquippedWeapon.transform.rotation, targetRotation, Time.deltaTime * followSpeed);
        }
        else if (CurrentWeapon != null && !IsWeaponEquipped)
        {
            // Get weapon's unequipped offsets
            Vector3 positionOffset = CurrentWeapon.unequippedPositionOffset;
            Vector3 rotationOffset = CurrentWeapon.unequippedRotationOffset;

            // Calculate target position
            Vector3 targetPosition = unequippedHoldPoint.position +
                unequippedHoldPoint.right * positionOffset.x +
                unequippedHoldPoint.up * positionOffset.y +
                unequippedHoldPoint.forward * positionOffset.z;

            // Move weapon to target position
            CurrentWeapon.transform.position = Vector3.Lerp(CurrentWeapon.transform.position, targetPosition, Time.deltaTime * followSpeed);

            // Calculate target rotation
            Quaternion baseRotation = unequippedHoldPoint.rotation;

            Quaternion rotX = Quaternion.AngleAxis(rotationOffset.x, Vector3.right);
            Quaternion rotY = Quaternion.AngleAxis(rotationOffset.y, Vector3.up);
            Quaternion rotZ = Quaternion.AngleAxis(rotationOffset.z, Vector3.forward);

            Quaternion targetRotation = baseRotation * rotX * rotY * rotZ;

            // Rotate weapon to target position
            CurrentWeapon.transform.rotation = Quaternion.Slerp(CurrentWeapon.transform.rotation, targetRotation, Time.deltaTime * followSpeed);
        }
    }

    public void PickUp(WeaponInteractable weapon)
    {
        if (CurrentWeapon != null) return;

        CurrentWeapon = weapon;
        EquippedWeapon = CurrentWeapon;

        // Attach weapon to the equipped hold point
        weapon.transform.SetParent(equippedHoldPoint);
        weapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        DisablePhysics(weapon);
    }

    public void Drop()
    {
        if (CurrentWeapon == null) return;

        WeaponInteractable weapon = CurrentWeapon;

        // Detach from player
        weapon.transform.SetParent(null);
        EnablePhysics(weapon);

        // Throw direction and force
        Vector3 throwDirection = playerCamera.transform.forward;
        float throwForce = 8f;

        var rb = weapon.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }

        CurrentWeapon = null;

        // If the dropped weapon was equipped, unequip it
        if (EquippedWeapon == weapon)
        {
            EquippedWeapon = null;
        }
    }

    public void EquipWeapon()
    {
        if (CurrentWeapon == null) return;

        EquippedWeapon = CurrentWeapon;

        // Attach weapon to the equipped hold point
        CurrentWeapon.transform.SetParent(equippedHoldPoint);
        CurrentWeapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        DisablePhysics(CurrentWeapon);
    }

    public void UnequipWeapon()
    {
        if (EquippedWeapon == null) return;

        WeaponInteractable weapon = EquippedWeapon;

        // Store weapon in the unequipped hold point
        if (unequippedHoldPoint != null)
        {
            weapon.transform.SetParent(unequippedHoldPoint);
            weapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            DisablePhysics(weapon);
        }
        else
        {
            // Otherwise drop it into the world
            weapon.transform.SetParent(null);
            EnablePhysics(weapon);
        }

        EquippedWeapon = null;
    }

    private void DisablePhysics(WeaponInteractable weapon)
    {
        // Freeze rigidbody and disable gravity while held
        var rb = weapon.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        // Disable collider
        var col = weapon.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
    }

    private void EnablePhysics(WeaponInteractable weapon)
    {
        // Restore rigidbody physics
        var rb = weapon.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
        }

        // Re-enable collider
        var col = weapon.GetComponent<Collider>();
        if (col != null)
            col.enabled = true;
    }
}
