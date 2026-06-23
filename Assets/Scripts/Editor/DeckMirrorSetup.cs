using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// One-shot helper that builds the deck "mirror": a selfie-style camera renders the player into a
    /// RenderTexture shown (horizontally flipped, so it reads like a mirror) on a quad mounted on the
    /// ferry. A <see cref="MirrorInteractable"/> lets the player press the interact key in front of it
    /// to list the run's active augments. It is additive and idempotent: re-running re-wires the rig
    /// and re-applies the texture; it never deletes anything else.
    ///
    /// After running, nudge the "Deck Mirror" transform on the ferry so the glass faces where you
    /// stand, and confirm the player body (Fraunz) is visible + animating — that is what the mirror shows.
    /// </summary>
    public static class DeckMirrorSetup
    {
        private const string MirrorName = "Deck Mirror";
        private const string CanvasName = "Mirror UI";
        private const string RenderTexturePath = "Assets/Rendering/DeckMirror.renderTexture";
        private const string MaterialPath = "Assets/Materials/DeckMirror.mat";

        // Starting pose relative to the ferry root — eye height on the deck. You WILL nudge this in the
        // Inspector to sit it against a railing/wall and face the deck. +Z of the mirror = toward the player.
        private static readonly Vector3 MirrorLocalPosition = new Vector3(0f, 11f, 15f);
        private static readonly Vector3 MirrorLocalEuler = new Vector3(0f, 180f, 0f);
        private static readonly Vector3 MirrorLocalScale = new Vector3(1.4f, 2.2f, 1f);

        [MenuItem("Tools/Rollfaehren Fury/Setup Deck Mirror")]
        public static void Setup()
        {
            AugmentSystem augmentSystem = Object.FindFirstObjectByType<AugmentSystem>();
            GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
            FerryController ferry = Object.FindFirstObjectByType<FerryController>();
            if (gameManager == null)
            {
                Debug.LogError("[Deck Mirror] No GameManager in the open scene — open Main.unity and try again.");
                return;
            }

            RenderTexture rt = CreateOrLoadRenderTexture();
            Material mirrorMat = CreateOrLoadMirrorMaterial(rt);

            // --- mirror object (parent), quad (glass), selfie camera ---
            GameObject mirror = FindInScene(MirrorName);
            bool created = mirror == null;
            if (created)
            {
                mirror = new GameObject(MirrorName);
                if (ferry != null)
                {
                    mirror.transform.SetParent(ferry.transform, false);
                }

                mirror.transform.localPosition = MirrorLocalPosition;
                mirror.transform.localRotation = Quaternion.Euler(MirrorLocalEuler);
                mirror.transform.localScale = Vector3.one;

                // Glass quad: a child rotated 180° so its visible face looks along the mirror's +Z (toward the player).
                GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Quad);
                glass.name = "Glass";
                glass.transform.SetParent(mirror.transform, false);
                glass.transform.localPosition = Vector3.zero;
                glass.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                glass.transform.localScale = MirrorLocalScale;
                StripCollider(glass);
                SetLayerRecursive(glass, LayerMask.NameToLayer("Ignore Raycast"));

                // Selfie camera: sits just in front of the glass, looks along +Z toward the player, renders
                // to the RenderTexture. Excludes Ignore Raycast so it never films the glass (feedback) or the
                // first-person weapon viewmodels.
                GameObject camObject = new GameObject("Mirror Camera");
                camObject.transform.SetParent(mirror.transform, false);
                camObject.transform.localPosition = new Vector3(0f, 0f, 0.03f);
                camObject.transform.localRotation = Quaternion.identity;
                Camera cam = camObject.AddComponent<Camera>();
                cam.targetTexture = rt;
                cam.clearFlags = CameraClearFlags.Skybox; // reflect the sky, like a real mirror
                cam.backgroundColor = new Color(0.05f, 0.06f, 0.08f, 1f); // fallback if no skybox
                cam.fieldOfView = 55f;
                cam.nearClipPlane = 0.05f;
                cam.farClipPlane = 40f;
                cam.cullingMask = ~(1 << LayerMask.NameToLayer("Ignore Raycast"));
            }

            ApplyMirrorMaterial(mirror, mirrorMat);

            // Re-apply the key camera settings so re-running on an existing mirror also fixes them
            // (e.g. an older Solid-Color clear that hid the sky).
            Camera mirrorCam = mirror.GetComponentInChildren<Camera>(true);
            if (mirrorCam != null)
            {
                mirrorCam.clearFlags = CameraClearFlags.Skybox;
                mirrorCam.targetTexture = rt;
                mirrorCam.cullingMask = ~(1 << LayerMask.NameToLayer("Ignore Raycast"));
                EditorUtility.SetDirty(mirrorCam);
            }

            // --- screen-space UI: a "Press E" prompt + the augment-list panel (both hidden by default) ---
            BuildUi(out GameObject prompt, out GameObject panel, out Text listText);

            // --- interactable + wiring ---
            MirrorInteractable interactable = mirror.GetComponent<MirrorInteractable>();
            if (interactable == null)
            {
                interactable = mirror.AddComponent<MirrorInteractable>();
            }

            SerializedObject so = new SerializedObject(interactable);
            SetRef(so, "augmentSystem", augmentSystem);
            SetRef(so, "gameManager", gameManager);
            SetRef(so, "promptObject", prompt);
            SetRef(so, "panelObject", panel);
            SetRef(so, "listText", listText);
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(gameManager.gameObject.scene);
            Debug.Log($"[Deck Mirror] {(created ? "Created" : "Re-wired")} the mirror" +
                      $"{(ferry == null ? " (no FerryController found — it is at the scene root; parent it to the ferry yourself)" : " on the ferry")}. " +
                      "Nudge the 'Deck Mirror' transform so the glass faces where you stand, then save the scene. " +
                      "The mirror shows the player body — make sure Fraunz is visible and animating (SimpleFPSController.animateCharacter).");
        }

        private static RenderTexture CreateOrLoadRenderTexture()
        {
            RenderTexture existing = AssetDatabase.LoadAssetAtPath<RenderTexture>(RenderTexturePath);
            if (existing != null)
            {
                return existing;
            }

            EnsureFolder(Path.GetDirectoryName(RenderTexturePath));
            RenderTexture rt = new RenderTexture(768, 1024, 16)
            {
                name = "DeckMirror",
                antiAliasing = 2,
            };
            AssetDatabase.CreateAsset(rt, RenderTexturePath);
            AssetDatabase.SaveAssets();
            return rt;
        }

        private static Material CreateOrLoadMirrorMaterial(RenderTexture rt)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null)
                {
                    shader = Shader.Find("Unlit/Texture");
                }

                EnsureFolder(Path.GetDirectoryName(MaterialPath));
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, MaterialPath);
            }

            mat.mainTexture = rt;
            if (mat.HasProperty("_BaseMap"))
            {
                mat.SetTexture("_BaseMap", rt);
            }

            // Flip horizontally so the image reads like a real mirror (left/right reversed).
            mat.mainTextureScale = new Vector2(-1f, 1f);
            mat.mainTextureOffset = new Vector2(1f, 0f);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            return mat;
        }

        private static void ApplyMirrorMaterial(GameObject mirror, Material mat)
        {
            if (mirror == null || mat == null)
            {
                return;
            }

            foreach (Renderer renderer in mirror.GetComponentsInChildren<Renderer>(true))
            {
                renderer.sharedMaterial = mat;
                EditorUtility.SetDirty(renderer);
            }
        }

        private static void BuildUi(out GameObject prompt, out GameObject panel, out Text listText)
        {
            GameObject canvasObject = FindInScene(CanvasName);
            if (canvasObject == null)
            {
                canvasObject = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Canvas canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 50;
                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
            }

            Font font = GetUiFont();

            // Prompt
            prompt = FindChild(canvasObject.transform, "Mirror Prompt");
            if (prompt == null)
            {
                prompt = CreateText("Mirror Prompt", canvasObject.transform, font, 26, TextAnchor.MiddleCenter,
                    new Vector2(0.5f, 0f), new Vector2(0f, 140f), new Vector2(520f, 48f));
                prompt.GetComponent<Text>().text = "Press E  –  Active Augments";
            }

            prompt.SetActive(false);

            // Panel (background + list text)
            panel = FindChild(canvasObject.transform, "Mirror Panel");
            if (panel == null)
            {
                panel = new GameObject("Mirror Panel", typeof(Image));
                panel.transform.SetParent(canvasObject.transform, false);
                Image bg = panel.GetComponent<Image>();
                bg.color = new Color(0.04f, 0.05f, 0.07f, 0.86f);
                RectTransform prt = panel.GetComponent<RectTransform>();
                prt.anchorMin = new Vector2(0.5f, 0.5f);
                prt.anchorMax = new Vector2(0.5f, 0.5f);
                prt.pivot = new Vector2(0.5f, 0.5f);
                prt.anchoredPosition = Vector2.zero;
                prt.sizeDelta = new Vector2(560f, 520f);

                GameObject listObject = CreateText("List", panel.transform, font, 18, TextAnchor.UpperLeft,
                    new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520f, 480f));
                listText = listObject.GetComponent<Text>();
                listText.supportRichText = true;
            }
            else
            {
                Transform list = panel.transform.Find("List");
                listText = list != null ? list.GetComponent<Text>() : panel.GetComponentInChildren<Text>(true);
            }

            panel.SetActive(false);
        }

        private static GameObject CreateText(string name, Transform parent, Font font, int size, TextAnchor anchor,
            Vector2 anchorPivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorPivot;
            rect.anchorMax = anchorPivot;
            rect.pivot = anchorPivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            return go;
        }

        private static Font GetUiFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return font;
        }

        private static void SetRef(SerializedObject so, string property, Object value)
        {
            SerializedProperty prop = so.FindProperty(property);
            if (prop != null && value != null)
            {
                prop.objectReferenceValue = value;
            }
        }

        private static void StripCollider(GameObject go)
        {
            foreach (Collider collider in go.GetComponentsInChildren<Collider>(true))
            {
                Object.DestroyImmediate(collider);
            }
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            if (layer < 0)
            {
                return;
            }

            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }

        private static void EnsureFolder(string folder)
        {
            folder = folder.Replace('\\', '/');
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        private static GameObject FindInScene(string objectName)
        {
            foreach (Transform transform in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (transform.gameObject.scene.IsValid() && transform.name == objectName)
                {
                    return transform.gameObject;
                }
            }

            return null;
        }

        private static GameObject FindChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            return child != null ? child.gameObject : null;
        }
    }
}
