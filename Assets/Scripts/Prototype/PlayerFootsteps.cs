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
        [SerializeField, Min(0.5f)] private float surfaceProbeDistance = 2.5f;
        [SerializeField] private LayerMask surfaceMask = ~0;
        [SerializeField] private string defaultSurface = "Wood";
        [SerializeField] private string[] woodObjectTokens = { "Ferry", "Jetty", "Shop Interior" };
        [SerializeField] private string[] grassTerrainLayers = { "NewLayer" };
        [SerializeField] private string[] gravelTerrainLayers =
        {
            "NewLayer 1",
            "NewLayer 2",
            "NewLayer 3"
        };

        private SimpleFPSController fpsController;
        private float stepTimer;

        private void Awake()
        {
            fpsController = GetComponent<SimpleFPSController>();
        }

        private void Update()
        {
            bool isMoving = fpsController.MoveInput.sqrMagnitude > 0.01f
                            && fpsController.IsGrounded
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
                    WwiseAudioRuntime.SetSwitch("SurfaceType", DetectSurface(), gameObject);
                    WwiseAudioRuntime.Post(stepsEventName, gameObject);
                }

                stepTimer = fpsController.IsSprinting ? sprintInterval : walkInterval;
            }
        }

        private string DetectSurface()
        {
            Vector3 origin = transform.position + Vector3.up * 0.25f;
            if (!Physics.Raycast(
                    origin,
                    Vector3.down,
                    out RaycastHit hit,
                    surfaceProbeDistance,
                    surfaceMask,
                    QueryTriggerInteraction.Ignore))
            {
                return defaultSurface;
            }

            Terrain terrain = hit.collider.GetComponent<Terrain>()
                              ?? hit.collider.GetComponentInParent<Terrain>();
            if (terrain != null)
            {
                string layerName = GetDominantTerrainLayer(terrain, hit.point);
                if (Contains(grassTerrainLayers, layerName))
                {
                    return "Grass";
                }

                if (Contains(gravelTerrainLayers, layerName))
                {
                    return "Gravel";
                }
            }

            Transform current = hit.collider.transform;
            while (current != null)
            {
                if (ContainsToken(woodObjectTokens, current.name))
                {
                    return "Wood";
                }

                current = current.parent;
            }

            return defaultSurface;
        }

        private static string GetDominantTerrainLayer(Terrain terrain, Vector3 worldPoint)
        {
            TerrainData data = terrain.terrainData;
            if (data == null || data.alphamapWidth <= 0 || data.alphamapHeight <= 0)
            {
                return string.Empty;
            }

            Vector3 local = worldPoint - terrain.transform.position;
            Vector3 size = data.size;
            int x = Mathf.Clamp(
                Mathf.FloorToInt(local.x / Mathf.Max(0.001f, size.x) * data.alphamapWidth),
                0,
                data.alphamapWidth - 1);
            int z = Mathf.Clamp(
                Mathf.FloorToInt(local.z / Mathf.Max(0.001f, size.z) * data.alphamapHeight),
                0,
                data.alphamapHeight - 1);

            float[,,] weights = data.GetAlphamaps(x, z, 1, 1);
            int dominant = 0;
            float highest = -1f;
            for (int i = 0; i < weights.GetLength(2); i++)
            {
                if (weights[0, 0, i] <= highest)
                {
                    continue;
                }

                highest = weights[0, 0, i];
                dominant = i;
            }

            TerrainLayer[] layers = data.terrainLayers;
            return dominant >= 0 && dominant < layers.Length && layers[dominant] != null
                ? layers[dominant].name
                : string.Empty;
        }

        private static bool Contains(string[] values, string target)
        {
            if (values == null || string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            foreach (string value in values)
            {
                if (string.Equals(value, target, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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
