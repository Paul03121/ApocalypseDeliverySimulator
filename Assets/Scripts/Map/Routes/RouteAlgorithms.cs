using System.Collections.Generic;
using UnityEngine;

public enum RouteAlgorithmType
{
    HeuristicSinglePackage,
    HeuristicMultiPackage,
    OptimalSinglePackage,
    OptimalMultiPackage
}

public class RouteAlgorithms
{
    public static List<RouteNode> ApplyAlgorithm(List<RouteNode> nodes, RouteAlgorithmType algorithm, Vector3 playerPosition)
    {
        // Defensive copy to avoid mutating external data
        List<RouteNode> workingList = new(nodes);
        List<RouteNode> result;

        switch (algorithm)
        {
            case RouteAlgorithmType.HeuristicSinglePackage:
                result = HeuristicSinglePackage(workingList, playerPosition);
                break;

            case RouteAlgorithmType.HeuristicMultiPackage:
                result = HeuristicMultiPackage(workingList, playerPosition);
                break;

            case RouteAlgorithmType.OptimalSinglePackage:
                result = OptimalSinglePackage(workingList, playerPosition);
                break;

            case RouteAlgorithmType.OptimalMultiPackage:
                result = OptimalMultiPackage(workingList, playerPosition);
                break;

            default:
                Debug.LogWarning("Unknown algorithm type. Returning original list.");
                return workingList;
        }

        float totalDistance = CalculateTotalDistance(playerPosition, result);

        Debug.Log($"[RouteAlgorithms] {algorithm} | Nodes: {result.Count} | Total Distance: {totalDistance:F2}");

        return result;
    }

    // Algorithm for heuristic - single package
    private static List<RouteNode> HeuristicSinglePackage(List<RouteNode> nodes, Vector3 playerPosition)
    {
        List<RouteNode> remaining = new(nodes);
        List<RouteNode> orderedRoute = new();

        List<RouteNode> activeReceivers = new();

        Vector3 currentPosition = playerPosition;

        // Extract active receivers
        for (int i = remaining.Count - 1; i >= 0; i--)
        {
            var node = remaining[i];

            if (node.type == RouteNodeType.Receiver && IsMissionActive(node.mission))
            {
                activeReceivers.Add(node);
                remaining.RemoveAt(i);
            }
        }

        // Visit active receivers using nearest-neighbor heuristic
        while (activeReceivers.Count > 0)
        {
            RouteNode closest = FindClosestNode(currentPosition, activeReceivers);

            orderedRoute.Add(closest);
            activeReceivers.Remove(closest);

            currentPosition = closest.worldTransform.position;
        }

        // Visit generated missions in giver -> receiver order
        while (remaining.Count > 0)
        {
            List<RouteNode> givers = remaining.FindAll(node => node.type == RouteNodeType.Giver);

            if (givers.Count == 0)
                break;

            // Find the nearest giver
            RouteNode giver = FindClosestNode(currentPosition, givers);

            orderedRoute.Add(giver);
            remaining.Remove(giver);

            // Find its corresponding receiver
            RouteNode receiver = remaining.Find(node => node.type == RouteNodeType.Receiver && node.mission == giver.mission);

            if (receiver != null)
            {
                orderedRoute.Add(receiver);
                remaining.Remove(receiver);

                currentPosition = receiver.worldTransform.position;
            }
            else
            {
                // Defensive fallback
                currentPosition = giver.worldTransform.position;
            }
        }

        return orderedRoute;
    }

