using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RollfaehrenFury.Prototype
{
    public sealed class RadarHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [Tooltip("The spawner that owns the alive enemy list.")]
        [SerializeField] private EnemySpawner enemySpawner;
        [Tooltip("World-space pivot for the radar (ferry or player transform).")]
        [SerializeField] private Transform centerTransform;
        [Tooltip("RectTransform of the circular radar background panel.")]
        [SerializeField] private RectTransform radarPanel;
        [Tooltip("Thin Image used as the rotating sweep line (pivot at its bottom-center).")]
        [SerializeField] private RectTransform sweepLine;

        [Header("Radar")]
        [Tooltip("World-space radius (metres) that maps to the edge of the radar circle.")]
        [SerializeField, Min(10f)] private float worldRadius = 100f;
        [Tooltip("Degrees per second the sweep line rotates.")]
        [SerializeField, Min(0f)] private float sweepSpeed = 60f;
        [Tooltip("When true the radar rotates with the ferry so forward is always up. When false it is world-north up.")]
        [SerializeField] private bool rotateWithFerry = true;
        [Tooltip("Enemies beyond worldRadius are clamped to the radar edge instead of hidden.")]
        [SerializeField] private bool clampOutOfRange = true;

        [Header("Dot appearance")]
        [SerializeField] private Color dotColor = Color.red;
        [SerializeField, Min(2f)] private float dotSize = 8f;

        private readonly List<RectTransform> dotPool = new List<RectTransform>();
        private float sweepAngle;

        private void Update()
        {
            bool isCrossing = gameManager != null && gameManager.State == PrototypeGameState.Playing;
            if (isCrossing)
                sweepAngle = (sweepAngle + sweepSpeed * Time.deltaTime) % 360f;
            if (sweepLine != null)
                sweepLine.localEulerAngles = new Vector3(0f, 0f, -sweepAngle);

            UpdateDots();
        }

        private void UpdateDots()
        {
            if (centerTransform == null || radarPanel == null)
                return;

            IReadOnlyList<SimpleEnemy> enemies = enemySpawner != null ? enemySpawner.AliveEnemies : null;
            int count = enemies != null ? enemies.Count : 0;

            GrowPool(count);

            float radarRadius = radarPanel.rect.width * 0.5f;
            float scale = radarRadius / worldRadius;

            // Build a rotation that maps world-XZ to radar-XY.
            // When rotateWithFerry is on the ferry's yaw is removed so forward is always "up" on radar.
            float yawDeg = rotateWithFerry ? centerTransform.eulerAngles.y : 0f;
            float yawRad = yawDeg * Mathf.Deg2Rad;
            float cos = Mathf.Cos(yawRad);
            float sin = Mathf.Sin(yawRad);

            for (int i = 0; i < dotPool.Count; i++)
            {
                RectTransform dot = dotPool[i];

                if (i >= count || enemies[i] == null)
                {
                    dot.gameObject.SetActive(false);
                    continue;
                }

                Vector3 offset = enemies[i].transform.position - centerTransform.position;

                // Rotate offset into ferry-local XZ space (world X→radar X, world Z→radar Y).
                float wx = offset.x;
                float wz = offset.z;
                Vector2 local = new Vector2(
                    cos * wx - sin * wz,
                    sin * wx + cos * wz);

                if (!clampOutOfRange && local.magnitude > worldRadius)
                {
                    dot.gameObject.SetActive(false);
                    continue;
                }

                Vector2 pos = local * scale;
                if (clampOutOfRange)
                    pos = Vector2.ClampMagnitude(pos, radarRadius - dotSize * 0.5f);

                dot.anchoredPosition = pos;
                dot.gameObject.SetActive(true);
            }
        }

        private void GrowPool(int needed)
        {
            while (dotPool.Count < needed)
            {
                GameObject go = new GameObject("RadarDot", typeof(Image));
                go.transform.SetParent(radarPanel, false);
                Image img = go.GetComponent<Image>();
                img.color = dotColor;
                RectTransform rt = img.rectTransform;
                rt.sizeDelta = new Vector2(dotSize, dotSize);
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                dotPool.Add(rt);
            }
        }
    }
}
