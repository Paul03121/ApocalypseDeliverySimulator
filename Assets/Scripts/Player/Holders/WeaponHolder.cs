using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    [Header("References")]
    public Transform holdPoint;
    public Transform unequippedHoldPoint;
    public Camera playerCamera;

    public WeaponInteractable currentWeapon { get; private set; }
    public WeaponInteractable equippedWeapon { get; private set; }

    public bool IsHoldingWeapon => currentWeapon != null;
    public bool IsWeaponEquipped => equippedWeapon != null;

    public void PickUp(WeaponInteractable weapon)
    {
        if (currentWeapon != null) return;

        currentWeapon = weapon;
        equippedWeapon = currentWeapon;

        // Attach weapon to the equipped hold point
        weapon.transform.SetParent(holdPoint);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;

        DisablePhysics(weapon);
    }

    public void Drop()
    {
        if (currentWeapon == null) return;

        WeaponInteractable w = currentWeapon;

        // Detach from player
        w.transform.SetParent(null);
        EnablePhysics(w);

        // Throw direction and force
        Vector3 throwDirection = playerCamera.transform.forward;
        float throwForce = 8f;

        var rb = w.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }

        currentWeapon = null;

        // If the dropped weapon was equipped, unequip it
        if (equippedWeapon == w)
        {
            equippedWeapon = null;
        }
    }

    public void EquipWeapon()
    {
        if (currentWeapon == null) return;

        equippedWeapon = currentWeapon;

        // Attach weapon to the equipped hold point
        currentWeapon.transform.SetParent(holdPoint);
        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;

        DisablePhysics(currentWeapon);
    }

    public void UnequipWeapon()
    {
        if (equippedWeapon == null) return;

        WeaponInteractable w = equippedWeapon;

        // Store weapon in the unequipped hold point
        if (unequippedHoldPoint != null)
        {
            w.transform.SetParent(unequippedHoldPoint);
            w.transform.localPosition = Vector3.zero;
            w.transform.localRotation = Quaternion.identity;

            DisablePhysics(w);
        }
        else
        {
            // Otherwise drop it into the world
            w.transform.SetParent(null);
            EnablePhysics(w);
        }

        equippedWeapon = null;
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
