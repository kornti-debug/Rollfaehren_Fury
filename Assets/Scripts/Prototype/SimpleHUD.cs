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
        private Text warningText;
        private float warningHideTime;
        private float healthFillTarget = 1f;
        private float crossingFillTarget;

        private void Awake()
        {
            BuildImprovedHud();
        }

        private void Update()
        {
            if (warningText != null && warningText.gameObject.activeSelf && Time.unscaledTime >= warningHideTime)
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

        public void SetStats(float ferryHealth, float ferryMaxHealth, int money, int round, float crossingProgress, string weaponName, int weaponIndex, int weaponCount, float weaponDamage, float shotsPerSecond)
        {
            if (ferryHealthText != null)
            {
                ferryHealthText.text = $"Ferry: {Mathf.CeilToInt(ferryHealth)} / {Mathf.CeilToInt(ferryMaxHealth)}";
            }

            healthFillTarget = ferryMaxHealth <= 0f ? 0f : Mathf.Clamp01(ferryHealth / ferryMaxHealth);

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

            crossingFillTarget = Mathf.Clamp01(crossingProgress);

            if (weaponStatsText != null)
            {
                string slot = weaponCount > 1 ? $" [{weaponIndex}/{weaponCount}]" : string.Empty;
                weaponStatsText.text = $"{weaponName}{slot}  {weaponDamage:0} dmg | {shotsPerSecond * 60f:0} RPM";
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

        // Restructures the builder-generated HUD at runtime: weapon stats move to a
        // bottom-right panel, and a top-center swarm-warning banner is added. The top-left
        // status panel (health / money / round / crossing) is left where it is.
        private void BuildImprovedHud()
        {
            RectTransform root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            // Compact + subtle card background behind the top-left status cluster.
            if (gameplayPanel != null)
            {
                gameplayPanel.transform.localScale = Vector3.one * 0.72f;
                Image statusBackground = gameplayPanel.GetComponent<Image>();
                if (statusBackground == null)
                {
                    statusBackground = gameplayPanel.AddComponent<Image>();
                }

                statusBackground.color = new Color(0.04f, 0.06f, 0.09f, 0.5f);
                statusBackground.raycastTarget = false;
            }

            // Give the bars a clear "empty" colour so the lost/remaining part reads off the bar.
            TintBarBackground(ferryHealthFill, new Color(0.14f, 0.14f, 0.16f, 0.85f));
            TintBarBackground(crossingFill, new Color(0.14f, 0.14f, 0.16f, 0.85f));

            // Money gets its own top-right card.
            if (moneyText != null)
            {
                GameObject moneyPanel = new GameObject("Money HUD", typeof(RectTransform), typeof(Image));
                moneyPanel.transform.SetParent(root, false);
                RectTransform moneyRect = moneyPanel.GetComponent<RectTransform>();
                moneyRect.anchorMin = moneyRect.anchorMax = moneyRect.pivot = new Vector2(1f, 1f);
                moneyRect.anchoredPosition = new Vector2(-28f, -28f);
                moneyRect.sizeDelta = new Vector2(280f, 64f);

                Image moneyBackground = moneyPanel.GetComponent<Image>();
                moneyBackground.color = new Color(0.04f, 0.06f, 0.09f, 0.5f);
                moneyBackground.raycastTarget = false;

                moneyText.transform.SetParent(moneyPanel.transform, false);
                RectTransform moneyTextRect = moneyText.rectTransform;
                moneyTextRect.anchorMin = Vector2.zero;
                moneyTextRect.anchorMax = Vector2.one;
                moneyTextRect.offsetMin = new Vector2(16f, 8f);
                moneyTextRect.offsetMax = new Vector2(-16f, -8f);
                moneyText.alignment = TextAnchor.MiddleRight;
                moneyText.fontSize = 26;
            }

            if (weaponStatsText != null)
            {
                GameObject weaponPanel = new GameObject("Weapon HUD", typeof(RectTransform), typeof(Image));
                weaponPanel.transform.SetParent(root, false);
                RectTransform panelRect = weaponPanel.GetComponent<RectTransform>();
                panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = new Vector2(1f, 0f);
                panelRect.anchoredPosition = new Vector2(-28f, 28f);
                panelRect.sizeDelta = new Vector2(440f, 76f);

                Image background = weaponPanel.GetComponent<Image>();
                background.color = new Color(0.04f, 0.06f, 0.09f, 0.55f);
                background.raycastTarget = false;

                weaponStatsText.transform.SetParent(weaponPanel.transform, false);
                RectTransform statsRect = weaponStatsText.rectTransform;
                statsRect.anchorMin = Vector2.zero;
                statsRect.anchorMax = Vector2.one;
                statsRect.offsetMin = new Vector2(16f, 8f);
                statsRect.offsetMax = new Vector2(-16f, -8f);
                weaponStatsText.alignment = TextAnchor.MiddleRight;
                weaponStatsText.fontSize = 24;
            }

            GameObject warningObject = new GameObject("Swarm Warning", typeof(RectTransform));
            warningObject.transform.SetParent(root, false);
            RectTransform warningRect = warningObject.GetComponent<RectTransform>();
            warningRect.anchorMin = warningRect.anchorMax = warningRect.pivot = new Vector2(0.5f, 1f);
            warningRect.anchoredPosition = new Vector2(0f, -96f);
            warningRect.sizeDelta = new Vector2(960f, 64f);

            warningText = warningObject.AddComponent<Text>();
            warningText.font = ResolveFont();
            warningText.fontSize = 36;
            warningText.fontStyle = FontStyle.Bold;
            warningText.alignment = TextAnchor.MiddleCenter;
            warningText.color = new Color(1f, 0.35f, 0.2f);
            warningText.raycastTarget = false;
            warningObject.SetActive(false);
        }

        private Font ResolveFont()
        {
            if (weaponStatsText != null && weaponStatsText.font != null)
            {
                return weaponStatsText.font;
            }

            if (messageText != null && messageText.font != null)
            {
                return messageText.font;
            }

            if (ferryHealthText != null && ferryHealthText.font != null)
            {
                return ferryHealthText.font;
            }

            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
                bar.fillAmount = Mathf.MoveTowards(bar.fillAmount, target, Time.unscaledDeltaTime * 1.5f);
            }
        }

        private static void TintBarBackground(Image fill, Color color)
        {
            if (fill != null && fill.transform.parent != null)
            {
                Image background = fill.transform.parent.GetComponent<Image>();
                if (background != null)
                {
                    background.color = color;
                }
            }
        }

        private static Color HealthColor(float fill)
        {
            Color low = new Color(0.92f, 0.22f, 0.18f);
            Color mid = new Color(0.96f, 0.78f, 0.20f);
            Color high = new Color(0.18f, 0.85f, 0.38f);
            return fill < 0.5f
                ? Color.Lerp(low, mid, fill * 2f)
                : Color.Lerp(mid, high, (fill - 0.5f) * 2f);
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
