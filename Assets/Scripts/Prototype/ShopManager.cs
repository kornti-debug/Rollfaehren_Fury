using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    public sealed class ShopManager : MonoBehaviour
    {
        private enum UpgradeKind
        {
            Unlock,
            Damage,
            FireRate,
            ReserveCapacity,
            Reload,
            Ricochet
        }

        [Header("Runtime")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private WeaponSystem weaponSystem;

        [Header("Authored Shop UI")]
        [SerializeField] private List<Button> weaponTabs = new List<Button>();
        [SerializeField] private List<UpgradeCardView> upgradeCards = new List<UpgradeCardView>();
        [SerializeField] private UpgradeCardView refillCard;
        [SerializeField] private Text selectedWeaponNameText;
        [SerializeField] private Text selectedWeaponRequirementText;
        [SerializeField] private Image selectedWeaponAccent;
        [SerializeField] private WeaponStatRowView damageStatRow;
        [SerializeField] private WeaponStatRowView fireRateStatRow;
        [SerializeField] private WeaponStatRowView reloadStatRow;
        [SerializeField] private WeaponStatRowView reserveStatRow;
        [SerializeField] private WeaponStatRowView specialStatRow;

        [Header("Upgrade Icons")]
        [SerializeField] private Sprite unlockIcon;
        [SerializeField] private Sprite damageIcon;
        [SerializeField] private Sprite fireRateIcon;
        [SerializeField] private Sprite reloadIcon;
        [SerializeField] private Sprite magazineIcon;
        [SerializeField] private Sprite ricochetIcon;
        [SerializeField] private Sprite refillIcon;

        [Header("Economy")]
        [SerializeField, Min(1f)] private float costGrowthPerPurchase = 1.7f;
        [SerializeField] private int refillCost = 20;

        private readonly Dictionary<(int target, UpgradeKind kind), int> levels =
            new Dictionary<(int, UpgradeKind), int>();
        private readonly List<UpgradeKind> currentKinds = new List<UpgradeKind>();

        private bool uiBound;
        private int selectedTarget;
        private UpgradeKind? previewKind;

        private void Awake()
        {
            gameManager ??= FindFirstObjectByType<GameManager>();
            weaponSystem ??= FindFirstObjectByType<WeaponSystem>();
            BindUi();
        }

        public void OpenShop()
        {
            BindUi();
            if (weaponSystem != null && weaponSystem.WeaponCount > 0)
            {
                SelectTarget(Mathf.Clamp(selectedTarget, 0, weaponSystem.WeaponCount - 1));
            }
        }

        public void ResetPurchases()
        {
            levels.Clear();
            selectedTarget = 0;
            RefreshTree();
        }

        // Kept for serialized listeners from the retired flat shop.
        public void Buy(int index)
        {
        }

        private void BindUi()
        {
            if (uiBound)
            {
                return;
            }

            uiBound = true;
            for (int i = 0; i < weaponTabs.Count; i++)
            {
                Button tab = weaponTabs[i];
                if (tab == null)
                {
                    continue;
                }

                int captured = i;
                tab.onClick.RemoveAllListeners();
                tab.onClick.AddListener(() => SelectTarget(captured));
            }

            for (int i = 0; i < upgradeCards.Count; i++)
            {
                UpgradeCardView card = upgradeCards[i];
                if (card == null || card.Button == null)
                {
                    continue;
                }

                int captured = i;
                card.Button.onClick.RemoveAllListeners();
                card.Button.onClick.AddListener(() => BuyUpgradeSlot(captured));
                card.PreviewEntered += () => PreviewUpgradeSlot(captured);
                card.PreviewExited += () => ClearPreview(captured);
            }

            if (refillCard != null && refillCard.Button != null)
            {
                refillCard.Button.onClick.RemoveAllListeners();
                refillCard.Button.onClick.AddListener(BuyRefill);
            }
        }

        private void SelectTarget(int target)
        {
            if (weaponSystem == null || target < 0 || target >= weaponSystem.WeaponCount)
            {
                return;
            }

            selectedTarget = target;
            previewKind = null;
            currentKinds.Clear();
            currentKinds.AddRange(GetKindsFor(target));
            RefreshTree();
        }

        private void RefreshTree()
        {
            if (weaponSystem == null)
            {
                return;
            }

            RefreshWeaponTabs();
            Weapon weapon = weaponSystem.WeaponAt(selectedTarget);
            if (weapon == null)
            {
                return;
            }

            bool unlocked = weaponSystem.IsWeaponUnlocked(selectedTarget);
            RefreshSummary(weapon, unlocked);

            int money = gameManager != null ? gameManager.Money : 0;
            for (int i = 0; i < upgradeCards.Count; i++)
            {
                UpgradeCardView card = upgradeCards[i];
                if (card == null)
                {
                    continue;
                }

                bool used = i < currentKinds.Count;
                card.gameObject.SetActive(used);
                if (!used)
                {
                    continue;
                }

                RefreshCard(card, weapon, currentKinds[i], money);
            }

            RefreshRefill(weapon, unlocked, money);
        }

        private void RefreshWeaponTabs()
        {
            for (int i = 0; i < weaponTabs.Count; i++)
            {
                Button tab = weaponTabs[i];
                Weapon weapon = weaponSystem.WeaponAt(i);
                if (tab == null || weapon == null)
                {
                    continue;
                }

                bool unlocked = weaponSystem.IsWeaponUnlocked(i);
                Text label = tab.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = unlocked
                        ? weapon.DisplayName
                        : $"{weapon.DisplayName}\nLOCKED";
                }

                Image background = tab.GetComponent<Image>();
                if (background != null)
                {
                    background.color = i == selectedTarget
                        ? UiTheme.Warning
                        : unlocked
                            ? UiTheme.HullSoft
                            : UiTheme.WithAlpha(UiTheme.Hull, 0.92f);
                }
            }
        }

        private void RefreshSummary(Weapon weapon, bool unlocked)
        {
            SetText(selectedWeaponNameText, weapon.DisplayName);

            string requirement = unlocked ? "OWNED" : BuildUnlockRequirement(weapon);
            SetText(selectedWeaponRequirementText, requirement);
            if (selectedWeaponAccent != null)
            {
                selectedWeaponAccent.color = unlocked ? UiTheme.Success : UiTheme.Warning;
            }

            UpgradeKind? activePreview = ValidPreviewKind(weapon, unlocked);
            SetStatRows(weapon, activePreview);
        }

        private void RefreshCard(UpgradeCardView card, Weapon weapon, UpgradeKind kind, int money)
        {
            Image background = card.GetComponent<Image>();

            if (kind == UpgradeKind.Unlock)
            {
                WeaponDefinition definition = weapon.Definition;
                bool canUnlock = gameManager != null
                                 && definition != null
                                 && weaponSystem.CanUnlockWeapon(selectedTarget, gameManager.Round);
                int unlockCost = definition != null ? definition.UnlockPrice : 0;
                bool unlockInteractable = canUnlock && money >= unlockCost;
                card.SetContent(
                    unlockIcon,
                    UiTheme.Warning,
                    "UNLOCK WEAPON",
                    BuildUnlockRequirement(weapon),
                    string.Empty,
                    "LOCKED",
                    $"${unlockCost}",
                    true,
                    unlockInteractable,
                    false);
                SetCardColor(background, unlockInteractable, false);
                return;
            }

            int level = GetLevel(selectedTarget, kind);
            int max = KindMaxLevel(kind);
            int cost = EffectiveCost(kind, level);
            bool maxed = level >= max;
            bool interactable = !maxed && money >= cost;
            UpgradeValues(weapon, kind, out string currentValue, out string nextValue);
            card.SetContent(
                IconFor(kind),
                AccentFor(kind),
                KindLabel(kind),
                currentValue,
                maxed ? string.Empty : nextValue,
                maxed ? "MAX" : $"LV {level} / {max}",
                $"${cost}",
                !maxed,
                interactable,
                maxed);
            SetCardColor(background, interactable, maxed);
        }

        private void RefreshRefill(Weapon weapon, bool unlocked, int money)
        {
            if (refillCard == null)
            {
                return;
            }

            bool visible = unlocked && !weapon.HasInfiniteAmmo;
            refillCard.gameObject.SetActive(visible);
            if (!visible)
            {
                return;
            }

            bool full = weapon.IsAmmoFull;
            bool interactable = !full && money >= refillCost;
            refillCard.SetContent(
                refillIcon,
                UiTheme.Progress,
                "REFILL AMMO",
                $"AMMO {weapon.CurrentAmmo}/{weapon.MagazineSize}",
                full ? "FULL" : $"RESERVE {weapon.ReserveAmmo}/{weapon.MaxReserveAmmo}",
                full ? "FULL" : "SUPPLY",
                $"${refillCost}",
                !full,
                interactable,
                full);
            SetCardColor(refillCard.GetComponent<Image>(), interactable, full);
        }

        private void BuyUpgradeSlot(int slot)
        {
            if (slot < 0 || slot >= currentKinds.Count || gameManager == null)
            {
                return;
            }

            Weapon weapon = weaponSystem != null ? weaponSystem.WeaponAt(selectedTarget) : null;
            if (weapon == null)
            {
                return;
            }

            UpgradeKind kind = currentKinds[slot];
            if (kind == UpgradeKind.Unlock)
            {
                BuyWeaponUnlock(weapon);
                return;
            }

            int level = GetLevel(selectedTarget, kind);
            if (level >= KindMaxLevel(kind))
            {
                return;
            }

            int cost = EffectiveCost(kind, level);
            if (!gameManager.TrySpendMoney(cost))
            {
                RefreshTree();
                return;
            }

            ApplyUpgrade(weapon, kind);
            levels[(selectedTarget, kind)] = level + 1;
            RefreshTree();
        }

        private void BuyRefill()
        {
            Weapon weapon = weaponSystem != null ? weaponSystem.WeaponAt(selectedTarget) : null;
            if (weapon == null
                || gameManager == null
                || weapon.IsAmmoFull
                || !gameManager.TrySpendMoney(refillCost))
            {
                RefreshTree();
                return;
            }

            weapon.RefillAmmo();
            RefreshTree();
        }

        private void BuyWeaponUnlock(Weapon weapon)
        {
            WeaponDefinition definition = weapon.Definition;
            if (definition == null
                || gameManager == null
                || !weaponSystem.CanUnlockWeapon(selectedTarget, gameManager.Round)
                || !gameManager.TrySpendMoney(definition.UnlockPrice))
            {
                RefreshTree();
                return;
            }

            if (!weaponSystem.TryUnlockWeapon(selectedTarget))
            {
                Debug.LogError($"Weapon '{weapon.DisplayName}' could not be unlocked.", this);
                return;
            }

            SelectTarget(selectedTarget);
        }

        private IEnumerable<UpgradeKind> GetKindsFor(int target)
        {
            Weapon weapon = weaponSystem != null ? weaponSystem.WeaponAt(target) : null;
            if (weapon == null)
            {
                yield break;
            }

            if (!weaponSystem.IsWeaponUnlocked(target))
            {
                yield return UpgradeKind.Unlock;
                yield break;
            }

            yield return UpgradeKind.Damage;
            yield return UpgradeKind.FireRate;

            if (weapon.HasInfiniteAmmo)
            {
                if (weapon.DisplayName == "Harpoon")
                {
                    yield return UpgradeKind.Ricochet;
                }

                yield break;
            }

            yield return UpgradeKind.Reload;
            yield return UpgradeKind.ReserveCapacity;
        }

        private static void ApplyUpgrade(Weapon weapon, UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Damage:
                    weapon.MultiplyDamage(1.2f);
                    break;
                case UpgradeKind.FireRate:
                    weapon.MultiplyCooldown(0.82f);
                    break;
                case UpgradeKind.ReserveCapacity:
                    weapon.AddReserveMagazines(1);
                    break;
                case UpgradeKind.Reload:
                    weapon.MultiplyReloadDuration(0.82f);
                    break;
                case UpgradeKind.Ricochet:
                    weapon.AddRicochet(1);
                    break;
            }
        }

        private static string KindLabel(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Damage:
                    return "DAMAGE";
                case UpgradeKind.FireRate:
                    return "FIRE RATE";
                case UpgradeKind.ReserveCapacity:
                    return "EXTRA MAGAZINE";
                case UpgradeKind.Reload:
                    return "RELOAD SPEED";
                case UpgradeKind.Ricochet:
                    return "RICOCHET";
                default:
                    return kind.ToString().ToUpperInvariant();
            }
        }

        private static void UpgradeValues(
            Weapon weapon,
            UpgradeKind kind,
            out string currentValue,
            out string nextValue)
        {
            switch (kind)
            {
                case UpgradeKind.Damage:
                    currentValue = weapon.DamageDisplay;
                    nextValue = FormatDamage(weapon, weapon.Damage * 1.2f);
                    return;
                case UpgradeKind.FireRate:
                    currentValue = $"{weapon.ShotsPerSecond * 60f:0} RPM";
                    nextValue = $"{60f / (weapon.FireCooldown * 0.82f):0} RPM";
                    return;
                case UpgradeKind.ReserveCapacity:
                    currentValue = $"{weapon.ReserveMagazineCapacity} MAGS";
                    nextValue = $"{weapon.ReserveMagazineCapacity + 1} MAGS";
                    return;
                case UpgradeKind.Reload:
                    currentValue = $"{weapon.ReloadDuration:0.00} SEC";
                    nextValue = $"{weapon.ReloadDuration * 0.82f:0.00} SEC";
                    return;
                case UpgradeKind.Ricochet:
                    currentValue = $"{weapon.RicochetBounces} BOUNCE";
                    nextValue = $"{weapon.RicochetBounces + 1} BOUNCE";
                    return;
                default:
                    currentValue = string.Empty;
                    nextValue = string.Empty;
                    return;
            }
        }

        private void PreviewUpgradeSlot(int slot)
        {
            if (slot < 0 || slot >= currentKinds.Count || weaponSystem == null)
            {
                return;
            }

            UpgradeKind kind = currentKinds[slot];
            Weapon weapon = weaponSystem.WeaponAt(selectedTarget);
            if (weapon == null
                || kind == UpgradeKind.Unlock
                || GetLevel(selectedTarget, kind) >= KindMaxLevel(kind))
            {
                return;
            }

            previewKind = kind;
            RefreshSummary(weapon, weaponSystem.IsWeaponUnlocked(selectedTarget));
        }

        private void ClearPreview(int slot)
        {
            if (!previewKind.HasValue
                || slot < 0
                || slot >= currentKinds.Count
                || currentKinds[slot] != previewKind.Value)
            {
                return;
            }

            previewKind = null;
            Weapon weapon = weaponSystem != null ? weaponSystem.WeaponAt(selectedTarget) : null;
            if (weapon != null)
            {
                RefreshSummary(weapon, weaponSystem.IsWeaponUnlocked(selectedTarget));
            }
        }

        private UpgradeKind? ValidPreviewKind(Weapon weapon, bool unlocked)
        {
            if (!unlocked || weapon == null || !previewKind.HasValue)
            {
                return null;
            }

            UpgradeKind kind = previewKind.Value;
            return kind != UpgradeKind.Unlock
                   && GetLevel(selectedTarget, kind) < KindMaxLevel(kind)
                ? kind
                : null;
        }

        private void SetStatRows(Weapon weapon, UpgradeKind? preview)
        {
            string previewDamage = preview == UpgradeKind.Damage
                ? FormatDamage(weapon, weapon.Damage * 1.2f)
                : null;
            string previewRate = preview == UpgradeKind.FireRate
                ? $"{60f / (weapon.FireCooldown * 0.82f):0} RPM"
                : null;
            string previewReload = preview == UpgradeKind.Reload
                ? $"{weapon.ReloadDuration * 0.82f:0.00} SEC"
                : null;
            string previewReserve = preview == UpgradeKind.ReserveCapacity
                ? $"{weapon.ReserveMagazineCapacity + 1} MAGS"
                : null;
            string previewSpecial = preview == UpgradeKind.Ricochet
                ? $"{weapon.RicochetBounces + 1} BOUNCE"
                : null;

            damageStatRow?.SetContent(
                damageIcon,
                AccentFor(UpgradeKind.Damage),
                "DAMAGE",
                weapon.DamageDisplay,
                previewDamage);
            fireRateStatRow?.SetContent(
                fireRateIcon,
                AccentFor(UpgradeKind.FireRate),
                "FIRE RATE",
                $"{weapon.ShotsPerSecond * 60f:0} RPM",
                previewRate);
            reloadStatRow?.SetContent(
                reloadIcon,
                AccentFor(UpgradeKind.Reload),
                "RELOAD",
                weapon.HasInfiniteAmmo ? "N/A" : $"{weapon.ReloadDuration:0.00} SEC",
                previewReload);
            reserveStatRow?.SetContent(
                magazineIcon,
                AccentFor(UpgradeKind.ReserveCapacity),
                "RESERVE",
                weapon.HasInfiniteAmmo ? "UNLIMITED" : $"{weapon.ReserveMagazineCapacity} MAGS",
                previewReserve);

            if (specialStatRow != null)
            {
                bool showSpecial = weapon.DisplayName == "Harpoon";
                specialStatRow.gameObject.SetActive(showSpecial);
                if (showSpecial)
                {
                    specialStatRow.SetContent(
                        ricochetIcon,
                        AccentFor(UpgradeKind.Ricochet),
                        "RICOCHET",
                        $"{weapon.RicochetBounces} BOUNCE",
                        previewSpecial);
                }
            }
        }

        private Sprite IconFor(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Unlock:
                    return unlockIcon;
                case UpgradeKind.Damage:
                    return damageIcon;
                case UpgradeKind.FireRate:
                    return fireRateIcon;
                case UpgradeKind.Reload:
                    return reloadIcon;
                case UpgradeKind.ReserveCapacity:
                    return magazineIcon;
                case UpgradeKind.Ricochet:
                    return ricochetIcon;
                default:
                    return null;
            }
        }

        private static Color AccentFor(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Damage:
                    return UiTheme.Siren;
                case UpgradeKind.FireRate:
                    return UiTheme.Progress;
                case UpgradeKind.Reload:
                    return UiTheme.Success;
                case UpgradeKind.ReserveCapacity:
                    return UiTheme.Foam;
                case UpgradeKind.Ricochet:
                    return UiTheme.Warning;
                default:
                    return UiTheme.Warning;
            }
        }

        private string BuildUnlockRequirement(Weapon weapon)
        {
            WeaponDefinition definition = weapon != null ? weapon.Definition : null;
            if (definition == null)
            {
                return "UNAVAILABLE";
            }

            if (selectedTarget > 0 && !weaponSystem.IsWeaponUnlocked(selectedTarget - 1))
            {
                return $"REQUIRES {weaponSystem.WeaponAt(selectedTarget - 1)?.DisplayName?.ToUpperInvariant()}";
            }

            if (gameManager != null && gameManager.Round < definition.MinimumUnlockRound)
            {
                return $"AVAILABLE ROUND {definition.MinimumUnlockRound}";
            }

            return $"AVAILABLE  |  ${definition.UnlockPrice}";
        }

        private static int KindBaseCost(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Damage:
                    return 15;
                case UpgradeKind.FireRate:
                    return 10;
                case UpgradeKind.ReserveCapacity:
                    return 15;
                case UpgradeKind.Reload:
                    return 20;
                case UpgradeKind.Ricochet:
                    return 30;
                default:
                    return 20;
            }
        }

        private static int KindMaxLevel(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Ricochet:
                    return 1;
                case UpgradeKind.ReserveCapacity:
                    return 3;
                default:
                    return 5;
            }
        }

        private int EffectiveCost(UpgradeKind kind, int level)
        {
            return level <= 0
                ? KindBaseCost(kind)
                : Mathf.RoundToInt(KindBaseCost(kind) * Mathf.Pow(costGrowthPerPurchase, level));
        }

        private int GetLevel(int target, UpgradeKind kind)
        {
            return levels.TryGetValue((target, kind), out int level) ? level : 0;
        }

        private static string FormatDamage(Weapon weapon, float damage)
        {
            int pellets = weapon.Definition != null ? weapon.Definition.PelletsPerShot : 1;
            return pellets > 1 ? $"{damage:0.#} \u00d7 {pellets}" : $"{damage:0.#}";
        }

        private static void SetCardColor(Image image, bool affordable, bool maxed)
        {
            if (image == null)
            {
                return;
            }

            image.color = maxed
                ? UiTheme.WithAlpha(UiTheme.Success, 0.82f)
                : affordable
                    ? UiTheme.WithAlpha(UiTheme.River, 0.96f)
                    : UiTheme.WithAlpha(UiTheme.HullSoft, 0.96f);
        }

        private static void SetText(Text label, string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }
    }
}
