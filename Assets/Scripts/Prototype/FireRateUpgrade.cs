using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Upgrades/Fire Rate", fileName = "FireRateUpgrade")]
    public sealed class FireRateUpgrade : UpgradeDefinition
    {
        [Tooltip("Multiplier applied to the active weapon's cooldown (< 1 = faster).")]
        [SerializeField] private float cooldownMultiplier = 0.82f;

        public override void Apply(UpgradeContext context)
        {
            context?.WeaponSystem?.MultiplyActiveCooldown(cooldownMultiplier);
        }
    }
}
