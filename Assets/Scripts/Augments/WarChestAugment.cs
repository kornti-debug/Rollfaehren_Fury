using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>Kriegskasse: a one-off cash injection right now.</summary>
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Augments/War Chest", fileName = "WarChestAugment")]
    public sealed class WarChestAugment : AugmentDefinition
    {
        [SerializeField, Min(0)] private int money = 75;

        public override void Apply(AugmentContext context)
        {
            context?.GameManager?.GrantMoney(money);
        }
    }
}
