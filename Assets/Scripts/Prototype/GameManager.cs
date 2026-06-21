using System;
using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    public enum PrototypeGameState
    {
        Idle,
        Preparation,
        Playing,
        Shop,
        AugmentDraft,
        GameOver
    }

    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private Health ferryHealth;
        [SerializeField] private FerryController ferryController;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private SimpleFPSController playerController;
        [SerializeField] private WeaponSystem weaponSystem;
        [SerializeField] private SimpleHUD hud;
        [SerializeField] private ShopManager shopManager;
        [SerializeField] private AugmentSystem augmentSystem;
        [SerializeField] private bool startOnPlay = true;
        [SerializeField] private int startingMoney = 40;
        [SerializeField] private int roundCompletionReward = 25;

        [Header("Testing (turn off later)")]
        [Tooltip("Overrides ferry max health for testing. 0 = use the scene value.")]
        [SerializeField] private float testFerryMaxHealth = 100f;

        private int money;
        private int round = 1;
        private float perRoundHealFraction;
        private int healPerKill;
        private bool killStreakEnabled;
        private int killStreakEvery = 5;
        private float killStreakSpeedMultiplier = 1.4f;
        private float killStreakSpeedDuration = 5f;
        private int killStreakCount;

        public static GameManager Instance { get; private set; }

        public event Action EnemyKilled;
        public event Action FerryDamaged;
        public event Action RoundCompleted;
        public event Action GameOverReached;
        public event Action UpgradeBought;

        public PrototypeGameState State { get; private set; } = PrototypeGameState.Idle;
        public bool AllowsGameplayInput => !IsPaused
            && (State == PrototypeGameState.Playing || State == PrototypeGameState.Preparation);
        public bool AllowsShopSceneEntry => !IsPaused
            && State == PrototypeGameState.Preparation
            && !IsInsideShop
            && !IsShopOverlayOpen;
        public bool AllowsShopInteraction => !IsPaused
            && State == PrototypeGameState.Preparation
            && IsInsideShop;
        public bool IsPaused { get; private set; }
        public bool IsInsideShop { get; private set; }
        public int Money => money;
        public int Round => round;
        public float CrossingProgress => ferryController != null ? ferryController.Progress : 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ResolveReferences();
        }

        private void OnEnable()
        {
            if (ferryHealth != null)
            {
                ferryHealth.Died += HandleFerryDestroyed;
                ferryHealth.HealthChanged += HandleFerryHealthChanged;
            }

            if (ferryController != null)
            {
                ferryController.Arrived += HandleFerryArrived;
            }
        }

        private void Start()
        {
            hud?.Bind(this);

            if (startOnPlay)
            {
                StartNewGame();
            }
            else
            {
                RefreshHud();
            }
        }

        private void OnDisable()
        {
            if (ferryHealth != null)
            {
                ferryHealth.Died -= HandleFerryDestroyed;
                ferryHealth.HealthChanged -= HandleFerryHealthChanged;
            }

            if (ferryController != null)
            {
                ferryController.Arrived -= HandleFerryArrived;
            }
        }

        private void Update()
        {
            RefreshHud();
        }

        public void Configure(Health ferry, EnemySpawner spawner, SimpleFPSController controller, WeaponSystem weapons, SimpleHUD simpleHud)
        {
            if (ferryHealth != null)
            {
                ferryHealth.Died -= HandleFerryDestroyed;
                ferryHealth.HealthChanged -= HandleFerryHealthChanged;
            }

            ferryHealth = ferry;
            enemySpawner = spawner;
            playerController = controller;
            weaponSystem = weapons;
            hud = simpleHud;

            if (ferryHealth != null)
            {
                ferryHealth.Died += HandleFerryDestroyed;
                ferryHealth.HealthChanged += HandleFerryHealthChanged;
            }
        }

        public void StartNewGame()
        {
            round = 1;
            money = startingMoney;
            IsPaused = false;
            IsInsideShop = false;
            perRoundHealFraction = 0f;
            healPerKill = 0;
            killStreakEnabled = false;
            killStreakCount = 0;
            ferryHealth?.ResetHealth();
            if (testFerryMaxHealth > 0f)
            {
                ferryHealth?.SetMaxHealth(testFerryMaxHealth, true);
            }
            shopManager?.ResetPurchases();
            enemySpawner?.ResetAugments();
            enemySpawner?.StopRound(true);
            weaponSystem?.ResetAllWeapons(); // fresh run: clear weapon upgrades/augment buffs + full ammo (afterwards refill only via the shop)
            ferryController?.ResetToDockA();
            EnterPreparation();
        }

        public void StartNextRound()
        {
            round++;
            EnterPreparation();
        }

        public void RestartGame()
        {
            StartNewGame();
        }

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            if (paused)
            {
                SetPlayerInput(false, false);
                return;
            }

            RestoreInputForCurrentState();
        }

        public void RegisterEnemyKilled(int reward)
        {
            money += Mathf.Max(0, reward);

            // Bilge Pump augment: each kill patches a little ferry health.
            if (healPerKill > 0)
            {
                ferryHealth?.Heal(healPerKill);
            }

            // Adrenaline augment: every Nth kill grants a short movement-speed burst.
            if (killStreakEnabled)
            {
                killStreakCount++;
                if (killStreakCount % Mathf.Max(1, killStreakEvery) == 0)
                {
                    playerController?.ApplyTimedSpeedMultiplier(killStreakSpeedMultiplier, killStreakSpeedDuration);
                }
            }

            EnemyKilled?.Invoke();
            RefreshHud();
        }

        public void RegisterEnemyReachedFerry(SimpleEnemy enemy, float damage)
        {
            FerryDamaged?.Invoke();
            RefreshHud();
        }

        /// <summary>Spends money for the node-tree shop (effect is applied by the shop on success).</summary>
        public bool TrySpendMoney(int cost)
        {
            if (!TrySpend(cost))
            {
                hud?.ShowMessage("Not enough money");
                return false;
            }

            UpgradeBought?.Invoke();
            hud?.SetShopMoney(money);
            RefreshHud();
            return true;
        }

        public bool TryPurchase(UpgradeDefinition upgrade, int cost)
        {
            if (upgrade == null)
            {
                return false;
            }

            if (!TrySpend(cost))
            {
                hud?.ShowMessage("Not enough money");
                return false;
            }

            upgrade.Apply(new UpgradeContext(weaponSystem, ferryHealth));
            UpgradeBought?.Invoke();
            hud?.ShowMessage($"Bought {upgrade.DisplayName}");
            RefreshHud();
            return true;
        }

        public bool IsShopOverlayOpen { get; private set; }

        public bool OpenShopOverlay()
        {
            if (!AllowsShopInteraction)
            {
                return false;
            }

            IsShopOverlayOpen = true;
            SetPlayerInput(false, false);
            hud?.ShowShopOverlay(money);
            shopManager?.OpenShop();
            return true;
        }

        public void CloseShopOverlay()
        {
            if (!IsShopOverlayOpen)
            {
                return;
            }

            IsShopOverlayOpen = false;
            hud?.ShowGameplay();
            SetPlayerInput(true, false);
        }

        public bool TryBeginShopVisit()
        {
            if (!AllowsShopSceneEntry)
            {
                return false;
            }

            IsInsideShop = true;
            SetPlayerInput(false, false);
            return true;
        }

        public void CompleteShopEntry()
        {
            if (IsInsideShop && !IsPaused)
            {
                SetPlayerInput(true, false);
            }
        }

        public void PrepareShopExit()
        {
            if (!IsInsideShop)
            {
                return;
            }

            if (IsShopOverlayOpen)
            {
                CloseShopOverlay();
            }

            SetPlayerInput(false, false);
        }

        public void CompleteShopExit()
        {
            IsInsideShop = false;
            IsShopOverlayOpen = false;
            RestoreInputForCurrentState();
        }

        public void ApplyCrossingSpeedup(float factor)
        {
            float durationFactor = Mathf.Clamp(factor, 0.1f, 1f);
            ferryController?.MultiplySpeed(1f / durationFactor);
        }

        public void AddPerRoundHeal(float fraction)
        {
            perRoundHealFraction += Mathf.Max(0f, fraction);
        }

        public void AddHealPerKill(int amount)
        {
            healPerKill += Mathf.Max(0, amount);
        }

        public void EnableKillStreakSpeed(int everyKills, float speedMultiplier, float duration)
        {
            killStreakEnabled = true;
            killStreakEvery = Mathf.Max(1, everyKills);
            killStreakSpeedMultiplier = Mathf.Max(1f, speedMultiplier);
            killStreakSpeedDuration = Mathf.Max(0.1f, duration);
        }

        public void EnableReloadDamageBuff(float multiplier, float duration)
        {
            weaponSystem?.EnableReloadDamageBuffOnAll(multiplier, duration);
        }

        public void MultiplyWeaponReload(float multiplier)
        {
            weaponSystem?.MultiplyAllReloadDuration(multiplier);
        }

        public void GrantMoney(int amount)
        {
            money += Mathf.Max(0, amount);
            RefreshHud();
        }

        public void AddFerryMaxHealth(float amount)
        {
            if (ferryHealth == null || amount <= 0f)
            {
                return;
            }

            ferryHealth.SetMaxHealth(ferryHealth.MaxHealth + amount, false);
            ferryHealth.Heal(amount);
        }

        private void ApplyPerRoundHeal()
        {
            if (perRoundHealFraction > 0f && ferryHealth != null)
            {
                ferryHealth.Heal(perRoundHealFraction * ferryHealth.MaxHealth);
            }
        }

        public bool BeginCrossing()
        {
            if (State != PrototypeGameState.Preparation || IsInsideShop || ferryController == null)
            {
                return false;
            }

            if (!ferryController.BeginCrossing())
            {
                return false;
            }

            State = PrototypeGameState.Playing;
            IsShopOverlayOpen = false;
            SetPlayerInput(true, true);
            hud?.ShowGameplay();
            enemySpawner?.BeginRound(round, this);
            RefreshHud();
            return true;
        }

        private void EnterPreparation()
        {
            State = PrototypeGameState.Preparation;
            IsShopOverlayOpen = false;
            enemySpawner?.StopRound(true);
            SetPlayerInput(true, false);
            hud?.ShowGameplay();
            hud?.ShowMessage(string.Empty);
            RefreshHud();
        }

        private void CompleteRound()
        {
            if (State != PrototypeGameState.Playing)
            {
                return;
            }

            State = PrototypeGameState.AugmentDraft;
            money += roundCompletionReward + round * 5;
            IsShopOverlayOpen = false;
            ferryController?.Stop();
            SetPlayerInput(false, false);
            enemySpawner?.StopRound(true);
            ApplyPerRoundHeal();
            RoundCompleted?.Invoke();
            hud?.ShowAugmentDraft(round);
            augmentSystem?.OpenDraft();
            RefreshHud();
        }

        private void HandleFerryDestroyed(Health destroyedHealth)
        {
            if (State == PrototypeGameState.GameOver)
            {
                return;
            }

            State = PrototypeGameState.GameOver;
            ferryController?.Stop();
            SetPlayerInput(false, false);
            enemySpawner?.StopRound(true);
            GameOverReached?.Invoke();
            hud?.ShowGameOver(round, money);
            RefreshHud();
        }

        private void HandleFerryHealthChanged(Health health, float current, float max)
        {
            RefreshHud();
        }

        private bool TrySpend(int cost)
        {
            if (money < cost)
            {
                return false;
            }

            money -= cost;
            return true;
        }

        private void SetPlayerInput(bool movementEnabled, bool weaponsEnabled)
        {
            playerController?.SetInputEnabled(movementEnabled);
            weaponSystem?.SetInputEnabled(weaponsEnabled);
        }

        private void RestoreInputForCurrentState()
        {
            switch (State)
            {
                case PrototypeGameState.Preparation:
                    SetPlayerInput(true, false);
                    break;
                case PrototypeGameState.Playing:
                    SetPlayerInput(true, true);
                    break;
                default:
                    SetPlayerInput(false, false);
                    break;
            }
        }

        private void HandleFerryArrived()
        {
            CompleteRound();
        }

        private void RefreshHud()
        {
            float current = ferryHealth != null ? ferryHealth.CurrentHealth : 0f;
            float max = ferryHealth != null ? ferryHealth.MaxHealth : 1f;
            float weaponDamage = weaponSystem != null ? weaponSystem.ActiveDamage : 0f;
            float shotsPerSecond = weaponSystem != null ? weaponSystem.ActiveShotsPerSecond : 0f;
            string weaponName = weaponSystem != null ? weaponSystem.ActiveWeaponName : "None";
            int weaponSlot = weaponSystem != null ? weaponSystem.ActiveIndex + 1 : 0;
            int weaponCount = weaponSystem != null ? weaponSystem.WeaponCount : 0;
            int ammoInMagazine = weaponSystem != null ? weaponSystem.ActiveAmmo : 0;
            int magazineSize = weaponSystem != null ? weaponSystem.ActiveMagazineSize : 0;
            int reserveAmmo = weaponSystem != null ? weaponSystem.ActiveReserveAmmo : 0;
            bool infiniteAmmo = weaponSystem == null || weaponSystem.ActiveHasInfiniteAmmo;
            bool isReloading = weaponSystem != null && weaponSystem.ActiveIsReloading;
            float reloadProgress = weaponSystem != null ? weaponSystem.ActiveReloadProgress : 1f;
            hud?.SetStats(current, max, money, round, CrossingProgress, weaponName, weaponSlot, weaponCount, weaponDamage, shotsPerSecond, ammoInMagazine, magazineSize, reserveAmmo, infiniteAmmo, isReloading, reloadProgress);
        }

        private void ResolveReferences()
        {
            if (ferryHealth == null)
            {
                ferryHealth = FindFirstObjectByType<FerryDamageTarget>()?.FerryHealth;
            }

            if (enemySpawner == null)
            {
                enemySpawner = FindFirstObjectByType<EnemySpawner>();
            }

            if (ferryController == null)
            {
                ferryController = FindFirstObjectByType<FerryController>();
            }

            if (playerController == null)
            {
                playerController = FindFirstObjectByType<SimpleFPSController>();
            }

            if (weaponSystem == null)
            {
                weaponSystem = FindFirstObjectByType<WeaponSystem>();
            }

            if (hud == null)
            {
                hud = FindFirstObjectByType<SimpleHUD>();
            }

            if (shopManager == null)
            {
                shopManager = FindFirstObjectByType<ShopManager>();
            }

            if (augmentSystem == null)
            {
                augmentSystem = FindFirstObjectByType<AugmentSystem>();
            }
        }
    }
}
