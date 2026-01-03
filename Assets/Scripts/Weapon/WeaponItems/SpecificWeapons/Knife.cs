using UnityEngine;

public class Knife : MeleeWeapon
{
    private void Reset()
    {
        weaponName = "Knife";
        weaponId = 1;
        damage = 6.5f;
        range = 1.5f;
        cooldown = 1.3f;
    }

    protected override void OnMeleeAttack()
    {
        // TODO: Sound
    }
}
