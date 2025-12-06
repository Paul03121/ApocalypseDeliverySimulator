using UnityEngine;

public class Bat : MeleeWeapon
{
    private void Reset()
    {
        weaponName = "Bat";
        damage = 10f;
        range = 2f;
        cooldown = 0.8f;
    }

    protected override void OnMeleeAttack()
    {
        // TODO: Sound

        // TODO: Animation
    }
}
