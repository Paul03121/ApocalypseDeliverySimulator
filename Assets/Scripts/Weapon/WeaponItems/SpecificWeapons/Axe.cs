using UnityEngine;

public class Axe : MeleeWeapon
{
    private void Reset()
    {
        weaponName = "Axe";
        damage = 15f;
        range = 2.2f;
        cooldown = 1.2f;
    }

    protected override void OnMeleeAttack()
    {
        // TODO: Sound

        // TODO: Animation
    }
}