    // Algorithm for heuristic - multiple packages
    private static List<RouteNode> HeuristicMultiPackage(List<RouteNode> nodes, Vector3 playerPosition)
    {
        List<RouteNode> remaining = new(nodes);
        List<RouteNode> orderedRoute = new();

        // Tracks missions whose giver has already been visited
        HashSet<DeliveryMission> missionsWithVisitedGiver = new();

        Vector3 currentPosition = playerPosition;

        while (remaining.Count > 0)
        {
            RouteNode closest = null;
            float minDistance = float.MaxValue;

            // Find closest valid node from current position
            foreach (var node in remaining)
            {
                // Validate if receiver can be visited
                if (node.type == RouteNodeType.Receiver && !IsMissionActive(node.mission) && !missionsWithVisitedGiver.Contains(node.mission))
                    continue;

                float dist = Vector3.Distance(currentPosition, node.worldTransform.position);

                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = node;
                }
            }

            // No valid node found
            if (closest == null)
                break;

            orderedRoute.Add(closest);
            remaining.Remove(closest);

            // Visiting a giver unlocks its receiver
            if (closest.type == RouteNodeType.Giver)
                missionsWithVisitedGiver.Add(closest.mission);

            currentPosition = closest.worldTransform.position;
        }

        return orderedRoute;
    }

    // Algorithm for optimal route - single package
    private static List<RouteNode> OptimalSinglePackage(List<RouteNode> nodes, Vector3 playerPosition)
    {
        List<RouteNode> remaining = new(nodes);
        List<RouteNode> activeReceivers = new();

        // Separate active receivers
        for (int i = remaining.Count - 1; i >= 0; i--)
        {
            var node = remaining[i];

            if (node.type == RouteNodeType.Receiver && IsMissionActive(node.mission))
            {
                activeReceivers.Add(node);
                remaining.RemoveAt(i);
            }
        }

        float bestDistance = float.MaxValue;
        List<RouteNode> bestRoute = null;

        // Call backtracking for single package
        SearchOptimalSinglePackageRoute(
            playerPosition,
            activeReceivers,
            remaining,
            new List<RouteNode>(),
            0f,
            ref bestDistance,
            ref bestRoute
        );

        if (bestRoute != null)
            return bestRoute;
        else
            return new List<RouteNode>();
    }

    // Algorithm for optimal route - multiple packages
    private static List<RouteNode> OptimalMultiPackage(List<RouteNode> nodes, Vector3 playerPosition)
    {
        float bestDistance = float.MaxValue;
        List<RouteNode> bestRoute = null;

        // Call backtracking for multiple packages
        SearchOptimalMultiPackageRoute(
            playerPosition,
            nodes,
            new List<RouteNode>(),
            new HashSet<DeliveryMission>(),
            0f,
            ref bestDistance,
            ref bestRoute
        );

        if (bestRoute != null)
            return bestRoute;
        else
            return new List<RouteNode>();
    }

    private static RouteNode FindClosestNode(Vector3 origin, List<RouteNode> nodes)
    {
        RouteNode closest = null;
        float minDistance = float.MaxValue;

        foreach (var node in nodes)
        {
            float dist = Vector3.Distance(origin, node.worldTransform.position);

            if (dist < minDistance)
            {
                minDistance = dist;
                closest = node;
            }
        }

        return closest;
    }

    private static float CalculateTotalDistance(Vector3 playerPosition, List<RouteNode> route)
    {
        if (route == null || route.Count == 0)
            return 0f;

        float total = 0f;
        Vector3 current = playerPosition;

        foreach (var node in route)
        {
            float dist = Vector3.Distance(current, node.worldTransform.position);
            total += dist;
            current = node.worldTransform.position;
        }

        return total;
    }

    private static bool IsMissionActive(DeliveryMission mission)
    {
        return mission.State == MissionState.Active;
    }

    // Backtracking for single package
    private static void SearchOptimalSinglePackageRoute(Vector3 currentPosition, List<RouteNode> activeReceivers, List<RouteNode> remaining, List<RouteNode> currentRoute, float currentDistance, ref float bestDistance, ref List<RouteNode> bestRoute)
    {
        // Branch and bound pruning
        if (currentDistance >= bestDistance)
            return;

        // All nodes visited
        if (activeReceivers.Count == 0 && remaining.Count == 0)
        {
            bestDistance = currentDistance;
            bestRoute = new List<RouteNode>(currentRoute);
            return;
        }

        // Active receivers must be resolved first
        if (activeReceivers.Count > 0)
        {
            for (int i = 0; i < activeReceivers.Count; i++)
            {
                var receiver = activeReceivers[i];
                float dist = Vector3.Distance(currentPosition, receiver.worldTransform.position);

                currentRoute.Add(receiver);
                activeReceivers.RemoveAt(i);

                // Recursive call
                SearchOptimalSinglePackageRoute(
                    receiver.worldTransform.position,
                    activeReceivers,
                    remaining,
                    currentRoute,
                    currentDistance + dist,
                    ref bestDistance,
                    ref bestRoute
                );

                // Rollback
                activeReceivers.Insert(i, receiver);
                currentRoute.RemoveAt(currentRoute.Count - 1);
            }

            return;
        }

        // Choose a giver and immediately its receiver
        for (int i = 0; i < remaining.Count; i++)
        {
            var giver = remaining[i];
            if (giver.type != RouteNodeType.Giver)
                continue;

            var receiver = remaining.Find(node => node.type == RouteNodeType.Receiver && node.mission == giver.mission);

            if (receiver == null)
                continue;

            float distToGiver = Vector3.Distance(currentPosition, giver.worldTransform.position);
            float distToReceiver = Vector3.Distance(giver.worldTransform.position, receiver.worldTransform.position);

            // Snapshot for future rollback preserving original positions
            var remainingSnapshot = new List<RouteNode>(remaining);

            currentRoute.Add(giver);
            currentRoute.Add(receiver);

            remaining.Remove(giver);
            remaining.Remove(receiver);

            // Recursive call
            SearchOptimalSinglePackageRoute(
                receiver.worldTransform.position,
                activeReceivers,
                remaining,
                currentRoute,
                currentDistance + distToGiver + distToReceiver,
                ref bestDistance,
                ref bestRoute
            );

            // Rollback using snapshot
            remaining.Clear();
            remaining.AddRange(remainingSnapshot);

            currentRoute.RemoveAt(currentRoute.Count - 1);
            currentRoute.RemoveAt(currentRoute.Count - 1);
        }
    }

    // Backtracking for multiple packages
    private static void SearchOptimalMultiPackageRoute(Vector3 currentPosition, List<RouteNode> remaining, List<RouteNode> currentRoute, HashSet<DeliveryMission> missionsWithVisitedGiver, float currentDistance, ref float bestDistance, ref List<RouteNode> bestRoute)
    {
        // Branch and bound pruning
        if (currentDistance >= bestDistance)
            return;

        // All nodes visited
        if (remaining.Count == 0)
        {
            bestDistance = currentDistance;
            bestRoute = new List<RouteNode>(currentRoute);
            return;
        }

        // Build all posible branches
        for (int i = 0; i < remaining.Count; i++)
        {
            var node = remaining[i];

            // Validate if receiver can be visited
            if (node.type == RouteNodeType.Receiver && !IsMissionActive(node.mission) && !missionsWithVisitedGiver.Contains(node.mission))
                continue;

            float dist = Vector3.Distance(currentPosition, node.worldTransform.position);

            currentRoute.Add(node);
            remaining.RemoveAt(i);

            // Visiting a giver unlocks its receiver
            bool addedGiver = false;
            if (node.type == RouteNodeType.Giver)
            {
                missionsWithVisitedGiver.Add(node.mission);
                addedGiver = true;
            }

            // Recursive call
            SearchOptimalMultiPackageRoute(
                node.worldTransform.position,
                remaining,
                currentRoute,
                missionsWithVisitedGiver,
                currentDistance + dist,
                ref bestDistance,
                ref bestRoute
            );

            // Rollback
            if (addedGiver)
                missionsWithVisitedGiver.Remove(node.mission);

            remaining.Insert(i, node);
            currentRoute.RemoveAt(currentRoute.Count - 1);
        }
    }
}
