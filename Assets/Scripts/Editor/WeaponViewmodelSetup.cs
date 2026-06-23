using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// One-shot helper that wires the Easy Weapons gun models onto your existing weapons. For each
    /// matching weapon it instantiates the model under the fire camera and adds a configured
    /// <see cref="WeaponVisuals"/> (model + muzzle point + muzzle flash + hit FX). It is additive and
    /// idempotent: it skips any weapon that already has a WeaponVisuals and never deletes or moves
    /// anything else. This is NOT the prototype scene builder — it only touches weapon viewmodels.
    ///
    /// Run AFTER converting the Easy Weapons materials to URP, then nudge each "&lt;weapon&gt; Viewmodel"
    /// transform so it sits right in view, and drag in the fire/reload sounds.
    /// </summary>
    public static class WeaponViewmodelSetup
    {
        private static readonly Dictionary<string, string> ModelByWeaponName = new Dictionary<string, string>
        {
            { "Pistol", "Assets/Easy Weapons/Extra/Pistol_Model/pistol_model.fbx" },
            { "Shotgun", "Assets/Easy Weapons/Extra/Shotgun_Model/shotgun_model.fbx" },
            { "Assault Rifle", "Assets/Easy Weapons/Extra/M4_Model/M4_model.fbx" },
        };

        private const string MuzzleFlashPath = "Assets/Easy Weapons/Prefabs/Effects/Muzzle Flashes/Muzzle_Flash_1.prefab";
        private const string HitEffectPath = "Assets/Easy Weapons/Prefabs/Effects/Hit Effects/Sparks.prefab";

        // The textured (URP-converted) material per gun. The raw FBX imports with a blank "No Name"
        // material, so we force the proper textured one onto the model's renderers.
        private static readonly Dictionary<string, string> MaterialByWeaponName = new Dictionary<string, string>
        {
            { "Pistol", "Assets/Easy Weapons/Extra/Pistol_Model/Materials/Pistol.mat" },
            { "Shotgun", "Assets/Easy Weapons/Extra/Shotgun_Model/Materials/shotgun.mat" },
            { "Assault Rifle", "Assets/Easy Weapons/Extra/M4_Model/Materials/m.mat" },
        };

        // Where each gun spawns relative to the camera: X = right, Y = up/down (negative = lower),
        // Z = forward. Tweak this one line to move all viewmodels at once.
        private static readonly Vector3 DefaultViewmodelPosition = new Vector3(0.18f, -0.25f, 0.4f);

        [MenuItem("Tools/Rollfaehren Fury/Setup Weapon Viewmodels")]
        public static void Setup()
        {
            WeaponSystem system = Object.FindFirstObjectByType<WeaponSystem>();
            if (system == null)
            {
                Debug.LogError("[Viewmodel Setup] No WeaponSystem in the open scene — open Main.unity and try again.");
                return;
            }

            Camera camera = ResolveFireCamera(system);
            if (camera == null)
            {
                Debug.LogError("[Viewmodel Setup] No weapon camera found. Assign WeaponSystem.fireCamera or tag the player camera MainCamera.");
                return;
            }

            GameObject muzzleFlash = AssetDatabase.LoadAssetAtPath<GameObject>(MuzzleFlashPath);
            GameObject hitEffect = AssetDatabase.LoadAssetAtPath<GameObject>(HitEffectPath);

            int created = 0;
            int retextured = 0;
            int skipped = 0;
            for (int i = 0; i < system.WeaponCount; i++)
            {
                Weapon weapon = system.WeaponAt(i);
                if (weapon == null)
                {
                    continue;
                }

                if (!ModelByWeaponName.TryGetValue(weapon.DisplayName, out string modelPath))
                {
                    skipped++;
                    continue; // e.g. Harpoon — no Easy Weapons model
                }

                Material texturedMaterial = MaterialByWeaponName.TryGetValue(weapon.DisplayName, out string materialPath)
                    ? AssetDatabase.LoadAssetAtPath<Material>(materialPath)
                    : null;

                // Already set up: just (re)apply the textured material to its existing model and move on.
                WeaponVisuals existing = weapon.GetComponent<WeaponVisuals>();
                if (existing != null)
                {
                    GameObject existingModel = new SerializedObject(existing).FindProperty("modelRoot").objectReferenceValue as GameObject;
                    if (ApplyMaterial(existingModel, texturedMaterial))
                    {
                        retextured++;
                    }
                    else
                    {
                        skipped++;
                    }

                    continue;
                }

                GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                if (modelAsset == null)
                {
                    Debug.LogWarning($"[Viewmodel Setup] Model not found at {modelPath} (did the import path differ?). Skipping {weapon.DisplayName}.");
                    skipped++;
                    continue;
                }

                // Instantiate the model under the camera with a rough default pose (you'll nudge it).
                GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset, camera.transform);
                model.name = $"{weapon.DisplayName} Viewmodel";
                model.transform.localPosition = DefaultViewmodelPosition;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;

                // Viewmodels are cosmetic ONLY: strip any physics and put them on Ignore Raycast so a
                // model can never block the aim raycast or shove the player around, no matter its pose/scale.
                foreach (Collider collider in model.GetComponentsInChildren<Collider>(true))
                {
                    Object.DestroyImmediate(collider);
                }
                foreach (Rigidbody body in model.GetComponentsInChildren<Rigidbody>(true))
                {
                    Object.DestroyImmediate(body);
                }
                SetLayerRecursive(model, LayerMask.NameToLayer("Ignore Raycast"));
                ApplyMaterial(model, texturedMaterial);

                GameObject muzzle = new GameObject("Muzzle");
                muzzle.transform.SetParent(model.transform, false);
                muzzle.transform.localPosition = new Vector3(0f, 0f, 0.5f);

                WeaponVisuals visuals = weapon.gameObject.AddComponent<WeaponVisuals>();
                SerializedObject so = new SerializedObject(visuals);
                so.FindProperty("weapon").objectReferenceValue = weapon;
                so.FindProperty("modelRoot").objectReferenceValue = model;
                so.FindProperty("muzzlePoint").objectReferenceValue = muzzle.transform;
                if (muzzleFlash != null)
                {
                    so.FindProperty("muzzleFlashPrefab").objectReferenceValue = muzzleFlash;
                }
                if (hitEffect != null)
                {
                    so.FindProperty("hitEffectPrefab").objectReferenceValue = hitEffect;
                }
                so.ApplyModifiedPropertiesWithoutUndo();

                created++;
            }

            EditorSceneManager.MarkSceneDirty(system.gameObject.scene);
            Debug.Log($"[Viewmodel Setup] Done — wired {created} new, re-textured {retextured} existing, skipped {skipped}. " +
                      "Nudge each '<weapon> Viewmodel' under the camera to sit right in view, " +
                      "drag fire/reload sounds onto each WeaponVisuals, and save the scene.");
        }

        // Forces a single material onto every renderer slot of the model. Returns true if it touched anything.
        private static bool ApplyMaterial(GameObject model, Material material)
        {
            if (model == null || material == null)
            {
                return false;
            }

            bool applied = false;
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                int slots = Mathf.Max(1, renderer.sharedMaterials.Length);
                Material[] materials = new Material[slots];
                for (int i = 0; i < slots; i++)
                {
                    materials[i] = material;
                }

                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
                applied = true;
            }

            return applied;
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

        private static Camera ResolveFireCamera(WeaponSystem system)
        {
            SerializedObject so = new SerializedObject(system);
            SerializedProperty property = so.FindProperty("fireCamera");
            if (property != null && property.objectReferenceValue is Camera camera && camera != null)
            {
                return camera;
            }

            return Camera.main;
        }
    }
}
