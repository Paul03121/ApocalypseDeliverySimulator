using UnityEngine;

public class PackageInteractable : Interactable
{
    [Header("State")]
    public bool isBeingHeld = false;        // Tracks whether the package is currently being held

    private PackageHolder holder;           // Cached reference to the PackageHolder
    private IPackageModule[] modules;       // Package modules

    protected override void Awake()
    {
        base.Awake();

        // Detect modules attached to the package and initialize them
        modules = GetComponents<IPackageModule>();
        foreach (var module in modules)
            module.Initialize(this);
    }

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

        if (!isBeingHeld)
            PickUp();
    }

    private void PickUp()
    {
        isBeingHeld = true;
        isInteractionDisabled = true;   // Disable interaction while being carried

        // TODO: Add pickup sound effect

        holder.PickUp(this);

        // Notify modules
        NotifyPickedUp();

        Debug.Log("Package picked up");
    }

    public void Drop()
    {
        isBeingHeld = false;
        isInteractionDisabled = false;  // Re-enable interaction once dropped

        // TODO: Add drop sound effect

        holder.Drop();

        // Notify modules
        NotifyDropped();

        Debug.Log("Package dropped");
    }

    public void Deliver()
    {
        isBeingHeld = false;
        isInteractionDisabled = false;  // Re-enable interaction once delivered

        // TODO: Add delivered sound effect

        holder.Deliver();

        // Notify modules
        NotifyDelivered();
    }

    public void NotifyPickedUp()
    {
        foreach (var module in modules)
            module.OnPackagePickedUp(this);
    }

    public void NotifyDropped()
    {
        foreach (var module in modules)
            module.OnPackageDropped(this);
    }

    public void NotifyDelivered()
    {
        foreach (var module in modules)
            module.OnPackageDelivered(this);
    }
}
