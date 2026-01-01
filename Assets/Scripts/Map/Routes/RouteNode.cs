using UnityEngine;

public enum RouteNodeType
{
    Giver,
    Receiver
}

public class RouteNode
{
    [Header("Mission Data")]
    public DeliveryMission mission;
    public RouteNodeType type;

    [Header("World Reference")]
    public Transform worldTransform;

    public RouteNode(DeliveryMission mission, RouteNodeType type, Transform worldTransform)
    {
        this.mission = mission;
        this.type = type;
        this.worldTransform = worldTransform;
    }
}