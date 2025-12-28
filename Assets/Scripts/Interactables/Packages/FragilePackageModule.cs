using UnityEngine;

public class FragilePackageModule : MonoBehaviour, IPackageModule
{
    private PackageInteractable package;

    public void Initialize(PackageInteractable pkg)
    {
        // Cache package reference and subscribe to hit event
        package = pkg;
        package.OnHitReceived += OnPackageHit;
    }

    public void OnPackagePickedUp(PackageInteractable package) { }

    public void OnPackageDropped(PackageInteractable package) { }

    public void OnPackageDelivered(PackageInteractable package)
    {
        // Unsubscribe once the package is delivered
        Unsubscribe();
    }

    private void OnDestroy()
    {
        // Unsubscribe if package is destroyed
        Unsubscribe();
    }

    private void Unsubscribe()
    {
        if (package != null)
            package.OnHitReceived -= OnPackageHit;
    }

    private void OnPackageHit(int hits)
    {
        // Fragile packages break on the first hit
        if (package.IsDamaged)
            return;

        Debug.LogWarning("[FragilePackage] Broken on first hit");
        package.MarkDamaged();

        // No further hit processing needed once damaged
        Unsubscribe();
    }
}