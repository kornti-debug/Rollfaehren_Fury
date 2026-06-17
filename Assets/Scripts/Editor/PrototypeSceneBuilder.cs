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
    public static class PrototypeSceneBuilder
    {
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string MenuScenePath = "Assets/Scenes/Menu.unity";
        private const string MainScenePath = "Assets/Scenes/Main.unity";
        private const string EnemyPrefabPath = "Assets/Prefabs/PrototypeEnemy.prefab";
        private const string ProjectInputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string PlayerVisualPrefabPath = "Assets/Prefabs/Character/Fraunz/T-Pose.fbx";
        private const string PlayerAnimatorControllerPath = "Assets/Prefabs/Character/Fraunz/FraunzAnimationController.controller";
        private const string PlayerVisualName = "Fraunz Visual";
        private const float EnemySpawnHeight = 1f;
        private const float PlayerVisualScale = 0.85f;

        private static readonly Vector3 PlayerStartPosition = new Vector3(0f, 7.37f, -1.8f);
        private static readonly Vector3 FerryAimPointPosition = new Vector3(0f, 4.97f, 0f);
        private static readonly Vector3 FerryDamageColliderCenter = new Vector3(0f, 7.070751f, 2.7608166f);
        private static readonly Vector3 FerryDamageColliderSize = new Vector3(20f, 5.4464755f, 45.15598f);
        private static readonly Vector3 FerryWalkColliderPosition = new Vector3(0f, 10.3f, 0f);

        [MenuItem("Rollfaehren Fury/Build Prototype Scene")]
        public static void BuildPrototypeScene()
        {
            EnsureProjectFolders();

            Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);

            Material waterMaterial = EnsureMaterial("Assets/Materials/PrototypeWater.mat", new Color(0.05f, 0.35f, 0.7f, 0.75f));
            Material shoreMaterial = EnsureMaterial("Assets/Materials/PrototypeShore.mat", new Color(0.32f, 0.62f, 0.26f));
            Material enemyMaterial = EnsureMaterial("Assets/Materials/PrototypeEnemy.mat", new Color(0.85f, 0.12f, 0.18f));

            GameObject ferry = EnsureFerry();
            Health ferryHealth = EnsureComponent<Health>(ferry);
            SetFloat(ferryHealth, "maxHealth", 100f);
            FerryDamageTarget ferryTarget = EnsureFerryDamageTarget(ferry, ferryHealth);

            SimpleFPSController playerController = EnsurePlayer();
            WeaponSystem weaponSystem = EnsureWeapon(playerController);
            Transform[] spawnPoints = EnsureSpawnPoints(ferryTarget.transform);
            SimpleEnemy enemyPrefab = EnsureEnemyPrefab(enemyMaterial);

            CreatePrototypeEnvironment(waterMaterial, shoreMaterial);

            SimpleHUD hud = EnsureHud();
            GameManager gameManager = EnsureGameManager(ferryHealth, ferryTarget, playerController, weaponSystem, hud, enemyPrefab, spawnPoints);
            EnsureShopManager(gameManager);
            EnsureAudioEvents(gameManager, weaponSystem);
            EnsureGameplayMenuInputObject();
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

        [MenuItem("Rollfaehren Fury/Build Bootstrap And Menu Scenes")]
        public static void BuildBootstrapAndMenuScenes()
        {
            EnsureProjectFolders();
            CreateBootstrapScene();
            CreateMenuScene();
            EnsureGameplayMenuReturnInMainScene();
            ConfigureBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Rollfaehren Fury bootstrap/menu scenes built. Start from Bootstrap.unity or Menu.unity to test the menu flow.");
        }

        public static void BuildBootstrapAndMenuScenesFromCommandLine()
        {
            BuildBootstrapAndMenuScenes();
        }

        public static void RepairPlayerCharacterVisualFromCommandLine()
        {
            EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            RepairPlayerCharacterVisual();
        }

        [MenuItem("Rollfaehren Fury/Repair Player Character Visual")]
        public static void RepairPlayerCharacterVisual()
        {
            GameObject player = GameObject.Find("Prototype Player");
            if (player == null)
            {
                Debug.LogWarning("Prototype Player was not found in the open scene.");
                return;
            }

            HidePrimitivePlayerShell(player);
            EnsurePlayerVisual(player.transform);

            Scene scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Selection.activeGameObject = player;
            Debug.Log("Fraunz player visual repaired. Press Play to test idle/running animation.");
        }

        private static void EnsureProjectFolders()
        {
            EnsureFolder("Assets", "Scripts");
            EnsureFolder("Assets/Scripts", "Prototype");
            EnsureFolder("Assets/Scripts", "Editor");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets", "Materials");
            EnsureFolder("Assets", "UI");
            EnsureFolder("Assets", "Weapons");
            EnsureFolder("Assets", "Upgrades");
        }

        private static void CreateBootstrapScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject loaderObject = new GameObject("Bootstrap Loader");
            EnsureComponent<BootstrapLoader>(loaderObject);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        }

        private static void CreateMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cameraObject = new GameObject("Menu Camera");
            Camera camera = EnsureComponent<Camera>(cameraObject);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.07f, 0.11f);
            camera.transform.position = new Vector3(0f, 0f, -10f);

            GameObject canvasObject = new GameObject("Main Menu Canvas");
            Canvas canvas = EnsureComponent<Canvas>(canvasObject);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = EnsureComponent<CanvasScaler>(canvasObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            EnsureComponent<GraphicRaycaster>(canvasObject);

            GameObject controllerObject = new GameObject("Main Menu Controller");
            MainMenuController controller = EnsureComponent<MainMenuController>(controllerObject);

            GameObject mainPanel = CreateUiPanel(canvasObject.transform, "Main Panel", TextAnchor.MiddleCenter, Vector2.zero, new Vector2(680f, 500f));
            Image mainBackground = EnsureComponent<Image>(mainPanel);
            mainBackground.color = new Color(0.03f, 0.05f, 0.07f, 0.88f);
            CreateText(mainPanel.transform, "Title", "Rollfaehren Fury", new Vector2(0f, 165f), new Vector2(620f, 64f), 44, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            CreateText(mainPanel.transform, "Subtitle", "Protect the ferry. Survive the crossing.", new Vector2(0f, 105f), new Vector2(620f, 34f), 22, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            CreateButton(mainPanel.transform, "New Game Button", "New Game", new Vector2(0f, 34f), out Button newGameButton);
            CreateButton(mainPanel.transform, "Settings Button", "Settings", new Vector2(0f, -34f), out Button settingsButton);
            CreateButton(mainPanel.transform, "Quit Button", "Quit", new Vector2(0f, -102f), out Button quitButton);

            GameObject settingsPanel = CreateUiPanel(canvasObject.transform, "Settings Panel", TextAnchor.MiddleCenter, Vector2.zero, new Vector2(680f, 420f));
            Image settingsBackground = EnsureComponent<Image>(settingsPanel);
            settingsBackground.color = new Color(0.03f, 0.05f, 0.07f, 0.9f);
            CreateText(settingsPanel.transform, "Settings Title", "Settings", new Vector2(0f, 122f), new Vector2(620f, 48f), 36, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            CreateText(settingsPanel.transform, "Settings Text", "Settings placeholder for volume, mouse sensitivity, and graphics options.", new Vector2(0f, 42f), new Vector2(560f, 84f), 20, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            CreateButton(settingsPanel.transform, "Settings Back Button", "Back", new Vector2(0f, -102f), out Button backButton);
            settingsPanel.SetActive(false);

            UnityEventTools.AddPersistentListener(newGameButton.onClick, controller.NewGame);
            UnityEventTools.AddPersistentListener(settingsButton.onClick, controller.ShowSettings);
            UnityEventTools.AddPersistentListener(quitButton.onClick, controller.QuitGame);
            UnityEventTools.AddPersistentListener(backButton.onClick, controller.ShowMain);

            SetObject(controller, "mainPanel", mainPanel);
            SetObject(controller, "settingsPanel", settingsPanel);
            SetObject(controller, "firstSelectedButton", newGameButton.gameObject);
            SetObject(controller, "settingsFirstSelectedButton", backButton.gameObject);

            EnsureEventSystem();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MenuScenePath);
        }

        private static void EnsureGameplayMenuReturnInMainScene()
        {
            Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            EnsureGameplayMenuInputObject();
            EnsureEventSystem();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureGameplayMenuInputObject()
        {
            GameObject inputObject = GameObject.Find("Gameplay Menu Input");
            if (inputObject == null)
            {
                inputObject = new GameObject("Gameplay Menu Input");
            }

            EnsureComponent<GameplayMenuInput>(inputObject);
        }

        private static GameObject EnsureFerry()
        {
            GameObject ferry = GameObject.Find("Ferry");
            if (ferry == null)
            {
                ferry = new GameObject("Ferry");
                ferry.transform.position = Vector3.zero;
            }

            GameObject deck = FindChild(ferry.transform, "Prototype Ferry Deck");
            if (deck != null)
            {
                Object.DestroyImmediate(deck);
            }

            EnsureInvisibleWalkCollider(ferry);

            return ferry;
        }

        private static void EnsureInvisibleWalkCollider(GameObject ferry)
        {
            GameObject walkColliderObject = FindChild(ferry.transform, "Ferry Walk Collider");
            if (walkColliderObject == null)
            {
                walkColliderObject = new GameObject("Ferry Walk Collider");
                walkColliderObject.transform.SetParent(ferry.transform, false);
            }

            walkColliderObject.transform.localPosition = FerryWalkColliderPosition;
            walkColliderObject.transform.localRotation = Quaternion.identity;
            walkColliderObject.transform.localScale = Vector3.one;

            BoxCollider walkCollider = EnsureComponent<BoxCollider>(walkColliderObject);
            walkCollider.isTrigger = false;
            walkCollider.center = new Vector3(0f, 0.1f, 0f);
            walkCollider.size = new Vector3(20f, 0.35f, 45f);
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

            aimPoint.localPosition = FerryAimPointPosition;

            BoxCollider collider = EnsureComponent<BoxCollider>(ferry);
            collider.isTrigger = true;
            collider.center = FerryDamageColliderCenter;
            collider.size = FerryDamageColliderSize;

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

            player.transform.position = PlayerStartPosition;
            player.transform.rotation = Quaternion.identity;

            HidePrimitivePlayerShell(player);

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

            RemoveStrayCameras(player.transform, camera);

            SimpleFPSController controller = EnsureComponent<SimpleFPSController>(player);
            SetObject(controller, "cameraRoot", cameraRoot);
            SetFloat(controller, "pitchClamp", 82f);
            EnsurePlayerVisual(player.transform);
            return controller;
        }

        private static void RemoveStrayCameras(Transform playerRoot, Camera playerCamera)
        {
            foreach (Camera sceneCamera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                if (sceneCamera == null || sceneCamera == playerCamera)
                {
                    continue;
                }

                if (!sceneCamera.transform.IsChildOf(playerRoot))
                {
                    Debug.Log($"Removing stray scene camera '{sceneCamera.name}' so only the player camera renders.", sceneCamera);
                    Object.DestroyImmediate(sceneCamera.gameObject);
                }
            }
        }

        private static void HidePrimitivePlayerShell(GameObject player)
        {
            MeshRenderer renderer = player.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            CapsuleCollider collider = player.GetComponent<CapsuleCollider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        private static void EnsurePlayerVisual(Transform player)
        {
            GameObject visual = FindChild(player, PlayerVisualName);
            if (visual == null)
            {
                GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerVisualPrefabPath);
                if (visualPrefab == null)
                {
                    Debug.LogWarning($"Player visual prefab was not found at '{PlayerVisualPrefabPath}'.");
                    return;
                }

                visual = PrefabUtility.InstantiatePrefab(visualPrefab, player) as GameObject;
                if (visual == null)
                {
                    Debug.LogWarning($"Could not instantiate player visual prefab '{PlayerVisualPrefabPath}'.");
                    return;
                }

                visual.name = PlayerVisualName;
            }

            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one * PlayerVisualScale;

            foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
            }

            foreach (Rigidbody rigidbody in visual.GetComponentsInChildren<Rigidbody>(true))
            {
                Object.DestroyImmediate(rigidbody);
            }

            Animator animator = visual.GetComponent<Animator>();
            if (animator == null)
            {
                animator = visual.GetComponentInChildren<Animator>(true);
            }

            if (animator == null)
            {
                animator = visual.AddComponent<Animator>();
            }

            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PlayerAnimatorControllerPath);
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
            }
            else
            {
                Debug.LogWarning($"Player animator controller was not found at '{PlayerAnimatorControllerPath}'.");
            }

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            EditorUtility.SetDirty(visual);
            EditorUtility.SetDirty(animator);
        }

        private static WeaponSystem EnsureWeapon(SimpleFPSController playerController)
        {
            Camera camera = playerController.GetComponentInChildren<Camera>();

            WeaponDefinition pistol = EnsureWeaponDefinition(
                "Assets/Weapons/Pistol.asset", "Pistol", WeaponFireMode.Hitscan,
                25f, 250f, 0.2f, 0.45f, 1, 0f);
            WeaponDefinition shotgun = EnsureWeaponDefinition(
                "Assets/Weapons/Shotgun.asset", "Shotgun", WeaponFireMode.Spread,
                11f, 55f, 0.75f, 0f, 8, 12f);
            WeaponDefinition harpoon = EnsureWeaponDefinition(
                "Assets/Weapons/Harpoon.asset", "Harpoon", WeaponFireMode.Projectile,
                120f, 300f, 1.4f, 0f, 1, 0f,
                45f, 18f, 4f);
            WeaponDefinition flamethrower = EnsureWeaponDefinition(
                "Assets/Weapons/Flamethrower.asset", "Flamethrower", WeaponFireMode.Spread,
                5f, 11f, 0.04f, 0f, 8, 14f);

            GameObject weaponsParent = FindChild(camera.transform, "Weapons");
            if (weaponsParent != null)
            {
                Object.DestroyImmediate(weaponsParent);
            }

            weaponsParent = new GameObject("Weapons");
            weaponsParent.transform.SetParent(camera.transform, false);

            Weapon pistolWeapon = CreateWeaponObject(weaponsParent.transform, "Weapon - Pistol", pistol);
            Weapon shotgunWeapon = CreateWeaponObject(weaponsParent.transform, "Weapon - Shotgun", shotgun);
            Weapon harpoonWeapon = CreateWeaponObject(weaponsParent.transform, "Weapon - Harpoon", harpoon);
            Weapon flamethrowerWeapon = CreateWeaponObject(weaponsParent.transform, "Weapon - Flamethrower", flamethrower);

            WeaponTracer tracer = EnsureWeaponTracer(camera.transform);
            SetObject(pistolWeapon, "tracer", tracer);
            SetObject(shotgunWeapon, "tracer", tracer);
            SetObject(harpoonWeapon, "tracer", tracer);
            SetObject(flamethrowerWeapon, "tracer", tracer);

            WeaponSystem weaponSystem = EnsureComponent<WeaponSystem>(camera.gameObject);
            SetObject(weaponSystem, "fireCamera", camera);
            SetObject(weaponSystem, "ignoredRoot", playerController.transform);
            SetObjectList(weaponSystem, "weapons", new Object[] { pistolWeapon, shotgunWeapon, harpoonWeapon, flamethrowerWeapon });
            SetInt(weaponSystem, "startWeaponIndex", 0);
            return weaponSystem;
        }

        private static Weapon CreateWeaponObject(Transform parent, string objectName, WeaponDefinition definition)
        {
            GameObject weaponObject = new GameObject(objectName);
            weaponObject.transform.SetParent(parent, false);
            Weapon weapon = EnsureComponent<Weapon>(weaponObject);
            SetObject(weapon, "definition", definition);
            return weapon;
        }

        private static WeaponTracer EnsureWeaponTracer(Transform cameraTransform)
        {
            GameObject tracerObject = FindChild(cameraTransform, "Weapon Tracer");
            if (tracerObject == null)
            {
                tracerObject = new GameObject("Weapon Tracer");
                tracerObject.transform.SetParent(cameraTransform, false);
            }

            WeaponTracer weaponTracer = EnsureComponent<WeaponTracer>(tracerObject);
            SetInt(weaponTracer, "poolSize", 32);
            SetFloat(weaponTracer, "width", 0.05f);
            SetFloat(weaponTracer, "duration", 0.07f);
            return weaponTracer;
        }

        private static WeaponDefinition EnsureWeaponDefinition(
            string path, string displayName, WeaponFireMode fireMode,
            float damage, float range, float fireCooldown, float aimAssistRadius, int pelletsPerShot, float spreadAngle,
            float projectileSpeed = 40f, float projectileGravity = 18f, float projectileLifetime = 4f)
        {
            WeaponDefinition definition = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<WeaponDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            SetString(definition, "displayName", displayName);
            SetEnum(definition, "fireMode", (int)fireMode);
            SetFloat(definition, "damage", damage);
            SetFloat(definition, "range", range);
            SetFloat(definition, "fireCooldown", fireCooldown);
            SetFloat(definition, "aimAssistRadius", aimAssistRadius);
            SetInt(definition, "pelletsPerShot", pelletsPerShot);
            SetFloat(definition, "spreadAngle", spreadAngle);
            SetFloat(definition, "projectileSpeed", projectileSpeed);
            SetFloat(definition, "projectileGravity", projectileGravity);
            SetFloat(definition, "projectileLifetime", projectileLifetime);
            return definition;
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
                new Vector3(-58f, 0f, -34f),
                new Vector3(58f, 0f, -34f),
                new Vector3(-65f, 0f, 0f),
                new Vector3(65f, 0f, 0f),
                new Vector3(-58f, 0f, 34f),
                new Vector3(58f, 0f, 34f)
            };

            Transform[] points = new Transform[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject point = new GameObject($"Enemy Spawn {i + 1}");
                point.transform.SetParent(parent.transform, false);
                point.transform.position = new Vector3(
                    center.position.x + positions[i].x,
                    EnemySpawnHeight,
                    center.position.z + positions[i].z);
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
            WeaponSystem weaponSystem,
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
            SetObject(gameManager, "weaponSystem", weaponSystem);
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
            SetBool(spawner, "useFixedSpawnHeight", true);
            SetFloat(spawner, "spawnHeight", EnemySpawnHeight);

            return gameManager;
        }

        private static void EnsureAudioEvents(GameManager gameManager, WeaponSystem weaponSystem)
        {
            PrototypeAudioEvents audioEvents = EnsureComponent<PrototypeAudioEvents>(gameManager.gameObject);
            SetObject(audioEvents, "gameManager", gameManager);
            SetObject(audioEvents, "weaponSystem", weaponSystem);
            SetBool(audioEvents, "postEvents", false);
        }

        private static ShopManager EnsureShopManager(GameManager gameManager)
        {
            UpgradeDefinition[] catalog =
            {
                EnsureUpgrade<WeaponDamageUpgrade>("Assets/Upgrades/WeaponDamage.asset", "Damage +10", "More weapon damage.", 50, true,
                    upgrade => SetFloat(upgrade, "amount", 10f)),
                EnsureUpgrade<FireRateUpgrade>("Assets/Upgrades/FireRate.asset", "Fire Rate +18%", "Faster fire rate.", 45, true,
                    upgrade => SetFloat(upgrade, "cooldownMultiplier", 0.82f)),
                EnsureUpgrade<FerryHealthUpgrade>("Assets/Upgrades/FerryHealth.asset", "Repair + Max HP", "Heal and raise ferry max health.", 60, true,
                    upgrade => SetFloat(upgrade, "amount", 25f)),
                EnsureUpgrade<RicochetUpgrade>("Assets/Upgrades/Ricochet.asset", "Querschlaeger (Master)", "Shots ricochet to the nearest enemy.", 150, false,
                    upgrade => SetInt(upgrade, "bounces", 1)),
            };

            ShopManager shopManager = EnsureComponent<ShopManager>(gameManager.gameObject);
            SetObject(shopManager, "gameManager", gameManager);
            SetObjectList(shopManager, "catalog", catalog);

            Button[] shopButtons = new Button[catalog.Length];
            for (int i = 0; i < shopButtons.Length; i++)
            {
                Button button = FindSceneButton($"Shop Upgrade Button {i}");
                shopButtons[i] = button;
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    UnityEventTools.AddIntPersistentListener(button.onClick, shopManager.Buy, i);
                    EditorUtility.SetDirty(button);
                }
            }

            SetObjectList(shopManager, "buttons", shopButtons);
            SetObject(gameManager, "shopManager", shopManager);
            return shopManager;
        }

        private static T EnsureUpgrade<T>(string path, string displayName, string description, int cost, bool repeatable, System.Action<T> configure)
            where T : UpgradeDefinition
        {
            T upgrade = AssetDatabase.LoadAssetAtPath<T>(path);
            if (upgrade == null)
            {
                upgrade = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(upgrade, path);
            }

            SetString(upgrade, "displayName", displayName);
            SetString(upgrade, "description", description);
            SetInt(upgrade, "cost", cost);
            SetBool(upgrade, "repeatable", repeatable);
            configure?.Invoke(upgrade);
            return upgrade;
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
            crosshairRect.pivot = new Vector2(0.5f, 0.5f);
            crosshairRect.anchoredPosition = Vector2.zero;

            GameObject shopPanel = CreateUiPanel(canvasObject.transform, "Shop Panel", TextAnchor.MiddleCenter, Vector2.zero, new Vector2(620f, 430f));
            Image shopBackground = EnsureComponent<Image>(shopPanel);
            shopBackground.color = new Color(0.04f, 0.06f, 0.08f, 0.88f);
            Text shopTitle = CreateText(shopPanel.transform, "Shop Title", "Round survived", new Vector2(0f, 160f), new Vector2(560f, 40f), 30, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            Text shopMoney = CreateText(shopPanel.transform, "Shop Money", "Money: $0", new Vector2(0f, 112f), new Vector2(560f, 32f), 22, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            float[] shopButtonY = { 78f, 26f, -26f, -78f };
            for (int i = 0; i < shopButtonY.Length; i++)
            {
                CreateButton(shopPanel.transform, $"Shop Upgrade Button {i}", "Upgrade", new Vector2(0f, shopButtonY[i]), out _);
            }
            CreateButton(shopPanel.transform, "Next Round Button", "Next Round", new Vector2(0f, -150f), out _);
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
            SetObject(hud, "gameOverPanel", gameOverPanel);
            SetObject(hud, "gameOverText", gameOverText);

            return hud;
        }

        private static void ConfigureHudButtons(GameManager gameManager)
        {
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

            InputSystemUIInputModule inputModule = EnsureComponent<InputSystemUIInputModule>(eventSystem.gameObject);
            InputActionAsset projectActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ProjectInputActionsPath);
            if (projectActions != null && inputModule.actionsAsset != projectActions)
            {
                if (inputModule.actionsAsset == null)
                {
                    inputModule.AssignDefaultActions();
                }

                inputModule.actionsAsset = projectActions;
                EditorUtility.SetDirty(inputModule);
            }
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
                new EditorBuildSettingsScene(BootstrapScenePath, true),
                new EditorBuildSettingsScene(MenuScenePath, true),
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

        private static void SetObjectList(Object target, string propertyName, Object[] values)
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

        private static void SetString(Object target, string propertyName, string value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.stringValue = value;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(target);
            }
        }

        private static void SetEnum(Object target, string propertyName, int value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.enumValueIndex = value;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(target);
            }
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
