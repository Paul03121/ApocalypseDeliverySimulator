using UnityEngine;

// Handles package delivery when interacting with a delivery spot
public class DeliverySpotInteractable : Interactable
{
    protected override void OnInteract()
    {
        // Reset interaction lock so this interactable can be triggered again
        isInteracted = false;

        // Retrieve the PlayerInteraction component
        PlayerInteraction player = FindObjectOfType<PlayerInteraction>();
        if (player == null)
        {
            Debug.LogError("PlayerInteraction not found");
            return;
        }

        // Retrieve the package the player is currently carrying
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

            Debug.Log("Package delivered at delivery spot");
        }
        else
        {
            Debug.LogWarning("Player is not holding a valid package");
        }
    }
}