using UnityEngine;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    public sealed class SimpleHUD : MonoBehaviour
    {
        [Header("Gameplay")]
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private Text ferryHealthText;
        [SerializeField] private Image ferryHealthFill;
        [SerializeField] private Text moneyText;
        [SerializeField] private Text roundText;
        [SerializeField] private Text crossingText;
        [SerializeField] private Image crossingFill;
        [SerializeField] private Text weaponStatsText;
        [SerializeField] private Text messageText;
        [SerializeField] private Text warningText;
        [SerializeField] private GameObject reloadBarRoot;
        [SerializeField] private Image reloadBarFill;
        [SerializeField] private Text reloadBarLabel;

        [Header("Shop")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Text shopTitleText;
        [SerializeField] private Text shopMoneyText;
        [SerializeField] private GameObject nextRoundButton;
        [SerializeField] private GameObject closeShopButton;

        [Header("Augment Draft")]
        [SerializeField] private GameObject augmentDraftPanel;

        [Header("Game Over")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text gameOverText;

        private GameManager gameManager;
        private float warningHideTime;
        private float healthFillTarget = 1f;
        private float crossingFillTarget;

        private void Awake()
        {
            MakeFillable(ferryHealthFill);
            MakeFillable(crossingFill);
            MakeFillable(reloadBarFill);
            SetActive(reloadBarRoot, false);
            SetActive(warningText != null ? warningText.gameObject : null, false);
        }

        private void Update()
        {
            if (warningText != null
                && warningText.gameObject.activeSelf
                && Time.unscaledTime >= warningHideTime)
            {
                warningText.gameObject.SetActive(false);
            }

            AnimateBar(ferryHealthFill, healthFillTarget);
            AnimateBar(crossingFill, crossingFillTarget);
            if (ferryHealthFill != null)
            {
                ferryHealthFill.color = HealthColor(ferryHealthFill.fillAmount);
            }
        }

        public void Bind(GameManager manager)
        {
            gameManager = manager;
        }

        public void SetStats(
            float ferryHealth,
            float ferryMaxHealth,
            int money,
            int round,
            float crossingProgress,
            string weaponName,
            int weaponIndex,
            int weaponCount,
            string weaponDamage,
            float shotsPerSecond,
            int ammoInMagazine,
            int magazineSize,
            int reserveAmmo,
            bool infiniteAmmo,
            bool isReloading,
            float reloadProgress)
        {
            SetText(
                ferryHealthText,
                $"FERRY  {Mathf.CeilToInt(ferryHealth)} / {Mathf.CeilToInt(ferryMaxHealth)}");
            healthFillTarget = ferryMaxHealth <= 0f
                ? 0f
                : Mathf.Clamp01(ferryHealth / ferryMaxHealth);

            SetText(moneyText, $"${money}");
            SetText(roundText, $"ROUND {round}");

            int percent = Mathf.RoundToInt(Mathf.Clamp01(crossingProgress) * 100f);
            SetText(crossingText, $"CROSSING  {percent}%");
            crossingFillTarget = Mathf.Clamp01(crossingProgress);

            if (weaponStatsText != null)
            {
                string slot = weaponCount > 1 ? $"  [{weaponIndex}/{weaponCount}]" : string.Empty;
                string ammo = infiniteAmmo
                    ? "AMMO  UNLIMITED"
                    : isReloading
                        ? $"RELOADING  |  RESERVE {Mathf.Max(0, reserveAmmo)}"
                        : $"AMMO {Mathf.Max(0, ammoInMagazine)}/{magazineSize}  |  RESERVE {Mathf.Max(0, reserveAmmo)}";
                weaponStatsText.text =
                    $"{weaponName.ToUpperInvariant()}{slot}\n{weaponDamage} DMG  |  {shotsPerSecond * 60f:0} RPM\n{ammo}";
            }

            bool showReload = isReloading && !infiniteAmmo;
            SetActive(reloadBarRoot, showReload);
            if (reloadBarFill != null)
            {
                reloadBarFill.fillAmount = Mathf.Clamp01(reloadProgress);
            }

            SetText(reloadBarLabel, "RELOADING");
        }

        public void ShowGameplay()
        {
            SetActive(gameplayPanel, true);
            SetActive(shopPanel, false);
            SetActive(gameOverPanel, false);
            SetActive(augmentDraftPanel, false);
            ShowMessage(string.Empty);
        }

        public void ShowShop(int completedRound, int money)
        {
            ShowShopPanel($"ROUND {completedRound} SURVIVED", money, true);
        }

        public void ShowShopOverlay(int money)
        {
            ShowShopPanel("FERRY SUPPLY OFFICE", money, false);
        }

        public void SetShopMoney(int money)
        {
            SetText(shopMoneyText, $"AVAILABLE FUNDS  ${money}");
        }

        private void ShowShopPanel(string title, int money, bool showNextRound)
        {
            SetActive(gameplayPanel, true);
            SetActive(shopPanel, true);
            SetActive(gameOverPanel, false);
            SetActive(augmentDraftPanel, false);
            SetText(shopTitleText, title);
            SetShopMoney(money);
            SetActive(nextRoundButton, showNextRound);
            SetActive(closeShopButton, !showNextRound);
        }

        public void ShowAugmentDraft(int completedRound)
        {
            SetActive(gameplayPanel, true);
            SetActive(shopPanel, false);
            SetActive(gameOverPanel, false);
            SetActive(augmentDraftPanel, true);
        }

        public void ShowGameOver(int round, int money)
        {
            SetActive(gameplayPanel, true);
            SetActive(shopPanel, false);
            SetActive(gameOverPanel, true);
            SetActive(augmentDraftPanel, false);
            SetText(gameOverText, $"FERRY LOST\nROUND {round}  |  ${money}");
        }

        public void ShowMessage(string message)
        {
            SetText(messageText, message);
        }

        public void ShowWarning(string message, float duration)
        {
            if (warningText == null)
            {
                return;
            }

            warningText.text = message;
            warningText.gameObject.SetActive(true);
            warningHideTime = Time.unscaledTime + Mathf.Max(0.1f, duration);
        }

        public void StartNextRound()
        {
            gameManager?.StartNextRound();
        }

        public void RestartGame()
        {
            gameManager?.RestartGame();
        }

        private static void AnimateBar(Image bar, float target)
        {
            if (bar != null)
            {
                bar.fillAmount = Mathf.MoveTowards(
                    bar.fillAmount,
                    target,
                    Time.unscaledDeltaTime * 1.5f);
            }
        }

        private static void MakeFillable(Image bar)
        {
            if (bar == null)
            {
                return;
            }

            bar.type = Image.Type.Filled;
            bar.fillMethod = Image.FillMethod.Horizontal;
            bar.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        private static Color HealthColor(float fill)
        {
            return fill < 0.5f
                ? Color.Lerp(UiTheme.Siren, UiTheme.Warning, fill * 2f)
                : Color.Lerp(UiTheme.Warning, UiTheme.Success, (fill - 0.5f) * 2f);
        }

        private static void SetText(Text label, string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }

        private static void SetActive(GameObject target, bool isActive)
        {
            if (target != null)
            {
                target.SetActive(isActive);
            }
        }
    }
}
