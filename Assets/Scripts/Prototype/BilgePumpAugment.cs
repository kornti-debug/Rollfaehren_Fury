using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>Bilge Pump: repairs a little ferry health on every enemy kill.</summary>
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Augments/Bilge Pump", fileName = "BilgePumpAugment")]
    public sealed class BilgePumpAugment : AugmentDefinition
    {
        [SerializeField, Min(0f)] private float healPerKill = 0.5f;
        [SerializeField, Min(0f)] private float maxHealPerCrossing = 10f;

        public override void Apply(AugmentContext context)
        {
            context?.GameManager?.AddHealPerKill(healPerKill, maxHealPerCrossing);
        }
    }
}
