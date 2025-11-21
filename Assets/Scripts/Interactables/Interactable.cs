using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("References")]
    public InteractableIconController iconController;

    protected bool isInteracted = false;           // Prevents multiple rapid interactions
    protected bool isInteractionDisabled = false;  // Determines whether interaction is allowed

    public bool IsInteractionDisabled => isInteractionDisabled;

    protected virtual void Awake()
    {
        // Automatically locate the icon controller in children (if present)
        iconController = GetComponentInChildren<InteractableIconController>(true);

        // Hide icons at startup
        if (iconController != null)
            iconController.HideAll();
    }

    public virtual void OnProximity()
    {
        // Display the proximity icon when the player is nearby
        if (iconController != null)
            iconController.ShowProximityIcon();
    }

    public virtual void OnBecomeInteractable()
    {
        // Display the interaction icon when the object is within range
        if (iconController != null)
            iconController.ShowInteractIcon();
    }

    public virtual void OnLoseFocus()
    {
        // Hide all icons when the player is no longer focused on this object
        if (iconController != null)
            iconController.HideAll();
    }

    public void Interact()
    {
        if (isInteracted) return; // Avoid double-triggering
        isInteracted = true;

        // Always hide icons when the interaction begins
        if (iconController != null)
            iconController.HideAll();

        OnInteract();
    }

    protected abstract void OnInteract(); // Implemented by derived classes
}
