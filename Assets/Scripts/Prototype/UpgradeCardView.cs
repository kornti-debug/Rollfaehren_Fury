using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    public sealed class UpgradeCardView : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        ISelectHandler,
        IDeselectHandler
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private Text titleText;
        [SerializeField] private Text currentValueText;
        [SerializeField] private Text arrowText;
        [SerializeField] private Text nextValueText;
        [SerializeField] private Text levelText;
        [SerializeField] private GameObject costRoot;
        [SerializeField] private Image costIcon;
        [SerializeField] private Text costText;

        private bool pointerInside;
        private bool selected;
        private bool previewActive;

        public Button Button => button;
        public event Action PreviewEntered;
        public event Action PreviewExited;

        public void SetContent(
            Sprite sprite,
            Color iconColor,
            string title,
            string currentValue,
            string nextValue,
            string level,
            string cost,
            bool showCost,
            bool interactable,
            bool maxed)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }

            if (icon != null)
            {
                icon.sprite = sprite;
                icon.color = maxed ? UiTheme.Muted : iconColor;
                icon.enabled = sprite != null;
            }

            SetText(titleText, title, maxed ? UiTheme.Muted : UiTheme.Foam);
            SetText(currentValueText, currentValue, maxed ? UiTheme.Muted : UiTheme.Foam);

            bool hasNextValue = !string.IsNullOrWhiteSpace(nextValue);
            SetText(arrowText, hasNextValue ? "\u2192" : string.Empty, UiTheme.Muted);
            SetText(nextValueText, nextValue, maxed ? UiTheme.Muted : UiTheme.Success);
            SetText(levelText, level, maxed ? UiTheme.Success : UiTheme.Muted);
            SetText(costText, cost, UiTheme.Warning);

            if (costRoot != null)
            {
                costRoot.SetActive(showCost);
            }

            if (costIcon != null)
            {
                costIcon.color = UiTheme.Warning;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            pointerInside = true;
            UpdatePreviewState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerInside = false;
            UpdatePreviewState();
        }

        public void OnSelect(BaseEventData eventData)
        {
            selected = true;
            UpdatePreviewState();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            selected = false;
            UpdatePreviewState();
        }

        private void OnDisable()
        {
            pointerInside = false;
            selected = false;
            if (previewActive)
            {
                previewActive = false;
                PreviewExited?.Invoke();
            }
        }

        private void UpdatePreviewState()
        {
            bool shouldPreview = pointerInside || selected;
            if (shouldPreview == previewActive)
            {
                return;
            }

            previewActive = shouldPreview;
            if (previewActive)
            {
                PreviewEntered?.Invoke();
            }
            else
            {
                PreviewExited?.Invoke();
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
