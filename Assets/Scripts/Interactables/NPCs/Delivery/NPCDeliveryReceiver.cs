using UnityEngine;

public class NPCDeliveryReceiver : Interactable
{
    public DeliveryFlag flag;

    private DeliveryMission mission;

    public void AssignMission(DeliveryMission mission)
    {
        this.mission = mission;
    }

    protected override void OnInteract()
    {
        isInteracted = false;

        // Get player interaction component and package being carried
        var player = FindObjectOfType<PlayerInteraction>();
        var carried = player.GetCarriedPackage();

        // Only proceed if player carries the package assigned to the mission
        if (carried != mission.package)
            return;

        // Deliver the package
        carried.Deliver();
        player.ClearCarriedPackage();

        // Complete the mission
        DeliveryManager.Instance.CompleteMission(mission);

        Destroy(gameObject);
    }
}
