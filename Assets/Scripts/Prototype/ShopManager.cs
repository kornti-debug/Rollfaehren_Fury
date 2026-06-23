using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Node-tree weapon shop. The player clicks a weapon node and connecting lines branch out to its
    /// purchasable nodes. Every weapon has power upgrades (Damage, Fire Rate) that level up with an
    /// escalating cost; weapons with ammo also get Faster Reload + Refill Ammo (tops the magazine and
    /// reserve back to the current cap), and the Harpoon gets Ricochet. Built entirely at runtime
    /// under the existing Shop Panel, so no scene/builder work is needed.
    /// </summary>
    public sealed class ShopManager : MonoBehaviour
    {
        private enum UpgradeKind { Unlock, Damage, FireRate, Reload, Refill, Ricochet }

        [SerializeField] private GameManager gameManager;
        [SerializeField] private WeaponSystem weaponSystem;
        [Tooltip("Existing shop buttons; the first is reused as a style template and its parent as the node container.")]
        [SerializeField] private List<Button> buttons = new List<Button>();
        [Tooltip("Each owned copy of a leveled upgrade multiplies its next cost by this.")]
        [SerializeField, Min(1f)] private float costGrowthPerPurchase = 1.7f;
        [SerializeField] private int refillCost = 20;

        private const int MaxUpgradeSlots = 4;

        // Layout (anchored positions inside the Shop Panel) — tune if the spacing feels off.
        private const float WeaponColumnX = -200f;
        private const float UpgradeColumnX = 140f;
        private const float ColumnCenterY = -10f;
        private const float WeaponNodeSpacing = 48f;
        private const float UpgradeNodeSpacing = 46f;
        private static readonly Vector2 WeaponNodeSize = new Vector2(155f, 40f);
        private static readonly Vector2 UpgradeNodeSize = new Vector2(176f, 38f);
        private static readonly Vector2 UnlockNodeSize = new Vector2(230f, 64f);

        private readonly Dictionary<(int target, UpgradeKind kind), int> levels = new Dictionary<(int, UpgradeKind), int>();
        private readonly List<Button> weaponNodes = new List<Button>();
        private readonly List<Button> upgradeSlots = new List<Button>();
        private readonly List<Image> lines = new List<Image>();
        private readonly List<UpgradeKind> currentKinds = new List<UpgradeKind>();
        private bool built;
        private int selectedTarget;

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }

            if (weaponSystem == null)
            {
                weaponSystem = FindFirstObjectByType<WeaponSystem>();
            }
        }

        public void OpenShop()
        {
            EnsureBuilt();
            if (built && weaponSystem != null && weaponSystem.WeaponCount > 0)
            {
                SelectTarget(Mathf.Clamp(selectedTarget, 0, weaponSystem.WeaponCount - 1));
            }
        }

        public void ResetPurchases()
        {
            levels.Clear();
            selectedTarget = 0;
            if (built)
            {
                SelectTarget(0);
            }
        }

        // The old flat-list buttons (now hidden) still reference this through their serialized
        // onClick, and the editor scene builder wires it. Kept as a no-op.
        public void Buy(int index)
        {
        }

        private void EnsureBuilt()
        {
            if (built)
            {
                return;
            }

            built = true;

            Button template = buttons.Count > 0 ? buttons[0] : null;
            if (template == null)
            {
                return;
            }

            Transform parent = template.transform.parent;

            // One node per weapon down the left side.
            int weaponCount = weaponSystem != null ? weaponSystem.WeaponCount : 0;
            for (int i = 0; i < weaponCount; i++)
            {
                string label = weaponSystem.WeaponAt(i)?.DisplayName ?? $"Weapon {i + 1}";
                Vector2 pos = new Vector2(WeaponColumnX, ColumnY(i, weaponCount, WeaponNodeSpacing));
                Button node = CreateNode(template, parent, $"Weapon Node {i}", label, pos, WeaponNodeSize);
                int captured = i;
                node.onClick.AddListener(() => SelectTarget(captured));
                weaponNodes.Add(node);
            }

            // Reused pool of upgrade nodes + connecting lines (shown per selection).
            for (int i = 0; i < MaxUpgradeSlots; i++)
            {
                lines.Add(CreateLine(parent));

                Button slot = CreateNode(template, parent, $"Upgrade Node {i}", string.Empty,
                    new Vector2(UpgradeColumnX, 0f), UpgradeNodeSize);
                int captured = i;
                slot.onClick.AddListener(() => BuyUpgradeSlot(captured));
                slot.gameObject.SetActive(false);
                upgradeSlots.Add(slot);
            }

            // Hide the old flat-list buttons (kept only as a style template).
            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i] != null)
                {
                    buttons[i].gameObject.SetActive(false);
                }
            }

            if (weaponCount > 0)
            {
                SelectTarget(0);
            }
        }

        private void SelectTarget(int target)
        {
            selectedTarget = target;

            currentKinds.Clear();
            currentKinds.AddRange(GetKindsFor(target));

            Vector2 fromEdge = SelectedNodeRightEdge();
            int count = currentKinds.Count;
            for (int i = 0; i < upgradeSlots.Count; i++)
            {
                bool used = i < count;
                upgradeSlots[i].gameObject.SetActive(used);
                lines[i].gameObject.SetActive(used);
                if (!used)
                {
                    continue;
                }

                Vector2 pos = new Vector2(UpgradeColumnX, ColumnY(i, count, UpgradeNodeSpacing));
                Vector2 nodeSize = currentKinds[i] == UpgradeKind.Unlock
                    ? UnlockNodeSize
                    : UpgradeNodeSize;
                RectTransform slotRect = (RectTransform)upgradeSlots[i].transform;
                slotRect.anchoredPosition = pos;
                slotRect.sizeDelta = nodeSize;
                SetLine(lines[i], fromEdge, new Vector2(pos.x - nodeSize.x * 0.5f, pos.y));
            }

            HighlightSelectedNode();
            RefreshTree();
        }

        private void RefreshTree()
        {
            int money = gameManager != null ? gameManager.Money : 0;
            Weapon weapon = weaponSystem != null ? weaponSystem.WeaponAt(selectedTarget) : null;
            RefreshWeaponNodes();

            for (int i = 0; i < currentKinds.Count && i < upgradeSlots.Count; i++)
            {
                UpgradeKind kind = currentKinds[i];
                Button slot = upgradeSlots[i];
                Text label = slot.GetComponentInChildren<Text>();

                if (kind == UpgradeKind.Unlock)
                {
                    RefreshUnlockSlot(slot, label, weapon, money);
                    continue;
                }

                if (kind == UpgradeKind.Refill)
                {
                    bool full = weapon == null || weapon.IsAmmoFull;
                    slot.interactable = !full && money >= refillCost;
                    if (label != null)
                    {
                        label.text = full ? "Refill Ammo\nfull" : $"Refill Ammo\n${refillCost}";
                    }

                    continue;
                }

                int level = GetLevel(selectedTarget, kind);
                int max = KindMaxLevel(kind);
                int cost = EffectiveCost(kind, level);
                bool maxed = level >= max;

                slot.interactable = !maxed && money >= cost;
                if (label != null)
                {
                    label.text = maxed
                        ? $"{KindLabel(kind)}\nmax {level}/{max}"
                        : $"{KindLabel(kind)}\n{level}/{max}   ${cost}";
                }
            }
        }

        private void BuyUpgradeSlot(int slot)
        {
            if (slot < 0 || slot >= currentKinds.Count)
            {
                return;
            }

            Weapon weapon = weaponSystem != null ? weaponSystem.WeaponAt(selectedTarget) : null;
            if (weapon == null || gameManager == null)
            {
                return;
            }

            UpgradeKind kind = currentKinds[slot];
            if (kind == UpgradeKind.Unlock)
            {
                BuyWeaponUnlock(weapon);
                return;
            }

            if (kind == UpgradeKind.Refill)
            {
                if (weapon.IsAmmoFull || !gameManager.TrySpendMoney(refillCost))
                {
                    return;
                }

                weapon.RefillAmmo();
                RefreshTree();
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
                return;
            }

            ApplyUpgrade(weapon, kind);
            levels[(selectedTarget, kind)] = level + 1;
            RefreshTree();
        }

        private static void ApplyUpgrade(Weapon weapon, UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Damage: weapon.MultiplyDamage(1.25f); break;
                case UpgradeKind.FireRate: weapon.MultiplyCooldown(0.82f); break;
                case UpgradeKind.Reload: weapon.MultiplyReloadDuration(0.82f); break;
                case UpgradeKind.Ricochet: weapon.AddRicochet(1); break;
            }
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

            if (weapon.MagazineSize > 0)
            {
                yield return UpgradeKind.Reload;
                yield return UpgradeKind.Refill;
            }

            if (weapon.DisplayName == "Harpoon")
            {
                yield return UpgradeKind.Ricochet;
            }
        }

        private static string KindLabel(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Damage: return "Damage +25%";
                case UpgradeKind.FireRate: return "Fire Rate";
                case UpgradeKind.Reload: return "Faster Reload";
                case UpgradeKind.Ricochet: return "Ricochet";
                default: return kind.ToString();
            }
        }

        private static int KindBaseCost(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Damage: return 15;
                case UpgradeKind.FireRate: return 10;
                case UpgradeKind.Reload: return 20;
                case UpgradeKind.Ricochet: return 30;
                default: return 20;
            }
        }

        private static int KindMaxLevel(UpgradeKind kind)
        {
            return kind == UpgradeKind.Ricochet ? 1 : 3;
        }

        private int EffectiveCost(UpgradeKind kind, int level)
        {
            int baseCost = KindBaseCost(kind);
            if (level <= 0)
            {
                return baseCost;
            }

            return Mathf.RoundToInt(baseCost * Mathf.Pow(costGrowthPerPurchase, level));
        }

        private int GetLevel(int target, UpgradeKind kind)
        {
            return levels.TryGetValue((target, kind), out int level) ? level : 0;
        }

        // --- UI helpers -----------------------------------------------------

        private static float ColumnY(int index, int count, float spacing)
        {
            float top = (count - 1) * 0.5f * spacing;
            return ColumnCenterY + top - index * spacing;
        }

        private Vector2 SelectedNodeRightEdge()
        {
            if (selectedTarget >= 0 && selectedTarget < weaponNodes.Count)
            {
                Vector2 center = ((RectTransform)weaponNodes[selectedTarget].transform).anchoredPosition;
                return new Vector2(center.x + WeaponNodeSize.x * 0.5f, center.y);
            }

            return new Vector2(WeaponColumnX + WeaponNodeSize.x * 0.5f, ColumnCenterY);
        }

        private void HighlightSelectedNode()
        {
            for (int i = 0; i < weaponNodes.Count; i++)
            {
                Image image = weaponNodes[i].GetComponent<Image>();
                if (image != null)
                {
                    bool unlocked = weaponSystem != null && weaponSystem.IsWeaponUnlocked(i);
                    if (i == selectedTarget)
                    {
                        image.color = unlocked
                            ? new Color(0.18f, 0.42f, 0.70f, 0.95f)
                            : new Color(0.58f, 0.38f, 0.10f, 0.95f);
                    }
                    else
                    {
                        image.color = unlocked
                            ? new Color(0.10f, 0.14f, 0.20f, 0.90f)
                            : new Color(0.16f, 0.16f, 0.18f, 0.90f);
                    }
                }
            }
        }

        private void RefreshWeaponNodes()
        {
            for (int i = 0; i < weaponNodes.Count; i++)
            {
                Button node = weaponNodes[i];
                Weapon weapon = weaponSystem != null ? weaponSystem.WeaponAt(i) : null;
                if (node == null || weapon == null)
                {
                    continue;
                }

                Text label = node.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = weaponSystem.IsWeaponUnlocked(i)
                        ? weapon.DisplayName
                        : $"{weapon.DisplayName}\nLOCKED";
                }
            }

            HighlightSelectedNode();
        }

        private void RefreshUnlockSlot(Button slot, Text label, Weapon weapon, int money)
        {
            WeaponDefinition definition = weapon != null ? weapon.Definition : null;
            if (definition == null || weaponSystem == null || gameManager == null)
            {
                slot.interactable = false;
                if (label != null)
                {
                    label.text = "Unavailable";
                }

                return;
            }

            bool prerequisiteOwned = selectedTarget == 0 || weaponSystem.IsWeaponUnlocked(selectedTarget - 1);
            bool canUnlock = weaponSystem.CanUnlockWeapon(selectedTarget, gameManager.Round);
            slot.interactable = canUnlock && money >= definition.UnlockPrice;

            if (label == null)
            {
                return;
            }

            string requirement = string.Empty;
            if (!prerequisiteOwned)
            {
                Weapon prerequisite = weaponSystem.WeaponAt(selectedTarget - 1);
                string prerequisiteName = prerequisite != null ? prerequisite.DisplayName : "previous weapon";
                requirement = $"\nRequires {prerequisiteName}";
            }

            label.text =
                $"Unlock {weapon.DisplayName}\n${definition.UnlockPrice} | Round {definition.MinimumUnlockRound}{requirement}";
        }

        private void BuyWeaponUnlock(Weapon weapon)
        {
            WeaponDefinition definition = weapon.Definition;
            if (definition == null
                || !weaponSystem.CanUnlockWeapon(selectedTarget, gameManager.Round)
                || !gameManager.TrySpendMoney(definition.UnlockPrice))
            {
                RefreshTree();
                return;
            }

            if (!weaponSystem.TryUnlockWeapon(selectedTarget))
            {
                Debug.LogError($"Weapon '{weapon.DisplayName}' passed shop validation but could not be unlocked.", this);
                return;
            }

            SelectTarget(selectedTarget);
        }

        private Button CreateNode(Button template, Transform parent, string objectName, string label, Vector2 pos, Vector2 size)
        {
            Button node = Instantiate(template, parent);
            node.name = objectName;
            node.gameObject.SetActive(true);
            node.onClick = new Button.ButtonClickedEvent();

            RectTransform rect = (RectTransform)node.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            Text text = node.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = label;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 8;
                text.resizeTextMaxSize = 18;
            }

            return node;
        }

        private static Image CreateLine(Transform parent)
        {
            GameObject lineObject = new GameObject("Upgrade Line", typeof(RectTransform), typeof(Image));
            lineObject.transform.SetParent(parent, false);
            lineObject.transform.SetAsFirstSibling(); // draw behind the node buttons

            Image image = lineObject.GetComponent<Image>();
            image.color = new Color(0.45f, 0.65f, 1f, 0.45f);
            image.raycastTarget = false;

            RectTransform rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            return image;
        }

        private static void SetLine(Image line, Vector2 from, Vector2 to)
        {
            Vector2 delta = to - from;
            RectTransform rect = line.rectTransform;
            rect.anchoredPosition = from + delta * 0.5f;
            rect.sizeDelta = new Vector2(delta.magnitude, 3f);
            rect.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }
    }
}
