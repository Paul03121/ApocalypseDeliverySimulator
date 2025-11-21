using UnityEngine;

public class InteractableIconController : MonoBehaviour
{
    [Header("Icon References")]
    [SerializeField] private GameObject proximityIcon;   // Icon shown when player is nearby
    [SerializeField] private GameObject interactIcon;    // Icon shown when interaction is available

    private void Awake()
    {
        // Ensure icons start hidden to avoid flickering on scene load
        HideAll();
    }

    // Displays the proximity indicator and hides the interact indicator
    public void ShowProximityIcon()
    {
        if (proximityIcon != null)
            proximityIcon.SetActive(true);

        if (interactIcon != null)
            interactIcon.SetActive(false);
    }

    // Displays the interact indicator and hides the proximity indicator
    public void ShowInteractIcon()
    {
        if (interactIcon != null)
            interactIcon.SetActive(true);

        if (proximityIcon != null)
            proximityIcon.SetActive(false);
    }

    // Hides all icons for this interactable
    public void HideAll()
    {
        if (proximityIcon != null)
            proximityIcon.SetActive(false);

        if (interactIcon != null)
            interactIcon.SetActive(false);
    }
}
