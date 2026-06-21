using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Upgrades/Resupply Ammo", fileName = "ResupplyUpgrade")]
    public sealed class ResupplyUpgrade : UpgradeDefinition
    {
        public override void Apply(UpgradeContext context)
        {
            context?.WeaponSystem?.RefillAllAmmo();
        }
    }
}
