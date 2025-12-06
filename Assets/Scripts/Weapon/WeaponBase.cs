using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("General Stats")]
    public string weaponName = "Weapon";
    public float damage = 20f;
    public float range = 1f;
    public float cooldown = 1f;

    protected float nextAttackTime = 0f;

    public virtual bool CanAttack()
    {
        return Time.time >= nextAttackTime;
    }

    public virtual void RegisterAttack()
    {
        nextAttackTime = Time.time + cooldown;
    }

    // Implemented by child classes
    public abstract void Attack();
}
