using UnityEngine;

public class PerishablePackageModule : MonoBehaviour, IPackageModule
{
    [Header("Timing")]
    [SerializeField] private float maxDuration = 180f;      // Damage the package after 3 minutes
    private float timer = 0f;
    private bool timerRunning = false;

    private PackageInteractable package;

    public void Initialize(PackageInteractable pkg)
    {
        package = pkg;

        // Start timer when package is generated
        if (!timerRunning)
        {
            timerRunning = true;
            timer = 0f;
            Debug.Log("[PerishableModule] Timer started.");
        }
    }

    public void OnPackagePickedUp(PackageInteractable package) { }

    public void OnPackageDropped(PackageInteractable package) { }

    public void OnPackageDelivered(PackageInteractable package)
    {
        // Stop updating the timer
        timerRunning = false;
    }

    private void Update()
    {
        if (!timerRunning || package.IsDamaged)
            return;

        // Accumulate elapsed time
        timer += Time.deltaTime;

        // Check if the max duration time has been reached
        if (timer >= maxDuration)
        {
            Debug.LogWarning("[PerishableModule] Perishable package expired!");
            package.MarkDamaged();
        }
    }
}
