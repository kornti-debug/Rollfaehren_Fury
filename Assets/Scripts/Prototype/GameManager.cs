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
        [SerializeField] private int startingMoney = 0;
        [SerializeField] private int roundCompletionReward = 25;

        private int money;
        private int round = 1;
        private float perRoundHealFraction;

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
            ferryHealth?.ResetHealth();
            shopManager?.ResetPurchases();
            enemySpawner?.ResetAugments();
            enemySpawner?.StopRound(true);
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
            EnemyKilled?.Invoke();
            RefreshHud();
        }

        public void RegisterEnemyReachedFerry(SimpleEnemy enemy, float damage)
        {
            FerryDamaged?.Invoke();
            RefreshHud();
        }

        public bool TryPurchase(UpgradeDefinition upgrade)
        {
            if (upgrade == null)
            {
                return false;
            }

            if (!TrySpend(upgrade.Cost))
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
            hud?.ShowMessage("Use the console in the ferry house to start");
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
            hud?.SetStats(current, max, money, round, CrossingProgress, weaponName, weaponSlot, weaponCount, weaponDamage, shotsPerSecond);
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
