using System;
using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    public enum PrototypeGameState
    {
        Idle,
        Playing,
        Shop,
        GameOver
    }

    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private Health ferryHealth;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private SimpleFPSController playerController;
        [SerializeField] private HitscanWeapon playerWeapon;
        [SerializeField] private SimpleHUD hud;
        [SerializeField] private bool startOnPlay = true;
        [SerializeField] private float crossingDuration = 45f;
        [SerializeField] private int startingMoney = 0;
        [SerializeField] private int roundCompletionReward = 25;
        [SerializeField] private int weaponDamageUpgradeCost = 50;
        [SerializeField] private int fireRateUpgradeCost = 45;
        [SerializeField] private int ferryHealthUpgradeCost = 60;
        [SerializeField] private float weaponDamageUpgradeAmount = 10f;
        [SerializeField] private float fireRateUpgradeMultiplier = 0.82f;
        [SerializeField] private float ferryHealthUpgradeAmount = 25f;

        private float crossingTimer;
        private int money;
        private int round = 1;

        public static GameManager Instance { get; private set; }

        public event Action EnemyKilled;
        public event Action FerryDamaged;
        public event Action RoundCompleted;
        public event Action GameOverReached;
        public event Action UpgradeBought;

        public PrototypeGameState State { get; private set; } = PrototypeGameState.Idle;
        public bool AllowsGameplayInput => State == PrototypeGameState.Playing;
        public int Money => money;
        public int Round => round;
        public float CrossingProgress => crossingDuration <= 0f ? 1f : Mathf.Clamp01(crossingTimer / crossingDuration);

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
        }

        private void Update()
        {
            if (State != PrototypeGameState.Playing)
            {
                return;
            }

            crossingTimer += Time.deltaTime;
            if (crossingTimer >= crossingDuration)
            {
                CompleteRound();
            }

            RefreshHud();
        }

        public void Configure(Health ferry, EnemySpawner spawner, SimpleFPSController controller, HitscanWeapon weapon, SimpleHUD simpleHud)
        {
            if (ferryHealth != null)
            {
                ferryHealth.Died -= HandleFerryDestroyed;
                ferryHealth.HealthChanged -= HandleFerryHealthChanged;
            }

            ferryHealth = ferry;
            enemySpawner = spawner;
            playerController = controller;
            playerWeapon = weapon;
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
            crossingTimer = 0f;
            ferryHealth?.ResetHealth();
            StartRound();
        }

        public void StartNextRound()
        {
            round++;
            StartRound();
        }

        public void RestartGame()
        {
            StartNewGame();
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

        public void BuyWeaponDamageUpgrade()
        {
            if (!TrySpend(weaponDamageUpgradeCost))
            {
                hud?.ShowMessage("Not enough money");
                return;
            }

            playerWeapon?.AddDamage(weaponDamageUpgradeAmount);
            UpgradeBought?.Invoke();
            hud?.ShowMessage($"+{weaponDamageUpgradeAmount:0} weapon damage");
            RefreshHud();
        }

        public void BuyFireRateUpgrade()
        {
            if (!TrySpend(fireRateUpgradeCost))
            {
                hud?.ShowMessage("Not enough money");
                return;
            }

            playerWeapon?.MultiplyCooldown(fireRateUpgradeMultiplier);
            UpgradeBought?.Invoke();
            hud?.ShowMessage("Faster fire rate");
            RefreshHud();
        }

        public void BuyFerryHealthUpgrade()
        {
            if (!TrySpend(ferryHealthUpgradeCost))
            {
                hud?.ShowMessage("Not enough money");
                return;
            }

            if (ferryHealth != null)
            {
                ferryHealth.SetMaxHealth(ferryHealth.MaxHealth + ferryHealthUpgradeAmount, true);
            }

            UpgradeBought?.Invoke();
            hud?.ShowMessage($"+{ferryHealthUpgradeAmount:0} ferry health");
            RefreshHud();
        }

        private void StartRound()
        {
            State = PrototypeGameState.Playing;
            crossingTimer = 0f;
            SetGameplayInput(true);
            hud?.ShowGameplay();
            enemySpawner?.BeginRound(round, this);
            RefreshHud();
        }

        private void CompleteRound()
        {
            if (State != PrototypeGameState.Playing)
            {
                return;
            }

            State = PrototypeGameState.Shop;
            money += roundCompletionReward + round * 5;
            SetGameplayInput(false);
            enemySpawner?.StopRound(true);
            RoundCompleted?.Invoke();
            hud?.ShowShop(round, money, weaponDamageUpgradeCost, fireRateUpgradeCost, ferryHealthUpgradeCost);
            RefreshHud();
        }

        private void HandleFerryDestroyed(Health destroyedHealth)
        {
            if (State == PrototypeGameState.GameOver)
            {
                return;
            }

            State = PrototypeGameState.GameOver;
            SetGameplayInput(false);
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

        private void SetGameplayInput(bool isEnabled)
        {
            playerController?.SetInputEnabled(isEnabled);
            playerWeapon?.SetInputEnabled(isEnabled);
        }

        private void RefreshHud()
        {
            float current = ferryHealth != null ? ferryHealth.CurrentHealth : 0f;
            float max = ferryHealth != null ? ferryHealth.MaxHealth : 1f;
            float weaponDamage = playerWeapon != null ? playerWeapon.Damage : 0f;
            float shotsPerSecond = playerWeapon != null ? playerWeapon.ShotsPerSecond : 0f;
            hud?.SetStats(current, max, money, round, CrossingProgress, weaponDamage, shotsPerSecond);
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

            if (playerController == null)
            {
                playerController = FindFirstObjectByType<SimpleFPSController>();
            }

            if (playerWeapon == null)
            {
                playerWeapon = FindFirstObjectByType<HitscanWeapon>();
            }

            if (hud == null)
            {
                hud = FindFirstObjectByType<SimpleHUD>();
            }
        }
    }
}
