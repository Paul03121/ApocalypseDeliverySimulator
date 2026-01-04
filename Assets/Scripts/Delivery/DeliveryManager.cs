using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance;

    public static event System.Action OnDeliveryCompleted;

    [Header("Data")]
    private readonly List<DeliveryMission> generatedMissions = new();
    private readonly List<DeliveryNPCSpawner> registeredSpawners = new();

    public List<DeliveryMission> GeneratedMissions => generatedMissions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterDeliveryNPCSpawner(DeliveryNPCSpawner spawner)
    {
        // Register a new NPC spawner
        if (!registeredSpawners.Contains(spawner))
            registeredSpawners.Add(spawner);
    }

    public void GenerateMissions()
    {
        int max = DeliveryFlags.Instance.GetMaxDeliveries();

        // Generate missions until max count is reached
        while (generatedMissions.Count < max)
        {
            // Stop if mission generation fails
            if (!GenerateSingleMission())
                break;
        }
    }

    private bool GenerateSingleMission()
    {
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

        // Select a giver spawner
        var reservedGiverSpawner = SelectGiverSpawner(availableGiverSpawners, availableReceiverSpawners);

        if (reservedGiverSpawner == null)
        {
            Debug.Log("[Delivery Manager] Only one receiver and no alternative giver available");
            return false;
        }

        // Remove giver from possible receivers to avoid duplication
        availableReceiverSpawners.Remove(reservedGiverSpawner);

        // Randomly select a receiver spawner
        var reservedReceiverSpawner = availableReceiverSpawners[Random.Range(0, availableReceiverSpawners.Count)];

        // Create mission
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

        // Add icons to the map
        MapUIManager.Instance.RegisterGiverGenerated(mission, reservedGiverSpawner.transform);
        MapUIManager.Instance.RegisterReceiverGenerated(mission, reservedReceiverSpawner.transform);

        return true;
    }

    public bool GenerateForcedMission(DeliveryFlag forcedFlag)
    {
        if (registeredSpawners.Count < 2)
        {
            Debug.Log("[DeliveryManager] Not enough spawners for forced mission");
            return false;
        }

        // Find valid giver spawners with same flag
        var availableGiverSpawners = new List<DeliveryNPCSpawner>();

        foreach (var spawner in registeredSpawners)
        {
            if (!spawner.IsSelectable)
                continue;

            if (spawner.HasGiverForFlag(forcedFlag))
                availableGiverSpawners.Add(spawner);
        }

        if (availableGiverSpawners.Count == 0)
        {
            Debug.Log($"[DeliveryManager] No giver found for forced flag {forcedFlag}");
            return false;
        }

        // Pick random giver
        var reservedGiverSpawner = availableGiverSpawners[Random.Range(0, availableGiverSpawners.Count)];

        // Find valid receiver spawners with same flag
        var availableReceiverSpawners = new List<DeliveryNPCSpawner>();

        foreach (var spawner in registeredSpawners)
        {
            if (spawner == reservedGiverSpawner || !spawner.IsSelectable)
                continue;

            if (spawner.HasReceiverForFlag(forcedFlag))
                availableReceiverSpawners.Add(spawner);
        }

        if (availableReceiverSpawners.Count == 0)
        {
            Debug.Log($"[DeliveryManager] No receiver found for forced flag {forcedFlag}");
            return false;
        }

        // Pick random receiver
        var reservedReceiverSpawner = availableReceiverSpawners[Random.Range(0, availableReceiverSpawners.Count)];

        // Create mission
        var mission = new DeliveryMission(reservedGiverSpawner, reservedReceiverSpawner);

        // Reserve spawners for the mission
        if (!reservedGiverSpawner.Reserve(mission))
        {
            Debug.Log("[Delivery Manager] Failed to reserve giver spawner for forced mission");
            return false;
        }

        if (!reservedReceiverSpawner.Reserve(mission))
        {
            Debug.Log("[Delivery Manager] Failed to reserve receiver spawner for forced mission");
            reservedGiverSpawner.Release(mission);
            return false;
        }

        // Force giver spawner
        mission.MarkAsForced();
        reservedGiverSpawner.ForceSpawnGiver(mission, forcedFlag);

        generatedMissions.Add(mission);

        // Add icons to the map
        MapUIManager.Instance.RegisterGiverGenerated(mission, reservedGiverSpawner.transform);
        MapUIManager.Instance.RegisterReceiverGenerated(mission, reservedReceiverSpawner.transform);

        return true;
    }

    private DeliveryNPCSpawner SelectGiverSpawner(List<DeliveryNPCSpawner> availableGiverSpawners, List<DeliveryNPCSpawner> availableReceiverSpawners)
    {
        // Special case: Avoid blocking the only possible receiver
        if (availableReceiverSpawners.Count == 1)
        {
            var lastReceiver = availableReceiverSpawners[0];

            if (availableGiverSpawners.Contains(lastReceiver))
            {
                // Choose a giver different from receiver
                return availableGiverSpawners.Find(spawner => spawner != lastReceiver);
            }
        }

        // Default case: Randomly select a giver spawner
        return availableGiverSpawners[Random.Range(0, availableGiverSpawners.Count)];
    }

    public void ActivateMission(DeliveryMission mission, PackageInteractable package)
    {
        if (mission.IsForced)
        {
            // Force spawning receiver
            mission.reservedReceiverSpawner.ForceSpawnReceiver(mission);
        }
        else
        {
            // Spawn a regular receiver
            mission.reservedReceiverSpawner.SpawnReceiver(mission);
        }

        mission.package = package;
        mission.Activate();

        // Update map icons
        MapUIManager.Instance.SetGiverInactive(mission, mission.reservedGiverSpawner.transform);
        MapUIManager.Instance.SetReceiverActive(mission, mission.reservedReceiverSpawner.transform);
    }

    public void CompleteMission(DeliveryMission mission)
    {
        // Mark mission as completed
        mission.Complete();

        // Show results UI
        DeliveryResultUIManager.Instance.Show(mission);

        // Clear and release spawners after completion
        mission.reservedGiverSpawner.Clear();
        mission.reservedReceiverSpawner.Clear();

        mission.reservedGiverSpawner.Release(mission);
        mission.reservedReceiverSpawner.Release(mission);

        // Remove map icons
        MapUIManager.Instance.UnregisterGiver(mission);
        MapUIManager.Instance.UnregisterReceiver(mission);

        generatedMissions.Remove(mission);

        // Notify listeners
        OnDeliveryCompleted?.Invoke();
    }
}