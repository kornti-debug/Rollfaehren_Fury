using System.Collections.Generic;
using RollfaehrenFury.Prototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RollfaehrenFury.Editor
{
    public static class ShopSceneBuilder
    {
        private const string MainScenePath = "Assets/Scenes/Main.unity";
        private const string ShopScenePath = "Assets/Scenes/ShopInterior.unity";
        private const string ShopNpcPrefabPath = "Assets/Prefabs/NPC_Shop.prefab";
        private const string VendingMachinePrefabPath = "Assets/Models/VendingMachine.fbx";
        private const string ShopInteriorRootName = "Shop Interior Root";
        private static readonly Vector3 ShopInteriorWorldPosition = new Vector3(5000f, 500f, 5000f);

        [MenuItem("Rollfaehren Fury/Configure Shared Shop Portals")]
        public static void ConfigureSharedShopPortals()
        {
            Scene mainScene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
            SimpleFPSController player = Object.FindFirstObjectByType<SimpleFPSController>();
            SimpleHUD hud = Object.FindFirstObjectByType<SimpleHUD>();
            if (gameManager == null || player == null || hud == null)
            {
                Debug.LogError("Main scene is missing the GameManager, player, or HUD.");
                return;
            }

            ShopSceneCoordinator coordinator = EnsureComponent<ShopSceneCoordinator>(gameManager.gameObject);
            SetObject(coordinator, "gameManager", gameManager);
            SetObject(coordinator, "playerController", player);

            GameObject prompt = EnsurePrompt(hud.transform);
            ConfigurePortal("shophouse", "ShoreA", prompt);
            ConfigurePortal("shophouse 1", "ShoreB", prompt);

            EditorSceneManager.MarkSceneDirty(mainScene);
            EditorSceneManager.SaveScene(mainScene);
            Debug.Log("Shared shop portals configured in Main.unity.");
        }

        public static void ConfigureSharedShopPortalsFromCommandLine()
        {
            ConfigureSharedShopPortals();
        }

        [MenuItem("Rollfaehren Fury/Build Shared Shop Interior")]
        public static void BuildSharedShopInterior()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ShopScenePath) != null)
            {
                RepairSharedShopInterior();
                return;
            }

            Material wallMaterial = EnsureMaterial(
                "Assets/Materials/ShopInteriorWall.mat",
                new Color(0.38f, 0.34f, 0.28f));
            Material floorMaterial = EnsureMaterial(
                "Assets/Materials/ShopInteriorFloor.mat",
                new Color(0.18f, 0.22f, 0.2f));
            Material counterMaterial = EnsureMaterial(
                "Assets/Materials/ShopInteriorCounter.mat",
                new Color(0.48f, 0.2f, 0.12f));

            Scene shopScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            shopScene.name = SceneFlow.ShopInteriorSceneName;

            GameObject room = new GameObject(ShopInteriorRootName);
            CreateBlock(room.transform, "Floor", new Vector3(0f, -0.1f, 0f), new Vector3(8f, 0.2f, 10f), floorMaterial);
            CreateBlock(room.transform, "Ceiling", new Vector3(0f, 4f, 0f), new Vector3(8f, 0.2f, 10f), wallMaterial);
            CreateBlock(room.transform, "Back Wall", new Vector3(0f, 2f, -5f), new Vector3(8f, 4f, 0.2f), wallMaterial);
            CreateBlock(room.transform, "Left Wall", new Vector3(-4f, 2f, 0f), new Vector3(0.2f, 4f, 10f), wallMaterial);
            CreateBlock(room.transform, "Right Wall", new Vector3(4f, 2f, 0f), new Vector3(0.2f, 4f, 10f), wallMaterial);
            CreateBlock(room.transform, "Front Wall Left", new Vector3(-2.5f, 2f, 5f), new Vector3(3f, 4f, 0.2f), wallMaterial);
            CreateBlock(room.transform, "Front Wall Right", new Vector3(2.5f, 2f, 5f), new Vector3(3f, 4f, 0.2f), wallMaterial);
            CreateBlock(room.transform, "Door", new Vector3(0f, 2f, 5f), new Vector3(2f, 4f, 0.2f), wallMaterial);
            CreateBlock(room.transform, "Counter", new Vector3(0f, 0.6f, -2f), new Vector3(4.5f, 1.2f, 1f), counterMaterial);

            Transform spawn = new GameObject("Shop Interior Spawn").transform;
            spawn.SetParent(room.transform, false);
            spawn.localPosition = new Vector3(0f, 0.05f, 2.6f);
            spawn.localRotation = Quaternion.Euler(0f, 180f, 0f);

            GameObject exit = new GameObject("Shop Exit");
            exit.transform.SetParent(room.transform, false);
            exit.transform.localPosition = new Vector3(0f, 1f, 4.35f);
            BoxCollider exitCollider = exit.AddComponent<BoxCollider>();
            exitCollider.isTrigger = true;
            exitCollider.size = new Vector3(2f, 2f, 1.2f);
            exit.AddComponent<ShopInteriorExit>();
            EnsureComponent<AkGameObj>(exit);

            GameObject npcPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ShopNpcPrefabPath);
            if (npcPrefab != null)
            {
                GameObject npc = PrefabUtility.InstantiatePrefab(npcPrefab, shopScene) as GameObject;
                if (npc != null)
                {
                    npc.name = "NPC_Shop";
                    npc.transform.SetParent(room.transform, false);
                    npc.transform.SetLocalPositionAndRotation(new Vector3(0f, 0f, -3f), Quaternion.identity);
                    npc.transform.localScale = Vector3.one;
                    ShopInteractable interactable = npc.GetComponent<ShopInteractable>();
                    if (interactable != null)
                    {
                        interactable.enabled = true;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Shop NPC prefab was not found at '{ShopNpcPrefabPath}'.");
            }

            GameObject lightObject = new GameObject("Shop Light");
            lightObject.transform.SetParent(room.transform, false);
            Light shopLight = lightObject.AddComponent<Light>();
            shopLight.type = LightType.Point;
            shopLight.range = 15f;
            shopLight.intensity = 5f;
            shopLight.color = new Color(1f, 0.82f, 0.62f);
            lightObject.transform.localPosition = new Vector3(0f, 2.5f, 0f);

            room.transform.position = ShopInteriorWorldPosition;

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.28f, 0.3f, 0.32f);

            EditorSceneManager.MarkSceneDirty(shopScene);
            EditorSceneManager.SaveScene(shopScene, ShopScenePath);
            AddShopSceneToBuildSettings();
            FinalizeMainShopAccess();
            AssetDatabase.SaveAssets();
            Debug.Log("Shared ShopInterior.unity created and added to Build Settings.");
        }

        public static void BuildSharedShopInteriorFromCommandLine()
        {
            BuildSharedShopInterior();
        }

        [MenuItem("Rollfaehren Fury/Repair Shared Shop Interior")]
        public static void RepairSharedShopInterior()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Scene shopScene = EditorSceneManager.OpenScene(ShopScenePath, OpenSceneMode.Single);
            GameObject root = FindSceneObjectInScene(shopScene, ShopInteriorRootName)
                ?? FindSceneObjectInScene(shopScene, "Shop Interior");
            if (root == null)
            {
                Debug.LogError("ShopInterior scene is missing its room root.");
                return;
            }

            bool rootNeedsMoving = Vector3.Distance(root.transform.position, ShopInteriorWorldPosition) > 0.01f;
            root.name = ShopInteriorRootName;

            foreach (GameObject sceneRoot in shopScene.GetRootGameObjects())
            {
                if (sceneRoot == root)
                {
                    continue;
                }

                sceneRoot.transform.SetParent(root.transform, true);
            }

            if (rootNeedsMoving)
            {
                root.transform.position = ShopInteriorWorldPosition;
            }

            ApplyRoomPolish(shopScene, root.transform);

            foreach (ShopInteriorExit exit in root.GetComponentsInChildren<ShopInteriorExit>(true))
            {
                EnsureComponent<AkGameObj>(exit.gameObject);
            }

            AddShopSceneToBuildSettings();
            EditorSceneManager.MarkSceneDirty(shopScene);
            EditorSceneManager.SaveScene(shopScene);
            AssetDatabase.SaveAssets();
            Debug.Log("Shared ShopInterior.unity repaired without rebuilding manual room content.");
        }

        private static void ApplyRoomPolish(Scene shopScene, Transform room)
        {
            SetLocalTransform(shopScene, "Ceiling", new Vector3(0f, 4.964f, 0f), new Vector3(8f, 0.2f, 10f));
            SetLocalTransform(shopScene, "Back Wall", new Vector3(0f, 2f, -5f), new Vector3(8f, 6f, 0.2f));
            SetLocalTransform(shopScene, "Left Wall", new Vector3(-4f, 2f, 0f), new Vector3(0.2f, 6f, 10f));
            SetLocalTransform(shopScene, "Right Wall", new Vector3(4f, 2f, 0f), new Vector3(0.2f, 6f, 10f));
            SetLocalTransform(shopScene, "Front Wall Left", new Vector3(-2.5f, 2f, 5f), new Vector3(3f, 6f, 0.2f));
            SetLocalTransform(shopScene, "Front Wall Right", new Vector3(2.5f, 2f, 5f), new Vector3(3f, 6f, 0.2f));

            GameObject counter = FindSceneObjectInScene(shopScene, "Counter");
            if (counter == null)
            {
                Material counterMaterial = EnsureMaterial(
                    "Assets/Materials/ShopInteriorCounter.mat",
                    new Color(0.48f, 0.2f, 0.12f));
                counter = CreateBlock(
                    room,
                    "Counter",
                    new Vector3(0f, 0.71f, -2f),
                    new Vector3(4.5f, 2.24f, 1f),
                    counterMaterial);
            }
            else
            {
                counter.transform.SetLocalPositionAndRotation(new Vector3(0f, 0.71f, -2f), Quaternion.identity);
                counter.transform.localScale = new Vector3(4.5f, 2.24f, 1f);
            }

            GameObject npc = FindSceneObjectInScene(shopScene, "NPC_Shop");
            if (npc != null)
            {
                npc.transform.localScale = Vector3.one * 1.5f;
            }

            GameObject shopLight = FindSceneObjectInScene(shopScene, "Shop Light");
            if (shopLight != null)
            {
                EnsureComponent<UniversalAdditionalLightData>(shopLight);
            }
        }

        private static void SetLocalTransform(
            Scene scene,
            string objectName,
            Vector3 localPosition,
            Vector3 localScale)
        {
            GameObject target = FindSceneObjectInScene(scene, objectName);
            if (target == null)
            {
                Debug.LogWarning($"Shop interior object '{objectName}' was not found; manual content was left unchanged.");
                return;
            }

            target.transform.localPosition = localPosition;
            target.transform.localScale = localScale;
        }

        public static void RepairSharedShopInteriorFromCommandLine()
        {
            RepairSharedShopInterior();
        }

        private static void ConfigurePortal(string objectName, string shopId, GameObject prompt)
        {
            GameObject house = FindSceneObjectIncludingInactive(objectName);
            if (house == null)
            {
                Debug.LogWarning($"Shop house '{objectName}' was not found in Main.unity.");
                return;
            }

            ShopScenePortal portal = EnsureComponent<ShopScenePortal>(house);
            EnsureComponent<AkGameObj>(house);
            SetString(portal, "sceneName", SceneFlow.ShopInteriorSceneName);
            SetString(portal, "shopId", shopId);
            SetObject(portal, "promptObject", prompt);

            bool hasTrigger = false;
            foreach (Collider collider in house.GetComponents<Collider>())
            {
                hasTrigger |= collider.isTrigger;
            }

            if (!hasTrigger)
            {
                BoxCollider trigger = house.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.center = new Vector3(0f, 1.5f, 3f);
                trigger.size = new Vector3(2f, 3f, 2f);
            }
        }

        private static GameObject EnsurePrompt(Transform hud)
        {
            GameObject prompt = FindSceneObjectIncludingInactive("Shop Scene Prompt");
            Text label;
            if (prompt == null)
            {
                prompt = new GameObject("Shop Scene Prompt");
                prompt.transform.SetParent(hud, false);
                label = prompt.AddComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.fontSize = 24;
                label.color = Color.white;
                label.alignment = TextAnchor.MiddleCenter;
                label.raycastTarget = false;
                RectTransform rect = label.rectTransform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0f, 140f);
                rect.sizeDelta = new Vector2(500f, 40f);
            }
            else
            {
                label = prompt.GetComponent<Text>();
            }

            if (label != null)
            {
                label.text = "Press E - Enter shop";
            }

            prompt.SetActive(false);
            return prompt;
        }

        private static void FinalizeMainShopAccess()
        {
            ConfigureSharedShopPortals();
            Scene mainScene = SceneManager.GetActiveScene();

            GameObject ferryNpc = FindSceneObjectIncludingInactive("NPC_Shop");
            GameObject ferryRoot = GameObject.Find("Ferry_Root");
            if (ferryNpc != null && ferryRoot != null && ferryNpc.transform.IsChildOf(ferryRoot.transform))
            {
                Object.DestroyImmediate(ferryNpc);
            }

            if (ferryRoot != null)
            {
                GameObject decoration = FindSceneObjectIncludingInactive("Vending Machine Decoration");
                if (decoration == null)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(VendingMachinePrefabPath);
                    decoration = prefab != null
                        ? PrefabUtility.InstantiatePrefab(prefab, ferryRoot.transform) as GameObject
                        : GameObject.CreatePrimitive(PrimitiveType.Cube);
                }

                if (decoration != null)
                {
                    decoration.name = "Vending Machine Decoration";
                    decoration.transform.SetParent(ferryRoot.transform, false);
                    decoration.transform.localPosition = new Vector3(1.9f, 10.4f, 6.4f);
                    decoration.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                    decoration.transform.localScale = Vector3.one;

                    ShopInteractable ferryShop = decoration.GetComponent<ShopInteractable>();
                    if (ferryShop != null)
                    {
                        Object.DestroyImmediate(ferryShop);
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(mainScene);
            EditorSceneManager.SaveScene(mainScene);
        }

        public static void ValidateSharedShopSetupFromCommandLine()
        {
            Scene mainScene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            if (Object.FindFirstObjectByType<ShopSceneCoordinator>() == null)
            {
                throw new System.InvalidOperationException("Main scene is missing ShopSceneCoordinator.");
            }

            ShopScenePortal[] portals = Object.FindObjectsByType<ShopScenePortal>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            if (portals.Length != 2)
            {
                throw new System.InvalidOperationException($"Expected 2 shop portals, found {portals.Length}.");
            }

            HashSet<string> shopIds = new HashSet<string>();
            GameObject sharedPrompt = null;
            foreach (ShopScenePortal portal in portals)
            {
                SerializedObject serializedPortal = new SerializedObject(portal);
                SerializedProperty shopId = serializedPortal.FindProperty("shopId");
                SerializedProperty promptObject = serializedPortal.FindProperty("promptObject");
                GameObject portalPrompt = promptObject?.objectReferenceValue as GameObject;
                if (shopId == null || string.IsNullOrWhiteSpace(shopId.stringValue))
                {
                    throw new System.InvalidOperationException($"Shop portal '{portal.name}' has no shop ID.");
                }

                if (portalPrompt == null)
                {
                    throw new System.InvalidOperationException($"Shop portal '{portal.name}' has no prompt reference.");
                }

                shopIds.Add(shopId.stringValue);
                sharedPrompt ??= portalPrompt;
                if (sharedPrompt != portalPrompt)
                {
                    throw new System.InvalidOperationException("Shop portals must reference the shared shop prompt.");
                }
            }

            if (!shopIds.Contains("ShoreA") || !shopIds.Contains("ShoreB"))
            {
                throw new System.InvalidOperationException("Shop portals must include ShoreA and ShoreB.");
            }

            if (FindSceneObjectIncludingInactive("Vending Machine Decoration") == null)
            {
                Debug.LogWarning("Optional ferry vending-machine decoration is not present in Main.unity.");
            }

            Scene shopScene = EditorSceneManager.OpenScene(ShopScenePath, OpenSceneMode.Additive);
            GameObject shopRoot = FindSceneObjectInScene(shopScene, ShopInteriorRootName);
            GameObject shopSpawn = FindSceneObjectInScene(shopScene, "Shop Interior Spawn");
            GameObject shopExit = FindSceneObjectInScene(shopScene, "Shop Exit");
            GameObject shopNpc = FindSceneObjectInScene(shopScene, "NPC_Shop");
            GameObject shopLight = FindSceneObjectInScene(shopScene, "Shop Light");
            if (shopRoot == null
                || Vector3.Distance(shopRoot.transform.position, ShopInteriorWorldPosition) > 0.01f
                || shopSpawn == null
                || shopExit == null
                || shopNpc == null
                || shopLight == null
                || !shopSpawn.transform.IsChildOf(shopRoot.transform)
                || !shopExit.transform.IsChildOf(shopRoot.transform)
                || !shopNpc.transform.IsChildOf(shopRoot.transform)
                || !shopLight.transform.IsChildOf(shopRoot.transform))
            {
                throw new System.InvalidOperationException(
                    "ShopInterior scene is not isolated or its spawn, exit, NPC, and light are not below the room root.");
            }

            Debug.Log("Shared shop scene validation passed.");
            EditorSceneManager.CloseScene(shopScene, true);
            EditorSceneManager.SetActiveScene(mainScene);
        }

        private static void AddShopSceneToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool found = false;
            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path == ShopScenePath)
                {
                    scenes[i] = new EditorBuildSettingsScene(ShopScenePath, true);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                scenes.Add(new EditorBuildSettingsScene(ShopScenePath, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static GameObject CreateBlock(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent, false);
            block.transform.localPosition = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = material;
            return block;
        }

        private static Material EnsureMaterial(string path, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject FindSceneObjectIncludingInactive(string objectName)
        {
            foreach (Transform transform in Object.FindObjectsByType<Transform>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (transform.gameObject.scene.IsValid() && transform.name == objectName)
                {
                    return transform.gameObject;
                }
            }

            return null;
        }

        private static GameObject FindSceneObjectInScene(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (transform.name == objectName)
                    {
                        return transform.gameObject;
                    }
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
            if (property != null)
            {
                property.objectReferenceValue = value;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(target);
            }
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
    }
}
