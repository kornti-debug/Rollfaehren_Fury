using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Upgrades/Ferry Health", fileName = "FerryHealthUpgrade")]
    public sealed class FerryHealthUpgrade : UpgradeDefinition
    {
        [SerializeField] private float amount = 25f;

        public override void Apply(UpgradeContext context)
        {
            Health ferry = context?.FerryHealth;
            if (ferry != null)
            {
                ferry.SetMaxHealth(ferry.MaxHealth + amount, true);
            }
        }
    }
}
