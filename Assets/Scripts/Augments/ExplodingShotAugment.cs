using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>Exploding Shot: projectiles explode on hit and damage enemies around.</summary>
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Augments/Exploding Shot", fileName = "ExplodingShotAugment")]
    public sealed class ExplodingShotAugment : AugmentDefinition
    {
        [SerializeField, Min(1f)] private float damageMultiplier = 0.2f;
        [SerializeField, Min(0f)] private float radius = 5f;

        public override void Apply(AugmentContext context)
        {
            context?.GameManager?.EnableExplodingShot(radius, damageMultiplier);
        }
    }
}