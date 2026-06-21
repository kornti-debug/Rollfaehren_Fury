using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>Rapid Reload: permanently shortens every weapon's reload time.</summary>
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Augments/Rapid Reload", fileName = "RapidReloadAugment")]
    public sealed class RapidReloadAugment : AugmentDefinition
    {
        [Tooltip("Multiplier on reload time (0.7 = 30% faster).")]
        [SerializeField, Range(0.1f, 1f)] private float reloadMultiplier = 0.7f;

        public override void Apply(AugmentContext context)
        {
            context?.GameManager?.MultiplyWeaponReload(reloadMultiplier);
        }
    }
}
