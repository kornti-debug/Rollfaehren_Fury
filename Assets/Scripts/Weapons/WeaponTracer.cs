using System.Collections.Generic;
using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Placeholder shot visual: draws a brief line from the muzzle to each hit
    /// point so hitscan/spread shots are visible in the Game view while there are
    /// no weapon or projectile assets yet. Pooled LineRenderers, no allocations per shot.
    /// </summary>
    public sealed class WeaponTracer : MonoBehaviour
    {
        [SerializeField] private float duration = 0.05f;
        [SerializeField] private float width = 0.03f;
        [SerializeField] private Color color = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField, Min(1)] private int poolSize = 16;

        private readonly List<LineRenderer> pool = new List<LineRenderer>();
        private readonly List<float> hideTimes = new List<float>();
        private Material lineMaterial;
        private int nextIndex;

        private void Awake()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            lineMaterial = new Material(shader);

            for (int i = 0; i < Mathf.Max(1, poolSize); i++)
            {
                pool.Add(CreateLine());
                hideTimes.Add(0f);
            }
        }

        private void Update()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i].enabled && Time.time >= hideTimes[i])
                {
                    pool[i].enabled = false;
                }
            }
        }

        public void Show(Vector3 start, Vector3 end)
        {
            if (pool.Count == 0)
            {
                return;
            }

            int index = nextIndex;
            nextIndex = (nextIndex + 1) % pool.Count;

            LineRenderer line = pool[index];
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.enabled = true;
            hideTimes[index] = Time.time + duration;
        }

        private LineRenderer CreateLine()
        {
            GameObject lineObject = new GameObject("Tracer");
            lineObject.transform.SetParent(transform, false);

            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.material = lineMaterial;
            line.startColor = color;
            line.endColor = color;
            line.startWidth = width;
            line.endWidth = width;
            line.numCapVertices = 0;
            line.useWorldSpace = true;
            line.textureMode = LineTextureMode.Stretch;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }
    }
}
