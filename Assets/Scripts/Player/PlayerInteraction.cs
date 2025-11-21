using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float proximityRange = 6f;     // Distance at which the player sees proximity icon
    public float interactRange = 2f;      // Distance required to allow interaction

    private Interactable currentClosest;   // Currently closest interactable object
    private PackageInteractable carriedPackage; // Reference to the package the player is holding
    private Transform player;              // Cached player transform

    void Start()
    {
        player = transform; // Cache for performance
    }

    void Update()
    {
        DetectInteractables();
        HandleInteractionInput();
        HandleDropInput();
    }

    // Handles the interaction logic when the player presses the interaction key
    private void HandleInteractionInput()
    {
        if (!Input.GetKeyDown(KeyCode.E))
            return;

        // If carrying a package, prevent interaction with other packages
        if (carriedPackage != null && currentClosest is PackageInteractable)
            return;

        // Validate interaction distance
        if (currentClosest != null &&
            Vector3.Distance(player.position, currentClosest.transform.position) <= interactRange)
        {
            currentClosest.Interact();

            // If the interacted object is a package and it is now being held
            if (currentClosest is PackageInteractable pkg && pkg.isBeingHeld)
            {
                SetCarriedPackage(pkg);
            }
        }
    }

    // Handles dropping a currently carried package
    private void HandleDropInput()
    {
        if (carriedPackage == null)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            carriedPackage.Drop();
            ClearCarriedPackage();
        }
    }

    // Scans all interactable objects and determines which one is closest
    private void DetectInteractables()
    {
        Interactable[] interactables = FindObjectsOfType<Interactable>();

        Interactable closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var obj in interactables)
        {
            if (obj.IsInteractionDisabled)
                continue; // Skip entirely if interaction is disabled

            float dist = Vector3.Distance(player.position, obj.transform.position);

            // Track closest valid interactable
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = obj;
            }

            UpdateIconState(obj, dist);
        }

        currentClosest = closest;
    }

    // Determines which interaction icon (if any) should be shown based on range and state
    private void UpdateIconState(Interactable obj, float dist)
    {
        // When carrying a package, hide icons of other packages
        if (carriedPackage != null && obj is PackageInteractable)
        {
            obj.OnLoseFocus();
            return;
        }

        // Outside proximity range, hide icons
        if (dist > proximityRange)
        {
            obj.OnLoseFocus();
            return;
        }

        // Within proximity but not close enough to interact, show proximity icon
        if (dist <= proximityRange && dist > interactRange)
        {
            obj.OnProximity();
            return;
        }

        // Within interact range, show interaction icon
        if (dist <= interactRange)
        {
            obj.OnBecomeInteractable();
        }
    }

    // Stores reference to the package currently being carried
    public void SetCarriedPackage(PackageInteractable p)
    {
        carriedPackage = p;
    }

    // Returns the package the player is carrying (if any)
    public PackageInteractable GetCarriedPackage()
    {
        return carriedPackage;
    }

    // Clears the reference to the carried package after dropping it
    public void ClearCarriedPackage()
    {
        carriedPackage = null;
    }
}
