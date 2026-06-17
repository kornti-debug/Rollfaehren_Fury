using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Upgrades/Weapon Damage", fileName = "WeaponDamageUpgrade")]
    public sealed class WeaponDamageUpgrade : UpgradeDefinition
    {
        [SerializeField] private float amount = 10f;

        public override void Apply(UpgradeContext context)
        {
            context?.WeaponSystem?.AddDamageToActive(amount);
        }
    }
}
