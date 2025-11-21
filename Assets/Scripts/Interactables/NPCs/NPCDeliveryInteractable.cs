using UnityEngine;

// Handles delivering a package when interacting with an NPC
public class NPCDeliveryInteractable : Interactable
{
    protected override void OnInteract()
    {
        // Reset interaction lock for this object
        isInteracted = false;

        // TODO: Add NPC interaction

        // Retrieve the PlayerInteraction component
        PlayerInteraction player = FindObjectOfType<PlayerInteraction>();
        if (player == null)
        {
            Debug.LogError("PlayerInteraction not found");
            return;
        }

        // Check if the player is carrying a package
        var carriedPackage = player.GetCarriedPackage();
        if (carriedPackage == null)
        {
            Debug.Log("You don't have a package to deliver");
            return;
        }

        // Deliver the package
        PackageHolder holder = FindObjectOfType<PackageHolder>();
        if (holder != null && holder.IsHoldingPackage)
        {
            holder.Deliver();
            player.ClearCarriedPackage();

            Debug.Log("Package delivered to NPC");
        }
        else
        {
            Debug.LogWarning("Player is not holding a valid package");
        }
    }
}