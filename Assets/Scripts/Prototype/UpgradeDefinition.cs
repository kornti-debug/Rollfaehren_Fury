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
        [Tooltip("How many times this upgrade can be bought per run (base upgrades 3, master 1).")]
        [SerializeField, Min(1)] private int maxPurchases = 3;

        public string DisplayName => displayName;
        public string Description => description;
        public int Cost => cost;
        public int MaxPurchases => Mathf.Max(1, maxPurchases);

        /// <summary>Configures a runtime-created upgrade (used when the shop adds entries from code rather than assets).</summary>
        public void InitRuntime(string upgradeName, string upgradeDescription, int upgradeCost, int upgradeMaxPurchases)
        {
            displayName = upgradeName;
            description = upgradeDescription;
            cost = Mathf.Max(0, upgradeCost);
            maxPurchases = Mathf.Max(1, upgradeMaxPurchases);
        }

        public abstract void Apply(UpgradeContext context);
    }
}
