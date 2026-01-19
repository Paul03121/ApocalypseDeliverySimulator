using System;
using System.Collections.Generic;
using UnityEngine;

public enum MapIconType
{
    Player,
    Shop,
    GiverActive,
    GiverInactive,
    ReceiverActive,
    ReceiverInactive
}

[DefaultExecutionOrder(-100)]
public class MapIconManager : MonoBehaviour
{
    public static MapIconManager Instance { get; private set; }

    [Serializable] private struct IconPrefabEntry
    {
        public MapIconType type;
        public GameObject prefab;
    }

    [Header("Icon Prefabs")]
    [SerializeField] private IconPrefabEntry[] iconPrefabs;

    private readonly Dictionary<MapIconType, GameObject> prefabByType = new();
    private readonly Dictionary<(object, MapIconType), MapIconData> icons = new();

    // Read-only access for map renderers
    public IReadOnlyDictionary<(object, MapIconType), MapIconData> Icons => icons;

    // Fired when icons are added or removed
    public event Action OnIconsChanged;

    private void Awake()
    {
        Instance = this;

        // Build prefab lookup table
        foreach (var entry in iconPrefabs)
            prefabByType[entry.type] = entry.prefab;
    }

    public void RegisterIcon(object owner, MapIconType type, Transform target)
    {
        var key = (owner, type);

        // Prevent duplicate icon registration
        if (icons.ContainsKey(key))
            return;

        // Validate prefab availability
        if (!prefabByType.TryGetValue(type, out var prefab))
        {
            Debug.LogError($"No prefab registered for MapIconType: {type}");
            return;
        }

        // Store icon data
        icons.Add(key, new MapIconData(owner, type, target, prefab));

        // Notify listeners
        OnIconsChanged?.Invoke();
    }

    public void UnregisterIcon(object owner, MapIconType type)
    {
        // Notify listeners only if removal was successful
        if (icons.Remove((owner, type)))
            OnIconsChanged?.Invoke();
    }
}