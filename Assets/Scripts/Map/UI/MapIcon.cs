using UnityEngine;

public enum MapIconType
{
    Player,
    Shop,
    GiverActive,
    GiverInactive,
    ReceiverInactive,
    ReceiverActive
}

public class MapIcon : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    public RectTransform rectTransform;

    private void LateUpdate()
    {
        // Ensure references exist before updating
        if (target == null || MapUIManager.Instance == null)
            return;

        // Synchronize map icon with a world-space target
        rectTransform.anchoredPosition = MapUIManager.Instance.WorldToMapPosition(target.position);
    }
}