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

    [Header("Map Icons")]
    [SerializeField] private GameObject playerIconPrefab;
    [SerializeField] private GameObject shopIconPrefab;
    [SerializeField] private GameObject giverActiveIconPrefab;
    [SerializeField] private GameObject giverInactiveIconPrefab;
    [SerializeField] private GameObject receiverInactiveIconPrefab;
    [SerializeField] private GameObject receiverActiveIconPrefab;

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

    // Active map icons indexed by (owner, type)
    private readonly Dictionary<(object, MapIconType), MapIcon> activeIcons = new();

    private void Awake()
    {
        Instance = this;

        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth == null)
            Debug.LogError("PlayerHealth not found in scene");
    }

    private void Start()
    {
        RegisterPlayer();
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

    void OnEnable()
    {
        playerHealth.OnPlayerDeathStarted += HandlePlayerDeathStarted;
        playerHealth.OnPlayerDeathEnded += HandlePlayerDeathEnded;
    }

    void OnDisable()
    {
        playerHealth.OnPlayerDeathStarted -= HandlePlayerDeathStarted;
        playerHealth.OnPlayerDeathEnded -= HandlePlayerDeathEnded;
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

    public void CenterMapOnPlayer(bool clamp = true)
    {
        Vector2 playerMapPos = GetPlayerMapPosition();

        mapContent.anchoredPosition = -playerMapPos * currentZoom;

        if (clamp)
            ClampMapPosition();
    }

    public void RegisterPlayer()
    {
        if (playerTransform == null)
        {
            Debug.LogError("MapUIManager: PlayerTransform not assigned");
            return;
        }

        CreateIcon(this, MapIconType.Player, playerIconPrefab, playerTransform);
    }

    public void RegisterShop(object shop, Transform shopTransform)
    {
        CreateIcon(shop, MapIconType.Shop, shopIconPrefab, shopTransform);
    }

    public void UnregisterShop(object shop)
    {
        RemoveIcon(shop, MapIconType.Shop);
    }

    public void RegisterGiverGenerated(object mission, Transform giver)
    {
        CreateIcon(mission, MapIconType.GiverActive, giverActiveIconPrefab, giver);
    }

    public void SetGiverInactive(object mission, Transform giver)
    {
        RemoveIcon(mission, MapIconType.GiverActive);
        CreateIcon(mission, MapIconType.GiverInactive, giverInactiveIconPrefab, giver);
    }

    public void UnregisterGiver(object mission)
    {
        RemoveIcon(mission, MapIconType.GiverInactive);
    }

    public void RegisterReceiverGenerated(object mission, Transform receiver)
    {
        CreateIcon(mission, MapIconType.ReceiverInactive, receiverInactiveIconPrefab, receiver);
    }

    public void SetReceiverActive(object mission, Transform receiver)
    {
        RemoveIcon(mission, MapIconType.ReceiverInactive);
        CreateIcon(mission, MapIconType.ReceiverActive, receiverActiveIconPrefab, receiver);
    }

    public void UnregisterReceiver(object mission)
    {
        RemoveIcon(mission, MapIconType.ReceiverActive);
    }

    private void CreateIcon(object owner, MapIconType type, GameObject prefab, Transform target)
    {
        var key = (owner, type);

        // Prevent duplicate icons
        if (activeIcons.ContainsKey(key))
            return;

        GameObject iconObject = Instantiate(prefab, iconsParent);
        MapIcon icon = iconObject.GetComponent<MapIcon>();

        icon.target = target;
        icon.rectTransform = iconObject.GetComponent<RectTransform>();

        activeIcons.Add(key, icon);

        // Ensure player icon is always on top
        BringPlayerIconToFront();
    }

    private void RemoveIcon(object owner, MapIconType type)
    {
        var key = (owner, type);

        if (!activeIcons.TryGetValue(key, out MapIcon icon))
            return;

        Destroy(icon.gameObject);
        activeIcons.Remove(key);
    }

    private void BringPlayerIconToFront()
    {
        var key = (this, MapIconType.Player);

        if (activeIcons.TryGetValue(key, out MapIcon playerIcon))
        {
            playerIcon.rectTransform.SetAsLastSibling();
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