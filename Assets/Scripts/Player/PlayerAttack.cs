using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public LayerMask enemyLayer;

    [Header("References")]
    private Animator animator;
    private WeaponHolder weaponHolder;
    private PlayerHealth playerHealth;
    
    private bool isBlocked = false;

    private void Awake()
    {
        weaponHolder = GetComponent<WeaponHolder>();
        animator = GetComponentInChildren<Animator>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // Block if player is dead
        if (isBlocked) return;

        // Stop working if game is paused or if player died
        if (GameStateManager.Instance.IsPaused || GameStateManager.Instance.IsGameOver)
            return;

        // If no weapon holder is available or no weapon is equipped
        if (weaponHolder == null || !weaponHolder.IsWeaponEquipped)
            return;

        // Get the interactable weapon currently equipped
        WeaponInteractable interactableWeapon = weaponHolder.EquippedWeapon;
        if (interactableWeapon == null)
            return;

        // Get the actual weapon behavior script
        WeaponBase weapon = interactableWeapon.GetComponent<WeaponBase>();
        if (weapon == null)
        {
            Debug.LogError("Equipped weapon does not contain a WeaponBase component.");
            return;
        }

        // Attack input check
        if (Input.GetMouseButtonDown(0) && weapon.CanAttack())
        {
            PerformAttack(weapon);
        }
    }

    void OnEnable()
    {
        playerHealth.OnPlayerDeathStarted += HandlePlayerDeathStarted;
        playerHealth.OnPlayerDeathEnded += HandlePlayerDeathEnded;
    }

    void OnDisable()
    {
        playerHealth.OnPlayerDeathStarted -= HandlePlayerDeathStarted;
        playerHealth.OnPlayerDeathEnded -= HandlePlayerDeathEnded;
    }

    private void HandlePlayerDeathStarted()
    {
        isBlocked = true;
    }

    private void HandlePlayerDeathEnded()
    {
        isBlocked = false;
    }

    private void PerformAttack(WeaponBase weapon)
    {
        // Notify animator
        animator.SetTrigger("AttackTrigger");

        // Trigger the weapon's internal attack logic
        weapon.Attack();

        // Get ray origin and direction from the first person camera
        Camera cam = weaponHolder.playerCamera;
        Vector3 origin = cam.transform.position;
        Vector3 direction = cam.transform.forward;

        // Perform raycast
        if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, weapon.Range, enemyLayer))
        {
            EnemyHealth enemyHealth = hitInfo.collider.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(weapon.Damage);
                Debug.Log($"{weapon.Damage} damage applied to {hitInfo.collider.name}. Remaining health: {enemyHealth.GetCurrentHealth()}");
            }
            else
            {
                Debug.LogWarning($"Hit object ({hitInfo.collider.name}) does not contain an EnemyHealth component.");
            }
        }
        else
        {
            Debug.Log("The attack did not hit any enemy.");
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && weaponHolder.IsWeaponEquipped)
        {
            WeaponInteractable interactableWeapon = weaponHolder.EquippedWeapon;
            WeaponBase weapon = interactableWeapon.GetComponent<WeaponBase>();

            Camera cam = weaponHolder.playerCamera;
            Vector3 origin = cam.transform.position;
            Vector3 direction = cam.transform.forward;

            if (origin == Vector3.zero)
                return;

            // Draw weapon range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin, weapon.Range);

            // Draw the attack ray
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, origin + direction * weapon.Range);

            // Draw hit endpoint indicator
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(origin + direction * weapon.Range, 0.05f);
        }
    }
}