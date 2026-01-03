using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    [SerializeField] private FirstPersonCameraController firstPersonCamera;

    void Awake()
    {
        // Search components in parent
        playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
            Debug.LogError("PlayerMovement not found in parent");

        playerHealth = GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
            Debug.LogError("PlayerHealth not found in parent");
    }

    public void StartDeathDrop()
    {
        playerMovement.StartDeathDrop();
    }

    public void DeathAnimationEnded()
    {
        playerHealth.NotifyPlayerDeathEnded();
    }

    public void JumpAnimStarted()
    {
        firstPersonCamera.SetJumpingState(true);
    }

    public void JumpAnimEnded()
    {
        firstPersonCamera.SetJumpingState(false);
    }

    public void AttackMeleeAnimStarted()
    {
        firstPersonCamera.SetAttackingState(true);
    }

    public void AttackMeleeAnimEnded()
    {
        firstPersonCamera.SetAttackingState(false);
    }

    public void AttackKnifeAnimStarted()
    {
        playerMovement.StartAttackRotation();

        if (playerMovement.IsMoving)
            firstPersonCamera.SetAttackingState(true);
    }

    public void AttackKnifeAnimEnded()
    {
        playerMovement.StopAttackRotation();
        firstPersonCamera.SetAttackingState(false);
    }
}
