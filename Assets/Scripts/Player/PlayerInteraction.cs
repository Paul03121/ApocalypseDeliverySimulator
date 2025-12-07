using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float proximityRange = 7.5f;         // Distance at which the player sees proximity icon
    public float interactRange = 4f;            // Distance required to allow interaction

    private Interactable currentClosest;        // Currently closest interactable object
    private PackageInteractable carriedPackage; // Reference to the package the player is holding
    private WeaponInteractable carriedWeapon;   // Reference to the weapon the player is holding
    private Transform player;                   // Cached player transform
    private Camera playerCamera;                // Cached first person camera

    void Start()
    {
        player = transform;
        playerCamera = Camera.main;
    }

    void Update()
    {
        DetectInteractables();

        // Stop working if game is paused or if player died
        if (GameStateManager.Instance.IsPaused || GameStateManager.Instance.IsGameOver)
            return;

        HandleInteractionInput();
        HandleDropInput();
        HandleEquipInput();
    }

    // Handles the interaction logic when the player presses the interaction key
    private void HandleInteractionInput()
    {
        if (currentClosest == null)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            // Interaction distance
            float dist = Vector3.Distance(player.position, currentClosest.transform.position);
            if (dist > interactRange)
                return;

            if (carriedPackage != null)
            {
                // If carrying a package, prevent interaction with other packages
                if (currentClosest is PackageInteractable)
                {
                    Debug.Log("Ya llevas un paquete.");
                    return;
                }

                // If carrying a package, prevent interaction with weapons
                if (currentClosest is WeaponInteractable)
                {
                    Debug.Log("No puedes recoger armas mientras llevas un paquete.");
                    return;
                }
            }

            if (carriedWeapon != null)
            {
                // If carrying a weapon, prevent interaction with other weapons
                if (currentClosest is WeaponInteractable)
                {
                    Debug.Log("Ya llevas un arma.");
                    return;
                }

                // If a weapon is being held, prevent interaction with packages
                if (carriedWeapon.isBeingHeld && currentClosest is PackageInteractable)
                {
                    Debug.Log("No puedes recoger paquetes mientras llevas un arma equipada.");
                    return;
                }
            }

            // Interact
            currentClosest.Interact();

            // If the interacted object is a package and it is now being held
            if (currentClosest is PackageInteractable pkg && pkg.isBeingHeld)
            {
                SetCarriedPackage(pkg);
            }

            // If the interacted object is a weapon and it is now being held
            if (currentClosest is WeaponInteractable wpn && wpn.isBeingHeld)
            {
                SetCarriedWeapon(wpn);
            }
        }
    }

    // Handles dropping a currently carried object
    private void HandleDropInput()
    {
        if (carriedPackage == null && carriedWeapon == null)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Drop Package
            if (carriedPackage != null)
            {
                carriedPackage.Drop();
                ClearCarriedPackage();
                return;
            }

            // Drop Weapon
            if (carriedWeapon != null)
            {
                if (carriedWeapon.isBeingHeld)
                {
                    carriedWeapon.Drop();
                    ClearCarriedWeapon();
                }
            }
        }
    }

    // Handles equipping or unequipping weapon
    private void HandleEquipInput()
    {
        if (carriedWeapon == null)
            return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            // Unequip weapon
            if (carriedWeapon.isBeingHeld)
            {
                carriedWeapon.UnequipWeapon();
                return;
            }

            // Equip weapon
            if (!carriedWeapon.isBeingHeld)
            {
                if (carriedPackage != null)
                {
                    Debug.Log("No puedes equipar el arma mientras sostienes un paquete.");
                    return;
                }
                carriedWeapon.EquipWeapon();
                return;
            }
        }
    }

    // Scan all interactable objects
    private void DetectInteractables()
    {
        Interactable[] interactables = FindObjectsOfType<Interactable>();

        // Proximity icons
        foreach (var obj in interactables)
        {
            if (obj.IsInteractionDisabled)
            {
                obj.OnLoseFocus();
                continue;
            }

            float dist = Vector3.Distance(player.position, obj.transform.position);
            UpdateIconState(obj, dist, false);
        }

        // Detect what the player is actually looking at
        currentClosest = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            Interactable hitInteractable = hit.collider.GetComponent<Interactable>();

            if (hitInteractable != null && !hitInteractable.IsInteractionDisabled)
            {
                float dist = Vector3.Distance(player.position, hitInteractable.transform.position);

                if (dist <= interactRange)
                {
                    currentClosest = hitInteractable;

                    UpdateIconState(hitInteractable, dist, true);
                }
            }
        }
    }

    // Determines which interaction icon (if any) should be shown based on range and state
    private void UpdateIconState(Interactable obj, float dist, bool isLookedAt)
    {
        // When carrying a package, hide icons of other packages and weapons
        if (carriedPackage != null && (obj is PackageInteractable || obj is WeaponInteractable))
        {
            obj.OnLoseFocus();
            return;
        }

        // When carrying a weapon, hide icons of other weapons
        if (carriedWeapon != null && obj is WeaponInteractable)
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




        if (!isLookedAt)
        {
            // Within proximity but not close enough to interact, show proximity icon
            if (dist <= proximityRange && dist > interactRange)
            {
                obj.OnProximity();
                return;
            }

            // If close enough but not looking, show proximity icon
            if (dist <= interactRange)
            {
                obj.OnProximity();
                return;
            }
        }

        // If close enough and looking, show interaction icon
        if (isLookedAt && dist <= interactRange)
        {
            obj.OnBecomeInteractable();
            return;
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

    // Stores reference to the weapon currently being carried
    public void SetCarriedWeapon(WeaponInteractable w)
    {
        carriedWeapon = w;
    }

    // Returns the weapon the player is carrying (if any)
    public WeaponInteractable GetCarriedWeapon()
    {
        return carriedWeapon;
    }

    // Clears the reference to the carried weapon after dropping it
    public void ClearCarriedWeapon()
    {
        carriedWeapon = null;
    }
}