using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Round-end augment draft. Offers a few random augments from the pool on draft
    /// buttons; picking one applies it (via <see cref="AugmentContext"/>) and starts
    /// the next round through <see cref="GameManager.StartNextRound"/>.
    /// </summary>
    public sealed class AugmentSystem : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private List<AugmentDefinition> pool = new List<AugmentDefinition>();
        [SerializeField] private List<AugmentCardView> draftCards = new List<AugmentCardView>();
        [Header("Category Icons")]
        [SerializeField] private Sprite ferryIcon;
        [SerializeField] private Sprite weaponIcon;
        [SerializeField] private Sprite playerIcon;
        [SerializeField] private Sprite economyIcon;
        [SerializeField] private Sprite enemiesIcon;
        [SerializeField] private Sprite worldIcon;
        [Tooltip("Add the weapon/utility augments (Bilge Pump, Reload Fury, Rapid Reload, Adrenaline) to the pool at startup.")]
        [SerializeField] private bool addExtraAugments = true;

        private readonly List<AugmentDefinition> offered = new List<AugmentDefinition>();
        private readonly HashSet<AugmentDefinition> acquiredUnique = new HashSet<AugmentDefinition>();
        private readonly List<AugmentDefinition> acquired = new List<AugmentDefinition>();
        private bool extraAugmentsAdded;

        /// <summary>Every augment picked this run, in pick order (a repeatable augment appears once per pick).
        /// Used by the deck mirror's active-augment readout. Cleared on a new run via <see cref="ResetRun"/>.</summary>
        public IReadOnlyList<AugmentDefinition> AcquiredAugments => acquired;

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }

            if (spawner == null)
            {
                spawner = FindFirstObjectByType<EnemySpawner>();
            }

            AddExtraAugments();
        }

        // Appends the newer augments to the pool as runtime instances, so they appear in the draft
        // without needing separate assets or scene wiring (their values come from the script defaults).
        private void AddExtraAugments()
        {
            if (!addExtraAugments || extraAugmentsAdded)
            {
                return;
            }

            extraAugmentsAdded = true;

            BilgePumpAugment bilge = ScriptableObject.CreateInstance<BilgePumpAugment>();
            bilge.InitRuntime(
                "Bilge Pump",
                "Repair 0.5 ferry HP per kill, up to 10 HP each crossing",
                false,
                AugmentCategory.Ferry,
                "Repair 0.5 HP per kill, up to 10 HP per crossing");
            pool.Add(bilge);

            ReloadFuryAugment fury = ScriptableObject.CreateInstance<ReloadFuryAugment>();
            fury.InitRuntime(
                "Reload Fury",
                "+50% weapon damage for 10s after each reload",
                true,
                AugmentCategory.Weapon);
            pool.Add(fury);

            RapidReloadAugment rapid = ScriptableObject.CreateInstance<RapidReloadAugment>();
            rapid.InitRuntime(
                "Rapid Reload",
                "All weapons reload 30% faster",
                true,
                AugmentCategory.Weapon);
            pool.Add(rapid);

            AdrenalineAugment adrenaline = ScriptableObject.CreateInstance<AdrenalineAugment>();
            adrenaline.InitRuntime(
                "Adrenaline",
                "+40% move speed for 5s every 5th kill",
                true,
                AugmentCategory.Player);
            pool.Add(adrenaline);
        }

        /// <summary>Called by GameManager at round end.</summary>
        public void OpenDraft()
        {
            offered.Clear();

            List<AugmentDefinition> available = new List<AugmentDefinition>();
            foreach (AugmentDefinition augment in pool)
            {
                if (augment != null && (augment.IsRepeatable || !acquiredUnique.Contains(augment)))
                {
                    available.Add(augment);
                }
            }

            for (int i = 0; i < draftCards.Count; i++)
            {
                AugmentCardView card = draftCards[i];

                if (card == null || available.Count == 0)
                {
                    offered.Add(null);
                    if (card != null)
                    {
                        card.gameObject.SetActive(false);
                    }

                    continue;
                }

                int pick = Random.Range(0, available.Count);
                AugmentDefinition chosen = available[pick];
                available.RemoveAt(pick);
                offered.Add(chosen);

                card.SetContent(chosen, IconFor(chosen.Category), AccentFor(chosen.Category));
            }
        }

        /// <summary>Bound to draft button i (UnityEvent int listener) by the scene builder.</summary>
        public void Pick(int index)
        {
            if (index >= 0 && index < offered.Count && offered[index] != null)
            {
                AugmentDefinition selected = offered[index];
                selected.Apply(new AugmentContext(gameManager, spawner));
                acquired.Add(selected);
                if (!selected.IsRepeatable)
                {
                    acquiredUnique.Add(selected);
                }
            }

            gameManager?.StartNextRound();
        }

        public void ResetRun()
        {
            acquiredUnique.Clear();
            acquired.Clear();
            offered.Clear();
        }

        private Sprite IconFor(AugmentCategory category)
        {
            switch (category)
            {
                case AugmentCategory.Ferry:
                    return ferryIcon;
                case AugmentCategory.Weapon:
                    return weaponIcon;
                case AugmentCategory.Player:
                    return playerIcon;
                case AugmentCategory.Economy:
                    return economyIcon;
                case AugmentCategory.Enemies:
                    return enemiesIcon;
                default:
                    return worldIcon;
            }
        }

        private static Color AccentFor(AugmentCategory category)
        {
            switch (category)
            {
                case AugmentCategory.Ferry:
                    return UiTheme.Progress;
                case AugmentCategory.Weapon:
                    return UiTheme.Siren;
                case AugmentCategory.Player:
                    return UiTheme.Success;
                case AugmentCategory.Economy:
                    return UiTheme.Warning;
                case AugmentCategory.Enemies:
                    return UiTheme.WarningDark;
                default:
                    return UiTheme.Muted;
            }
        }
    }
}
