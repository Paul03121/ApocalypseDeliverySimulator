using UnityEngine;

public class Bat : MeleeWeapon
{
    private void Reset()
    {
        weaponName = "Bat";
        weaponId = 2;
        damage = 9f;
        range = 1.8f;
        cooldown = 1.65f;
    }

    protected override void OnMeleeAttack()
    {
        // TODO: Sound
    }
}
