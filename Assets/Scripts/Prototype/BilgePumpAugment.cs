using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>Bilge Pump: repairs a little ferry health on every enemy kill.</summary>
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Augments/Bilge Pump", fileName = "BilgePumpAugment")]
    public sealed class BilgePumpAugment : AugmentDefinition
    {
        [SerializeField, Min(0)] private int healPerKill = 1;

        public override void Apply(AugmentContext context)
        {
            context?.GameManager?.AddHealPerKill(healPerKill);
        }
    }
}
