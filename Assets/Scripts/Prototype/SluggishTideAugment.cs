using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>Träge Strömung: all enemies move slower for the rest of the run.</summary>
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Augments/Sluggish Tide", fileName = "SluggishTideAugment")]
    public sealed class SluggishTideAugment : AugmentDefinition
    {
        [SerializeField, Range(0.1f, 1f)] private float speedMultiplier = 0.8f;

        public override void Apply(AugmentContext context)
        {
            context?.Spawner?.AddSpeedMultiplier(speedMultiplier);
        }
    }
}
