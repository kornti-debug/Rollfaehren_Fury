using System;
using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class FerryController : MonoBehaviour
    {
        [SerializeField] private Transform dockA;
        [SerializeField] private Transform dockB;
        [SerializeField] private SimpleFPSController playerController;
        [SerializeField, Min(0.1f)] private float crossingSpeed = 12f;
        [SerializeField, Min(0f)] private float departureDistance = 35f;
        [SerializeField, Range(16, 128)] private int routeSamples = 64;

        private Rigidbody ferryBody;
        private Vector3 routeStart;
        private Vector3 routeStartHandle;
        private Vector3 routeEndHandle;
        private Vector3 routeEnd;
        private Quaternion routeEndRotation;
        private float[] routeDistances;
        private float routeLength = 1f;
        private float traveledDistance;
        private float speedMultiplier = 1f;
        private bool atDockA = true;
        private bool capturedDeckOffset;
        private Vector3 playerDeckLocalOffset;
        private Quaternion playerDeckLocalRotation = Quaternion.identity;

        public event Action Arrived;

        public bool IsCrossing { get; private set; }
        public bool AtDockA => atDockA;
        public float Progress { get; private set; }

        // Current travel speed / velocity while crossing (forward is the route tangent).
        public float CurrentSpeed => IsCrossing ? crossingSpeed * speedMultiplier : 0f;
        public Vector3 Velocity => IsCrossing ? transform.forward * CurrentSpeed : Vector3.zero;

        private void Awake()
        {
            ferryBody = GetComponent<Rigidbody>();
            ferryBody.isKinematic = true;
            ferryBody.useGravity = false;
            ferryBody.interpolation = RigidbodyInterpolation.None;

            if (playerController == null)
            {
                playerController = FindFirstObjectByType<SimpleFPSController>();
            }
        }

        private void Start()
        {
            CapturePlayerDeckOffset();
        }

        // Remembers where the player stands on the deck relative to the ferry, so a restart can
        // teleport them back onto the boat instead of leaving them where the ferry was destroyed.
        private void CapturePlayerDeckOffset()
        {
            if (playerController == null || capturedDeckOffset)
            {
                return;
            }

            Quaternion inverse = Quaternion.Inverse(transform.rotation);
            playerDeckLocalOffset = inverse * (playerController.transform.position - transform.position);
            playerDeckLocalRotation = inverse * playerController.transform.rotation;
            capturedDeckOffset = true;
        }

        private void LateUpdate()
        {
            if (!IsCrossing)
            {
                return;
            }

            Vector3 previousPosition = transform.position;
            Quaternion previousRotation = transform.rotation;

            traveledDistance = Mathf.Min(
                routeLength,
                traveledDistance + crossingSpeed * speedMultiplier * Time.deltaTime);
            float routeT = GetRouteParameter(traveledDistance);
            Vector3 nextPosition = EvaluateRoute(routeT);
            Vector3 tangent = EvaluateRouteTangent(routeT);
            Quaternion nextRotation = tangent.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(tangent.normalized, Vector3.up)
                : previousRotation;

            if (traveledDistance >= routeLength)
            {
                nextPosition = routeEnd;
                nextRotation = routeEndRotation;
            }

            transform.SetPositionAndRotation(nextPosition, nextRotation);
            Physics.SyncTransforms();
            playerController?.MoveWithPlatform(previousPosition, previousRotation, nextPosition, nextRotation);

            Progress = routeLength <= 0.001f
                ? 1f
                : Mathf.Clamp01(traveledDistance / routeLength);

            if (traveledDistance >= routeLength)
            {
                FinishCrossing();
            }
        }

        public void Configure(Transform firstDock, Transform secondDock, SimpleFPSController player)
        {
            dockA = firstDock;
            dockB = secondDock;
            playerController = player;
        }

        public bool BeginCrossing()
        {
            Transform destination = atDockA ? dockB : dockA;
            if (destination == null || IsCrossing)
            {
                return false;
            }

            BuildRoute(transform, destination);
            traveledDistance = 0f;
            Progress = 0f;
            IsCrossing = true;
            return true;
        }

        public void ResetToDockA()
        {
            IsCrossing = false;
            atDockA = true;
            Progress = 0f;
            speedMultiplier = 1f;

            if (dockA == null)
            {
                return;
            }

            CapturePlayerDeckOffset(); // first reset captures the offset relative to the current pose

            transform.SetPositionAndRotation(dockA.position, dockA.rotation);
            Physics.SyncTransforms();

            // Teleport (collision-free) the player back onto the deck. MoveWithPlatform here would
            // run a CharacterController.Move across the whole reset distance and snag on geometry.
            if (playerController != null && capturedDeckOffset)
            {
                Vector3 deckPosition = dockA.position + dockA.rotation * playerDeckLocalOffset;
                Quaternion deckRotation = dockA.rotation * playerDeckLocalRotation;
                playerController.Teleport(deckPosition, deckRotation);
            }
        }

        public void Stop()
        {
            IsCrossing = false;
        }

        public void MultiplySpeed(float multiplier)
        {
            speedMultiplier = Mathf.Max(0.1f, speedMultiplier * multiplier);
        }

        private void FinishCrossing()
        {
            IsCrossing = false;
            Progress = 1f;
            atDockA = !atDockA;
            Arrived?.Invoke();
        }

        private void BuildRoute(Transform start, Transform destination)
        {
            routeStart = start.position;
            routeEnd = destination.position;
            routeEndRotation = destination.rotation;
            routeStartHandle = routeStart + start.forward * departureDistance;
            routeEndHandle = routeEnd - destination.forward * departureDistance;

            int sampleCount = Mathf.Clamp(routeSamples, 16, 128);
            routeDistances = new float[sampleCount + 1];
            Vector3 previous = routeStart;
            float distance = 0f;

            for (int i = 1; i <= sampleCount; i++)
            {
                float t = i / (float)sampleCount;
                Vector3 point = EvaluateRoute(t);
                distance += Vector3.Distance(previous, point);
                routeDistances[i] = distance;
                previous = point;
            }

            routeLength = Mathf.Max(0.001f, distance);
        }

        private float GetRouteParameter(float distance)
        {
            if (routeDistances == null || routeDistances.Length < 2)
            {
                return Mathf.Clamp01(distance / routeLength);
            }

            int lastIndex = routeDistances.Length - 1;
            for (int i = 1; i <= lastIndex; i++)
            {
                if (distance > routeDistances[i])
                {
                    continue;
                }

                float segmentStart = routeDistances[i - 1];
                float segmentLength = Mathf.Max(0.0001f, routeDistances[i] - segmentStart);
                float segmentT = (distance - segmentStart) / segmentLength;
                return ((i - 1) + segmentT) / lastIndex;
            }

            return 1f;
        }

        private Vector3 EvaluateRoute(float t)
        {
            float inverse = 1f - t;
            return inverse * inverse * inverse * routeStart
                + 3f * inverse * inverse * t * routeStartHandle
                + 3f * inverse * t * t * routeEndHandle
                + t * t * t * routeEnd;
        }

        private Vector3 EvaluateRouteTangent(float t)
        {
            float inverse = 1f - t;
            return 3f * inverse * inverse * (routeStartHandle - routeStart)
                + 6f * inverse * t * (routeEndHandle - routeStartHandle)
                + 3f * t * t * (routeEnd - routeEndHandle);
        }
    }
}
