using UnityEngine;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    public sealed class WeaponStatRowView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text labelText;
        [SerializeField] private Text currentValueText;
        [SerializeField] private Text arrowText;
        [SerializeField] private Text previewValueText;

        public void SetContent(
            Sprite sprite,
            Color iconColor,
            string label,
            string currentValue,
            string previewValue = null)
        {
            if (icon != null)
            {
                icon.sprite = sprite;
                icon.color = iconColor;
                icon.enabled = sprite != null;
            }

            if (labelText != null)
            {
                labelText.text = label ?? string.Empty;
                labelText.color = UiTheme.Muted;
            }

            if (currentValueText != null)
            {
                currentValueText.text = currentValue ?? string.Empty;
                currentValueText.color = UiTheme.Foam;
            }

            bool previewing = !string.IsNullOrWhiteSpace(previewValue);
            if (arrowText != null)
            {
                arrowText.text = previewing ? "\u2192" : string.Empty;
                arrowText.color = UiTheme.Muted;
            }

            if (previewValueText != null)
            {
                previewValueText.text = previewing ? previewValue : string.Empty;
                previewValueText.color = UiTheme.Success;
            }
        }
    }
}
