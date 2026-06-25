using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>Adrenaline: every Nth kill grants a short movement-speed burst.</summary>
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Augments/Adrenaline", fileName = "AdrenalineAugment")]
    public sealed class AdrenalineAugment : AugmentDefinition
    {
        [SerializeField, Min(1)] private int everyKills = 5;
        [SerializeField, Min(1f)] private float speedMultiplier = 1.4f;
        [SerializeField, Min(0.1f)] private float duration = 5f;

        public override void Apply(AugmentContext context)
        {
            context?.GameManager?.EnableKillStreakSpeed(everyKills, speedMultiplier, duration);
        }
    }
}
