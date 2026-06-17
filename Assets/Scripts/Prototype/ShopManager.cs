using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Drives the shop. Holds a catalog of <see cref="UpgradeDefinition"/> assets and a
    /// parallel list of UI buttons; labels/affordability refresh from the catalog, and
    /// purchases go through <see cref="GameManager.TryPurchase"/>. Each upgrade can be
    /// bought up to <see cref="UpgradeDefinition.MaxPurchases"/> times per run.
    /// </summary>
    public sealed class ShopManager : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private List<UpgradeDefinition> catalog = new List<UpgradeDefinition>();
        [SerializeField] private List<Button> buttons = new List<Button>();

        private readonly Dictionary<UpgradeDefinition, int> purchaseCounts = new Dictionary<UpgradeDefinition, int>();

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }
        }

        public void OpenShop()
        {
            RefreshButtons();
        }

        public void Buy(int index)
        {
            if (index < 0 || index >= catalog.Count)
            {
                return;
            }

            UpgradeDefinition definition = catalog[index];
            if (definition == null || GetCount(definition) >= definition.MaxPurchases)
            {
                return;
            }

            if (gameManager != null && gameManager.TryPurchase(definition))
            {
                purchaseCounts[definition] = GetCount(definition) + 1;
                RefreshButtons();
            }
        }

        public void ResetPurchases()
        {
            purchaseCounts.Clear();
        }

        private int GetCount(UpgradeDefinition definition)
        {
            return purchaseCounts.TryGetValue(definition, out int count) ? count : 0;
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

                int count = GetCount(definition);
                bool soldOut = count >= definition.MaxPurchases;
                button.interactable = !soldOut && money >= definition.Cost;

                Text label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = soldOut
                        ? $"{definition.DisplayName} — maxed ({count}/{definition.MaxPurchases})"
                        : $"{definition.DisplayName} (${definition.Cost})  {count}/{definition.MaxPurchases}";
                }
            }
        }
    }
}
