using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance;

    [Header("Data")]
    private readonly List<DeliveryMission> generatedMissions = new();
    private readonly List<DeliveryNPCSpawner> registeredSpawners = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Delay mission generation to allow spawner registration
        Invoke(nameof(GenerateMissions), 1f);
    }

    public void RegisterDeliveryNPCSpawner(DeliveryNPCSpawner spawner)
    {
        // Register a new NPC spawner
        registeredSpawners.Add(spawner);
    }

    public void GenerateMissions()
    {
        int max = DeliveryFlags.Instance.GetMaxDeliveries();

        // Generate missions until max count is reached
        while (generatedMissions.Count < max)
        {
            if (!GenerateSingleMission())
                break;  // Stop if mission generation fails
        }
    }

    private bool GenerateSingleMission()
    {
        Debug.Log("[Delivery Manager] Attempting to generate a new delivery mission");

        if (registeredSpawners.Count < 2)
        {
            Debug.Log("[Delivery Manager] Not enough spawners registered to generate mission");
            return false;
        }

        // Find available spawners that can act as giver or receiver
        var availableGiverSpawners = registeredSpawners.FindAll(spawner => spawner.IsSelectable && spawner.HasValidGiver());
        var availableReceiverSpawners = registeredSpawners.FindAll(spawner => spawner.IsSelectable && spawner.HasValidReceiver());

        if (availableGiverSpawners.Count == 0 || availableReceiverSpawners.Count == 0)
        {
            Debug.Log("[Delivery Manager] No available giver or receiver spawners");
            return false;
        }

        // Randomly select a giver spawner and remove it from receivers to avoid duplication
        var reservedGiverSpawner = availableGiverSpawners[Random.Range(0, availableGiverSpawners.Count)];
        availableReceiverSpawners.Remove(reservedGiverSpawner);

        if (availableReceiverSpawners.Count == 0)
        {
            Debug.Log("[Delivery Manager] No valid receiver spawner after filtering giver");
            return false;
        }

        // Randomly select a receiver spawner
        var reservedReceiverSpawner = availableReceiverSpawners[Random.Range(0, availableReceiverSpawners.Count)];

        var mission = new DeliveryMission(reservedGiverSpawner, reservedReceiverSpawner);

        // Reserve spawners for the mission
        if (!reservedGiverSpawner.Reserve(mission))
        {
            Debug.Log("[Delivery Manager] Failed to reserve giver spawner");
            return false;
        }

        if (!reservedReceiverSpawner.Reserve(mission))
        {
            Debug.Log("[Delivery Manager] Failed to reserve receiver spawner");
            reservedGiverSpawner.Release(mission);
            return false;
        }

        reservedGiverSpawner.SpawnGiver(mission);

        generatedMissions.Add(mission);
        Debug.Log($"[Delivery Manager] Mission created");
        return true;
    }

    public void ActivateMission(DeliveryMission mission, PackageInteractable package)
    {
        mission.reservedReceiverSpawner.SpawnReceiver(mission);

        mission.package = package;
        mission.Activate();
        Debug.Log("[Delivery Manager] Mission activated");
    }

    public void CompleteMission(DeliveryMission mission)
    {
        // Clear and release spawners after completion
        mission.reservedGiverSpawner.Clear();
        mission.reservedReceiverSpawner.Clear();

        mission.reservedGiverSpawner.Release(mission);
        mission.reservedReceiverSpawner.Release(mission);

        generatedMissions.Remove(mission);

        Debug.Log("[Delivery Manager] Mission completed and spawners released");

        // Try to generate new missions
        GenerateMissions();
    }
}