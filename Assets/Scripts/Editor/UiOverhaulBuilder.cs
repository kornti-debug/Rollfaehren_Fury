using System.Collections.Generic;
using RollfaehrenFury.Prototype;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RollfaehrenFury.Editor
{
    public static class UiOverhaulBuilder
    {
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string MenuScenePath = "Assets/Scenes/Menu.unity";
        private const string MainScenePath = "Assets/Scenes/Main.unity";
        private const string ShopScenePath = "Assets/Scenes/ShopInterior.unity";
        private const string ProjectInputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string UiPrefabFolder = "Assets/UI/Prefabs";
        private const string FontPath = "Assets/UI/Fonts/BarlowSemiCondensed-SemiBold.ttf";
        private static readonly string[] MenuGeneratedRoots =
        {
            "Menu Background",
            "Menu Readability Scrim",
            "Main Panel",
            "Settings Panel"
        };

        private static readonly string[] GameplayGeneratedRoots =
        {
            "Gameplay Panel",
            "Shop Panel",
            "Pause Panel",
            "Pause Settings Panel",
            "Augment Draft Panel",
            "Game Over Panel",
            "Close Shop Button"
        };

        [MenuItem("Rollfaehren Fury/Build Ferry Hazard UI")]
        public static void BuildAll()
        {
            EnsureFolders();
            CreateThemePrefabs();
            BuildMenuScene();
            BuildMainSceneUi();
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Ferry hazard UI rebuilt and scene references repaired.");
        }

        public static void BuildAllFromCommandLine()
        {
            BuildAll();
        }

        private static void BuildMenuScene()
        {
            Scene scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(MenuScenePath) != null
                ? EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            EnsureMenuCamera();

            GameObject canvasObject = FindSceneObject("Main Menu Canvas");
            if (canvasObject == null || canvasObject.GetComponent<UiLayoutMarker>() == null)
            {
                if (canvasObject != null)
                {
                    Object.DestroyImmediate(canvasObject);
                }

                canvasObject = CreateCanvas("Main Menu Canvas");
                EnsureComponent<UiLayoutMarker>(canvasObject);
            }

            DestroyGeneratedSceneObjects(MenuGeneratedRoots);
            ClearGeneratedRoots(canvasObject.transform, MenuGeneratedRoots);
            BuildMenuCanvas(canvasObject.transform);
            MainMenuController controller = EnsureMenuController();
            WireMainMenu(canvasObject.transform, controller);
            EnsureEventSystem();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MenuScenePath);
        }

        private static void BuildMainSceneUi()
        {
            Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
            WeaponSystem weaponSystem = Object.FindFirstObjectByType<WeaponSystem>();
            AugmentSystem augmentSystem = gameManager != null ? gameManager.GetComponent<AugmentSystem>() : null;

            GameObject canvasObject = FindSceneObject("Rollfaehren Fury Prototype HUD");
            if (canvasObject == null || canvasObject.GetComponent<UiLayoutMarker>() == null)
            {
                if (canvasObject != null)
                {
                    Object.DestroyImmediate(canvasObject);
                }

                canvasObject = CreateCanvas("Rollfaehren Fury Prototype HUD");
                EnsureComponent<UiLayoutMarker>(canvasObject);
            }

            DestroyGeneratedSceneObjects(GameplayGeneratedRoots);
            ClearGeneratedRoots(canvasObject.transform, GameplayGeneratedRoots);
            BuildGameplayCanvas(canvasObject.transform);
            SimpleHUD hud = EnsureComponent<SimpleHUD>(canvasObject);
            ShopManager shopManager = gameManager != null ? EnsureComponent<ShopManager>(gameManager.gameObject) : null;
            GameplayMenuInput pauseInput = EnsureGameplayMenuInput(gameManager);

            WireHud(canvasObject.transform, hud);
            WireShop(canvasObject.transform, shopManager, gameManager, weaponSystem);
            WirePause(canvasObject.transform, pauseInput, gameManager);
            WireAugments(canvasObject.transform, augmentSystem, gameManager);
            WireGameOver(canvasObject.transform, gameManager);
            EnsureUiAudio(canvasObject);
            EnsureEventSystem();

            if (gameManager != null)
            {
                SetObject(gameManager, "shopManager", shopManager);
                SetObject(gameManager, "hud", hud);
                SetObject(gameManager, "augmentSystem", augmentSystem);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void BuildMenuCanvas(Transform canvas)
        {
            GameObject background = CreateFullRect(canvas, "Menu Background", UiTheme.River);
            background.GetComponent<Image>().raycastTarget = false;

            GameObject scrim = CreateFullRect(canvas, "Menu Readability Scrim", UiTheme.WithAlpha(UiTheme.Hull, 0.78f));
            scrim.GetComponent<Image>().raycastTarget = false;

            GameObject mainPanel = CreatePanel(canvas, "Main Panel", Vector2.zero, new Vector2(560f, 600f), TextAnchor.MiddleCenter);
            AddHeader(mainPanel.transform, "Title", "ROLLFAEHREN\nFURY", new Vector2(24f, -26f), new Vector2(472f, 142f), 58);
            CreateText(mainPanel.transform, "Subtitle", "Protect the ferry. Survive the crossing.", new Vector2(24f, -176f), new Vector2(460f, 44f), 24, TextAnchor.MiddleLeft, UiTheme.Muted, TextAnchor.UpperLeft);
            CreateButton(mainPanel.transform, "New Game Button", "NEW GAME", new Vector2(24f, -258f), new Vector2(360f, 58f), out _);
            CreateButton(mainPanel.transform, "Settings Button", "SETTINGS", new Vector2(24f, -332f), new Vector2(360f, 58f), out _);
            CreateButton(mainPanel.transform, "Quit Button", "QUIT", new Vector2(24f, -406f), new Vector2(360f, 58f), out _);

            GameObject settings = CreatePanel(canvas, "Settings Panel", Vector2.zero, new Vector2(720f, 560f), TextAnchor.MiddleCenter);
            settings.SetActive(false);
            BuildSettingsPanel(settings.transform, "MENU SETTINGS", "Settings Back Button");
        }

        private static void BuildGameplayCanvas(Transform canvas)
        {
            GameObject gameplayPanel = CreateFullRect(canvas, "Gameplay Panel", Color.clear);
            gameplayPanel.GetComponent<Image>().raycastTarget = false;

            GameObject topLeft = CreatePanel(gameplayPanel.transform, "Status Block", new Vector2(22f, -22f), new Vector2(360f, 190f), TextAnchor.UpperLeft);
            CreateText(topLeft.transform, "Round Text", "ROUND 1", new Vector2(18f, -12f), new Vector2(190f, 30f), 24, TextAnchor.MiddleLeft, UiTheme.Foam, TextAnchor.UpperLeft);
            CreateText(topLeft.transform, "Money Text", "$0", new Vector2(-18f, -12f), new Vector2(120f, 30f), 24, TextAnchor.MiddleRight, UiTheme.Warning, TextAnchor.UpperRight);
            CreateText(topLeft.transform, "Ferry Health Text", "FERRY 100 / 100", new Vector2(18f, -56f), new Vector2(300f, 24f), 18, TextAnchor.MiddleLeft, UiTheme.Muted, TextAnchor.UpperLeft);
            CreateBar(topLeft.transform, "Ferry Health Bar", new Vector2(18f, -86f), new Vector2(304f, 16f), UiTheme.Success, out _);
            CreateText(topLeft.transform, "Crossing Text", "CROSSING 0%", new Vector2(18f, -122f), new Vector2(300f, 24f), 18, TextAnchor.MiddleLeft, UiTheme.Muted, TextAnchor.UpperLeft);
            CreateBar(topLeft.transform, "Crossing Bar", new Vector2(18f, -152f), new Vector2(304f, 16f), UiTheme.Progress, out _);

            GameObject weaponBlock = CreatePanel(gameplayPanel.transform, "Weapon Block", new Vector2(-22f, 22f), new Vector2(360f, 112f), TextAnchor.LowerRight);
            CreateText(weaponBlock.transform, "Weapon Stats Text", "HARPOON\n25 DMG | 60 RPM\nAMMO UNLIMITED", new Vector2(18f, -14f), new Vector2(320f, 86f), 20, TextAnchor.MiddleLeft, UiTheme.Foam, TextAnchor.UpperLeft);

            CreateCrosshair(gameplayPanel.transform);
            CreateText(gameplayPanel.transform, "Message Text", string.Empty, new Vector2(0f, 118f), new Vector2(760f, 34f), 22, TextAnchor.MiddleCenter, UiTheme.Foam, TextAnchor.LowerCenter);
            CreateText(gameplayPanel.transform, "Warning Text", string.Empty, new Vector2(0f, 164f), new Vector2(760f, 36f), 22, TextAnchor.MiddleCenter, UiTheme.Warning, TextAnchor.LowerCenter).gameObject.SetActive(false);

            GameObject reload = CreatePanel(gameplayPanel.transform, "Reload Bar Root", new Vector2(0f, 74f), new Vector2(280f, 38f), TextAnchor.LowerCenter);
            CreateBar(reload.transform, "Reload Bar", Vector2.zero, new Vector2(230f, 12f), UiTheme.Warning, out _);
            CreateText(reload.transform, "Reload Label", "RELOADING", new Vector2(0f, 10f), new Vector2(230f, 18f), 13, TextAnchor.MiddleCenter, UiTheme.Foam, TextAnchor.MiddleCenter);
            reload.SetActive(false);

            BuildShopPanel(canvas);
            BuildPausePanels(canvas);
            BuildAugmentPanel(canvas);
            BuildGameOverPanel(canvas);
        }

        private static void BuildShopPanel(Transform canvas)
        {
            GameObject shop = CreateFullRect(canvas, "Shop Panel", UiTheme.WithAlpha(UiTheme.Hull, 0.92f));
            shop.SetActive(false);

            GameObject frame = CreatePanel(shop.transform, "Shop Frame", Vector2.zero, new Vector2(1500f, 820f), TextAnchor.MiddleCenter);
            CreateText(frame.transform, "Shop Title", "FERRY SUPPLY OFFICE", new Vector2(32f, -24f), new Vector2(620f, 48f), 36, TextAnchor.MiddleLeft, UiTheme.Foam, TextAnchor.UpperLeft);
            CreateText(frame.transform, "Shop Money", "AVAILABLE FUNDS $0", new Vector2(-32f, -28f), new Vector2(420f, 42f), 26, TextAnchor.MiddleRight, UiTheme.Warning, TextAnchor.UpperRight);

            GameObject tabs = CreatePanel(frame.transform, "Weapon Tabs", new Vector2(32f, -104f), new Vector2(280f, 516f), TextAnchor.UpperLeft);
            string[] names = { "HARPOON", "PISTOL", "SHOTGUN", "ASSAULT RIFLE" };
            for (int i = 0; i < names.Length; i++)
            {
                CreateButton(tabs.transform, $"Weapon Tab {i}", names[i], new Vector2(18f, -22f - i * 112f), new Vector2(244f, 82f), out _);
            }

            GameObject summary = CreatePanel(frame.transform, "Weapon Summary", new Vector2(340f, -104f), new Vector2(430f, 516f), TextAnchor.UpperLeft);
            Image accent = CreateBlock(summary.transform, "Selected Weapon Accent", new Vector2(18f, -24f), new Vector2(10f, 120f), UiTheme.Warning, TextAnchor.UpperLeft);
            accent.raycastTarget = false;
            CreateText(summary.transform, "Selected Weapon Name", "HARPOON", new Vector2(42f, -22f), new Vector2(340f, 42f), 32, TextAnchor.MiddleLeft, UiTheme.Foam, TextAnchor.UpperLeft);
            CreateText(summary.transform, "Selected Weapon Stats", "25 damage | 60 RPM\nUnlimited ammunition", new Vector2(42f, -84f), new Vector2(340f, 96f), 22, TextAnchor.UpperLeft, UiTheme.Muted, TextAnchor.UpperLeft);
            CreateText(summary.transform, "Selected Weapon Requirement", "OWNED", new Vector2(42f, -214f), new Vector2(340f, 60f), 24, TextAnchor.MiddleLeft, UiTheme.Warning, TextAnchor.UpperLeft);

            GameObject grid = CreatePanel(frame.transform, "Upgrade Grid", new Vector2(-32f, -104f), new Vector2(660f, 516f), TextAnchor.UpperRight);
            for (int i = 0; i < 4; i++)
            {
                float x = i % 2 == 0 ? 20f : 340f;
                float y = i < 2 ? -24f : -264f;
                CreateButton(grid.transform, $"Upgrade Card {i}", "UPGRADE", new Vector2(x, y), new Vector2(300f, 210f), out _);
            }

            CreateButton(frame.transform, "Close Shop Button", "X", new Vector2(-32f, -24f), new Vector2(52f, 52f), out _, TextAnchor.UpperRight);
            CreateButton(frame.transform, "Refill Ammo Button", "REFILL AMMO", new Vector2(340f, 34f), new Vector2(300f, 58f), out _);
            CreateButton(frame.transform, "Next Round Button", "NEXT ROUND", new Vector2(-360f, 34f), new Vector2(260f, 58f), out _);
        }

        private static void BuildPausePanels(Transform canvas)
        {
            GameObject pause = CreateFullRect(canvas, "Pause Panel", UiTheme.WithAlpha(Color.black, 0.58f));
            GameObject column = CreatePanel(pause.transform, "Pause Command Panel", Vector2.zero, new Vector2(440f, 560f), TextAnchor.MiddleCenter);
            CreateText(column.transform, "Pause Title", "PAUSED", new Vector2(24f, -28f), new Vector2(350f, 56f), 40, TextAnchor.MiddleLeft, UiTheme.Foam, TextAnchor.UpperLeft);
            CreateButton(column.transform, "Pause Resume Button", "RESUME", new Vector2(24f, -116f), new Vector2(330f, 56f), out _);
            CreateButton(column.transform, "Pause New Game Button", "NEW GAME", new Vector2(24f, -186f), new Vector2(330f, 56f), out _);
            CreateButton(column.transform, "Pause Settings Button", "SETTINGS", new Vector2(24f, -256f), new Vector2(330f, 56f), out _);
            CreateButton(column.transform, "Pause Main Menu Button", "MAIN MENU", new Vector2(24f, -326f), new Vector2(330f, 56f), out _);
            CreateButton(column.transform, "Pause Quit Button", "QUIT", new Vector2(24f, -396f), new Vector2(330f, 56f), out _);
            pause.SetActive(false);

            GameObject settings = CreateFullRect(canvas, "Pause Settings Panel", UiTheme.WithAlpha(Color.black, 0.62f));
            GameObject frame = CreatePanel(settings.transform, "Pause Settings Frame", Vector2.zero, new Vector2(720f, 560f), TextAnchor.MiddleCenter);
            BuildSettingsPanel(frame.transform, "SETTINGS", "Pause Settings Back Button");
            settings.SetActive(false);
        }

        private static void BuildAugmentPanel(Transform canvas)
        {
            GameObject augment = CreateFullRect(canvas, "Augment Draft Panel", UiTheme.WithAlpha(UiTheme.Hull, 0.92f));
            GameObject frame = CreatePanel(augment.transform, "Augment Frame", Vector2.zero, new Vector2(1420f, 620f), TextAnchor.MiddleCenter);
            CreateText(frame.transform, "Augment Title", "CHOOSE YOUR NEXT EDGE", new Vector2(0f, -34f), new Vector2(1100f, 56f), 38, TextAnchor.MiddleCenter, UiTheme.Foam, TextAnchor.UpperCenter);
            for (int i = 0; i < 3; i++)
            {
                float x = -460f + i * 460f;
                CreateButton(frame.transform, $"Augment Draft Button {i}", "AUGMENT", new Vector2(x, -148f), new Vector2(390f, 330f), out Button button);
                Text label = button.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.fontSize = 23;
                    label.supportRichText = true;
                    label.horizontalOverflow = HorizontalWrapMode.Wrap;
                    label.verticalOverflow = VerticalWrapMode.Overflow;
                }
            }

            augment.SetActive(false);
        }

        private static void BuildGameOverPanel(Transform canvas)
        {
            GameObject gameOver = CreateFullRect(canvas, "Game Over Panel", UiTheme.WithAlpha(UiTheme.Hull, 0.92f));
            GameObject frame = CreatePanel(gameOver.transform, "Game Over Frame", Vector2.zero, new Vector2(760f, 430f), TextAnchor.MiddleCenter);
            AddHeader(frame.transform, "Game Over Header", "FERRY LOST", new Vector2(0f, -36f), new Vector2(640f, 76f), 52);
            CreateText(frame.transform, "Game Over Text", "ROUND 1 | $0", new Vector2(0f, -132f), new Vector2(620f, 92f), 28, TextAnchor.MiddleCenter, UiTheme.Muted, TextAnchor.UpperCenter);
            CreateButton(frame.transform, "Restart Button", "RESTART RUN", new Vector2(-150f, 128f), new Vector2(240f, 58f), out _);
            CreateButton(frame.transform, "Game Over Main Menu Button", "MAIN MENU", new Vector2(150f, 128f), new Vector2(240f, 58f), out _);
            gameOver.SetActive(false);
        }

        private static void BuildSettingsPanel(Transform parent, string title, string backButtonName)
        {
            CreateText(parent, "Settings Title", title, new Vector2(0f, -34f), new Vector2(620f, 52f), 38, TextAnchor.MiddleCenter, UiTheme.Foam, TextAnchor.UpperCenter);
            CreateSliderRow(parent, "Master Volume Row", "MASTER", new Vector2(0f, -130f), out Slider master, out Text masterValue);
            CreateSliderRow(parent, "Music Volume Row", "MUSIC", new Vector2(0f, -210f), out Slider music, out Text musicValue);
            CreateSliderRow(parent, "SFX Volume Row", "SFX", new Vector2(0f, -290f), out Slider sfx, out Text sfxValue);
            CreateSliderRow(parent, "Sensitivity Row", "MOUSE", new Vector2(0f, -370f), out Slider sensitivity, out Text sensitivityValue);
            CreateButton(parent, backButtonName, "BACK", new Vector2(0f, 34f), new Vector2(260f, 58f), out _, TextAnchor.LowerCenter);

            SettingsPanelController controller = EnsureComponent<SettingsPanelController>(parent.gameObject);
            SetObject(controller, "masterVolumeSlider", master);
            SetObject(controller, "musicVolumeSlider", music);
            SetObject(controller, "sfxVolumeSlider", sfx);
            SetObject(controller, "mouseSensitivitySlider", sensitivity);
            SetObject(controller, "masterValueText", masterValue);
            SetObject(controller, "musicValueText", musicValue);
            SetObject(controller, "sfxValueText", sfxValue);
            SetObject(controller, "sensitivityValueText", sensitivityValue);
        }

        private static void WireMainMenu(Transform canvas, MainMenuController controller)
        {
            GameObject mainPanel = Child(canvas, "Main Panel");
            GameObject settingsPanel = Child(canvas, "Settings Panel");
            Button newGame = FindButton(canvas, "New Game Button");
            Button settings = FindButton(canvas, "Settings Button");
            Button quit = FindButton(canvas, "Quit Button");
            Button back = FindButton(canvas, "Settings Back Button");

            ReplaceListener(newGame, controller.NewGame);
            ReplaceListener(settings, controller.ShowSettings);
            ReplaceListener(quit, controller.QuitGame);
            ReplaceListener(back, controller.ShowMain);

            SetObject(controller, "mainPanel", mainPanel);
            SetObject(controller, "settingsPanel", settingsPanel);
            SetObject(controller, "firstSelectedButton", newGame != null ? newGame.gameObject : null);
            SetObject(controller, "settingsFirstSelectedButton", back != null ? back.gameObject : null);
            EnsureUiAudio(canvas.gameObject);
        }

        private static void WireHud(Transform canvas, SimpleHUD hud)
        {
            Transform gameplay = canvas.Find("Gameplay Panel");
            Transform status = gameplay?.Find("Status Block");
            Transform weapon = gameplay?.Find("Weapon Block");
            Transform reload = gameplay?.Find("Reload Bar Root");

            SetObject(hud, "gameplayPanel", gameplay != null ? gameplay.gameObject : null);
            SetObject(hud, "roundText", FindText(status, "Round Text"));
            SetObject(hud, "ferryHealthText", FindText(status, "Ferry Health Text"));
            SetObject(hud, "ferryHealthFill", FindImage(status, "Ferry Health Bar/Fill"));
            SetObject(hud, "crossingText", FindText(status, "Crossing Text"));
            SetObject(hud, "crossingFill", FindImage(status, "Crossing Bar/Fill"));
            SetObject(hud, "moneyText", FindText(status, "Money Text"));
            SetObject(hud, "weaponStatsText", FindText(weapon, "Weapon Stats Text"));
            SetObject(hud, "messageText", FindText(gameplay, "Message Text"));
            SetObject(hud, "warningText", FindText(gameplay, "Warning Text"));
            SetObject(hud, "reloadBarRoot", reload != null ? reload.gameObject : null);
            SetObject(hud, "reloadBarFill", FindImage(reload, "Reload Bar/Fill"));
            SetObject(hud, "reloadBarLabel", FindText(reload, "Reload Label"));
            SetObject(hud, "shopPanel", Child(canvas, "Shop Panel"));
            SetObject(hud, "shopTitleText", FindText(canvas, "Shop Panel/Shop Frame/Shop Title"));
            SetObject(hud, "shopMoneyText", FindText(canvas, "Shop Panel/Shop Frame/Shop Money"));
            Button nextRound = FindButton(canvas, "Next Round Button");
            Button close = FindButton(canvas, "Close Shop Button");
            SetObject(hud, "nextRoundButton", nextRound != null ? nextRound.gameObject : null);
            SetObject(hud, "closeShopButton", close != null ? close.gameObject : null);
            SetObject(hud, "augmentDraftPanel", Child(canvas, "Augment Draft Panel"));
            SetObject(hud, "gameOverPanel", Child(canvas, "Game Over Panel"));
            SetObject(hud, "gameOverText", FindText(canvas, "Game Over Panel/Game Over Frame/Game Over Text"));
        }

        private static void WireShop(Transform canvas, ShopManager shopManager, GameManager gameManager, WeaponSystem weaponSystem)
        {
            if (shopManager == null)
            {
                return;
            }

            List<Object> tabs = new List<Object>();
            for (int i = 0; i < 4; i++)
            {
                Button tab = FindButton(canvas, $"Weapon Tab {i}");
                if (tab != null)
                {
                    tabs.Add(tab);
                }
            }

            List<Object> cards = new List<Object>();
            for (int i = 0; i < 4; i++)
            {
                Button card = FindButton(canvas, $"Upgrade Card {i}");
                if (card != null)
                {
                    cards.Add(card);
                }
            }

            SetObject(shopManager, "gameManager", gameManager);
            SetObject(shopManager, "weaponSystem", weaponSystem);
            SetObjectList(shopManager, "weaponTabs", tabs.ToArray());
            SetObjectList(shopManager, "upgradeCards", cards.ToArray());
            SetObject(shopManager, "refillButton", FindButton(canvas, "Refill Ammo Button"));
            SetObject(shopManager, "selectedWeaponNameText", FindText(canvas, "Shop Panel/Shop Frame/Weapon Summary/Selected Weapon Name"));
            SetObject(shopManager, "selectedWeaponStatsText", FindText(canvas, "Shop Panel/Shop Frame/Weapon Summary/Selected Weapon Stats"));
            SetObject(shopManager, "selectedWeaponRequirementText", FindText(canvas, "Shop Panel/Shop Frame/Weapon Summary/Selected Weapon Requirement"));
            SetObject(shopManager, "selectedWeaponAccent", FindImage(canvas, "Shop Panel/Shop Frame/Weapon Summary/Selected Weapon Accent"));
        }

        private static void WirePause(Transform canvas, GameplayMenuInput pauseInput, GameManager gameManager)
        {
            if (pauseInput == null)
            {
                return;
            }

            SetObject(pauseInput, "gameManager", gameManager);
            SetObject(pauseInput, "pausePanel", Child(canvas, "Pause Panel"));
            SetObject(pauseInput, "settingsPanel", Child(canvas, "Pause Settings Panel"));

            ReplaceListener(FindButton(canvas, "Pause Resume Button"), pauseInput.Resume);
            ReplaceListener(FindButton(canvas, "Pause New Game Button"), pauseInput.RestartRun);
            ReplaceListener(FindButton(canvas, "Pause Settings Button"), pauseInput.ShowSettings);
            ReplaceListener(FindButton(canvas, "Pause Main Menu Button"), pauseInput.ReturnToMenu);
            ReplaceListener(FindButton(canvas, "Pause Quit Button"), pauseInput.QuitGame);
            ReplaceListener(FindButton(canvas, "Pause Settings Back Button"), pauseInput.BackToPause);
        }

        private static void WireAugments(Transform canvas, AugmentSystem augmentSystem, GameManager gameManager)
        {
            if (augmentSystem == null)
            {
                return;
            }

            Button[] draftButtons = new Button[3];
            for (int i = 0; i < draftButtons.Length; i++)
            {
                Button button = FindButton(canvas, $"Augment Draft Button {i}");
                draftButtons[i] = button;
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    UnityEventTools.AddIntPersistentListener(button.onClick, augmentSystem.Pick, i);
                    EditorUtility.SetDirty(button);
                }
            }

            SetObjectList(augmentSystem, "draftButtons", draftButtons);
            SetObject(augmentSystem, "gameManager", gameManager);
        }

        private static void WireGameOver(Transform canvas, GameManager gameManager)
        {
            if (gameManager == null)
            {
                return;
            }

            ReplaceListener(FindButton(canvas, "Next Round Button"), gameManager.StartNextRound);
            ReplaceListener(FindButton(canvas, "Close Shop Button"), gameManager.CloseShopOverlay);
            ReplaceListener(FindButton(canvas, "Restart Button"), gameManager.RestartGame);
            ReplaceListener(FindButton(canvas, "Game Over Main Menu Button"), SceneFlow.LoadMenu);
        }

        private static void CreateThemePrefabs()
        {
            CreatePrefabIfMissing("HazardPanel.prefab", () => CreatePanel(null, "Hazard Panel", Vector2.zero, new Vector2(420f, 240f), TextAnchor.MiddleCenter));
            CreatePrefabIfMissing("PrimaryButton.prefab", () => CreateButton(null, "Primary Button", "PRIMARY", Vector2.zero, new Vector2(300f, 58f), out _).transform.parent.gameObject);
            CreatePrefabIfMissing("SecondaryButton.prefab", () => CreateButton(null, "Secondary Button", "SECONDARY", Vector2.zero, new Vector2(300f, 58f), out _).transform.parent.gameObject);
            CreatePrefabIfMissing("WeaponTab.prefab", () => CreateButton(null, "Weapon Tab", "WEAPON", Vector2.zero, new Vector2(244f, 82f), out _).transform.parent.gameObject);
            CreatePrefabIfMissing("UpgradeCard.prefab", () => CreateButton(null, "Upgrade Card", "UPGRADE", Vector2.zero, new Vector2(300f, 210f), out _).transform.parent.gameObject);
            CreatePrefabIfMissing("SettingsSliderRow.prefab", () =>
            {
                GameObject row = CreatePanel(null, "Settings Slider Row", Vector2.zero, new Vector2(580f, 54f), TextAnchor.MiddleCenter);
                CreateSliderRow(row.transform, "Slider Row", "SETTING", Vector2.zero, out _, out _);
                return row;
            });
            CreatePrefabIfMissing("StatRow.prefab", () => CreateText(null, "Stat Row", "STAT  VALUE", Vector2.zero, new Vector2(320f, 32f), 20, TextAnchor.MiddleLeft, UiTheme.Foam, TextAnchor.MiddleCenter).gameObject);
            CreatePrefabIfMissing("WarningHeader.prefab", () => CreateText(null, "Warning Header", "WARNING", Vector2.zero, new Vector2(480f, 52f), 34, TextAnchor.MiddleCenter, UiTheme.Warning, TextAnchor.MiddleCenter).gameObject);
        }

        private static void CreatePrefabIfMissing(string fileName, System.Func<GameObject> factory)
        {
            string path = $"{UiPrefabFolder}/{fileName}";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                return;
            }

            GameObject instance = factory();
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
        }

        private static void ClearGeneratedRoots(Transform canvas, string[] rootNames)
        {
            if (canvas == null || rootNames == null)
            {
                return;
            }

            foreach (string rootName in rootNames)
            {
                bool removedAny;
                do
                {
                    removedAny = false;
                    Transform child = canvas.Find(rootName);
                    if (child != null)
                    {
                        Object.DestroyImmediate(child.gameObject);
                        removedAny = true;
                    }
                }
                while (removedAny);
            }
        }

        private static void DestroyGeneratedSceneObjects(string[] objectNames)
        {
            if (objectNames == null)
            {
                return;
            }

            HashSet<string> names = new HashSet<string>(objectNames);
            foreach (Transform transform in Object.FindObjectsByType<Transform>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (transform == null
                    || !transform.gameObject.scene.IsValid()
                    || !names.Contains(transform.name))
                {
                    continue;
                }

                Object.DestroyImmediate(transform.gameObject);
            }
        }

        private static GameObject CreateCanvas(string name)
        {
            GameObject canvasObject = new GameObject(name);
            Canvas canvas = EnsureComponent<Canvas>(canvasObject);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = EnsureComponent<CanvasScaler>(canvasObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            EnsureComponent<GraphicRaycaster>(canvasObject);
            return canvasObject;
        }

        private static GameObject CreateFullRect(Transform parent, string name, Color color)
        {
            GameObject panel = new GameObject(name);
            if (parent != null)
            {
                panel.transform.SetParent(parent, false);
            }

            RectTransform rect = EnsureComponent<RectTransform>(panel);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            Image image = EnsureComponent<Image>(panel);
            image.color = color;
            return panel;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, TextAnchor anchor)
        {
            GameObject panel = new GameObject(name);
            if (parent != null)
            {
                panel.transform.SetParent(parent, false);
            }

            RectTransform rect = EnsureComponent<RectTransform>(panel);
            ConfigureRect(rect, anchor, anchoredPosition, size);
            Image image = EnsureComponent<Image>(panel);
            image.color = UiTheme.WithAlpha(UiTheme.HullSoft, 0.94f);
            Outline outline = EnsureComponent<Outline>(panel);
            outline.effectColor = UiTheme.Warning;
            outline.effectDistance = new Vector2(2f, -2f);
            return panel;
        }

        private static Image CreateBlock(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, Color color, TextAnchor anchor)
        {
            GameObject block = new GameObject(name);
            block.transform.SetParent(parent, false);
            Image image = EnsureComponent<Image>(block);
            image.color = color;
            ConfigureRect(block.GetComponent<RectTransform>(), anchor, anchoredPosition, size);
            return image;
        }

        private static Text CreateText(Transform parent, string name, string text, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, Color color, TextAnchor anchor)
        {
            GameObject textObject = new GameObject(name);
            if (parent != null)
            {
                textObject.transform.SetParent(parent, false);
            }

            Text label = EnsureComponent<Text>(textObject);
            label.text = text;
            label.font = GetUiFont();
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            ConfigureRect(label.rectTransform, anchor, anchoredPosition, size);
            return label;
        }

        private static void AddHeader(Transform parent, string name, string text, Vector2 position, Vector2 size, int fontSize)
        {
            Text label = CreateText(parent, name, text, position, size, fontSize, TextAnchor.MiddleCenter, UiTheme.Warning, TextAnchor.UpperCenter);
            label.fontStyle = FontStyle.Bold;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        private static Text CreateButton(
            Transform parent,
            string name,
            string label,
            Vector2 anchoredPosition,
            Vector2 size,
            out Button button,
            TextAnchor anchor = TextAnchor.UpperLeft)
        {
            GameObject buttonObject = new GameObject(name);
            if (parent != null)
            {
                buttonObject.transform.SetParent(parent, false);
            }

            Image image = EnsureComponent<Image>(buttonObject);
            image.color = UiTheme.River;
            Outline outline = EnsureComponent<Outline>(buttonObject);
            outline.effectColor = UiTheme.Warning;
            outline.effectDistance = new Vector2(2f, -2f);
            button = EnsureComponent<Button>(buttonObject);
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = UiTheme.River;
            colors.highlightedColor = UiTheme.WarningDark;
            colors.selectedColor = UiTheme.Progress;
            colors.pressedColor = UiTheme.Warning;
            colors.disabledColor = UiTheme.WithAlpha(UiTheme.HullSoft, 0.56f);
            button.colors = colors;
            EnsureComponent<WwiseUIButtonAudio>(buttonObject);
            ConfigureRect(buttonObject.GetComponent<RectTransform>(), anchor, anchoredPosition, size);

            Text buttonText = CreateText(buttonObject.transform, "Label", label, Vector2.zero, new Vector2(size.x - 30f, size.y - 10f), 22, TextAnchor.MiddleCenter, UiTheme.Foam, TextAnchor.MiddleCenter);
            buttonText.fontStyle = FontStyle.Bold;
            return buttonText;
        }

        private static Image CreateBar(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, Color fillColor, out Image fill)
        {
            GameObject backgroundObject = new GameObject(name);
            backgroundObject.transform.SetParent(parent, false);
            Image background = EnsureComponent<Image>(backgroundObject);
            background.color = UiTheme.WithAlpha(UiTheme.Hull, 0.92f);
            ConfigureRect(background.rectTransform, TextAnchor.UpperLeft, anchoredPosition, size);

            GameObject fillObject = new GameObject("Fill");
            fillObject.transform.SetParent(backgroundObject.transform, false);
            fill = EnsureComponent<Image>(fillObject);
            fill.color = fillColor;
            fill.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 1f;
            RectTransform fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            return background;
        }

        private static void CreateSliderRow(Transform parent, string name, string label, Vector2 anchoredPosition, out Slider slider, out Text valueText)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);
            ConfigureRect(EnsureComponent<RectTransform>(row), TextAnchor.UpperCenter, anchoredPosition, new Vector2(580f, 54f));
            CreateText(row.transform, "Label", label, new Vector2(0f, 0f), new Vector2(120f, 36f), 20, TextAnchor.MiddleLeft, UiTheme.Foam, TextAnchor.MiddleLeft);
            valueText = CreateText(row.transform, "Value", "100%", new Vector2(0f, 0f), new Vector2(90f, 36f), 18, TextAnchor.MiddleRight, UiTheme.Warning, TextAnchor.MiddleRight);

            GameObject sliderObject = new GameObject("Slider");
            sliderObject.transform.SetParent(row.transform, false);
            ConfigureRect(EnsureComponent<RectTransform>(sliderObject), TextAnchor.MiddleCenter, new Vector2(0f, 0f), new Vector2(320f, 20f));
            slider = EnsureComponent<Slider>(sliderObject);
            slider.transition = Selectable.Transition.None;

            Image background = CreateBlock(sliderObject.transform, "Background", Vector2.zero, new Vector2(320f, 12f), UiTheme.Hull, TextAnchor.MiddleCenter);
            background.raycastTarget = true;
            Image fill = CreateBlock(sliderObject.transform, "Fill", Vector2.zero, new Vector2(320f, 12f), UiTheme.Warning, TextAnchor.MiddleCenter);
            RectTransform fillRect = fill.rectTransform;
            fillRect.anchorMin = new Vector2(0f, 0.5f);
            fillRect.anchorMax = new Vector2(1f, 0.5f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(sliderObject.transform, false);
            Image handleImage = EnsureComponent<Image>(handle);
            handleImage.color = UiTheme.Foam;
            ConfigureRect(handle.GetComponent<RectTransform>(), TextAnchor.MiddleLeft, Vector2.zero, new Vector2(18f, 30f));

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObject.transform, false);
            RectTransform fillAreaRect = EnsureComponent<RectTransform>(fillArea);
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;
            fill.transform.SetParent(fillArea.transform, false);
            fill.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            fill.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            fill.rectTransform.sizeDelta = new Vector2(0f, 12f);

            slider.fillRect = fill.rectTransform;
            slider.targetGraphic = handleImage;
            slider.handleRect = handle.GetComponent<RectTransform>();
            EnsureComponent<WwiseUIButtonAudio>(sliderObject);
        }

        private static void CreateCrosshair(Transform parent)
        {
            GameObject root = new GameObject("Crosshair");
            root.transform.SetParent(parent, false);
            RectTransform rect = EnsureComponent<RectTransform>(root);
            ConfigureRect(rect, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(32f, 32f));
            CreateBlock(root.transform, "Horizontal", Vector2.zero, new Vector2(24f, 3f), UiTheme.Foam, TextAnchor.MiddleCenter);
            CreateBlock(root.transform, "Vertical", Vector2.zero, new Vector2(3f, 24f), UiTheme.Foam, TextAnchor.MiddleCenter);
        }

        private static GameplayMenuInput EnsureGameplayMenuInput(GameManager gameManager)
        {
            GameObject inputObject = FindSceneObject("Gameplay Menu Input") ?? new GameObject("Gameplay Menu Input");
            GameplayMenuInput input = EnsureComponent<GameplayMenuInput>(inputObject);
            SetObject(input, "gameManager", gameManager);
            return input;
        }

        private static MainMenuController EnsureMenuController()
        {
            GameObject controllerObject = FindSceneObject("Main Menu Controller") ?? new GameObject("Main Menu Controller");
            return EnsureComponent<MainMenuController>(controllerObject);
        }

        private static void EnsureMenuCamera()
        {
            GameObject cameraObject = FindSceneObject("Menu Camera") ?? new GameObject("Menu Camera");
            Camera camera = EnsureComponent<Camera>(cameraObject);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = UiTheme.Hull;
            camera.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
            }

            StandaloneInputModule oldModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (oldModule != null)
            {
                Object.DestroyImmediate(oldModule);
            }

            InputSystemUIInputModule inputModule = EnsureComponent<InputSystemUIInputModule>(eventSystem.gameObject);
            InputActionAsset projectActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ProjectInputActionsPath);
            if (projectActions != null)
            {
                inputModule.actionsAsset = projectActions;
            }
        }

        private static void EnsureUiAudio(GameObject root)
        {
            foreach (Selectable selectable in root.GetComponentsInChildren<Selectable>(true))
            {
                EnsureComponent<WwiseUIButtonAudio>(selectable.gameObject);
                EditorUtility.SetDirty(selectable.gameObject);
            }
        }

        private static void ReplaceListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null || action == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            UnityEventTools.AddPersistentListener(button.onClick, action);
            EditorUtility.SetDirty(button);
        }

        private static void ConfigureRect(RectTransform rect, TextAnchor anchor, Vector2 anchoredPosition, Vector2 size)
        {
            Vector2 anchorPosition = AnchorToVector(anchor);
            rect.anchorMin = anchorPosition;
            rect.anchorMax = anchorPosition;
            rect.pivot = anchorPosition;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        private static Vector2 AnchorToVector(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft:
                    return new Vector2(0f, 1f);
                case TextAnchor.UpperCenter:
                    return new Vector2(0.5f, 1f);
                case TextAnchor.UpperRight:
                    return new Vector2(1f, 1f);
                case TextAnchor.MiddleLeft:
                    return new Vector2(0f, 0.5f);
                case TextAnchor.MiddleRight:
                    return new Vector2(1f, 0.5f);
                case TextAnchor.LowerLeft:
                    return Vector2.zero;
                case TextAnchor.LowerCenter:
                    return new Vector2(0.5f, 0f);
                case TextAnchor.LowerRight:
                    return new Vector2(1f, 0f);
                default:
                    return new Vector2(0.5f, 0.5f);
            }
        }

        private static Button FindButton(Transform root, string name)
        {
            Transform child = FindDeep(root, name);
            return child != null ? child.GetComponent<Button>() : null;
        }

        private static Text FindText(Transform root, string path)
        {
            Transform child = FindDeep(root, path);
            return child != null ? child.GetComponent<Text>() : null;
        }

        private static Image FindImage(Transform root, string path)
        {
            Transform child = FindDeep(root, path);
            return child != null ? child.GetComponent<Image>() : null;
        }

        private static GameObject Child(Transform root, string path)
        {
            Transform child = FindDeep(root, path);
            return child != null ? child.gameObject : null;
        }

        private static Transform FindDeep(Transform root, string path)
        {
            if (root == null || string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            Transform direct = root.Find(path);
            if (direct != null)
            {
                return direct;
            }

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == path)
                {
                    return child;
                }
            }

            return null;
        }

        private static GameObject FindSceneObject(string objectName)
        {
            foreach (Transform transform in Object.FindObjectsByType<Transform>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (transform.name == objectName && transform.gameObject.scene.IsValid())
                {
                    return transform.gameObject;
                }
            }

            return null;
        }

        private static T EnsureComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private static Font GetUiFont()
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "UI");
            EnsureFolder("Assets/UI", "Prefabs");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootstrapScenePath, true),
                new EditorBuildSettingsScene(MenuScenePath, true),
                new EditorBuildSettingsScene(MainScenePath, true),
                new EditorBuildSettingsScene(ShopScenePath, true)
            };
        }

        private static void SetObject(Object target, string propertyName, Object value)
        {
            if (target == null)
            {
                return;
            }

            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetObjectList(Object target, string propertyName, Object[] values)
        {
            if (target == null)
            {
                return;
            }

            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
