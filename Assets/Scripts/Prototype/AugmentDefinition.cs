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

        public abstract void Apply(AugmentContext context);
    }
}
