using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>Reload Fury: finishing a reload grants a damage boost for a few seconds.</summary>
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Augments/Reload Fury", fileName = "ReloadFuryAugment")]
    public sealed class ReloadFuryAugment : AugmentDefinition
    {
        [SerializeField, Min(1f)] private float damageMultiplier = 1.5f;
        [SerializeField, Min(0f)] private float duration = 10f;

        public override void Apply(AugmentContext context)
        {
            context?.GameManager?.EnableReloadDamageBuff(damageMultiplier, duration);
        }
    }
}
