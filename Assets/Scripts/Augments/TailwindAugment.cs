using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>Rückenwind: the ferry crosses faster, shortening enemy waves.</summary>
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Augments/Tailwind", fileName = "TailwindAugment")]
    public sealed class TailwindAugment : AugmentDefinition
    {
        [SerializeField, Range(0.1f, 1f)] private float crossingFactor = 0.85f;

        public override void Apply(AugmentContext context)
        {
            context?.GameManager?.ApplyCrossingSpeedup(crossingFactor);
        }
    }
}
