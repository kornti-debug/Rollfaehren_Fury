using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Drives the shop. Holds a catalog of <see cref="UpgradeDefinition"/> assets and builds
    /// one UI button per catalog entry at runtime (cloning the first serialized button as a
    /// template), so the catalog is the single source of truth and upgrades can be added without
    /// touching the scene. Purchases go through <see cref="GameManager.TryPurchase"/>; each upgrade
    /// can be bought up to <see cref="UpgradeDefinition.MaxPurchases"/> times per run.
    /// </summary>
    public sealed class ShopManager : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private List<UpgradeDefinition> catalog = new List<UpgradeDefinition>();
        [SerializeField] private List<Button> buttons = new List<Button>();

        [Header("Weapon Upgrades (added to the catalog at runtime)")]
        [Tooltip("Append the ammo/reload weapon upgrades to the catalog at startup so they appear in the shop.")]
        [SerializeField] private bool addWeaponUpgrades = true;
        [SerializeField] private int magazineUpgradeCost = 20;
        [SerializeField] private int magazineUpgradeRounds = 2;
        [SerializeField] private int reserveUpgradeCost = 15;
        [SerializeField] private int reserveUpgradeMagazines = 2;
        [SerializeField] private int reloadUpgradeCost = 20;
        [SerializeField] private float reloadUpgradeMultiplier = 0.82f;
        [SerializeField] private int resupplyCost = 15;
        [Tooltip("How often Resupply Ammo can be bought per run (it refills magazines + reserve).")]
        [SerializeField] private int resupplyMaxPerRun = 20;

        [Header("Cost Escalation")]
        [Tooltip("Each repeat purchase of the same upgrade multiplies its cost by this (1.7 = +70% per owned copy).")]
        [SerializeField, Min(1f)] private float costGrowthPerPurchase = 1.7f;
        [Tooltip("Only upgrades at or below this max-purchase count escalate. Consumables like Resupply (high max) stay a flat price.")]
        [SerializeField] private int escalatingMaxPurchases = 5;

        // Vertical band (anchored Y) inside the Shop Panel that the generated buttons fill.
        private const float ButtonTopY = 78f;
        private const float ButtonBottomY = -120f;
        private const float ButtonMaxStep = 52f;

        private readonly Dictionary<UpgradeDefinition, int> purchaseCounts = new Dictionary<UpgradeDefinition, int>();
        private bool buttonsBuilt;
        private bool weaponUpgradesAdded;

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }

            AddWeaponUpgrades();
        }

        public void OpenShop()
        {
            EnsureButtons();
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

            if (gameManager != null && gameManager.TryPurchase(definition, GetEffectiveCost(definition)))
            {
                purchaseCounts[definition] = GetCount(definition) + 1;
                RefreshButtons();
            }
        }

        // Cost grows with each copy already owned, so maxing an upgrade costs far more than the
        // first buy. Consumables (high max purchases, e.g. Resupply) keep their flat base price.
        private int GetEffectiveCost(UpgradeDefinition definition)
        {
            int bought = GetCount(definition);
            if (bought <= 0 || definition.MaxPurchases > escalatingMaxPurchases)
            {
                return definition.Cost;
            }

            return Mathf.RoundToInt(definition.Cost * Mathf.Pow(costGrowthPerPurchase, bought));
        }

        public void ResetPurchases()
        {
            purchaseCounts.Clear();
        }

        private int GetCount(UpgradeDefinition definition)
        {
            return purchaseCounts.TryGetValue(definition, out int count) ? count : 0;
        }

        // Appends the ammo/reload weapon upgrades to the catalog as runtime instances, so they
        // show up in the shop without needing separate assets or scene wiring.
        private void AddWeaponUpgrades()
        {
            if (!addWeaponUpgrades || weaponUpgradesAdded)
            {
                return;
            }

            weaponUpgradesAdded = true;

            MagazineUpgrade magazine = ScriptableObject.CreateInstance<MagazineUpgrade>();
            magazine.InitRuntime("Bigger Magazine", "+rounds per magazine (active weapon)", magazineUpgradeCost, 3);
            magazine.SetAmount(magazineUpgradeRounds);
            catalog.Add(magazine);

            ReserveAmmoUpgrade reserve = ScriptableObject.CreateInstance<ReserveAmmoUpgrade>();
            reserve.InitRuntime("Extra Ammo", "+spare magazines / max ammo (active weapon)", reserveUpgradeCost, 3);
            reserve.SetMagazines(reserveUpgradeMagazines);
            catalog.Add(reserve);

            ReloadSpeedUpgrade reload = ScriptableObject.CreateInstance<ReloadSpeedUpgrade>();
            reload.InitRuntime("Faster Reload", "shorter reload time (active weapon)", reloadUpgradeCost, 3);
            reload.SetMultiplier(reloadUpgradeMultiplier);
            catalog.Add(reload);

            ResupplyUpgrade resupply = ScriptableObject.CreateInstance<ResupplyUpgrade>();
            resupply.InitRuntime("Resupply Ammo", "refill all magazines + reserve", resupplyCost, resupplyMaxPerRun);
            catalog.Add(resupply);
        }

        // Builds/lays out one button per catalog entry once, cloning the first serialized button
        // as a visual template and re-wiring each button's click to its own catalog index.
        private void EnsureButtons()
        {
            if (buttonsBuilt)
            {
                return;
            }

            buttonsBuilt = true;

            Button template = buttons.Count > 0 ? buttons[0] : null;
            if (template == null || catalog.Count == 0)
            {
                return;
            }

            Transform parent = template.transform.parent;
            for (int i = buttons.Count; i < catalog.Count; i++)
            {
                Button clone = Instantiate(template, parent);
                clone.name = $"Shop Upgrade Button {i}";
                buttons.Add(clone);
            }

            float step = catalog.Count <= 1
                ? ButtonMaxStep
                : Mathf.Min(ButtonMaxStep, (ButtonTopY - ButtonBottomY) / (catalog.Count - 1));
            float height = Mathf.Clamp(step - 4f, 24f, 48f);

            for (int i = 0; i < buttons.Count; i++)
            {
                Button button = buttons[i];
                if (button == null)
                {
                    continue;
                }

                bool used = i < catalog.Count;
                button.gameObject.SetActive(used);
                if (!used)
                {
                    continue;
                }

                if (button.transform is RectTransform rect)
                {
                    rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = new Vector2(0f, ButtonTopY - i * step);
                    rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
                }

                Text label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.resizeTextForBestFit = true;
                    label.resizeTextMinSize = 10;
                    label.resizeTextMaxSize = 26;
                }

                int index = i;
                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(() => Buy(index));
            }
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
                int cost = GetEffectiveCost(definition);
                bool soldOut = count >= definition.MaxPurchases;
                button.interactable = !soldOut && money >= cost;

                Text label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = soldOut
                        ? $"{definition.DisplayName} — maxed ({count}/{definition.MaxPurchases})"
                        : $"{definition.DisplayName} (${cost})  {count}/{definition.MaxPurchases}";
                }
            }
        }
    }
}
