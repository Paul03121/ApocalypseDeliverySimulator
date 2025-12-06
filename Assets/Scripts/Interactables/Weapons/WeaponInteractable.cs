using UnityEngine;

public class WeaponInteractable : Interactable
{
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

        // Pick up the weapon
        PickUp();
    }

    public void PickUp()
    {
        isBeingHeld = true;
        isInteractionDisabled = true;   // Disable interaction while being carried

        // TODO: Add pickup sound effect

        holder.PickUp(this);
        Debug.Log("Weapon picked up");
    }

    public void Drop()
    {
        isBeingHeld = false;
        isInteractionDisabled = false;  // Re-enable interaction once dropped

        // TODO: Add drop sound effect

        holder.Drop();
        Debug.Log("Weapon dropped");
    }
    public void EquipWeapon()
    {
        isBeingHeld = true;

        // TODO: Add sound effect

        holder.EquipWeapon();
        Debug.Log("Weapon equipped");
    }

    public void UnequipWeapon()
    {
        isBeingHeld = false;

        // TODO: Add sound effect

        holder.UnequipWeapon();
        Debug.Log("Weapon unequipped");
    }
}