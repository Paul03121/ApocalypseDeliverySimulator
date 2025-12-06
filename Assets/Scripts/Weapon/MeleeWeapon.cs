using UnityEngine;

public abstract class MeleeWeapon : WeaponBase
{
    public override void Attack()
    {
        // Register attack
        RegisterAttack();

        // Trigger subclass attack behavior
        OnMeleeAttack();
    }

    // Implemented by child classes
    protected abstract void OnMeleeAttack();
}