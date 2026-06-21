using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Upgrades/Reserve Ammo", fileName = "ReserveAmmoUpgrade")]
    public sealed class ReserveAmmoUpgrade : UpgradeDefinition
    {
        [Tooltip("Spare magazines added to the active weapon's reserve / max ammo (no effect on unlimited weapons).")]
        [SerializeField] private int magazines = 2;

        public void SetMagazines(int spareMagazines) => magazines = spareMagazines;

        public override void Apply(UpgradeContext context)
        {
            context?.WeaponSystem?.AddReserveMagazinesToActive(magazines);
        }
    }
}
