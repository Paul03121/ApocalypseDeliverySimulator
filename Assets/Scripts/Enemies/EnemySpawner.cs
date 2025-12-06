using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    public float respawnDelay = 180f;      // Time required before this spawner is allowed to spawn again
    public float minPlayerDistance = 15f;  // Minimum distance the player must be from the spawner

    private GameObject currentEnemy;       // Reference to the currently spawned enemy
    private float lastSpawnTime = -999f;   // Time when the last enemy was spawned

    private Transform player;

    private void Start()
    {
        // Locate the player in the scene
        player = GameObject.FindWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("Player not found by EnemySpawner");
            enabled = false;
            return;
        }

        TrySpawn(); // Attempt initial spawn
    }

    private void Update()
    {
        TrySpawn();
    }

    private void TrySpawn()
    {
        // Do not spawn if an enemy is already active
        if (currentEnemy != null) return;

        // Respect the respawn cooldown
        if (Time.time - lastSpawnTime < respawnDelay) return;

        // Prevent spawning if the player is too close
        if (Vector3.Distance(transform.position, player.position) < minPlayerDistance) return;

        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        // Create a new enemy at the spawner's position
        currentEnemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
        lastSpawnTime = Time.time;

        // Subscribe to the enemy's death event to know when to free the spawner
        EnemyBase enemy = currentEnemy.GetComponent<EnemyBase>();
        if (enemy != null)
            enemy.OnDeath += HandleEnemyDeath;
    }

    private void HandleEnemyDeath()
    {
        // Clear the reference so the spawner is allowed to create a new enemy
        currentEnemy = null;
    }
}
