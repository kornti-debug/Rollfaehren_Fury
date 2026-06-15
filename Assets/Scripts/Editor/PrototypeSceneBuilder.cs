using RollfaehrenFury.Prototype;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RollfaehrenFury.Editor
{
    public static class PrototypeSceneBuilder
    {
        private const string MainScenePath = "Assets/Scenes/Main.unity";
        private const string EnemyPrefabPath = "Assets/Prefabs/PrototypeEnemy.prefab";

        [MenuItem("Rollfaehren Fury/Build Prototype Scene")]
        public static void BuildPrototypeScene()
        {
            EnsureProjectFolders();

            Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);

            Material deckMaterial = EnsureMaterial("Assets/Materials/PrototypeDeck.mat", new Color(0.55f, 0.36f, 0.2f));
            Material waterMaterial = EnsureMaterial("Assets/Materials/PrototypeWater.mat", new Color(0.05f, 0.35f, 0.7f, 0.75f));
            Material shoreMaterial = EnsureMaterial("Assets/Materials/PrototypeShore.mat", new Color(0.32f, 0.62f, 0.26f));
            Material enemyMaterial = EnsureMaterial("Assets/Materials/PrototypeEnemy.mat", new Color(0.85f, 0.12f, 0.18f));

            GameObject ferry = EnsureFerry(deckMaterial);
            Health ferryHealth = EnsureComponent<Health>(ferry);
            SetFloat(ferryHealth, "maxHealth", 100f);
            FerryDamageTarget ferryTarget = EnsureFerryDamageTarget(ferry, ferryHealth);

            SimpleFPSController playerController = EnsurePlayer();
            HitscanWeapon weapon = EnsureWeapon(playerController);
            Transform[] spawnPoints = EnsureSpawnPoints(ferryTarget.transform);
            SimpleEnemy enemyPrefab = EnsureEnemyPrefab(enemyMaterial);

            CreatePrototypeEnvironment(waterMaterial, shoreMaterial);

            SimpleHUD hud = EnsureHud();
            GameManager gameManager = EnsureGameManager(ferryHealth, ferryTarget, playerController, weapon, hud, enemyPrefab, spawnPoints);
            EnsureAudioEvents(gameManager, weapon);
            EnsureEventSystem();
            ConfigureHudButtons(gameManager);
            ConfigureBuildSettings();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = gameManager.gameObject;
            Debug.Log("Rollfaehren Fury prototype scene built. Press Play in Main.unity to test the MVP loop.");
        }

        public static void BuildPrototypeSceneFromCommandLine()
        {
            BuildPrototypeScene();
        }

        private static void EnsureProjectFolders()
        {
            EnsureFolder("Assets", "Scripts");
            EnsureFolder("Assets/Scripts", "Prototype");
            EnsureFolder("Assets/Scripts", "Editor");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets", "Materials");
            EnsureFolder("Assets", "UI");
        }

        private static GameObject EnsureFerry(Material deckMaterial)
        {
            GameObject ferry = GameObject.Find("Ferry");
            if (ferry == null)
            {
                ferry = new GameObject("Ferry");
                ferry.transform.position = Vector3.zero;
            }

            GameObject deck = FindChild(ferry.transform, "Prototype Ferry Deck");
            if (deck == null)
            {
                deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
                deck.name = "Prototype Ferry Deck";
                deck.transform.SetParent(ferry.transform, false);
            }

            deck.transform.localPosition = new Vector3(0f, 0f, 0f);
            deck.transform.localScale = new Vector3(10f, 0.35f, 6f);
            Renderer renderer = deck.GetComponent<Renderer>();
            renderer.sharedMaterial = deckMaterial;
            deck.GetComponent<Collider>().isTrigger = false;

            return ferry;
        }

        private static FerryDamageTarget EnsureFerryDamageTarget(GameObject ferry, Health ferryHealth)
        {
            GameObject target = FindChild(ferry.transform, "Ferry Damage Target");
            if (target != null)
            {
                Object.DestroyImmediate(target);
            }

            Transform aimPoint = ferry.transform.Find("Ferry Aim Point");
            if (aimPoint == null)
            {
                aimPoint = new GameObject("Ferry Aim Point").transform;
                aimPoint.SetParent(ferry.transform, false);
            }

            aimPoint.localPosition = new Vector3(0f, 3f, 0f);

            BoxCollider collider = EnsureComponent<BoxCollider>(ferry);
            collider.isTrigger = true;
            if (collider.size.x < 12f || collider.size.z < 20f)
            {
                collider.center = new Vector3(0f, 3f, 0f);
                collider.size = new Vector3(22f, 7f, 48f);
            }

            FerryDamageTarget damageTarget = EnsureComponent<FerryDamageTarget>(ferry);
            SetObject(damageTarget, "ferryHealth", ferryHealth);
            SetObject(damageTarget, "aimPoint", aimPoint);
            return damageTarget;
        }

        private static SimpleFPSController EnsurePlayer()
        {
            GameObject player = GameObject.Find("Prototype Player");
            if (player == null)
            {
                player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "Prototype Player";
            }

            player.transform.position = new Vector3(0f, 1.1f, -1.8f);
            player.transform.rotation = Quaternion.identity;

            CharacterController characterController = EnsureComponent<CharacterController>(player);
            characterController.height = 1.8f;
            characterController.radius = 0.35f;
            characterController.center = new Vector3(0f, 0.9f, 0f);

            Transform cameraRoot = player.transform.Find("Camera Root");
            if (cameraRoot == null)
            {
                cameraRoot = new GameObject("Camera Root").transform;
                cameraRoot.SetParent(player.transform, false);
            }

            cameraRoot.localPosition = new Vector3(0f, 1.55f, 0f);
            cameraRoot.localRotation = Quaternion.identity;

            Camera camera = EnsureComponent<Camera>(cameraRoot.gameObject);
            camera.tag = "MainCamera";
            camera.nearClipPlane = 0.05f;
            camera.fieldOfView = 72f;
            EnsureComponent<AudioListener>(cameraRoot.gameObject);

            SimpleFPSController controller = EnsureComponent<SimpleFPSController>(player);
            SetObject(controller, "cameraRoot", cameraRoot);
            return controller;
        }

        private static HitscanWeapon EnsureWeapon(SimpleFPSController playerController)
        {
            Camera camera = playerController.GetComponentInChildren<Camera>();
            HitscanWeapon weapon = EnsureComponent<HitscanWeapon>(camera.gameObject);
            SetObject(weapon, "fireCamera", camera);
            SetFloat(weapon, "damage", 25f);
            SetFloat(weapon, "range", 250f);
            SetFloat(weapon, "aimAssistRadius", 0.45f);
            SetFloat(weapon, "fireCooldown", 0.2f);
            SetObject(weapon, "ignoredRoot", playerController.transform);
            return weapon;
        }

        private static Transform[] EnsureSpawnPoints(Transform center)
        {
            GameObject parent = GameObject.Find("Enemy Spawn Points");
            if (parent != null)
            {
                Object.DestroyImmediate(parent);
            }

            parent = new GameObject("Enemy Spawn Points");
            Vector3[] positions =
            {
                new Vector3(-58f, 3f, -34f),
                new Vector3(58f, 3f, -34f),
                new Vector3(-65f, 4f, 0f),
                new Vector3(65f, 4f, 0f),
                new Vector3(-58f, 3f, 34f),
                new Vector3(58f, 3f, 34f)
            };

            Transform[] points = new Transform[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject point = new GameObject($"Enemy Spawn {i + 1}");
                point.transform.SetParent(parent.transform, false);
                point.transform.position = center.position + positions[i];
                points[i] = point.transform;
            }

            return points;
        }

        private static SimpleEnemy EnsureEnemyPrefab(Material enemyMaterial)
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
            if (existingPrefab != null)
            {
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(EnemyPrefabPath);
                ConfigureEnemyObject(prefabRoot, enemyMaterial);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, EnemyPrefabPath);
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                return AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath).GetComponent<SimpleEnemy>();
            }

            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            enemy.name = "PrototypeEnemy";
            ConfigureEnemyObject(enemy, enemyMaterial);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, EnemyPrefabPath);
            Object.DestroyImmediate(enemy);
            return prefab.GetComponent<SimpleEnemy>();
        }

        private static void ConfigureEnemyObject(GameObject enemy, Material enemyMaterial)
        {
            enemy.transform.localScale = new Vector3(1.35f, 1.35f, 1.35f);

            Renderer renderer = enemy.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = enemyMaterial;
            }

            SphereCollider collider = EnsureComponent<SphereCollider>(enemy);
            collider.isTrigger = true;
            collider.radius = 0.8f;

            Rigidbody rigidbody = EnsureComponent<Rigidbody>(enemy);
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;

            Health health = EnsureComponent<Health>(enemy);
            SetFloat(health, "maxHealth", 50f);
            EnsureComponent<SimpleEnemy>(enemy);
        }

        private static void CreatePrototypeEnvironment(Material waterMaterial, Material shoreMaterial)
        {
            GameObject parent = GameObject.Find("Prototype Environment");
            if (parent == null)
            {
                parent = new GameObject("Prototype Environment");
            }

            GameObject river = EnsureEnvironmentBlock(parent.transform, "River Placeholder");
            river.transform.position = new Vector3(0f, -0.25f, 0f);
            river.transform.localScale = new Vector3(358.71f, 0.1f, 600f);
            river.GetComponent<Renderer>().sharedMaterial = waterMaterial;

            GameObject shoreA = EnsureEnvironmentBlock(parent.transform, "Shore A Placeholder");
            shoreA.transform.position = new Vector3(-191f, -0.15f, 0f);
            shoreA.transform.localScale = new Vector3(22f, 0.25f, 600f);
            shoreA.GetComponent<Renderer>().sharedMaterial = shoreMaterial;

            GameObject shoreB = EnsureEnvironmentBlock(parent.transform, "Shore B Placeholder");
            shoreB.transform.position = new Vector3(197.8f, -0.15f, 0f);
            shoreB.transform.localScale = new Vector3(22f, 0.25f, 600f);
            shoreB.GetComponent<Renderer>().sharedMaterial = shoreMaterial;
        }

        private static GameObject EnsureEnvironmentBlock(Transform parent, string blockName)
        {
            GameObject block = FindChild(parent, blockName);
            if (block != null)
            {
                return block;
            }

            block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = blockName;
            block.transform.SetParent(parent, false);
            return block;
        }

        private static GameManager EnsureGameManager(
            Health ferryHealth,
            FerryDamageTarget ferryTarget,
            SimpleFPSController playerController,
            HitscanWeapon weapon,
            SimpleHUD hud,
            SimpleEnemy enemyPrefab,
            Transform[] spawnPoints)
        {
            GameObject managerObject = GameObject.Find("Prototype Game Manager");
            if (managerObject == null)
            {
                managerObject = new GameObject("Prototype Game Manager");
            }

            GameManager gameManager = EnsureComponent<GameManager>(managerObject);
            EnemySpawner spawner = EnsureComponent<EnemySpawner>(managerObject);

            SetObject(gameManager, "ferryHealth", ferryHealth);
            SetObject(gameManager, "enemySpawner", spawner);
            SetObject(gameManager, "playerController", playerController);
            SetObject(gameManager, "playerWeapon", weapon);
            SetObject(gameManager, "hud", hud);
            SetBool(gameManager, "startOnPlay", true);
            SetFloat(gameManager, "crossingDuration", 45f);

            SetObject(spawner, "enemyPrefab", enemyPrefab);
            SetObject(spawner, "ferryTarget", ferryTarget);
            SetObjectArray(spawner, "spawnPoints", spawnPoints);
            SetFloat(spawner, "spawnInterval", 1.6f);
            SetInt(spawner, "baseEnemiesPerRound", 8);
            SetInt(spawner, "extraEnemiesPerRound", 5);
            SetInt(spawner, "maxAliveEnemies", 14);
            SetFloat(spawner, "healthScalePerRound", 0.35f);
            SetFloat(spawner, "speedScalePerRound", 0.15f);
            SetFloat(spawner, "spawnDelayReductionPerRound", 0.18f);
            SetFloat(spawner, "fallbackSpawnRadius", 65f);

            return gameManager;
        }

        private static void EnsureAudioEvents(GameManager gameManager, HitscanWeapon weapon)
        {
            PrototypeAudioEvents audioEvents = EnsureComponent<PrototypeAudioEvents>(gameManager.gameObject);
            SetObject(audioEvents, "gameManager", gameManager);
            SetObject(audioEvents, "weapon", weapon);
            SetBool(audioEvents, "postEvents", false);
        }

        private static SimpleHUD EnsureHud()
        {
            GameObject canvasObject = GameObject.Find("Rollfaehren Fury Prototype HUD");
            if (canvasObject != null)
            {
                Object.DestroyImmediate(canvasObject);
            }

            canvasObject = new GameObject("Rollfaehren Fury Prototype HUD");
            Canvas canvas = EnsureComponent<Canvas>(canvasObject);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = EnsureComponent<CanvasScaler>(canvasObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            EnsureComponent<GraphicRaycaster>(canvasObject);

            SimpleHUD hud = EnsureComponent<SimpleHUD>(canvasObject);

            GameObject gameplayPanel = CreateUiPanel(canvasObject.transform, "Gameplay Panel", TextAnchor.UpperLeft, new Vector2(24f, -24f), new Vector2(520f, 300f));
            Text roundText = CreateText(gameplayPanel.transform, "Round Text", "Round 1", new Vector2(0f, 0f), new Vector2(420f, 34f), 26, TextAnchor.MiddleLeft);
            Text healthText = CreateText(gameplayPanel.transform, "Ferry Health Text", "Ferry: 100 / 100", new Vector2(0f, -42f), new Vector2(420f, 30f), 22, TextAnchor.MiddleLeft);
            Image healthFill = CreateBar(gameplayPanel.transform, "Ferry Health Bar", new Vector2(0f, -78f), new Color(0.1f, 0.85f, 0.45f));
            Text moneyText = CreateText(gameplayPanel.transform, "Money Text", "Money: $0", new Vector2(0f, -118f), new Vector2(420f, 30f), 22, TextAnchor.MiddleLeft);
            Text crossingText = CreateText(gameplayPanel.transform, "Crossing Text", "Crossing: 0%", new Vector2(0f, -154f), new Vector2(420f, 30f), 22, TextAnchor.MiddleLeft);
            Image crossingFill = CreateBar(gameplayPanel.transform, "Crossing Bar", new Vector2(0f, -190f), new Color(0.1f, 0.55f, 0.95f));
            Text weaponStatsText = CreateText(gameplayPanel.transform, "Weapon Stats Text", "Weapon: 25 dmg | 5.0/s", new Vector2(0f, -226f), new Vector2(520f, 30f), 20, TextAnchor.MiddleLeft);
            Text messageText = CreateText(gameplayPanel.transform, "Message Text", string.Empty, new Vector2(0f, -258f), new Vector2(520f, 30f), 20, TextAnchor.MiddleLeft);

            Text crosshair = CreateText(canvasObject.transform, "Crosshair", "+", Vector2.zero, new Vector2(48f, 48f), 30, TextAnchor.MiddleCenter);
            RectTransform crosshairRect = crosshair.rectTransform;
            crosshairRect.anchorMin = new Vector2(0.5f, 0.5f);
            crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
            crosshairRect.anchoredPosition = Vector2.zero;

            GameObject shopPanel = CreateUiPanel(canvasObject.transform, "Shop Panel", TextAnchor.MiddleCenter, Vector2.zero, new Vector2(620f, 430f));
            Image shopBackground = EnsureComponent<Image>(shopPanel);
            shopBackground.color = new Color(0.04f, 0.06f, 0.08f, 0.88f);
            Text shopTitle = CreateText(shopPanel.transform, "Shop Title", "Round survived", new Vector2(0f, 160f), new Vector2(560f, 40f), 30, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            Text shopMoney = CreateText(shopPanel.transform, "Shop Money", "Money: $0", new Vector2(0f, 112f), new Vector2(560f, 32f), 22, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            Text damageCost = CreateButton(shopPanel.transform, "Damage Upgrade Button", "Damage Upgrade ($50)", new Vector2(0f, 48f), out Button damageButton);
            Text fireRateCost = CreateButton(shopPanel.transform, "Fire Rate Upgrade Button", "Fire Rate Upgrade ($45)", new Vector2(0f, -16f), out Button fireRateButton);
            Text ferryHealthCost = CreateButton(shopPanel.transform, "Ferry Health Upgrade Button", "Ferry Health ($60)", new Vector2(0f, -80f), out Button healthButton);
            CreateButton(shopPanel.transform, "Next Round Button", "Next Round", new Vector2(0f, -158f), out Button nextRoundButton);
            shopPanel.SetActive(false);

            GameObject gameOverPanel = CreateUiPanel(canvasObject.transform, "Game Over Panel", TextAnchor.MiddleCenter, Vector2.zero, new Vector2(620f, 330f));
            Image gameOverBackground = EnsureComponent<Image>(gameOverPanel);
            gameOverBackground.color = new Color(0.1f, 0.02f, 0.02f, 0.9f);
            Text gameOverText = CreateText(gameOverPanel.transform, "Game Over Text", "Ferry destroyed", new Vector2(0f, 58f), new Vector2(560f, 120f), 30, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            CreateButton(gameOverPanel.transform, "Restart Button", "Restart", new Vector2(0f, -92f), out Button restartButton);
            gameOverPanel.SetActive(false);

            SetObject(hud, "gameplayPanel", gameplayPanel);
            SetObject(hud, "ferryHealthText", healthText);
            SetObject(hud, "ferryHealthFill", healthFill);
            SetObject(hud, "moneyText", moneyText);
            SetObject(hud, "roundText", roundText);
            SetObject(hud, "crossingText", crossingText);
            SetObject(hud, "crossingFill", crossingFill);
            SetObject(hud, "weaponStatsText", weaponStatsText);
            SetObject(hud, "messageText", messageText);
            SetObject(hud, "shopPanel", shopPanel);
            SetObject(hud, "shopTitleText", shopTitle);
            SetObject(hud, "shopMoneyText", shopMoney);
            SetObject(hud, "weaponDamageCostText", damageCost);
            SetObject(hud, "fireRateCostText", fireRateCost);
            SetObject(hud, "ferryHealthCostText", ferryHealthCost);
            SetObject(hud, "gameOverPanel", gameOverPanel);
            SetObject(hud, "gameOverText", gameOverText);

            damageButton.name = "Damage Upgrade Button";
            fireRateButton.name = "Fire Rate Upgrade Button";
            healthButton.name = "Ferry Health Upgrade Button";
            nextRoundButton.name = "Next Round Button";
            restartButton.name = "Restart Button";

            return hud;
        }

        private static void ConfigureHudButtons(GameManager gameManager)
        {
            AddButtonListener("Damage Upgrade Button", gameManager.BuyWeaponDamageUpgrade);
            AddButtonListener("Fire Rate Upgrade Button", gameManager.BuyFireRateUpgrade);
            AddButtonListener("Ferry Health Upgrade Button", gameManager.BuyFerryHealthUpgrade);
            AddButtonListener("Next Round Button", gameManager.StartNextRound);
            AddButtonListener("Restart Button", gameManager.RestartGame);
        }

        private static void AddButtonListener(string buttonName, UnityEngine.Events.UnityAction action)
        {
            Button button = FindSceneButton(buttonName);
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            UnityEventTools.AddPersistentListener(button.onClick, action);
            EditorUtility.SetDirty(button);
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

            EnsureComponent<InputSystemUIInputModule>(eventSystem.gameObject);
        }

        private static GameObject CreateUiPanel(Transform parent, string name, TextAnchor anchor, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform rect = EnsureComponent<RectTransform>(panel);
            ConfigureRect(rect, anchor, anchoredPosition, size);
            return panel;
        }

        private static Button FindSceneButton(string buttonName)
        {
            Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();
            foreach (Button button in buttons)
            {
                if (button.name == buttonName && button.gameObject.scene.IsValid())
                {
                    return button;
                }
            }

            return null;
        }

        private static Text CreateText(Transform parent, string name, string text, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, TextAnchor anchor = TextAnchor.UpperLeft)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            Text label = EnsureComponent<Text>(textObject);
            label.text = text;
            label.font = GetBuiltInFont();
            label.fontSize = fontSize;
            label.color = Color.white;
            label.alignment = alignment;
            label.raycastTarget = false;
            ConfigureRect(label.rectTransform, anchor, anchoredPosition, size);
            return label;
        }

        private static Image CreateBar(Transform parent, string name, Vector2 anchoredPosition, Color fillColor)
        {
            GameObject backgroundObject = new GameObject(name);
            backgroundObject.transform.SetParent(parent, false);
            Image background = EnsureComponent<Image>(backgroundObject);
            background.color = new Color(0f, 0f, 0f, 0.55f);
            ConfigureRect(background.rectTransform, TextAnchor.UpperLeft, anchoredPosition, new Vector2(320f, 22f));

            GameObject fillObject = new GameObject("Fill");
            fillObject.transform.SetParent(backgroundObject.transform, false);
            Image fill = EnsureComponent<Image>(fillObject);
            fill.color = fillColor;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 1f;

            RectTransform fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            return fill;
        }

        private static Text CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, out Button button)
        {
            GameObject buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);
            Image image = EnsureComponent<Image>(buttonObject);
            image.color = new Color(0.12f, 0.45f, 0.75f, 1f);
            button = EnsureComponent<Button>(buttonObject);
            ConfigureRect(buttonObject.GetComponent<RectTransform>(), TextAnchor.MiddleCenter, anchoredPosition, new Vector2(420f, 48f));

            Text buttonText = CreateText(buttonObject.transform, "Label", label, Vector2.zero, new Vector2(400f, 42f), 20, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            buttonText.color = Color.white;
            return buttonText;
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
                case TextAnchor.MiddleCenter:
                    return new Vector2(0.5f, 0.5f);
                default:
                    return new Vector2(0.5f, 0.5f);
            }
        }

        private static Material EnsureMaterial(string path, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Font GetBuiltInFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return font;
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainScenePath, true)
            };
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static GameObject FindChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            return child != null ? child.gameObject : null;
        }

        private static T EnsureComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private static void SetObject(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"Property '{propertyName}' was not found on {target.name}.", target);
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetObjectArray(Object target, string propertyName, Transform[] values)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"Property '{propertyName}' was not found on {target.name}.", target);
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

        private static void SetFloat(Object target, string propertyName, float value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(target);
            }
        }

        private static void SetInt(Object target, string propertyName, int value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.intValue = value;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(target);
            }
        }

        private static void SetBool(Object target, string propertyName, bool value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(target);
            }
        }
    }
}
