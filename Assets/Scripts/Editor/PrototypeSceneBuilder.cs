using RollfaehrenFury.Prototype;
using UnityEditor;
using UnityEditor.Animations;
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
        private const string ShopScenePath = "Assets/Scenes/ShopInterior.unity";
        private const string EnemyPrefabPath = "Assets/Prefabs/PrototypeEnemy.prefab";
        private const string AnimatedEnemyPrefabPath = "Assets/Prefabs/CHAR_Fish.prefab";
        private const string FishAnimatorControllerPath = "Assets/Animations/CarpAnimator.controller";
        private const string FishExplosionControllerPath = "Assets/Animations/FishExplosion.controller";
        private const string FishExplosionPrefabPath = "Assets/Prefabs/FishContactExplosion.prefab";
        private const string FishExplosionAnimationPath = "Assets/Models/Fish_Explode_Anim.fbx";
        private const string PigeonEnemyPrefabPath = "Assets/Prefabs/CHAR_Pigeon.prefab";
        private const string PigeonAnimatorControllerPath = "Assets/Animations/PigeonAnimator.controller";
        private const string VendingMachinePrefabPath = "Assets/Models/VendingMachine.fbx";
        private const string ProjectInputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string PlayerVisualPrefabPath = "Assets/Prefabs/CHAR_Fraunz.prefab";
        private const string PlayerAnimatorControllerPath = "Assets/Animations/FraunzAnimator.controller";
        private const string PlayerVisualName = "Fraunz Visual";
        private const float EnemySpawnHeight = 7f;
        private const float PlayerVisualScale = 1f;

        private static readonly Vector3 FerryStartPosition = new Vector3(261.30118f, 1.85f, 483.7371f);
        private static readonly Vector3 FerryDockBPosition = new Vector3(733.9988f, 1.85f, 493.8429f);
        private static readonly Vector3 PlayerStartPosition = new Vector3(259.54834f, 11.66f, 480.7458f);
        private static readonly Quaternion FerryStartRotation = Quaternion.Euler(0f, 177.139f, 0f);
        private static readonly Quaternion PlayerStartRotation = Quaternion.Euler(0f, 87.139f, 0f);
        private static readonly Vector3 VendingMachineLocalPosition = new Vector3(1.9f, 10.4f, 6.4f);
        private static readonly Vector3 RoundConsoleLocalPosition = new Vector3(-2.8f, 11.1f, 0.8f);
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
            EnsureWwiseFootsteps(playerController);
            WeaponSystem weaponSystem = EnsureWeapon(playerController);
            Transform[] spawnPoints = EnsureSpawnPoints(ferry.transform);
            SimpleEnemy enemyPrefab = EnsureEnemyPrefab(enemyMaterial);
            EnsureFishContactAnimation();

            CreatePrototypeEnvironment(waterMaterial, shoreMaterial);

            SimpleHUD hud = EnsureHud();
            GameManager gameManager = EnsureGameManager(ferryHealth, ferryTarget, playerController, weaponSystem, hud, enemyPrefab, spawnPoints);
            EnsureShopManager(gameManager);
            EnsureVendingMachine(gameManager);
            EnsureAugmentSystem(gameManager, gameManager.GetComponent<EnemySpawner>());
            EnsureAudioEvents(gameManager, weaponSystem);
            EnsureFerryRoundFlow(ferry, ferryTarget, playerController, gameManager, gameManager.GetComponent<EnemySpawner>(), hud);
            EnsureWwiseRuntime(gameManager, ferry);
            EnsureEventSystem();
            EnsureWwiseUiAndShopAudio();
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

        [MenuItem("Rollfaehren Fury/Upgrade Ferry Round Flow Scene")]
        public static void UpgradeFerryRoundFlowScene()
        {
            Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            GameObject ferry = GameObject.Find("Ferry");
            GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
            EnemySpawner spawner = Object.FindFirstObjectByType<EnemySpawner>();
            SimpleFPSController player = Object.FindFirstObjectByType<SimpleFPSController>();
            SimpleHUD hud = Object.FindFirstObjectByType<SimpleHUD>();
            FerryDamageTarget ferryTarget = Object.FindFirstObjectByType<FerryDamageTarget>();

            if (ferry == null || gameManager == null || spawner == null || player == null || hud == null || ferryTarget == null)
            {
                Debug.LogError("Main.unity is missing a required prototype object. Run Build Prototype Scene first.");
                return;
            }

            EnsurePigeonAnimator();
            EnsureFishContactAnimation();
            EnsureWwiseFootsteps(player);
            EnsureFerryRoundFlow(ferry, ferryTarget, player, gameManager, spawner, hud);
            EnsureWwiseRuntime(gameManager, ferry);
            EnsureEventSystem();
            EnsureWwiseUiAndShopAudio();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Ferry round flow upgraded without replacing terrain or jetty props.");
        }

        public static void UpgradeFerryRoundFlowSceneFromCommandLine()
        {
            UpgradeFerryRoundFlowScene();
        }

        public static void BuildWwiseAudioIntegrationFromCommandLine()
        {
            Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
            SimpleFPSController player = Object.FindFirstObjectByType<SimpleFPSController>();
            WeaponSystem weaponSystem = Object.FindFirstObjectByType<WeaponSystem>();
            GameObject ferry = FindSceneObjectIncludingInactive("Ferry_Root")
                               ?? FindSceneObjectIncludingInactive("Ferry");
            if (gameManager == null || player == null || weaponSystem == null || ferry == null)
            {
                Debug.LogError("Main.unity is missing GameManager, player, weapons, or Ferry_Root.");
                return;
            }

            EnsureWwiseFootsteps(player);
            EnsureAudioEvents(gameManager, weaponSystem);
            EnsureWwiseRuntime(gameManager, ferry);
            EnsureWwiseUiAndShopAudio();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ShopScenePath) != null)
            {
                Scene shopScene = EditorSceneManager.OpenScene(ShopScenePath, OpenSceneMode.Single);
                EnsureWwiseUiAndShopAudio();
                EditorSceneManager.MarkSceneDirty(shopScene);
                EditorSceneManager.SaveScene(shopScene);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Wwise gameplay, UI, and shop audio repaired.");
        }

        [MenuItem("Rollfaehren Fury/Integrate Wwise Footsteps")]
        public static void IntegrateWwiseFootsteps()
        {
            Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            SimpleFPSController player = Object.FindFirstObjectByType<SimpleFPSController>();
            if (player == null)
            {
                Debug.LogError("Main.unity is missing the player controller. Run Build Prototype Scene first.");
                return;
            }

            EnsureWwiseFootsteps(player);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Wwise footsteps integrated without rebuilding gameplay or UI objects.");
        }

        public static void IntegrateWwiseFootstepsFromCommandLine()
        {
            IntegrateWwiseFootsteps();
        }

        [MenuItem("Rollfaehren Fury/Integrate Fish Contact Explosion")]
        public static void IntegrateFishContactExplosion()
        {
            EnsureFishContactAnimation();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Fish contact explosion integrated into CHAR_Fish.prefab.");
        }

        public static void IntegrateFishContactExplosionFromCommandLine()
        {
            IntegrateFishContactExplosion();
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
            GameObject player = GameObject.Find("Player") ?? GameObject.Find("Prototype Player");
            if (player == null)
            {
                Debug.LogWarning("Player was not found in the open scene.");
                return;
            }

            HidePrimitivePlayerShell(player);
            EnsurePlayerVisual(player.transform);
            RemoveStandaloneFraunzPreview(player.transform);

            Scene scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Selection.activeGameObject = player;
            Debug.Log("Static Fraunz player visual restored.");
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
            EnsureFolder("Assets", "Augments");
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
            GameObject ferry = GameObject.Find("Ferry_Root") ?? GameObject.Find("Ferry");
            if (ferry == null)
            {
                ferry = new GameObject("Ferry_Root");
            }
            else
            {
                ferry.name = "Ferry_Root";
            }

            ferry.transform.position = FerryStartPosition;
            ferry.transform.rotation = FerryStartRotation;

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
            GameObject player = GameObject.Find("Player") ?? GameObject.Find("Prototype Player");
            if (player == null)
            {
                player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "Prototype Player";
            }

            player.transform.position = PlayerStartPosition;
            player.transform.rotation = PlayerStartRotation;

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
            SetBool(controller, "animateCharacter", true);
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
            if (visual != null && !IsPrefabInstanceFromPath(visual, PlayerVisualPrefabPath))
            {
                Object.DestroyImmediate(visual);
                visual = null;
            }

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

            visual.SetActive(true);
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

            RuntimeAnimatorController playerAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PlayerAnimatorControllerPath);
            if (playerAnimatorController == null)
            {
                Debug.LogWarning($"Player animator controller was not found at '{PlayerAnimatorControllerPath}'. Fraunz will remain static until the controller is restored.");
            }

            foreach (Animator animator in visual.GetComponentsInChildren<Animator>(true))
            {
                animator.runtimeAnimatorController = playerAnimatorController;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.enabled = playerAnimatorController != null;
                EditorUtility.SetDirty(animator);
            }

            Debug.Log($"Configured player visual '{visual.name}' with animator controller '{playerAnimatorController?.name ?? "<none>"}'.", visual);
            EditorUtility.SetDirty(visual);
        }

        private static void EnsureFishContactAnimation()
        {
            AnimationClip explosionClip = LoadAnimationClip(FishExplosionAnimationPath, "Armature|Explode");
            if (explosionClip == null)
            {
                Debug.LogWarning("Fish explosion animation clip could not be loaded.");
                return;
            }

            AnimatorController explosionController =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(FishExplosionControllerPath);
            if (explosionController == null)
            {
                explosionController =
                    AnimatorController.CreateAnimatorControllerAtPath(FishExplosionControllerPath);
            }

            explosionController.parameters = System.Array.Empty<AnimatorControllerParameter>();
            AnimatorStateMachine stateMachine = explosionController.layers[0].stateMachine;
            foreach (ChildAnimatorState childState in stateMachine.states)
            {
                stateMachine.RemoveState(childState.state);
            }

            AnimatorState explosionState = stateMachine.AddState("Explode", new Vector3(220f, 100f));
            explosionState.motion = explosionClip;
            stateMachine.defaultState = explosionState;
            EditorUtility.SetDirty(explosionController);
            AssetDatabase.SaveAssets();

            GameObject explosionPrefab = BuildFishExplosionEffect(explosionController);
            RuntimeAnimatorController swimController =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(FishAnimatorControllerPath);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(AnimatedEnemyPrefabPath);
            try
            {
                Animator animator = prefabRoot.GetComponentInChildren<Animator>(true);
                SimpleEnemy enemy = prefabRoot.GetComponentInChildren<SimpleEnemy>(true);
                if (animator == null || enemy == null)
                {
                    Debug.LogWarning("CHAR_Fish.prefab is missing its Animator or SimpleEnemy component.");
                    return;
                }

                animator.runtimeAnimatorController = swimController;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                SetObject(enemy, "contactEffectPrefab", explosionPrefab);
                SetFloat(enemy, "contactEffectDuration", Mathf.Max(0.1f, explosionClip.length + 0.05f));
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, AnimatedEnemyPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static GameObject BuildFishExplosionEffect(RuntimeAnimatorController controller)
        {
            GameObject explosionModel = AssetDatabase.LoadAssetAtPath<GameObject>(FishExplosionAnimationPath);
            if (explosionModel == null)
            {
                Debug.LogWarning("Fish explosion model could not be loaded.");
                return null;
            }

            GameObject effectRoot = new GameObject("Fish Contact Explosion");
            try
            {
                GameObject visual = PrefabUtility.InstantiatePrefab(explosionModel) as GameObject;
                visual.transform.SetParent(effectRoot.transform, false);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;

                Animator animator = visual.GetComponentInChildren<Animator>(true);
                if (animator == null)
                {
                    animator = visual.AddComponent<Animator>();
                }

                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                foreach (Collider effectCollider in effectRoot.GetComponentsInChildren<Collider>(true))
                {
                    Object.DestroyImmediate(effectCollider);
                }

                foreach (Rigidbody effectBody in effectRoot.GetComponentsInChildren<Rigidbody>(true))
                {
                    Object.DestroyImmediate(effectBody);
                }

                return PrefabUtility.SaveAsPrefabAsset(effectRoot, FishExplosionPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(effectRoot);
            }
        }

        private static AnimationClip LoadAnimationClip(string assetPath, string preferredName)
        {
            AnimationClip fallback = null;
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (asset is not AnimationClip clip || clip.name.StartsWith("__preview__", System.StringComparison.Ordinal))
                {
                    continue;
                }

                if (clip.name == preferredName)
                {
                    return clip;
                }

                fallback ??= clip;
            }

            return fallback;
        }

        private static bool IsPrefabInstanceFromPath(GameObject instance, string assetPath)
        {
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(instance);
            return source != null && AssetDatabase.GetAssetPath(source) == assetPath;
        }

        private static void RemoveStandaloneFraunzPreview(Transform player)
        {
            System.Collections.Generic.List<GameObject> previews = new System.Collections.Generic.List<GameObject>();
            foreach (Transform transform in Object.FindObjectsByType<Transform>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (transform.parent == null
                    && transform != player
                    && transform.name == "CHAR_Fraunz")
                {
                    previews.Add(transform.gameObject);
                }
            }

            foreach (GameObject preview in previews)
            {
                Object.DestroyImmediate(preview);
            }
        }

        private static WeaponSystem EnsureWeapon(SimpleFPSController playerController)
        {
            Camera camera = playerController.GetComponentInChildren<Camera>();

            WeaponDefinition pistol = EnsureWeaponDefinition(
                "Assets/Weapons/Pistol.asset", "Pistol", WeaponFireMode.Hitscan,
                25f, 300f, 0.2f, 0.45f, 1, 0f);
            WeaponDefinition shotgun = EnsureWeaponDefinition(
                "Assets/Weapons/Shotgun.asset", "Shotgun", WeaponFireMode.Spread,
                11f, 85f, 0.75f, 0f, 8, 12f);
            WeaponDefinition harpoon = EnsureWeaponDefinition(
                "Assets/Weapons/Harpoon.asset", "Harpoon", WeaponFireMode.Projectile,
                120f, 300f, 1.4f, 0f, 1, 0f,
                45f, 18f, 4f);
            WeaponDefinition assaultRifle = EnsureWeaponDefinition(
                "Assets/Weapons/Flamethrower.asset", "Assault Rifle", WeaponFireMode.Hitscan,
                18f, 280f, 0.1f, 0.5f, 1, 1f);

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
            Weapon assaultRifleWeapon = CreateWeaponObject(weaponsParent.transform, "Weapon - Assault Rifle", assaultRifle);

            WeaponTracer tracer = EnsureWeaponTracer(camera.transform);
            SetObject(pistolWeapon, "tracer", tracer);
            SetObject(shotgunWeapon, "tracer", tracer);
            SetObject(harpoonWeapon, "tracer", tracer);
            SetObject(assaultRifleWeapon, "tracer", tracer);

            WeaponSystem weaponSystem = EnsureComponent<WeaponSystem>(camera.gameObject);
            SetObject(weaponSystem, "fireCamera", camera);
            SetObject(weaponSystem, "ignoredRoot", playerController.transform);
            SetObjectList(weaponSystem, "weapons", new Object[] { harpoonWeapon, pistolWeapon, shotgunWeapon, assaultRifleWeapon });
            SetInt(weaponSystem, "startWeaponIndex", 1);
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

        private static Transform[] EnsureSpawnPoints(Transform ferry)
        {
            Vector3[] positions =
            {
                new Vector3(-55f, 5.15f, 35f),
                new Vector3(55f, 5.15f, 35f),
                new Vector3(-35f, 5.15f, 60f),
                new Vector3(35f, 5.15f, 60f),
                new Vector3(-15f, 5.15f, 78f),
                new Vector3(15f, 5.15f, 78f)
            };

            return RebuildSpawnPointGroup("Fish Spawn Points", ferry, "Fish Spawn", positions);
        }

        private static Transform[] EnsurePigeonSpawnPoints(Transform ferry)
        {
            Vector3[] positions =
            {
                new Vector3(-70f, 22f, 30f),
                new Vector3(70f, 25f, 30f),
                new Vector3(-45f, 20f, 60f),
                new Vector3(45f, 26f, 60f),
                new Vector3(-20f, 24f, 90f),
                new Vector3(20f, 21f, 90f)
            };

            return RebuildSpawnPointGroup("Pigeon Spawn Points", ferry, "Pigeon Spawn", positions);
        }

        private static Transform[] RebuildSpawnPointGroup(string groupName, Transform parentTransform, string pointName, Vector3[] positions)
        {
            GameObject oldLegacyGroup = groupName == "Fish Spawn Points" ? GameObject.Find("Enemy Spawn Points") : null;
            if (oldLegacyGroup != null)
            {
                Object.DestroyImmediate(oldLegacyGroup);
            }

            GameObject parent = GameObject.Find(groupName);
            if (parent != null)
            {
                Object.DestroyImmediate(parent);
            }

            parent = new GameObject(groupName);
            parent.transform.SetParent(parentTransform, false);

            Transform[] points = new Transform[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject point = new GameObject($"{pointName} {i + 1}");
                point.transform.SetParent(parent.transform, false);
                point.transform.localPosition = positions[i];
                points[i] = point.transform;
            }

            return points;
        }

        private static SimpleEnemy EnsureEnemyPrefab(Material enemyMaterial)
        {
            GameObject animatedEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AnimatedEnemyPrefabPath);
            SimpleEnemy animatedEnemy = animatedEnemyPrefab != null ? animatedEnemyPrefab.GetComponent<SimpleEnemy>() : null;
            if (animatedEnemy != null)
            {
                return animatedEnemy;
            }

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

            if (GameObject.Find("Terrain") != null || GameObject.Find("River Water Surface") != null)
            {
                SetEnvironmentBlockActive(parent.transform, "River Placeholder", false);
                SetEnvironmentBlockActive(parent.transform, "Shore A Placeholder", false);
                SetEnvironmentBlockActive(parent.transform, "Shore B Placeholder", false);
                return;
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

        private static void SetEnvironmentBlockActive(Transform parent, string blockName, bool active)
        {
            GameObject block = FindChild(parent, blockName);
            if (block != null)
            {
                block.SetActive(active);
            }
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
            GameObject managerObject = GameObject.Find("Game Manager") ?? GameObject.Find("Prototype Game Manager");
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
            SetInt(gameManager, "startingMoney", 100);

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
            SetObject(audioEvents, "ferryController", Object.FindFirstObjectByType<FerryController>());
            SetObject(audioEvents, "playerController", Object.FindFirstObjectByType<SimpleFPSController>());
            SetBool(audioEvents, "postEvents", true);
        }

        private static void EnsureWwiseFootsteps(SimpleFPSController playerController)
        {
            if (playerController == null)
            {
                return;
            }

            GameObject player = playerController.gameObject;
            EnsureComponent<AkGameObj>(player);

            PlayerFootsteps footsteps = EnsureComponent<PlayerFootsteps>(player);
            SetString(footsteps, "stepsEventName", WwiseAudioNames.PlaySteps);
            SetFloat(footsteps, "walkInterval", 0.45f);
            SetFloat(footsteps, "sprintInterval", 0.3f);
            SetFloat(footsteps, "surfaceProbeDistance", 3.5f);
            SetFloat(footsteps, "surfaceProbeRadius", 0.18f);
            SetString(footsteps, "defaultSurface", "Gravel");
            SetString(footsteps, "woodTag", "Wood");
            EnsureWoodSurfaceTags();

            GameObject wwiseGlobal = FindSceneObjectIncludingInactive("WwiseGlobal");
            if (wwiseGlobal == null)
            {
                Debug.LogWarning("WwiseGlobal was not found. The player footsteps are wired, but MainSoundBank could not be assigned.");
                return;
            }

            wwiseGlobal.SetActive(true);
            EditorUtility.SetDirty(wwiseGlobal);
        }

        private static void EnsureWwiseRuntime(GameManager gameManager, GameObject ferry)
        {
            GameObject wwiseGlobal = FindSceneObjectIncludingInactive("WwiseGlobal");
            if (wwiseGlobal == null)
            {
                Debug.LogWarning("WwiseGlobal was not found.", gameManager);
                return;
            }

            AkBank legacyBank = wwiseGlobal.GetComponent<AkBank>();
            if (legacyBank != null)
            {
                Object.DestroyImmediate(legacyBank);
            }

            EnsureComponent<AkGameObj>(wwiseGlobal);
            WwiseAudioRuntime runtime = EnsureComponent<WwiseAudioRuntime>(wwiseGlobal);
            SetObject(runtime, "gameManager", gameManager);
            SetString(runtime, "mainBankName", "MainSoundBank");
            SetString(runtime, "outdoorBankName", "OutdoorSoundBank");
            SetString(runtime, "indoorBankName", "IndoorSoundBank");
            wwiseGlobal.SetActive(true);
            EditorUtility.SetDirty(wwiseGlobal);

            FerryController ferryController = ferry.GetComponent<FerryController>();
            EnsureComponent<AkGameObj>(ferry);
            FerryAudio ferryAudio = EnsureComponent<FerryAudio>(ferry);
            SetObject(ferryAudio, "ferry", ferryController);
            SetFloat(ferryAudio, "rampUpDuration", 2f);
            SetFloat(ferryAudio, "rampDownDuration", 1.5f);
        }

        private static void EnsureWwiseUiAndShopAudio()
        {
            EnsureWoodSurfaceTags();

            foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
            {
                if (button.gameObject.scene.IsValid())
                {
                    EnsureComponent<WwiseUIButtonAudio>(button.gameObject);
                }
            }

            foreach (ShopScenePortal portal in Resources.FindObjectsOfTypeAll<ShopScenePortal>())
            {
                if (portal.gameObject.scene.IsValid())
                {
                    EnsureComponent<AkGameObj>(portal.gameObject);
                }
            }

            foreach (ShopInteriorExit exit in Resources.FindObjectsOfTypeAll<ShopInteriorExit>())
            {
                if (exit.gameObject.scene.IsValid())
                {
                    EnsureComponent<AkGameObj>(exit.gameObject);
                }
            }
        }

        private static void EnsureWoodSurfaceTags()
        {
            EnsureTag("Wood");

            foreach (Transform transform in Object.FindObjectsByType<Transform>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (!transform.gameObject.scene.IsValid())
                {
                    continue;
                }

                string objectName = transform.name;
                if (string.Equals(objectName, "Ferry_Root", System.StringComparison.OrdinalIgnoreCase)
                    || objectName.IndexOf("Jetty", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || objectName.IndexOf("Shop Interior Root", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    transform.gameObject.tag = "Wood";
                    EditorUtility.SetDirty(transform.gameObject);
                }
            }
        }

        private static void EnsureTag(string tagName)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets.Length == 0)
            {
                return;
            }

            SerializedObject tagManager = new SerializedObject(assets[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");
            for (int i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tagName)
                {
                    return;
                }
            }

            int index = tags.arraySize;
            tags.InsertArrayElementAtIndex(index);
            tags.GetArrayElementAtIndex(index).stringValue = tagName;
            tagManager.ApplyModifiedProperties();
        }

        private static ShopManager EnsureShopManager(GameManager gameManager)
        {
            UpgradeDefinition[] catalog =
            {
                EnsureUpgrade<WeaponDamageUpgrade>("Assets/Upgrades/WeaponDamage.asset", "Damage +10", "More weapon damage.", 10, 3,
                    upgrade => SetFloat(upgrade, "amount", 10f)),
                EnsureUpgrade<FireRateUpgrade>("Assets/Upgrades/FireRate.asset", "Fire Rate +18%", "Faster fire rate.", 10, 3,
                    upgrade => SetFloat(upgrade, "cooldownMultiplier", 0.82f)),
                EnsureUpgrade<FerryHealthUpgrade>("Assets/Upgrades/FerryHealth.asset", "Repair + Max HP", "Heal and raise ferry max health.", 10, 3,
                    upgrade => SetFloat(upgrade, "amount", 25f)),
                EnsureUpgrade<RicochetUpgrade>("Assets/Upgrades/Ricochet.asset", "Querschlaeger (Master)", "Shots ricochet to the nearest enemy.", 30, 1,
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

        private static T EnsureUpgrade<T>(string path, string displayName, string description, int cost, int maxPurchases, System.Action<T> configure)
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
            SetInt(upgrade, "maxPurchases", maxPurchases);
            configure?.Invoke(upgrade);
            return upgrade;
        }

        private static void EnsureVendingMachine(GameManager gameManager)
        {
            GameObject legacyMachine = GameObject.Find("Vending Machine");
            if (legacyMachine != null)
            {
                Object.DestroyImmediate(legacyMachine);
            }

            GameObject machine = GameObject.Find("Vending Machine Decoration");
            if (machine == null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(VendingMachinePrefabPath);
                machine = prefab != null
                    ? PrefabUtility.InstantiatePrefab(prefab) as GameObject
                    : GameObject.CreatePrimitive(PrimitiveType.Cube);
            }

            machine.name = "Vending Machine Decoration";
            GameObject ferry = GameObject.Find("Ferry_Root") ?? GameObject.Find("Ferry");
            if (ferry != null)
            {
                machine.transform.SetParent(ferry.transform, false);
                machine.transform.localPosition = VendingMachineLocalPosition;
                machine.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            }
            machine.transform.localScale = Vector3.one;

            ShopInteractable interactable = machine.GetComponent<ShopInteractable>();
            if (interactable != null)
            {
                Object.DestroyImmediate(interactable);
            }
        }

        private static GameObject EnsureShopPrompt()
        {
            GameObject canvasObject = GameObject.Find("Rollfaehren Fury Prototype HUD");
            if (canvasObject == null)
            {
                canvasObject = Object.FindFirstObjectByType<SimpleHUD>()?.gameObject;
            }

            if (canvasObject == null)
            {
                return null;
            }

            Transform existing = canvasObject.transform.Find("Shop Prompt");
            if (existing != null)
            {
                Text existingPrompt = existing.GetComponent<Text>();
                if (existingPrompt != null)
                {
                    existingPrompt.text = "Press E - Shop";
                    EditorUtility.SetDirty(existingPrompt);
                }

                return existing.gameObject;
            }

            Text prompt = CreateText(canvasObject.transform, "Shop Prompt", "Press E - Shop", new Vector2(0f, 140f), new Vector2(360f, 40f), 24, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            prompt.gameObject.SetActive(false);
            return prompt.gameObject;
        }

        private static void EnsureFerryRoundFlow(
            GameObject ferry,
            FerryDamageTarget ferryTarget,
            SimpleFPSController player,
            GameManager gameManager,
            EnemySpawner spawner,
            SimpleHUD hud)
        {
            RemovePlacedPigeon();

            Transform routeRoot = EnsureRootTransform("Ferry Route");
            Transform dockA = EnsureChildTransform(routeRoot, "Dock A");
            Transform dockB = EnsureChildTransform(routeRoot, "Dock B");
            Vector3 dockOffset = FerryDockBPosition - FerryStartPosition;
            dockA.position = ferry.transform.position;
            dockB.position = ferry.transform.position + dockOffset;
            dockA.rotation = ferry.transform.rotation;
            dockB.rotation = ferry.transform.rotation;

            Rigidbody ferryBody = EnsureComponent<Rigidbody>(ferry);
            ferryBody.useGravity = false;
            ferryBody.isKinematic = true;
            ferryBody.interpolation = RigidbodyInterpolation.None;

            FerryController ferryController = EnsureComponent<FerryController>(ferry);
            SetObject(ferryController, "dockA", dockA);
            SetObject(ferryController, "dockB", dockB);
            SetObject(ferryController, "playerController", player);
            SetFloat(ferryController, "crossingSpeed", 6f);
            SetFloat(ferryController, "departureDistance", 70f);
            SetInt(ferryController, "routeSamples", 64);
            SetObject(gameManager, "ferryController", ferryController);

            Transform[] fishPoints = EnsureSpawnPoints(ferry.transform);
            Transform[] pigeonPoints = EnsurePigeonSpawnPoints(ferry.transform);
            SimpleEnemy fishPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AnimatedEnemyPrefabPath)?.GetComponent<SimpleEnemy>();
            SimpleEnemy pigeonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PigeonEnemyPrefabPath)?.GetComponent<SimpleEnemy>();
            ConfigureEnemyProfiles(spawner, fishPrefab, fishPoints, pigeonPrefab, pigeonPoints);
            SetObject(spawner, "ferryTarget", ferryTarget);
            SetObject(spawner, "ferryController", ferryController);
            SetObject(spawner, "enemyPrefab", fishPrefab);
            SetObjectArray(spawner, "spawnPoints", fishPoints);
            SetFloat(spawner, "spawnStartProgress", 0.05f);
            SetFloat(spawner, "spawnEndProgress", 0.9f);

            EnsureVendingMachine(gameManager);
            EnsureRoundStartConsole(ferry.transform, gameManager, hud);
            EnsureGameplayPauseUi(gameManager, hud);
        }

        private static void EnsurePigeonAnimator()
        {
            RuntimeAnimatorController controller =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PigeonAnimatorControllerPath);
            if (controller == null)
            {
                Debug.LogWarning($"Pigeon animator controller was not found at {PigeonAnimatorControllerPath}.");
                return;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PigeonEnemyPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogWarning($"Pigeon prefab was not found at {PigeonEnemyPrefabPath}.");
                return;
            }

            Animator animator = EnsureComponent<Animator>(prefabRoot);
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.updateMode = AnimatorUpdateMode.Normal;

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PigeonEnemyPrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        private static void RemovePlacedPigeon()
        {
            SimpleEnemy[] sceneEnemies = Object.FindObjectsByType<SimpleEnemy>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            foreach (SimpleEnemy enemy in sceneEnemies)
            {
                if (enemy.gameObject.scene.IsValid()
                    && enemy.gameObject.name.StartsWith("CHAR_Pigeon", System.StringComparison.Ordinal))
                {
                    Object.DestroyImmediate(enemy.gameObject);
                }
            }
        }

        private static Transform EnsureRootTransform(string name)
        {
            GameObject root = GameObject.Find(name);
            if (root == null)
            {
                root = new GameObject(name);
            }

            return root.transform;
        }

        private static Transform EnsureChildTransform(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child == null)
            {
                child = new GameObject(name).transform;
                child.SetParent(parent, false);
            }

            return child;
        }

        private static void ConfigureEnemyProfiles(
            EnemySpawner spawner,
            SimpleEnemy fishPrefab,
            Transform[] fishPoints,
            SimpleEnemy pigeonPrefab,
            Transform[] pigeonPoints)
        {
            SerializedObject serializedSpawner = new SerializedObject(spawner);
            SerializedProperty profiles = serializedSpawner.FindProperty("enemyProfiles");
            profiles.arraySize = 2;
            ConfigureEnemyProfileProperty(
                profiles.GetArrayElementAtIndex(0),
                "Fish",
                fishPrefab,
                fishPoints,
                1,
                0.7f,
                true,
                EnemySpawnHeight);
            ConfigureEnemyProfileProperty(
                profiles.GetArrayElementAtIndex(1),
                "Pigeon",
                pigeonPrefab,
                pigeonPoints,
                2,
                0.3f,
                false,
                0f);
            serializedSpawner.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(spawner);
        }

        private static void ConfigureEnemyProfileProperty(
            SerializedProperty profile,
            string displayName,
            SimpleEnemy prefab,
            Transform[] points,
            int firstRound,
            float weight,
            bool useFixedHeight,
            float fixedHeight)
        {
            profile.FindPropertyRelative("displayName").stringValue = displayName;
            profile.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            profile.FindPropertyRelative("firstRound").intValue = firstRound;
            profile.FindPropertyRelative("spawnWeight").floatValue = weight;
            profile.FindPropertyRelative("useFixedSpawnHeight").boolValue = useFixedHeight;
            profile.FindPropertyRelative("fixedSpawnHeight").floatValue = fixedHeight;

            SerializedProperty spawnPointProperty = profile.FindPropertyRelative("spawnPoints");
            spawnPointProperty.arraySize = points.Length;
            for (int i = 0; i < points.Length; i++)
            {
                spawnPointProperty.GetArrayElementAtIndex(i).objectReferenceValue = points[i];
            }
        }

        private static void EnsureRoundStartConsole(Transform ferry, GameManager gameManager, SimpleHUD hud)
        {
            GameObject console = FindChild(ferry, "Round Start Console");
            if (console == null)
            {
                console = GameObject.CreatePrimitive(PrimitiveType.Cube);
                console.name = "Round Start Console";
                console.transform.SetParent(ferry, false);
            }

            console.transform.localPosition = RoundConsoleLocalPosition;
            console.transform.localRotation = Quaternion.identity;
            console.transform.localScale = new Vector3(1.2f, 0.8f, 0.8f);
            console.GetComponent<Renderer>().sharedMaterial = EnsureMaterial(
                "Assets/Materials/PrototypeRoundConsole.mat",
                new Color(0.15f, 0.55f, 0.32f));

            BoxCollider collider = EnsureComponent<BoxCollider>(console);
            collider.isTrigger = true;
            collider.size = new Vector3(3f, 3f, 3f);

            GameObject lever = FindChild(console.transform, "Lever");
            if (lever == null)
            {
                lever = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lever.name = "Lever";
                lever.transform.SetParent(console.transform, false);
                Object.DestroyImmediate(lever.GetComponent<Collider>());
            }

            lever.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            lever.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);
            lever.transform.localScale = new Vector3(0.12f, 0.9f, 0.12f);
            lever.GetComponent<Renderer>().sharedMaterial = EnsureMaterial(
                "Assets/Materials/PrototypeRoundConsoleLever.mat",
                new Color(0.9f, 0.22f, 0.12f));

            GameObject prompt = EnsureHudPrompt(hud.transform, "Round Start Prompt", "Press E", new Vector2(0f, 95f));
            RoundStartConsole interaction = EnsureComponent<RoundStartConsole>(console);
            SetObject(interaction, "gameManager", gameManager);
            SetFloat(interaction, "interactRange", 3.5f);
            SetObject(interaction, "promptObject", prompt);
        }

        private static GameObject EnsureHudPrompt(Transform hud, string name, string text, Vector2 position)
        {
            Transform existing = hud.Find(name);
            Text prompt;
            if (existing != null)
            {
                prompt = existing.GetComponent<Text>();
            }
            else
            {
                prompt = CreateText(hud, name, text, position, new Vector2(440f, 40f), 24, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            }

            prompt.text = text;
            prompt.gameObject.SetActive(false);
            return prompt.gameObject;
        }

        private static void EnsureGameplayPauseUi(GameManager gameManager, SimpleHUD hud)
        {
            GameObject inputObject = GameObject.Find("Gameplay Menu Input");
            if (inputObject == null)
            {
                inputObject = new GameObject("Gameplay Menu Input");
            }

            GameplayMenuInput[] staleInputs = Object.FindObjectsByType<GameplayMenuInput>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            foreach (GameplayMenuInput staleInput in staleInputs)
            {
                if (staleInput.gameObject != inputObject)
                {
                    Object.DestroyImmediate(staleInput);
                }
            }

            GameplayMenuInput pauseInput = EnsureComponent<GameplayMenuInput>(inputObject);
            SetObject(pauseInput, "gameManager", gameManager);

            Transform oldPause = hud.transform.Find("Pause Panel");
            if (oldPause != null)
            {
                Object.DestroyImmediate(oldPause.gameObject);
            }

            Transform oldSettings = hud.transform.Find("Pause Settings Panel");
            if (oldSettings != null)
            {
                Object.DestroyImmediate(oldSettings.gameObject);
            }

            GameObject pausePanel = CreateUiPanel(hud.transform, "Pause Panel", TextAnchor.MiddleCenter, Vector2.zero, new Vector2(560f, 520f));
            EnsureComponent<Image>(pausePanel).color = new Color(0.035f, 0.055f, 0.075f, 0.96f);
            CreateText(pausePanel.transform, "Pause Title", "Paused", new Vector2(0f, 205f), new Vector2(500f, 48f), 34, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            CreateButton(pausePanel.transform, "Pause Resume Button", "Resume", new Vector2(0f, 125f), out Button resume);
            CreateButton(pausePanel.transform, "Pause New Game Button", "New Game", new Vector2(0f, 65f), out Button newGame);
            CreateButton(pausePanel.transform, "Pause Settings Button", "Settings", new Vector2(0f, 5f), out Button settings);
            CreateButton(pausePanel.transform, "Pause Main Menu Button", "Main Menu", new Vector2(0f, -55f), out Button mainMenu);
            CreateButton(pausePanel.transform, "Pause Quit Button", "Quit", new Vector2(0f, -115f), out Button quit);

            GameObject settingsPanel = CreateUiPanel(hud.transform, "Pause Settings Panel", TextAnchor.MiddleCenter, Vector2.zero, new Vector2(560f, 320f));
            EnsureComponent<Image>(settingsPanel).color = new Color(0.035f, 0.055f, 0.075f, 0.96f);
            CreateText(settingsPanel.transform, "Pause Settings Title", "Settings", new Vector2(0f, 105f), new Vector2(500f, 44f), 32, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            CreateText(settingsPanel.transform, "Pause Settings Placeholder", "Settings options are coming later.", new Vector2(0f, 25f), new Vector2(500f, 36f), 20, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            CreateButton(settingsPanel.transform, "Pause Settings Back Button", "Back", new Vector2(0f, -85f), out Button back);

            resume.onClick.RemoveAllListeners();
            newGame.onClick.RemoveAllListeners();
            settings.onClick.RemoveAllListeners();
            mainMenu.onClick.RemoveAllListeners();
            quit.onClick.RemoveAllListeners();
            back.onClick.RemoveAllListeners();
            UnityEventTools.AddPersistentListener(resume.onClick, pauseInput.Resume);
            UnityEventTools.AddPersistentListener(newGame.onClick, pauseInput.RestartRun);
            UnityEventTools.AddPersistentListener(settings.onClick, pauseInput.ShowSettings);
            UnityEventTools.AddPersistentListener(mainMenu.onClick, pauseInput.ReturnToMenu);
            UnityEventTools.AddPersistentListener(quit.onClick, pauseInput.QuitGame);
            UnityEventTools.AddPersistentListener(back.onClick, pauseInput.BackToPause);

            SetObject(pauseInput, "pausePanel", pausePanel);
            SetObject(pauseInput, "settingsPanel", settingsPanel);
            pausePanel.SetActive(false);
            settingsPanel.SetActive(false);
        }

        private static void EnsureAugmentSystem(GameManager gameManager, EnemySpawner spawner)
        {
            AugmentDefinition[] pool =
            {
                EnsureAugment<TailwindAugment>("Assets/Augments/Tailwind.asset", "Tailwind", "Ferry crosses 15% faster.",
                    augment => SetFloat(augment, "crossingFactor", 0.85f)),
                EnsureAugment<RepairKitAugment>("Assets/Augments/RepairKit.asset", "Repair Kit", "Heal 5% ferry HP each round.",
                    augment => SetFloat(augment, "healFraction", 0.05f)),
                EnsureAugment<SwarmAugment>("Assets/Augments/Swarm.asset", "The Swarm", "2x enemies, half HP.",
                    augment => { SetFloat(augment, "countMultiplier", 2f); SetFloat(augment, "healthMultiplier", 0.5f); }),
                EnsureAugment<BruisersAugment>("Assets/Augments/Bruisers.asset", "Bruisers", "Half enemies, 2x HP.",
                    augment => { SetFloat(augment, "countMultiplier", 0.5f); SetFloat(augment, "healthMultiplier", 2f); }),
                EnsureAugment<SluggishTideAugment>("Assets/Augments/SluggishTide.asset", "Sluggish Tide", "Enemies move 20% slower.",
                    augment => SetFloat(augment, "speedMultiplier", 0.8f)),
                EnsureAugment<BountyAugment>("Assets/Augments/Bounty.asset", "Bounty", "+50% money per kill.",
                    augment => SetFloat(augment, "rewardMultiplier", 1.5f)),
                EnsureAugment<WarChestAugment>("Assets/Augments/WarChest.asset", "War Chest", "Instant $75.",
                    augment => SetInt(augment, "money", 75)),
                EnsureAugment<ReinforcedHullAugment>("Assets/Augments/ReinforcedHull.asset", "Reinforced Hull", "+50 ferry max HP.",
                    augment => SetFloat(augment, "bonusHealth", 50f)),
            };

            AugmentSystem augmentSystem = EnsureComponent<AugmentSystem>(gameManager.gameObject);
            SetObject(augmentSystem, "gameManager", gameManager);
            SetObject(augmentSystem, "spawner", spawner);
            SetObjectList(augmentSystem, "pool", pool);

            Button[] draftButtons = new Button[3];
            for (int i = 0; i < draftButtons.Length; i++)
            {
                Button button = FindSceneButton($"Augment Draft Button {i}");
                draftButtons[i] = button;
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    UnityEventTools.AddIntPersistentListener(button.onClick, augmentSystem.Pick, i);
                    EditorUtility.SetDirty(button);
                }
            }

            SetObjectList(augmentSystem, "draftButtons", draftButtons);
            SetObject(gameManager, "augmentSystem", augmentSystem);
        }

        private static T EnsureAugment<T>(string path, string displayName, string description, System.Action<T> configure)
            where T : AugmentDefinition
        {
            T augment = AssetDatabase.LoadAssetAtPath<T>(path);
            if (augment == null)
            {
                augment = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(augment, path);
            }

            SetString(augment, "displayName", displayName);
            SetString(augment, "description", description);
            configure?.Invoke(augment);
            return augment;
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
            CreateButton(shopPanel.transform, "Next Round Button", "Next Round", new Vector2(0f, -150f), out Button nextRoundButton);
            CreateButton(shopPanel.transform, "Close Shop Button", "Close", new Vector2(0f, -150f), out Button closeShopButton);
            shopPanel.SetActive(false);

            GameObject gameOverPanel = CreateUiPanel(canvasObject.transform, "Game Over Panel", TextAnchor.MiddleCenter, Vector2.zero, new Vector2(620f, 330f));
            Image gameOverBackground = EnsureComponent<Image>(gameOverPanel);
            gameOverBackground.color = new Color(0.1f, 0.02f, 0.02f, 0.9f);
            Text gameOverText = CreateText(gameOverPanel.transform, "Game Over Text", "Ferry destroyed", new Vector2(0f, 58f), new Vector2(560f, 120f), 30, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            CreateButton(gameOverPanel.transform, "Restart Button", "Restart", new Vector2(0f, -92f), out Button restartButton);
            gameOverPanel.SetActive(false);

            GameObject augmentPanel = CreateUiPanel(canvasObject.transform, "Augment Draft Panel", TextAnchor.MiddleCenter, Vector2.zero, new Vector2(700f, 460f));
            Image augmentBackground = EnsureComponent<Image>(augmentPanel);
            augmentBackground.color = new Color(0.04f, 0.06f, 0.09f, 0.92f);
            CreateText(augmentPanel.transform, "Augment Title", "Choose an Augment", new Vector2(0f, 188f), new Vector2(640f, 44f), 30, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            CreateAugmentButton(augmentPanel.transform, "Augment Draft Button 0", new Vector2(0f, 108f), out _);
            CreateAugmentButton(augmentPanel.transform, "Augment Draft Button 1", new Vector2(0f, -12f), out _);
            CreateAugmentButton(augmentPanel.transform, "Augment Draft Button 2", new Vector2(0f, -132f), out _);
            augmentPanel.SetActive(false);

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
            SetObject(hud, "nextRoundButton", nextRoundButton.gameObject);
            SetObject(hud, "closeShopButton", closeShopButton.gameObject);
            SetObject(hud, "augmentDraftPanel", augmentPanel);

            return hud;
        }

        private static void ConfigureHudButtons(GameManager gameManager)
        {
            AddButtonListener("Next Round Button", gameManager.StartNextRound);
            AddButtonListener("Close Shop Button", gameManager.CloseShopOverlay);
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

        // Taller button for the round-end augment draft so a two-line "Name + description"
        // label fits. The label wraps and overflows vertically instead of being truncated,
        // and supports rich text so AugmentSystem can render a bold title + smaller description.
        private static Text CreateAugmentButton(Transform parent, string name, Vector2 anchoredPosition, out Button button)
        {
            GameObject buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);
            Image image = EnsureComponent<Image>(buttonObject);
            image.color = new Color(0.12f, 0.45f, 0.75f, 1f);
            button = EnsureComponent<Button>(buttonObject);
            ConfigureRect(buttonObject.GetComponent<RectTransform>(), TextAnchor.MiddleCenter, anchoredPosition, new Vector2(640f, 108f));

            Text buttonText = CreateText(buttonObject.transform, "Label", "Augment", Vector2.zero, new Vector2(612f, 96f), 22, TextAnchor.MiddleCenter, TextAnchor.MiddleCenter);
            buttonText.color = Color.white;
            buttonText.supportRichText = true;
            buttonText.horizontalOverflow = HorizontalWrapMode.Wrap;
            buttonText.verticalOverflow = VerticalWrapMode.Overflow;
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
                new EditorBuildSettingsScene(MainScenePath, true),
                new EditorBuildSettingsScene(ShopScenePath, true)
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

        private static GameObject FindSceneObjectIncludingInactive(string objectName)
        {
            foreach (Transform transform in Object.FindObjectsByType<Transform>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (transform.name == objectName)
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

        private static void SetWwiseReference(Object target, string propertyName, string assetPath)
        {
            Object reference = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (reference == null)
            {
                Debug.LogWarning($"Wwise reference asset was not found at '{assetPath}'.", target);
                return;
            }

            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            SerializedProperty referenceProperty = property?.FindPropertyRelative("WwiseObjectReference");
            if (referenceProperty == null)
            {
                Debug.LogWarning($"Wwise property '{propertyName}' was not found on {target.name}.", target);
                return;
            }

            referenceProperty.objectReferenceValue = reference;
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
