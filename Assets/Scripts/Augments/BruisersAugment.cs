using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>Dicke Brocken: fewer enemies, each with more health (good for single-target/harpoon).</summary>
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Augments/Bruisers", fileName = "BruisersAugment")]
    public sealed class BruisersAugment : AugmentDefinition
    {
        [SerializeField] private float countMultiplier = 0.5f;
        [SerializeField] private float healthMultiplier = 2f;

        public override void Apply(AugmentContext context)
        {
            if (context?.Spawner == null)
            {
                return;
            }

            context.Spawner.AddCountMultiplier(countMultiplier);
            context.Spawner.AddHealthMultiplier(healthMultiplier);
        }
    }
}
