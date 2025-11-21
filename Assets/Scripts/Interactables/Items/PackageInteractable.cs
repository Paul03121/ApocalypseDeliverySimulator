using UnityEngine;

public class PackageInteractable : Interactable
{
    [Header("State")]
    public bool isBeingHeld = false;        // Tracks whether the package is currently being held

    private PackageHolder holder;           // Cached reference to the PackageHolder

    protected override void OnInteract()
    {
        // Retrieve PackageHolder only when first needed
        if (holder == null)
            holder = FindObjectOfType<PackageHolder>();

        if (holder == null)
        {
            Debug.LogError("PackageHolder not found");
            return;
        }

        // Reset to allow repeated interactions
        isInteracted = false;

        // Pick up or drop depending on current state
        if (!isBeingHeld)
            PickUp();
        else
            Drop();
    }

    private void PickUp()
    {
        isBeingHeld = true;
        isInteractionDisabled = true;   // Disable interaction while being carried

        // TODO: Add pickup sound effect

        holder.PickUp(this);
        Debug.Log("Package picked up");
    }

    public void Drop()
    {
        isBeingHeld = false;
        isInteractionDisabled = false;  // Re-enable interaction once dropped

        // TODO: Add drop sound effect

        holder.Drop();
        Debug.Log("Package dropped");
    }
}
