using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Upgrades/Reload Speed", fileName = "ReloadSpeedUpgrade")]
    public sealed class ReloadSpeedUpgrade : UpgradeDefinition
    {
        [Tooltip("Multiplier applied to the active weapon's reload time (< 1 = faster reload).")]
        [SerializeField] private float reloadMultiplier = 0.8f;

        public void SetMultiplier(float multiplier) => reloadMultiplier = multiplier;

        public override void Apply(UpgradeContext context)
        {
            context?.WeaponSystem?.MultiplyActiveReloadDuration(reloadMultiplier);
        }
    }
}
