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

        public void Bind(GameManager manager)
        {
            gameManager = manager;
        }

        public void SetStats(float ferryHealth, float ferryMaxHealth, int money, int round, float crossingProgress, string weaponName, int weaponIndex, int weaponCount, float weaponDamage, float shotsPerSecond)
        {
            if (ferryHealthText != null)
            {
                ferryHealthText.text = $"Ferry: {Mathf.CeilToInt(ferryHealth)} / {Mathf.CeilToInt(ferryMaxHealth)}";
            }

            if (ferryHealthFill != null)
            {
                ferryHealthFill.fillAmount = ferryMaxHealth <= 0f ? 0f : Mathf.Clamp01(ferryHealth / ferryMaxHealth);
            }

            if (moneyText != null)
            {
                moneyText.text = $"Money: ${money}";
            }

            if (roundText != null)
            {
                roundText.text = $"Round {round}";
            }

            int percent = Mathf.RoundToInt(Mathf.Clamp01(crossingProgress) * 100f);
            if (crossingText != null)
            {
                crossingText.text = $"Crossing: {percent}%";
            }

            if (crossingFill != null)
            {
                crossingFill.fillAmount = Mathf.Clamp01(crossingProgress);
            }

            if (weaponStatsText != null)
            {
                string slot = weaponCount > 1 ? $" [{weaponIndex}/{weaponCount}]" : string.Empty;
                weaponStatsText.text = $"{weaponName}{slot}  {weaponDamage:0} dmg | {shotsPerSecond:0.0}/s";
            }
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
            ShowShopPanel($"Round {completedRound} survived", money, true);
        }

        public void ShowShopOverlay(int money)
        {
            ShowShopPanel("Shop", money, false);
        }

        private void ShowShopPanel(string title, int money, bool showNextRound)
        {
            SetActive(gameplayPanel, true);
            SetActive(shopPanel, true);
            SetActive(gameOverPanel, false);
            SetActive(augmentDraftPanel, false);

            if (shopTitleText != null)
            {
                shopTitleText.text = title;
            }

            if (shopMoneyText != null)
            {
                shopMoneyText.text = $"Money: ${money}";
            }

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

            if (gameOverText != null)
            {
                gameOverText.text = $"Ferry destroyed\nReached round {round}\nMoney: ${money}";
            }
        }

        public void ShowMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }
        }

        public void StartNextRound()
        {
            gameManager?.StartNextRound();
        }

        public void RestartGame()
        {
            gameManager?.RestartGame();
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
