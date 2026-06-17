using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Data-driven, polymorphic shop upgrade. Concrete subclasses define the effect in
    /// <see cref="Apply"/>; the shop only knows name/cost/repeatable. This lets simple
    /// stat upgrades and exotic "master" upgrades (e.g. ricochet) live side by side.
    /// </summary>
    public abstract class UpgradeDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Upgrade";
        [SerializeField, TextArea] private string description = string.Empty;
        [SerializeField] private int cost = 50;
        [Tooltip("Repeatable base upgrades can be bought every round; one-off 'master' upgrades cannot.")]
        [SerializeField] private bool repeatable = true;

        public string DisplayName => displayName;
        public string Description => description;
        public int Cost => cost;
        public bool Repeatable => repeatable;

        public abstract void Apply(UpgradeContext context);
    }
}
