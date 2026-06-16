using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RollfaehrenFury.Editor
{
    public static class EnvironmentSceneTools
    {
        private const string MainScenePath = "Assets/Scenes/Main.unity";
        private const string WaterMaterialPath = "Assets/Materials/RiverWater_URP.mat";
        private const string WaterShaderName = "Rollfaehren Fury/URP/Scrolling River Water";
        private const string WaveTexturePath = "Assets/Assetstore/BasicToonWaterShader/Texture/wave1.png";

        [MenuItem("Rollfaehren Fury/Environment/Setup URP River Water")]
        public static void SetupRiverWater()
        {
            Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            Material waterMaterial = EnsureWaterMaterial();

            GameObject placeholder = GameObject.Find("River Placeholder");
            GameObject waterSurface = GameObject.Find("River Water Surface");
            if (waterSurface == null)
            {
                waterSurface = GameObject.CreatePrimitive(PrimitiveType.Plane);
                waterSurface.name = "River Water Surface";
            }

            if (placeholder != null)
            {
                waterSurface.transform.position = placeholder.transform.position + Vector3.up * 0.08f;
                waterSurface.transform.rotation = Quaternion.identity;
                waterSurface.transform.localScale = new Vector3(
                    Mathf.Max(1f, placeholder.transform.localScale.x / 10f),
                    1f,
                    Mathf.Max(1f, placeholder.transform.localScale.z / 10f));

                Renderer placeholderRenderer = placeholder.GetComponent<Renderer>();
                if (placeholderRenderer != null)
                {
                    placeholderRenderer.enabled = false;
                }

                Collider placeholderCollider = placeholder.GetComponent<Collider>();
                if (placeholderCollider != null)
                {
                    placeholderCollider.enabled = false;
                }

                if (waterSurface.transform.parent == null)
                {
                    waterSurface.transform.SetParent(placeholder.transform.parent, true);
                }
            }
            else
            {
                waterSurface.transform.position = new Vector3(500f, 3f, 500f);
                waterSurface.transform.rotation = Quaternion.identity;
                waterSurface.transform.localScale = new Vector3(55f, 1f, 95f);
            }

            Renderer waterRenderer = waterSurface.GetComponent<Renderer>();
            if (waterRenderer != null)
            {
                waterRenderer.sharedMaterial = waterMaterial;
                EditorUtility.SetDirty(waterRenderer);
            }

            Collider waterCollider = waterSurface.GetComponent<Collider>();
            if (waterCollider != null)
            {
                Object.DestroyImmediate(waterCollider);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = waterSurface;
            Debug.Log("URP river water setup complete. Use the River Water Surface object to tune size/height.");
        }

        public static void SetupRiverWaterFromCommandLine()
        {
            SetupRiverWater();
        }

        private static Material EnsureWaterMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(WaterMaterialPath);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find(WaterShaderName);
            if (shader == null)
            {
                throw new MissingReferenceException($"Shader '{WaterShaderName}' was not found.");
            }

            material = new Material(shader)
            {
                name = "RiverWater_URP",
                renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent
            };

            Texture2D waveTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(WaveTexturePath);
            if (waveTexture != null)
            {
                material.SetTexture("_WaveTex", waveTexture);
            }

            material.SetColor("_BaseColor", new Color(0.04f, 0.36f, 0.55f, 0.72f));
            material.SetColor("_FoamColor", new Color(0.84f, 0.98f, 1f, 1f));
            material.SetFloat("_WaveTiling", 14f);
            material.SetFloat("_WaveStrength", 0.42f);
            material.SetFloat("_Alpha", 0.72f);

            AssetDatabase.CreateAsset(material, WaterMaterialPath);
            return material;
        }
    }
}
