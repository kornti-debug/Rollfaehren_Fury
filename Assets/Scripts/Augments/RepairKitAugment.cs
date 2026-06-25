using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>Flickzeug: heals a fraction of the ferry's max health at the end of each round.</summary>
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Augments/Repair Kit", fileName = "RepairKitAugment")]
    public sealed class RepairKitAugment : AugmentDefinition
    {
        [SerializeField, Range(0f, 1f)] private float healFraction = 0.05f;

        public override void Apply(AugmentContext context)
        {
            context?.GameManager?.AddPerRoundHeal(healFraction);
        }
    }
}
