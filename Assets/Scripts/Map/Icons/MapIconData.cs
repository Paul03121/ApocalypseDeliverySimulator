using UnityEngine;

public class MapIconData
{
    public object owner;
    public MapIconType type;
    public Transform target;
    public GameObject prefab;

    public MapIconData(object owner, MapIconType type, Transform target, GameObject prefab)
    {
        this.owner = owner;
        this.type = type;
        this.target = target;
        this.prefab = prefab;
    }
}