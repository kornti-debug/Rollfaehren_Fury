using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [RequireComponent(typeof(SimpleFPSController))]
    public sealed class PlayerFootsteps : MonoBehaviour
    {
        [SerializeField] private AK.Wwise.Event stepsEvent;
        [SerializeField] private float walkInterval = 0.45f;
        [SerializeField] private float sprintInterval = 0.30f;

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
                if (stepsEvent != null
                    && stepsEvent.IsValid()
                    && AkUnitySoundEngine.IsInitialized())
                {
                    stepsEvent.Post(gameObject);
                }

                stepTimer = fpsController.IsSprinting ? sprintInterval : walkInterval;
            }
        }
    }
}
