using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("References")]
    public InteractableIconController iconController;

    [Header("Interaction Messages")]
    [TextArea(2, 4)]
    [SerializeField] protected List<string> interactionMessages = new();

    protected bool isInteracted = false;           // Prevents multiple rapid interactions
    protected bool isInteractionDisabled = false;  // Determines whether interaction is allowed

    public bool IsInteractionDisabled => isInteractionDisabled;

    // Define if an interactable must wait for message before interaction
    protected virtual bool WaitForMessage => false;

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

        if (WaitForMessage)
        {
            // Coroutine to wait for the message
            StartCoroutine(InteractionRoutine());
        }
        else
        {
            // Continue interaction without waiting
            TryShowInteractionMessage();

            OnInteract();
            isInteracted = false;
        }
    }

    private IEnumerator InteractionRoutine()
    {
        TryShowInteractionMessage();

        // Wait for the message to finish
        while (MessageUIManager.Instance.IsShowingMessage)
            yield return null;

        OnInteract();

        isInteracted = false;
    }

    protected void TryShowInteractionMessage()
    {
        if (interactionMessages == null || interactionMessages.Count == 0)
            return;

        // Show a random message from the messages list
        string message = interactionMessages[Random.Range(0, interactionMessages.Count)];
        MessageUIManager.Instance.ShowMessage(message);
    }

    protected abstract void OnInteract(); // Implemented by derived classes
}
