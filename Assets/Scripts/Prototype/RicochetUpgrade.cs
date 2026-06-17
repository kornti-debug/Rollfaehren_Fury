using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Pistol master upgrade "Querschläger": hits ricochet to the nearest other enemy.
    /// One-off (not repeatable). Adds bounces to the active weapon.
    /// </summary>
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Upgrades/Ricochet (Master)", fileName = "RicochetUpgrade")]
    public sealed class RicochetUpgrade : UpgradeDefinition
    {
        [SerializeField, Min(1)] private int bounces = 1;

        public override void Apply(UpgradeContext context)
        {
            context?.WeaponSystem?.AddRicochetToActive(bounces);
        }
    }
}
