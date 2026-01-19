using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUIManager : MonoBehaviour
{
    public static MapUIManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform mapContent;
    [SerializeField] private RectTransform mapViewport;
    [SerializeField] private RectTransform iconsParent;
    [SerializeField] private Transform playerTransform;

    [Header("World bounds")]
    [SerializeField] private Vector2 worldMin;
    [SerializeField] private Vector2 worldMax;

    [Header("Map Zoom")]
    [SerializeField] private float initialZoom = 3f;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 6f;

    [Header("Map Drag")]
    [SerializeField] private float dragSpeed = 1f;

    [Header("Routes")]
    [SerializeField] private Button heuristicRouteButton;
    [SerializeField] private Button optimalRouteButton;
    [SerializeField] private Toggle multiPackageToggle;

    private float currentZoom;
    private bool isDragging;
    private Vector2 lastMousePosition;

    [Header("Death System Block")]
    private PlayerHealth playerHealth;
    private bool isBlocked = false;

    private readonly Dictionary<(object, MapIconType), RectTransform> renderedIcons = new();

    private void Awake()
    {
        Instance = this;

        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth == null)
            Debug.LogError("PlayerHealth not found in scene");
    }

    private void OnEnable()
    {
        if (MapIconManager.Instance != null)
            MapIconManager.Instance.OnIconsChanged += RefreshIcons;

        playerHealth.OnPlayerDeathStarted += HandlePlayerDeathStarted;
        playerHealth.OnPlayerDeathEnded += HandlePlayerDeathEnded;
    }

    private void OnDisable()
    {
        if (MapIconManager.Instance != null)
            MapIconManager.Instance.OnIconsChanged -= RefreshIcons;

        playerHealth.OnPlayerDeathStarted -= HandlePlayerDeathStarted;
        playerHealth.OnPlayerDeathEnded -= HandlePlayerDeathEnded;
    }

    private void Update()
    {
        // Block if player is dead
        if (isBlocked) return;

        // Prevents multiple state changes in the same frame
        if (!GameStateManager.Instance.CanChangeState)
            return;

        if (Input.GetKeyDown(KeyCode.M))
        {
            // Open map only during gameplay
            if (GameStateManager.Instance.IsGameplay)
                OpenMap();

            // Close map with M if it is already open
            else if (GameStateManager.Instance.IsMap)
                CloseMap();
        }

        // Close map with Esc if it is already open
        if (GameStateManager.Instance.IsMap && Input.GetKeyDown(KeyCode.Escape))
            CloseMap();

        // Enable map interactions only when map is open
        if (GameStateManager.Instance.IsMap)
        {
            HandleZoomInput();
            HandleDragInput();

            // Center map on player
            if (Input.GetKeyDown(KeyCode.C))
                CenterMapOnPlayer();
        }
    }

    private void LateUpdate()
    {
        if (!GameStateManager.Instance.IsMap)
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
    }

    private void HandlePlayerDeathStarted()
    {
        isBlocked = true;
    }

    private void HandlePlayerDeathEnded()
    {
        isBlocked = false;
    }

    private void OpenMap()
    {
        // Switch game to map state
        GameStateManager.Instance.SetState(GameState.Map);

        // Reset zoom and position
        Canvas.ForceUpdateCanvases();
        currentZoom = initialZoom;
        mapContent.localScale = Vector3.one * currentZoom;

        CenterMapOnPlayer();

        UpdateRouteButtons();

        // Redraw current route (if any)
        RouteManager.Instance.DrawRoute();
    }

    private void CloseMap()
    {
        // Return game to gameplay state
        GameStateManager.Instance.SetState(GameState.Gameplay);
    }

    private void HandleZoomInput()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (scroll != 0)
            ApplyZoomAtCursor(scroll * zoomSpeed);

        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus))
            ApplyZoomAtCursor(zoomSpeed);

        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
            ApplyZoomAtCursor(-zoomSpeed);
    }

    private void ApplyZoomAtCursor(float delta)
    {
        float newZoom = Mathf.Clamp(currentZoom + delta, minZoom, maxZoom);
        if (Mathf.Approximately(newZoom, currentZoom))
            return;

        // Convert mouse position to local map space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mapViewport, Input.mousePosition, null, out Vector2 localMousePos);

        // Keep zoom centered on cursor
        Vector2 pivotOffset = mapContent.anchoredPosition - localMousePos;
        float zoomFactor = newZoom / currentZoom;
        Vector2 newOffset = pivotOffset * zoomFactor;

        mapContent.anchoredPosition = localMousePos + newOffset;
        currentZoom = newZoom;
        mapContent.localScale = Vector3.one * currentZoom;

        // Keep map inside viewport bounds
        ClampMapPosition();
    }

    private void HandleDragInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
            isDragging = false;

        // Move map content while dragging
        if (isDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastMousePosition;
            mapContent.anchoredPosition += delta * dragSpeed;
            lastMousePosition = Input.mousePosition;

            // Keep map inside viewport bounds
            ClampMapPosition();
        }
    }

    private void ClampMapPosition()
    {
        Vector2 viewportSize = mapViewport.rect.size;
        Vector2 contentSize = mapContent.rect.size * currentZoom;

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

    public Vector2 GetPlayerMapPosition()
    {
        if (playerTransform == null)
            return Vector2.zero;

        return WorldToMapPosition(playerTransform.position);
    }

    public void CenterMapOnPlayer()
    {
        Vector2 playerMapPos = GetPlayerMapPosition();
        mapContent.anchoredPosition = -playerMapPos * currentZoom;
        ClampMapPosition();
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

    private void UpdateRouteButtons()
    {
        // Routes available only if missions exist
        bool hasGeneratedMissions = DeliveryManager.Instance.GeneratedMissions.Count > 0;

        heuristicRouteButton.interactable = hasGeneratedMissions;
        optimalRouteButton.interactable = hasGeneratedMissions;

        if (hasGeneratedMissions)
        {
            heuristicRouteButton.onClick.RemoveAllListeners();
            heuristicRouteButton.onClick.AddListener(OnHeuristicRoutePressed);

            optimalRouteButton.onClick.RemoveAllListeners();
            optimalRouteButton.onClick.AddListener(OnOptimalRoutePressed);
        }
    }

    public void OnHeuristicRoutePressed()
    {
        RouteAlgorithmType algorithm;

        if (multiPackageToggle.isOn)
            algorithm = RouteAlgorithmType.HeuristicMultiPackage;
        else
            algorithm = RouteAlgorithmType.HeuristicSinglePackage;

        RouteManager.Instance.CreateRoute(DeliveryManager.Instance.GeneratedMissions, algorithm);
    }

    public void OnOptimalRoutePressed()
    {
        RouteAlgorithmType algorithm;

        if (multiPackageToggle.isOn)
            algorithm = RouteAlgorithmType.OptimalMultiPackage;
        else
            algorithm = RouteAlgorithmType.OptimalSinglePackage;

        RouteManager.Instance.CreateRoute(DeliveryManager.Instance.GeneratedMissions, algorithm);
    }
}