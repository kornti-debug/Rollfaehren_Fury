using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    public enum AugmentCategory
    {
        Ferry,
        Weapon,
        Player,
        Economy,
        Enemies,
        World
    }

    /// <summary>
    /// Data-driven, polymorphic round-end augment. The draft offers a few of these at
    /// the end of each round; picking one applies its effect for the rest of the run.
    /// </summary>
    public abstract class AugmentDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Augment";
        [SerializeField, TextArea] private string description = string.Empty;
        [SerializeField] private AugmentCategory category = AugmentCategory.World;
        [SerializeField, TextArea] private string benefitText = string.Empty;
        [SerializeField, TextArea] private string drawbackText = string.Empty;
        [SerializeField] private bool unique;

        public string DisplayName => displayName;
        public string Description => description;
        public AugmentCategory Category => category;
        public string BenefitText => string.IsNullOrWhiteSpace(benefitText) ? description : benefitText;
        public string DrawbackText => drawbackText;
        public bool IsRepeatable => !unique;

        /// <summary>Sets the label/description for a runtime-created augment (when the pool adds entries from code rather than assets).</summary>
        public void InitRuntime(
            string augmentName,
            string augmentDescription,
            bool canRepeat = true,
            AugmentCategory augmentCategory = AugmentCategory.World,
            string benefit = null,
            string drawback = null)
        {
            displayName = augmentName;
            description = augmentDescription;
            category = augmentCategory;
            benefitText = string.IsNullOrWhiteSpace(benefit) ? augmentDescription : benefit;
            drawbackText = drawback ?? string.Empty;
            unique = !canRepeat;
        }

        public abstract void Apply(AugmentContext context);
    }
}
