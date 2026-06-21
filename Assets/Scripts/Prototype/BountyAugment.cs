using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>Kopfgeld: every kill is worth more money for the rest of the run.</summary>
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Augments/Bounty", fileName = "BountyAugment")]
    public sealed class BountyAugment : AugmentDefinition
    {
        [SerializeField, Min(1f)] private float rewardMultiplier = 1.5f;

        public override void Apply(AugmentContext context)
        {
            context?.Spawner?.AddRewardMultiplier(rewardMultiplier);
        }
    }
}
