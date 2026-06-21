using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Upgrades/Magazine Size", fileName = "MagazineUpgrade")]
    public sealed class MagazineUpgrade : UpgradeDefinition
    {
        [Tooltip("Rounds added to the active weapon's magazine (no effect on unlimited weapons).")]
        [SerializeField] private int amount = 2;

        public void SetAmount(int rounds) => amount = rounds;

        public override void Apply(UpgradeContext context)
        {
            context?.WeaponSystem?.AddMagazineSizeToActive(amount);
        }
    }
}
