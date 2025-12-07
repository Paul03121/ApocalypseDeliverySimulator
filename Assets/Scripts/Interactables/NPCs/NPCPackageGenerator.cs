using UnityEngine;

public class NPCPackageGenerator : Interactable
{
    [Header("Package Generation")]
    public GameObject[] packagePrefabs;
    public Vector3 spawnOffset = new Vector3(1f, 0f, 0f);

    protected override void OnInteract()
    {
        // Prevent interaction if no prefabs assigned
        if (packagePrefabs == null || packagePrefabs.Length == 0)
        {
            Debug.LogWarning("[NPCPackageGenerator] No package prefabs assigned.");
            return;
        }

        isInteracted = false;

        SpawnRandomPackage();
    }

    private void SpawnRandomPackage()
    {
        // Pick a random prefab
        int index = Random.Range(0, packagePrefabs.Length);
        GameObject prefab = packagePrefabs[index];

        Vector3 spawnPos = transform.position + transform.TransformDirection(spawnOffset);

        // Spawn the package
        Instantiate(prefab, spawnPos, Quaternion.identity);

        Debug.Log($"[NPCPackageGenerator] Spawned package: {prefab.name}");
    }
}