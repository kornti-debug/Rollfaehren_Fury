using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RollfaehrenFury.Editor
{
    public static class UiIconGenerator
    {
        public const string IconFolder = "Assets/UI/Icons";
        private const int Size = 64;
        private static readonly Color Ink = Color.white;

        public static void EnsureIcons()
        {
            EnsureFolder("Assets/UI", "Icons");

            WriteIcon("damage", DrawDamage);
            WriteIcon("fire-rate", DrawFireRate);
            WriteIcon("reload", DrawReload);
            WriteIcon("magazine", DrawMagazine);
            WriteIcon("ricochet", DrawRicochet);
            WriteIcon("refill", DrawRefill);
            WriteIcon("money", DrawMoney);
            WriteIcon("ferry", DrawFerry);
            WriteIcon("player", DrawPlayer);
            WriteIcon("enemies", DrawEnemies);
            WriteIcon("world", DrawWorld);
            WriteIcon("unlock", DrawUnlock);
        }

        public static Sprite Load(string iconName)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{IconFolder}/{iconName}.png");
        }

        private static void WriteIcon(string name, System.Action<Texture2D> draw)
        {
            Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[Size * Size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            texture.SetPixels(pixels);
            draw(texture);
            texture.Apply();

            string path = $"{IconFolder}/{name}.png";
            byte[] bytes = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);

            bool changed = !File.Exists(path) || !BytesEqual(File.ReadAllBytes(path), bytes);
            if (changed)
            {
                File.WriteAllBytes(path, bytes);
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = Size;
            importer.SaveAndReimport();
        }

        private static void DrawDamage(Texture2D texture)
        {
            DrawCircle(texture, new Vector2(0.5f, 0.5f), 0.23f, 0.045f);
            DrawCircle(texture, new Vector2(0.5f, 0.5f), 0.07f, 0.045f);
            DrawLine(texture, new Vector2(0.12f, 0.5f), new Vector2(0.34f, 0.5f), 0.045f);
            DrawLine(texture, new Vector2(0.66f, 0.5f), new Vector2(0.88f, 0.5f), 0.045f);
            DrawLine(texture, new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.34f), 0.045f);
            DrawLine(texture, new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.88f), 0.045f);
        }

        private static void DrawFireRate(Texture2D texture)
        {
            DrawPolyline(texture, new[]
            {
                new Vector2(0.58f, 0.88f),
                new Vector2(0.24f, 0.48f),
                new Vector2(0.47f, 0.48f),
                new Vector2(0.38f, 0.12f),
                new Vector2(0.78f, 0.58f),
                new Vector2(0.54f, 0.58f),
                new Vector2(0.58f, 0.88f)
            }, 0.045f);
        }

        private static void DrawReload(Texture2D texture)
        {
            DrawArc(texture, new Vector2(0.5f, 0.5f), 0.31f, 30f, 300f, 0.045f);
            DrawLine(texture, new Vector2(0.75f, 0.73f), new Vector2(0.84f, 0.73f), 0.045f);
            DrawLine(texture, new Vector2(0.75f, 0.73f), new Vector2(0.77f, 0.84f), 0.045f);
        }

        private static void DrawMagazine(Texture2D texture)
        {
            DrawRoundedRect(texture, new Rect(0.29f, 0.14f, 0.42f, 0.72f), 0.045f);
            DrawLine(texture, new Vector2(0.36f, 0.72f), new Vector2(0.64f, 0.72f), 0.04f);
            DrawLine(texture, new Vector2(0.36f, 0.58f), new Vector2(0.64f, 0.58f), 0.04f);
            DrawLine(texture, new Vector2(0.36f, 0.44f), new Vector2(0.64f, 0.44f), 0.04f);
        }

        private static void DrawRicochet(Texture2D texture)
        {
            DrawLine(texture, new Vector2(0.15f, 0.25f), new Vector2(0.48f, 0.52f), 0.045f);
            DrawLine(texture, new Vector2(0.48f, 0.52f), new Vector2(0.82f, 0.76f), 0.045f);
            DrawLine(texture, new Vector2(0.48f, 0.18f), new Vector2(0.48f, 0.82f), 0.035f);
            DrawLine(texture, new Vector2(0.82f, 0.76f), new Vector2(0.70f, 0.75f), 0.045f);
            DrawLine(texture, new Vector2(0.82f, 0.76f), new Vector2(0.77f, 0.64f), 0.045f);
        }

        private static void DrawRefill(Texture2D texture)
        {
            DrawRoundedRect(texture, new Rect(0.14f, 0.24f, 0.72f, 0.5f), 0.045f);
            DrawLine(texture, new Vector2(0.5f, 0.34f), new Vector2(0.5f, 0.64f), 0.05f);
            DrawLine(texture, new Vector2(0.35f, 0.49f), new Vector2(0.65f, 0.49f), 0.05f);
            DrawLine(texture, new Vector2(0.28f, 0.74f), new Vector2(0.38f, 0.86f), 0.04f);
            DrawLine(texture, new Vector2(0.72f, 0.74f), new Vector2(0.62f, 0.86f), 0.04f);
        }

        private static void DrawMoney(Texture2D texture)
        {
            DrawCircle(texture, new Vector2(0.5f, 0.5f), 0.34f, 0.045f);
            DrawLine(texture, new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.28f), 0.04f);
            DrawPolyline(texture, new[]
            {
                new Vector2(0.64f, 0.64f),
                new Vector2(0.42f, 0.68f),
                new Vector2(0.35f, 0.55f),
                new Vector2(0.62f, 0.45f),
                new Vector2(0.58f, 0.31f),
                new Vector2(0.36f, 0.35f)
            }, 0.04f);
        }

        private static void DrawFerry(Texture2D texture)
        {
            DrawPolyline(texture, new[]
            {
                new Vector2(0.12f, 0.38f),
                new Vector2(0.25f, 0.2f),
                new Vector2(0.75f, 0.2f),
                new Vector2(0.88f, 0.38f),
                new Vector2(0.12f, 0.38f)
            }, 0.045f);
            DrawRoundedRect(texture, new Rect(0.34f, 0.4f, 0.32f, 0.25f), 0.04f);
            DrawLine(texture, new Vector2(0.2f, 0.72f), new Vector2(0.8f, 0.72f), 0.045f);
            DrawArc(texture, new Vector2(0.5f, 0.12f), 0.3f, 20f, 160f, 0.035f);
        }

        private static void DrawPlayer(Texture2D texture)
        {
            DrawCircle(texture, new Vector2(0.5f, 0.68f), 0.14f, 0.045f);
            DrawArc(texture, new Vector2(0.5f, 0.22f), 0.34f, 20f, 160f, 0.05f);
            DrawLine(texture, new Vector2(0.28f, 0.46f), new Vector2(0.72f, 0.46f), 0.045f);
        }

        private static void DrawEnemies(Texture2D texture)
        {
            DrawCircle(texture, new Vector2(0.37f, 0.58f), 0.18f, 0.04f);
            DrawCircle(texture, new Vector2(0.66f, 0.58f), 0.18f, 0.04f);
            DrawLine(texture, new Vector2(0.23f, 0.72f), new Vector2(0.17f, 0.86f), 0.04f);
            DrawLine(texture, new Vector2(0.77f, 0.72f), new Vector2(0.83f, 0.86f), 0.04f);
            DrawArc(texture, new Vector2(0.5f, 0.18f), 0.36f, 18f, 162f, 0.045f);
        }

        private static void DrawWorld(Texture2D texture)
        {
            DrawCircle(texture, new Vector2(0.5f, 0.5f), 0.34f, 0.045f);
            DrawArc(texture, new Vector2(0.5f, 0.5f), 0.18f, 90f, 270f, 0.035f);
            DrawArc(texture, new Vector2(0.5f, 0.5f), 0.18f, -90f, 90f, 0.035f);
            DrawLine(texture, new Vector2(0.18f, 0.5f), new Vector2(0.82f, 0.5f), 0.035f);
        }

        private static void DrawUnlock(Texture2D texture)
        {
            DrawRoundedRect(texture, new Rect(0.25f, 0.16f, 0.5f, 0.42f), 0.045f);
            DrawArc(texture, new Vector2(0.46f, 0.59f), 0.22f, 10f, 185f, 0.045f);
            DrawLine(texture, new Vector2(0.67f, 0.63f), new Vector2(0.84f, 0.63f), 0.045f);
            DrawCircle(texture, new Vector2(0.5f, 0.36f), 0.04f, 0.035f);
        }

        private static void DrawRoundedRect(Texture2D texture, Rect rect, float thickness)
        {
            DrawLine(texture, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin), thickness);
            DrawLine(texture, new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMax, rect.yMax), thickness);
            DrawLine(texture, new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMin, rect.yMax), thickness);
            DrawLine(texture, new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMin, rect.yMin), thickness);
        }

        private static void DrawPolyline(Texture2D texture, IReadOnlyList<Vector2> points, float thickness)
        {
            for (int i = 1; i < points.Count; i++)
            {
                DrawLine(texture, points[i - 1], points[i], thickness);
            }
        }

        private static void DrawArc(Texture2D texture, Vector2 center, float radius, float startDegrees, float endDegrees, float thickness)
        {
            const int segments = 28;
            Vector2 previous = PointOnCircle(center, radius, startDegrees);
            for (int i = 1; i <= segments; i++)
            {
                float angle = Mathf.Lerp(startDegrees, endDegrees, i / (float)segments);
                Vector2 next = PointOnCircle(center, radius, angle);
                DrawLine(texture, previous, next, thickness);
                previous = next;
            }
        }

        private static Vector2 PointOnCircle(Vector2 center, float radius, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return center + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;
        }

        private static void DrawCircle(Texture2D texture, Vector2 center, float radius, float thickness)
        {
            DrawArc(texture, center, radius, 0f, 360f, thickness);
        }

        private static void DrawLine(Texture2D texture, Vector2 from, Vector2 to, float thickness)
        {
            Vector2 a = from * (Size - 1);
            Vector2 b = to * (Size - 1);
            float radius = thickness * Size * 0.5f;
            int minX = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.x, b.x) - radius), 0, Size - 1);
            int maxX = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.x, b.x) + radius), 0, Size - 1);
            int minY = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.y, b.y) - radius), 0, Size - 1);
            int maxY = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.y, b.y) + radius), 0, Size - 1);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float distance = DistanceToSegment(new Vector2(x, y), a, b);
                    if (distance <= radius)
                    {
                        texture.SetPixel(x, y, Ink);
                    }
                }
            }
        }

        private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 delta = b - a;
            float sqrLength = delta.sqrMagnitude;
            if (sqrLength <= Mathf.Epsilon)
            {
                return Vector2.Distance(point, a);
            }

            float t = Mathf.Clamp01(Vector2.Dot(point - a, delta) / sqrLength);
            return Vector2.Distance(point, a + delta * t);
        }

        private static bool BytesEqual(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
