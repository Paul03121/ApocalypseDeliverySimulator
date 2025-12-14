using UnityEngine;
using UnityEngine.UI;

public class WeaponIconUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;           // UI Image used to display the weapon icon
    [SerializeField] private Sprite placeholderSprite;  // Fallback icon used when the weapon has no assigned sprite

    public void UpdateWeaponIcon(WeaponBase weapon, bool hasWeapon, bool isEquipped)
    {
        // Hide the icon if the player has no weapon
        if (!hasWeapon || weapon == null)
        {
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0f);
            return;
        }

        // Show the icon if the player has a weapon
        Sprite sprite = weapon.IconSprite != null
            ? weapon.IconSprite
            : placeholderSprite;

        iconImage.sprite = sprite;

        // Change icon opacity depending on equip state
        float alpha = isEquipped ? 1f : 0.3f;
        iconImage.color = new Color(1, 1, 1, alpha);
    }
}