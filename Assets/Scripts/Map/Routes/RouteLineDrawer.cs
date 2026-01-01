using System.Collections.Generic;
using UnityEngine;

public class RouteLineDrawer : MonoBehaviour
{
    public static RouteLineDrawer Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform linesParent;
    [SerializeField] private GameObject linePrefab;

    private readonly List<GameObject> activeLines = new();

    private void Awake()
    {
        Instance = this;
    }

    // Draw the route with the nodes provided
    public void DrawRoute(List<RouteNode> nodes)
    {
        Clear();

        Vector2 previous = MapUIManager.Instance.GetPlayerMapPosition();

        foreach (var node in nodes)
        {
            Vector2 next = MapUIManager.Instance.WorldToMapPosition(node.worldTransform.position);
            DrawLine(previous, next);
            previous = next;
        }
    }

    // Remove the first visual segment of the route
    public void RemoveFirstSegment()
    {
        if (activeLines.Count == 0)
            return;

        Destroy(activeLines[0]);
        activeLines.RemoveAt(0);
    }

    // Clear all route line segments from the map
    public void Clear()
    {
        foreach (var line in activeLines)
            Destroy(line);

        activeLines.Clear();
    }

    // Draw a single line between two positions
    private void DrawLine(Vector2 start, Vector2 end)
    {
        GameObject line = Instantiate(linePrefab, linesParent);
        activeLines.Add(line);

        RectTransform rt = line.GetComponent<RectTransform>();

        // Calculate direction vector
        Vector2 direction = end - start;

        // Position the line in the middle of the segment
        rt.anchoredPosition = start + direction / 2f;

        // Stretch the line to match the distance
        rt.sizeDelta = new Vector2(direction.magnitude, rt.sizeDelta.y);

        // Rotate the line to face the correct direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
