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

        private Rigidbody ferryBody;
        private Vector3 crossingStart;
        private Vector3 crossingDestination;
        private float crossingDistance = 1f;
        private float speedMultiplier = 1f;
        private bool atDockA = true;

        public event Action Arrived;

        public bool IsCrossing { get; private set; }
        public bool AtDockA => atDockA;
        public float Progress { get; private set; }

        private void Awake()
        {
            ferryBody = GetComponent<Rigidbody>();
            ferryBody.isKinematic = true;
            ferryBody.useGravity = false;

            if (playerController == null)
            {
                playerController = FindFirstObjectByType<SimpleFPSController>();
            }
        }

        private void FixedUpdate()
        {
            if (!IsCrossing)
            {
                return;
            }

            Vector3 current = ferryBody.position;
            float step = crossingSpeed * speedMultiplier * Time.fixedDeltaTime;
            Vector3 next = Vector3.MoveTowards(current, crossingDestination, step);
            Vector3 delta = next - current;

            ferryBody.MovePosition(next);
            playerController?.MoveWithPlatform(delta);

            float remaining = Vector3.Distance(next, crossingDestination);
            Progress = crossingDistance <= 0.001f
                ? 1f
                : Mathf.Clamp01(1f - remaining / crossingDistance);

            if (remaining <= 0.01f)
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

            crossingStart = ferryBody.position;
            crossingDestination = destination.position;
            crossingDistance = Mathf.Max(0.001f, Vector3.Distance(crossingStart, crossingDestination));
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

            Vector3 delta = dockA.position - transform.position;
            ferryBody.position = dockA.position;
            transform.position = dockA.position;
            playerController?.MoveWithPlatform(delta);
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
    }
}
