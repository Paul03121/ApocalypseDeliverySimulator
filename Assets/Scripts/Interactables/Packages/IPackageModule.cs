public interface IPackageModule
{
    void Initialize(PackageInteractable package); 
    void OnPackagePickedUp(PackageInteractable package);
    void OnPackageDropped(PackageInteractable package);
    void OnPackageDelivered(PackageInteractable package);
}
