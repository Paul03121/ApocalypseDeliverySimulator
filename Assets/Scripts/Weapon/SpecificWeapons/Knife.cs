using UnityEngine;

public class Knife : MeleeWeapon
{
    private void Reset()
    {
        weaponName = "Knife";
        damage = 5f;
        range = 1.5f;
        cooldown = 0.5f;
    }

    protected override void OnMeleeAttack()
    {
        // TODO: Sound

        // TODO: Animation
    }
}
