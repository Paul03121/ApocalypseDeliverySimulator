using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("General Stats")]
    [SerializeField] protected string weaponName = "Weapon";
    [SerializeField] protected int weaponId = 0;
    [SerializeField] protected float damage = 20f;
    [SerializeField] protected float range = 1f;
    [SerializeField] protected float cooldown = 1f;

    protected float nextAttackTime = 0f;

    [Header("UI")]
    [SerializeField] private Sprite iconSprite;

    public string WeaponName => weaponName;
    public int WeaponId => weaponId;
    public float Damage => damage;
    public float Range => range;
    public float Cooldown => cooldown;
    public Sprite IconSprite => iconSprite;

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
