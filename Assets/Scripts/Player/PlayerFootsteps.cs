using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [RequireComponent(typeof(SimpleFPSController))]
    [RequireComponent(typeof(AkGameObj))]
    public sealed class PlayerFootsteps : MonoBehaviour
    {
        [SerializeField] private string stepsEventName = WwiseAudioNames.PlaySteps;
        [SerializeField] private float walkInterval = 0.45f;
        [SerializeField] private float sprintInterval = 0.30f;
        [SerializeField, Min(0.5f)] private float surfaceProbeDistance = 3.5f;
        [SerializeField, Min(0.01f)] private float surfaceProbeRadius = 0.18f;
        [SerializeField] private LayerMask surfaceMask = ~0;
        [SerializeField] private string defaultSurface = "Gravel";
        [SerializeField] private string woodTag = "Wood";
        [SerializeField] private string[] woodObjectTokens = { "Ferry", "Jetty", "Shop Interior" };
        [SerializeField] private string[] grassTerrainLayerNames = { "NewLayer 4" };

        private SimpleFPSController fpsController;
        private float stepTimer;

        private void Awake()
        {
            fpsController = GetComponent<SimpleFPSController>();
        }

        private void Update()
        {
            bool hasSurface = TryGetSurfaceHit(out RaycastHit surfaceHit);
            bool isMoving = fpsController.MoveInput.sqrMagnitude > 0.01f
                            && hasSurface
                            && fpsController.InputEnabled;

            if (!isMoving)
            {
                stepTimer = 0f;
                return;
            }

            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                if (WwiseAudioRuntime.IsReady)
                {
                    WwiseAudioRuntime.SetSwitch("SurfaceType", DetectSurface(surfaceHit), gameObject);
                    WwiseAudioRuntime.Post(stepsEventName, gameObject);
                }

                stepTimer = fpsController.IsSprinting ? sprintInterval : walkInterval;
            }
        }

        private bool TryGetSurfaceHit(out RaycastHit hit)
        {
            Vector3 origin = transform.position + Vector3.up * 0.25f;
            return Physics.SphereCast(
                    origin,
                    surfaceProbeRadius,
                    Vector3.down,
                    out hit,
                    surfaceProbeDistance,
                    surfaceMask,
                    QueryTriggerInteraction.Ignore);
        }

        private string DetectSurface(RaycastHit surfaceHit)
        {
            Collider surfaceCollider = surfaceHit.collider;
            Transform current = surfaceCollider != null ? surfaceCollider.transform : null;
            while (current != null)
            {
                if ((!string.IsNullOrWhiteSpace(woodTag)
                     && string.Equals(current.tag, woodTag, System.StringComparison.Ordinal))
                    || ContainsToken(woodObjectTokens, current.name))
                {
                    return "Wood";
                }

                current = current.parent;
            }

            Terrain terrain = surfaceCollider != null
                ? surfaceCollider.GetComponentInParent<Terrain>()
                : null;
            if (terrain != null && TryGetDominantTerrainLayer(terrain, surfaceHit.point, out string layerName))
            {
                return ContainsToken(grassTerrainLayerNames, layerName)
                    ? "Grass"
                    : "Gravel";
            }

            return defaultSurface;
        }

        private static bool TryGetDominantTerrainLayer(
            Terrain terrain,
            Vector3 worldPosition,
            out string layerName)
        {
            layerName = string.Empty;
            TerrainData data = terrain != null ? terrain.terrainData : null;
            if (data == null
                || data.alphamapWidth <= 0
                || data.alphamapHeight <= 0
                || data.terrainLayers == null
                || data.terrainLayers.Length == 0)
            {
                return false;
            }

            Vector3 local = worldPosition - terrain.transform.position;
            float normalizedX = data.size.x > 0f ? Mathf.Clamp01(local.x / data.size.x) : 0f;
            float normalizedZ = data.size.z > 0f ? Mathf.Clamp01(local.z / data.size.z) : 0f;
            int mapX = Mathf.Clamp(
                Mathf.RoundToInt(normalizedX * (data.alphamapWidth - 1)),
                0,
                data.alphamapWidth - 1);
            int mapY = Mathf.Clamp(
                Mathf.RoundToInt(normalizedZ * (data.alphamapHeight - 1)),
                0,
                data.alphamapHeight - 1);

            float[,,] weights = data.GetAlphamaps(mapX, mapY, 1, 1);
            int layerCount = Mathf.Min(weights.GetLength(2), data.terrainLayers.Length);
            int dominant = -1;
            float strongest = float.MinValue;
            for (int i = 0; i < layerCount; i++)
            {
                if (weights[0, 0, i] > strongest)
                {
                    strongest = weights[0, 0, i];
                    dominant = i;
                }
            }

            TerrainLayer layer = dominant >= 0 ? data.terrainLayers[dominant] : null;
            if (layer == null)
            {
                return false;
            }

            layerName = layer.name;
            return true;
        }

        private static bool ContainsToken(string[] tokens, string target)
        {
            if (tokens == null || string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            foreach (string token in tokens)
            {
                if (!string.IsNullOrWhiteSpace(token)
                    && target.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
