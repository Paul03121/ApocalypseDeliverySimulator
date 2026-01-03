using UnityEngine;

public class WeaponInteractable : Interactable
{
    [Header("Hand Follow Settings")]
    public Vector3 equippedPositionOffset;
    public Vector3 equippedRotationOffset;
    public Vector3 unequippedPositionOffset;
    public Vector3 unequippedRotationOffset;

    [Header("State")]
    public bool isBeingHeld = false;

    private WeaponHolder holder;

    protected override void OnInteract()
    {
        if (isBeingHeld)
            return;

        // Locate the WeaponHolder in the scene
        holder = FindObjectOfType<WeaponHolder>();

        if (holder == null)
        {
            Debug.LogError("WeaponHolder not found");
            return;
        }

        // Reset interaction flags to allow future interactions
        isInteracted = false;

        PickUp();
    }

    public void PickUp()
    {
        isBeingHeld = true;
        isInteractionDisabled = true;   // Disable interaction while being carried

        // TODO: Add pickup sound effect

        holder.PickUp(this);
    }

    public void Drop()
    {
        isBeingHeld = false;
        isInteractionDisabled = false;  // Re-enable interaction once dropped

        // TODO: Add drop sound effect

        holder.Drop();
    }
    public void EquipWeapon()
    {
        isBeingHeld = true;

        // TODO: Add sound effect

        holder.EquipWeapon();
    }

    public void UnequipWeapon()
    {
        isBeingHeld = false;

        // TODO: Add sound effect

        holder.UnequipWeapon();
    }
}