using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Upgrades/Weapon Damage", fileName = "WeaponDamageUpgrade")]
    public sealed class WeaponDamageUpgrade : UpgradeDefinition
    {
        [Tooltip("Damage multiplier per purchase (1.25 = +25%). Scales every weapon evenly, including each shotgun pellet.")]
        [SerializeField] private float damageMultiplier = 1.25f;

        public override void Apply(UpgradeContext context)
        {
            context?.WeaponSystem?.MultiplyDamageToActive(damageMultiplier);
        }
    }
}
