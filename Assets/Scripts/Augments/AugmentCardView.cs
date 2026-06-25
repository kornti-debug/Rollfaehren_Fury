using UnityEngine;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    public sealed class AugmentCardView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image categoryIcon;
        [SerializeField] private Text categoryText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text benefitText;
        [SerializeField] private Text drawbackText;
        [SerializeField] private GameObject uniqueBadge;

        public Button Button => button;

        public void SetContent(AugmentDefinition augment, Sprite icon, Color accent)
        {
            if (augment == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            if (button != null)
            {
                button.interactable = true;
            }

            if (categoryIcon != null)
            {
                categoryIcon.sprite = icon;
                categoryIcon.color = accent;
                categoryIcon.enabled = icon != null;
            }

            SetText(categoryText, augment.Category.ToString().ToUpperInvariant(), accent);
            SetText(titleText, augment.DisplayName.ToUpperInvariant(), UiTheme.Foam);
            SetText(benefitText, augment.BenefitText, UiTheme.Success);

            bool hasDrawback = !string.IsNullOrWhiteSpace(augment.DrawbackText);
            if (drawbackText != null)
            {
                drawbackText.gameObject.SetActive(hasDrawback);
                SetText(drawbackText, hasDrawback ? augment.DrawbackText : string.Empty, UiTheme.Siren);
            }

            if (uniqueBadge != null)
            {
                uniqueBadge.SetActive(!augment.IsRepeatable);
            }
        }

        private static void SetText(Text target, string value, Color color)
        {
            if (target == null)
            {
                return;
            }

            target.text = value ?? string.Empty;
            target.color = color;
        }
    }
}
