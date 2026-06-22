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
                    WwiseAudioRuntime.SetSwitch("SurfaceType", DetectSurface(surfaceHit.collider), gameObject);
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

        private string DetectSurface(Collider surfaceCollider)
        {
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

            return defaultSurface;
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
