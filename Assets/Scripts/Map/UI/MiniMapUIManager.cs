using System.Collections.Generic;
using UnityEngine;

public class MiniMapUIManager : MonoBehaviour
{
    public static MiniMapUIManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform mapContent;
    [SerializeField] private RectTransform mapViewport;
    [SerializeField] private RectTransform iconsParent;
    [SerializeField] private Transform playerTransform;

    [Header("Settings")]
    [SerializeField] private float fixedZoom = 9f;
    [SerializeField] private float iconScale = 0.5f;
    [SerializeField] private Vector2 worldMin;
    [SerializeField] private Vector2 worldMax;

    private readonly Dictionary<(object, MapIconType), RectTransform> renderedIcons = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Set minimap scale with fixed zoom
        mapContent.localScale = Vector3.one * fixedZoom;
    }

    private void OnEnable()
    {
        MapIconManager.Instance.OnIconsChanged += RefreshIcons;
    }

    private void OnDisable()
    {
        MapIconManager.Instance.OnIconsChanged -= RefreshIcons;
    }

    private void LateUpdate()
    {
        if (!GameStateManager.Instance.IsGameplay)
            return;

        // Update position of each rendered icon
        foreach (var pair in MapIconManager.Instance.Icons)
        {
            // Skip icons that are not rendered in this map
            if (!renderedIcons.TryGetValue(pair.Key, out var rt))
                continue;

            MapIconData data = pair.Value;

            Vector2 mapPos = WorldToMapPosition(data.target.position);
            rt.anchoredPosition = mapPos;
        }

        // Always center map on player
        CenterOnPlayer();

        // Constantly update the drawn route
        if (RouteManager.Instance.HasActiveRoute)
            RouteManager.Instance.DrawRoute();
    }

    public Vector2 GetPlayerMapPosition()
    {
        if (playerTransform == null)
            return Vector2.zero;

        return WorldToMapPosition(playerTransform.position);
    }

    private void CenterOnPlayer()
    {
        Vector2 playerMapPos = GetPlayerMapPosition();
        mapContent.anchoredPosition = -playerMapPos * fixedZoom;
        ClampMapPosition();
    }

    private void ClampMapPosition()
    {
        Vector2 viewportSize = mapViewport.rect.size;
        Vector2 contentSize = mapContent.rect.size * fixedZoom;

        float maxX = Mathf.Max(0, (contentSize.x - viewportSize.x) / 2f);
        float maxY = Mathf.Max(0, (contentSize.y - viewportSize.y) / 2f);

        Vector2 clampedPos = mapContent.anchoredPosition;
        clampedPos.x = Mathf.Clamp(clampedPos.x, -maxX, maxX);
        clampedPos.y = Mathf.Clamp(clampedPos.y, -maxY, maxY);

        mapContent.anchoredPosition = clampedPos;
    }

    public Vector2 WorldToMapPosition(Vector3 worldPosition)
    {
        // Normalize world position within defined bounds
        float normalizedX = Mathf.InverseLerp(worldMin.x, worldMax.x, worldPosition.x);
        float normalizedY = Mathf.InverseLerp(worldMin.y, worldMax.y, worldPosition.z);

        // Convert normalized position to map space
        float mapX = (normalizedX - 0.5f) * mapContent.rect.width;
        float mapY = (normalizedY - 0.5f) * mapContent.rect.height;

        return new Vector2(mapX, mapY);
    }

    private void RefreshIcons()
    {
        // Get icon data from the icon manager
        var sourceIcons = MapIconManager.Instance.Icons;

        // Create missing icons
        foreach (var pair in sourceIcons)
        {
            // Skip icons already rendered
            if (renderedIcons.ContainsKey(pair.Key))
                continue;

            MapIconData data = pair.Value;

            // Instantiate icon prefab
            GameObject iconGO = Instantiate(data.prefab, iconsParent);
            RectTransform rt = iconGO.GetComponent<RectTransform>();

            rt.localScale = Vector3.one * iconScale;

            renderedIcons.Add(pair.Key, rt);
        }

        // Collect icons that no longer exist
        var toRemove = new List<(object, MapIconType)>();

        // Mark icons for removing
        foreach (var key in renderedIcons.Keys)
        {
            if (!sourceIcons.ContainsKey(key))
                toRemove.Add(key);
        }

        // Remove icons
        foreach (var key in toRemove)
        {
            Destroy(renderedIcons[key].gameObject);
            renderedIcons.Remove(key);
        }

        // Ensure player icon is always on top
        BringPlayerIconToFront();
    }

    private void BringPlayerIconToFront()
    {
        foreach (var pair in renderedIcons)
        {
            if (pair.Key.Item2 == MapIconType.Player)
            {
                pair.Value.SetAsLastSibling();
                return;
            }
        }
    }
}
