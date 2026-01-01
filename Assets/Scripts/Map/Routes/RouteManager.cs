using System.Collections.Generic;
using UnityEngine;

public class RouteManager : MonoBehaviour
{
    public static RouteManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform playerTransform;

    private List<RouteNode> currentRoute = new();
    private RouteAlgorithmType currentAlgorithm;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        // Listen for mission node visit events
        DeliveryMission.OnNodeVisited += HandleNodeVisited;
    }

    private void OnDisable()
    {
        // Stop listenig for mission node visit events
        DeliveryMission.OnNodeVisited -= HandleNodeVisited;
    }

    public bool HasActiveRoute => currentRoute != null && currentRoute.Count > 0;

    // Build and draw a new route using the selected algorithm
    public void CreateRoute(List<DeliveryMission> missions, RouteAlgorithmType algorithm)
    {
        ClearRoute();

        currentAlgorithm = algorithm;

        List<RouteNode> nodes = BuildNodesFromMissions(missions);

        currentRoute = RouteAlgorithms.ApplyAlgorithm(nodes, algorithm, playerTransform.position);

        DrawRoute();
    }

    // Draw the current route in the map
    public void DrawRoute()
    {
        if (!HasActiveRoute)
            return;

        RouteLineDrawer.Instance.DrawRoute(currentRoute);
    }

    // Clear the current route and its visual representation
    public void ClearRoute()
    {
        currentRoute.Clear();
        RouteLineDrawer.Instance.Clear();
    }

    // Convert mission data into route nodes depending on mission state
    private List<RouteNode> BuildNodesFromMissions(List<DeliveryMission> missions)
    {
        List<RouteNode> nodes = new();

        foreach (var mission in missions)
        {
            switch (mission.State)
            {
                case MissionState.Generated:
                    nodes.Add(new RouteNode(mission, RouteNodeType.Giver, mission.reservedGiverSpawner.transform));
                    nodes.Add(new RouteNode(mission, RouteNodeType.Receiver, mission.reservedReceiverSpawner.transform));
                    break;

                case MissionState.Active:
                    nodes.Add(new RouteNode(mission, RouteNodeType.Receiver, mission.reservedReceiverSpawner.transform));
                    break;

                case MissionState.Completed:
                    break;
            }
        }

        return nodes;
    }

    // Called when a route node is visited
    private void HandleNodeVisited(object mission, RouteNodeType type)
    {
        if (!HasActiveRoute)
            return;

        RouteNode nextNode = currentRoute[0];

        // Remove all matching nodes from the route
        currentRoute.RemoveAll(node => node.mission == mission && node.type == type);

        // Check if the visited node was the expected next node
        if (nextNode.mission == mission && nextNode.type == type)
        {
            RouteLineDrawer.Instance.RemoveFirstSegment();

            if (!HasActiveRoute)
            {
                ClearRoute();
            }
            else
            {
                // Required for real-time minimap updates
                RouteLineDrawer.Instance.DrawRoute(currentRoute);
            }
        }
        else
        {
            // Route order was broken, recalculate
            RecalculateRoute();
        }
    }

    // Recalculate the route from the current player position
    private void RecalculateRoute()
    {
        currentRoute = RouteAlgorithms.ApplyAlgorithm(currentRoute, currentAlgorithm, playerTransform.position);
        RouteLineDrawer.Instance.DrawRoute(currentRoute);
    }
}