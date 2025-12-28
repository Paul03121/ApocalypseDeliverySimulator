using UnityEngine;

public class PackageInteractable : Interactable
{
    [Header("State")]
    public bool isBeingHeld = false;

    [Header("Damage")]
    private int hitsReceived;
    private bool isDamaged = false;

    [Header("References")]
    private PackageHolder holder;
    private PlayerHealth playerHealth;
    private float lastPlayerHealth;

    private bool isSubscribed = false;

    private IPackageModule[] modules;

    public int HitsReceived => hitsReceived;
    public bool IsDamaged => isDamaged;

    public event System.Action<int> OnHitReceived;

    protected override void Awake()
    {
        base.Awake();

        // Detect modules attached to the package and initialize them
        modules = GetComponents<IPackageModule>();
        foreach (var module in modules)
            module.Initialize(this);
    }

    protected override void OnInteract()
    {
        // Retrieve PackageHolder only when first needed
        if (holder == null)
            holder = FindObjectOfType<PackageHolder>();

        if (holder == null)
        {
            Debug.LogError("PackageHolder not found");
            return;
        }

        // Reset to allow repeated interactions
        isInteracted = false;

        if (!isBeingHeld)
            PickUp();
    }

    private void PickUp()
    {
        isBeingHeld = true;
        isInteractionDisabled = true;   // Disable interaction while being carried

        // TODO: Add pickup sound effect

        SubscribeToDamage();
        holder.PickUp(this);

        // Notify modules
        NotifyPickedUp();
    }

    public void Drop()
    {
        isBeingHeld = false;
        isInteractionDisabled = false;  // Re-enable interaction once dropped

        // TODO: Add drop sound effect

        UnsubscribeFromDamage();
        holder.Drop();

        // Notify modules
        NotifyDropped();
    }

    public void Deliver()
    {
        isBeingHeld = false;
        isInteractionDisabled = false;  // Re-enable interaction once delivered

        // TODO: Add delivered sound effect

        UnsubscribeFromDamage();
        holder.Deliver();

        // Notify modules
        NotifyDelivered();
    }

    public void NotifyPickedUp()
    {
        foreach (var module in modules)
            module.OnPackagePickedUp(this);
    }

    public void NotifyDropped()
    {
        foreach (var module in modules)
            module.OnPackageDropped(this);
    }

    public void NotifyDelivered()
    {
        foreach (var module in modules)
            module.OnPackageDelivered(this);
    }

    private void SubscribeToDamage()
    {
        if (isSubscribed)
            return;

        // Find player health component
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("[Package] PlayerHealth not found");
            return;
        }

        // Subscribe to health change events to detect damage
        lastPlayerHealth = playerHealth.CurrentHealth;
        playerHealth.OnHealthChanged += OnDamageReceived;
        isSubscribed = true;
    }

    private void UnsubscribeFromDamage()
    {
        if (!isSubscribed || playerHealth == null)
            return;

        playerHealth.OnHealthChanged -= OnDamageReceived;
        isSubscribed = false;
    }

    private void OnDamageReceived(float current, float max)
    {
        if (!isBeingHeld)
            return;

        // If player health decreased, register a hit on the package
        if (current < lastPlayerHealth)
        {
            RegisterHit();
        }

        lastPlayerHealth = current;
    }

    public void RegisterHit()
    {
        if(isDamaged)
            return;

        hitsReceived++;

        // TODO: Play damage sound

        OnHitReceived?.Invoke(hitsReceived);

        // Mark as damaged after 10 hits
        if (hitsReceived >= 10)
            MarkDamaged();
    }

    public void MarkDamaged()
    {
        if (isDamaged)
            return;

        isDamaged = true;

        Debug.LogWarning("[Package] Package contents damaged");

        // TODO: Swap to damaged model
    }

    public float GetIntegrity()
    {
        if (isDamaged)
            return 0f;

        // Calculate integrity as percentage based on hits (1 hit = -10%)
        return Mathf.Clamp01(1f - hitsReceived * 0.1f);
    }
}