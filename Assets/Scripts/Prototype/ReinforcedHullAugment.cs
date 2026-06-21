using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>Verstärkter Rumpf: permanently raises the ferry's max health (and heals it).</summary>
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Augments/Reinforced Hull", fileName = "ReinforcedHullAugment")]
    public sealed class ReinforcedHullAugment : AugmentDefinition
    {
        [SerializeField, Min(0f)] private float bonusHealth = 50f;

        public override void Apply(AugmentContext context)
        {
            context?.GameManager?.AddFerryMaxHealth(bonusHealth);
        }
    }
}
