using System.Collections.Generic;
using UnityEngine;

public class DeliveryNPCSpawner : MonoBehaviour
{
    public DeliveryFlag minimumFlag;

    [Header("Prefabs for NPCs")]
    public List<NPCDeliveryGiver> giverPrefabs;
    public List<NPCDeliveryReceiver> receiverPrefabs;

    private DeliveryMission reservedMission;

    protected GameObject activeNPC;

    public bool IsReserved { get; private set; }
    public bool IsActive => DeliveryFlags.Instance.currentFlag >= minimumFlag;
    public bool IsSelectable => !IsReserved && IsActive && activeNPC == null;

    private void Start()
    {
        // Register this spawner with the DeliveryManager
        DeliveryManager.Instance.RegisterDeliveryNPCSpawner(this);
    }

    // Attempt to reserve this spawner for a mission
    public bool Reserve(DeliveryMission mission)
    {
        if (IsReserved || mission == null)
            return false;

        IsReserved = true;
        reservedMission = mission;
        return true;
    }

    // Release reservation if mission matches
    public void Release(DeliveryMission mission)
    {
        if (reservedMission != mission)
            return;

        IsReserved = false;
        reservedMission = null;
    }

    protected void RegisterSpawnedNPC(GameObject npc)
    {
        activeNPC = npc;
    }

    public void Clear()
    {
        activeNPC = null;
    }

    public bool HasValidGiver()
    {
        var valid = GetValidGiversForCurrentFlag();
        return valid != null && valid.Count > 0;
    }

    public bool HasValidReceiver()
    {
        return receiverPrefabs != null && receiverPrefabs.Count > 0;
    }

    // Get all giver prefabs that are valid for the current game progress
    private List<NPCDeliveryGiver> GetValidGiversForCurrentFlag()
    {
        if (giverPrefabs == null || giverPrefabs.Count == 0)
            return null;

        DeliveryFlag currentFlag = DeliveryFlags.Instance.currentFlag;

        return giverPrefabs.FindAll(prefabGiver => prefabGiver.flag >= minimumFlag && prefabGiver.flag <= currentFlag);
    }

    public void SpawnGiver(DeliveryMission mission)
    {
        Debug.Log($"[Delivery NPC Spawner] {name} spawning giver NPC");
        if (reservedMission != mission)
        {
            Debug.LogError($"[Delivery NPC Spawner] {name} failed: can't spawn NPC for this mission");
            return;
        }

        var valid = GetValidGiversForCurrentFlag();
        if (valid == null || valid.Count == 0)
        {
            Debug.LogError($"[Delivery NPC Spawner] {name} failed: called with no valid giver prefabs");
            return;
        }

        // Choose a random giver prefab
        var chosen = valid[Random.Range(0, valid.Count)];
        var giver = Instantiate(chosen, transform.position, transform.rotation);

        giver.AssignMission(mission);
        RegisterSpawnedNPC(giver.gameObject);

        // Assign the spawned giver NPC and its associated flag to the mission
        mission.giver = giver;
        mission.flag = chosen.flag;
        Debug.Log($"[Delivery NPC Spawner] {name} spawned giver NPC successfully");
    }

    public void SpawnReceiver(DeliveryMission mission)
    {
        Debug.Log($"[Delivery NPC Spawner] {name} spawning receiver NPC");
        if (reservedMission != mission)
        {
            Debug.LogError($"[Delivery NPC Spawner] {name} failed: can't spawn NPC for this mission");
            return;
        }

        // Choose a random receiver prefab
        var chosen = receiverPrefabs[Random.Range(0, receiverPrefabs.Count)];
        var receiver = Instantiate(chosen, transform.position, transform.rotation);

        receiver.AssignMission(mission);
        RegisterSpawnedNPC(receiver.gameObject);

        // Assign the spawned receiver NPC to the mission
        mission.receiver = receiver;
        Debug.Log($"[Delivery NPC Spawner] {name} spawned receiver NPC successfully");
    }
}