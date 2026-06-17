using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Drives the between-round shop. Holds a catalog of <see cref="UpgradeDefinition"/>
    /// assets and a parallel list of UI buttons; labels/affordability are refreshed from
    /// the catalog, and purchases go through <see cref="GameManager.TryPurchase"/>.
    /// </summary>
    public sealed class ShopManager : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private List<UpgradeDefinition> catalog = new List<UpgradeDefinition>();
        [SerializeField] private List<Button> buttons = new List<Button>();

        private readonly HashSet<UpgradeDefinition> purchasedOneOffs = new HashSet<UpgradeDefinition>();

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }
        }

        /// <summary>Called by GameManager when the shop opens (round complete).</summary>
        public void OpenShop()
        {
            RefreshButtons();
        }

        /// <summary>Bound to shop button i (UnityEvent int listener) by the scene builder.</summary>
        public void Buy(int index)
        {
            if (index < 0 || index >= catalog.Count)
            {
                return;
            }

            UpgradeDefinition definition = catalog[index];
            if (definition == null)
            {
                return;
            }

            if (!definition.Repeatable && purchasedOneOffs.Contains(definition))
            {
                return;
            }

            if (gameManager != null && gameManager.TryPurchase(definition))
            {
                if (!definition.Repeatable)
                {
                    purchasedOneOffs.Add(definition);
                }

                RefreshButtons();
            }
        }

        /// <summary>Reset one-off purchases for a fresh run.</summary>
        public void ResetPurchases()
        {
            purchasedOneOffs.Clear();
        }

        private void RefreshButtons()
        {
            int money = gameManager != null ? gameManager.Money : 0;

            for (int i = 0; i < buttons.Count; i++)
            {
                Button button = buttons[i];
                if (button == null)
                {
                    continue;
                }

                if (i >= catalog.Count || catalog[i] == null)
                {
                    button.gameObject.SetActive(false);
                    continue;
                }

                UpgradeDefinition definition = catalog[i];
                button.gameObject.SetActive(true);

                bool soldOut = !definition.Repeatable && purchasedOneOffs.Contains(definition);
                button.interactable = !soldOut && money >= definition.Cost;

                Text label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = soldOut
                        ? $"{definition.DisplayName} — owned"
                        : $"{definition.DisplayName} (${definition.Cost})";
                }
            }
        }
    }
}
