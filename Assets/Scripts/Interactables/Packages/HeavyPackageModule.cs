using UnityEngine;

public class HeavyPackageModule : MonoBehaviour, IPackageModule
{
    private PlayerMovement playerMovement;

    public void Initialize(PackageInteractable package)
    {
        // Retrieve PlayerMovement from the player
        playerMovement = FindObjectOfType<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogError("[HeavyModule] PlayerMovement not found.");
        }
    }

    public void OnPackagePickedUp(PackageInteractable package)
    {
        BlockMovement();
    }

    public void OnPackageDropped(PackageInteractable package)
    {
        UnblockMovement();
    }

    public void OnPackageDelivered(PackageInteractable package)
    {
        UnblockMovement();
    }

    private void BlockMovement()
    {
        if (playerMovement == null)
            return;

        // Blocks player abilities that should not be available while carrying a heavy package
        playerMovement.runBlocked = true;
        playerMovement.jumpBlocked = true;
        playerMovement.crouchBlocked = true;

        Debug.Log("[HeavyModule] Player movement abilities blocked due to heavy package.");
    }

    private void UnblockMovement()
    {
        if (playerMovement == null)
            return;

        // Restores all previously blocked movement abilities
        playerMovement.runBlocked = false;
        playerMovement.jumpBlocked = false;
        playerMovement.crouchBlocked = false;

        Debug.Log("[HeavyModule] Player movement abilities restored.");
    }
}