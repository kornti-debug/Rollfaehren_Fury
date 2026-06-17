using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>Der Schwarm: more enemies, each with less health (good for AoE/spread).</summary>
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Augments/Swarm", fileName = "SwarmAugment")]
    public sealed class SwarmAugment : AugmentDefinition
    {
        [SerializeField] private float countMultiplier = 2f;
        [SerializeField] private float healthMultiplier = 0.5f;

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
