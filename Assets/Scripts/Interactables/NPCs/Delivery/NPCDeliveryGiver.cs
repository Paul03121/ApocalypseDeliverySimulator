using UnityEngine;

public class NPCDeliveryGiver : Interactable
{
    public DeliveryFlag flag;
    public GameObject[] packagePrefabs;

    private DeliveryMission mission;

    protected override bool WaitForMessage => true;

    public void AssignMission(DeliveryMission mission)
    {
        this.mission = mission;
    }

    protected override void OnInteract()
    {
        // Choose a random package prefab and instantiate it near the giver
        var prefab = packagePrefabs[Random.Range(0, packagePrefabs.Length)];
        var package = Instantiate(prefab, transform.position + Vector3.up, Quaternion.identity).GetComponent<PackageInteractable>();

        // Ignore collision between this NPC's collider and the package's collider
        Collider npcCollider = GetComponent<Collider>();
        Collider packageCollider = package.GetComponent<Collider>();
        if (npcCollider != null && packageCollider != null)
            Physics.IgnoreCollision(npcCollider, packageCollider);

        // Activate the mission with the spawned package
        DeliveryManager.Instance.ActivateMission(mission, package);
        Destroy(gameObject);
    }
}