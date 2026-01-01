using System;
using UnityEngine;

public enum MissionState
{
    Generated,
    Active,
    Completed
}

public class DeliveryMission
{
    public MissionState State { get; private set; }

    public DeliveryFlag flag;

    [Header("Participants")]
    public PackageInteractable package;
    public NPCDeliveryGiver giver;
    public NPCDeliveryReceiver receiver;

    [Header("Reserved Spawners")]
    public DeliveryNPCSpawner reservedGiverSpawner;
    public DeliveryNPCSpawner reservedReceiverSpawner;

    [Header("Timing")]
    private float deliveryTime;
    private float startTime;

    [Header("Reward Calculation")]
    private int baseReward;
    private int timePenalty;
    private int integrityPenalty;
    private int finalReward;

    public float DeliveryTime => deliveryTime;
    public float Integrity => package.GetIntegrity();

    public int BaseReward => baseReward;
    public int TimePenalty => timePenalty;
    public int IntegrityPenalty => integrityPenalty;
    public int FinalReward => finalReward;

    // Event fired when a node is visited
    public static event Action<DeliveryMission, RouteNodeType> OnNodeVisited;

    public DeliveryMission(DeliveryNPCSpawner reservedGiverSpawner, DeliveryNPCSpawner reservedReceiverSpawner)
    {
        // Assign reserved spawners and sets initial state
        this.reservedGiverSpawner = reservedGiverSpawner;
        this.reservedReceiverSpawner = reservedReceiverSpawner;

        State = MissionState.Generated;
    }

    public void Activate()
    {
        // Activate mission and record start time
        State = MissionState.Active;
        startTime = Time.time;

        // Notify listeners
        OnNodeVisited?.Invoke(this, RouteNodeType.Giver);
    }

    public void Complete()
    {
        if (State != MissionState.Active)
            return;

        // Complete mission and calculate rewards
        State = MissionState.Completed;
        deliveryTime = Time.time - startTime;
        CalculateReward();

        // Notify listeners
        OnNodeVisited?.Invoke(this, RouteNodeType.Receiver);
    }

    private void CalculateReward()
    {
        // Get base reward for this delivery mission
        baseReward = DeliveryFlags.Instance.GetBaseRewardForFlag(flag);

        // Calculate time penalty
        float extraTime = Mathf.Max(0, deliveryTime - 180f);
        int timeSteps = Mathf.FloorToInt(extraTime / 10f);
        timePenalty = timeSteps * 5;

        // Calculate integrity penalty
        float lostIntegrity = 1f - Integrity;
        int integritySteps = Mathf.RoundToInt(lostIntegrity * 10f);
        integrityPenalty = integritySteps * 5;

        // Calculate final reward
        finalReward = Mathf.Max(0, baseReward - timePenalty - integrityPenalty);
    }
}