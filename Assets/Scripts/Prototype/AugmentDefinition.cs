using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Data-driven, polymorphic round-end augment. The draft offers a few of these at
    /// the end of each round; picking one applies its effect for the rest of the run.
    /// </summary>
    public abstract class AugmentDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Augment";
        [SerializeField, TextArea] private string description = string.Empty;

        public string DisplayName => displayName;
        public string Description => description;

        /// <summary>Sets the label/description for a runtime-created augment (when the pool adds entries from code rather than assets).</summary>
        public void InitRuntime(string augmentName, string augmentDescription)
        {
            displayName = augmentName;
            description = augmentDescription;
        }

        public abstract void Apply(AugmentContext context);
    }
}
