using System.Collections.Generic;
using UnityEngine;

public class RouteLineDrawer : MonoBehaviour
{
    public static RouteLineDrawer Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject firstLinePrefab;
    [SerializeField] private GameObject standardLinePrefab;

    private readonly List<GameObject> activeLines = new();

    private void Awake()
    {
        Instance = this;
    }

    // Draw the route with the nodes provided
    public void DrawRoute(List<RouteNode> nodes, RectTransform linesParent, bool isMinimap)
    {
        Clear();

        Vector2 previous;

        // Get player position based on map type
        if (isMinimap)
            previous = MiniMapUIManager.Instance.GetPlayerMapPosition();
        else
            previous = MapUIManager.Instance.GetPlayerMapPosition();

        for (int i = 0; i < nodes.Count; i++)
        {
            Vector2 next;
            
            // Get next node position based on map type
            if (isMinimap)
                next = MiniMapUIManager.Instance.WorldToMapPosition(nodes[i].worldTransform.position);
            else
                next = MapUIManager.Instance.WorldToMapPosition(nodes[i].worldTransform.position);

            // Choose a different line prefab for first line
            GameObject linePrefab;
            if (i == 0)
                linePrefab = firstLinePrefab;
            else
                linePrefab = standardLinePrefab;

            // Draw the line and keep the first one in the front
            GameObject line = DrawLine(previous, next, linePrefab, linesParent, isMinimap);
            if (i > 0)
                line.transform.SetAsFirstSibling();

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
    private GameObject DrawLine(Vector2 start, Vector2 end, GameObject linePrefab, RectTransform linesParent, bool isMinimap)
    {
        GameObject line = Instantiate(linePrefab, linesParent);
        activeLines.Add(line);

        RectTransform rt = line.GetComponent<RectTransform>();

        // Calculate direction vector
        Vector2 direction = end - start;

        // Position the line in the middle of the segment
        rt.anchoredPosition = start + direction / 2f;

        // Set line thickness based on map type
        float lineThickness;
        if (isMinimap)
            lineThickness = rt.sizeDelta.y * 0.5f;
        else
            lineThickness = rt.sizeDelta.y;

        // Stretch the line to match the distance and thickness
        rt.sizeDelta = new Vector2(direction.magnitude, lineThickness);

        // Rotate the line to face the correct direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0f, 0f, angle);

        return line;
    }
}
