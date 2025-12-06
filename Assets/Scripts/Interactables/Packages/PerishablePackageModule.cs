using UnityEngine;

public class PerishablePackageModule : MonoBehaviour, IPackageModule
{
    [SerializeField] private float maxDuration = 10f;

    private float timer = 0f;
    private bool timerStarted = false;
    private bool isExpired = false;

    private PackageInteractable package;

    public void Initialize(PackageInteractable pkg)
    {
        package = pkg;
    }

    public void OnPackagePickedUp(PackageInteractable package)
    {
        // Start timer only the first time the package is picked up
        if (!timerStarted)
        {
            timerStarted = true;
            timer = 0f;
            Debug.Log("[PerishableModule] Timer started.");
        }
    }

    public void OnPackageDropped(PackageInteractable package) { }

    public void OnPackageDelivered(PackageInteractable package)
    {
        // Stop updating the timer
        timerStarted = false;
    }

    private void Update()
    {
        if (!timerStarted || isExpired)
            return;

        // Accumulate elapsed time
        timer += Time.deltaTime;

        // Check if the expiration time has been reached
        if (timer >= maxDuration)
        {
            Debug.LogWarning("[PerishableModule] Perishable package expired!");
            isExpired = true;

            // Destroy the package after expiration
            if (package != null)
                Destroy(package.gameObject);
        }
    }
}
