using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    public sealed class FerryAudio : MonoBehaviour
    {
        [SerializeField] private FerryController ferry;
        [SerializeField] private AK.Wwise.Event playEngineEvent;
        [SerializeField, Min(0f)] private float rampUpDuration = 2f;
        [SerializeField, Min(0f)] private float rampDownDuration = 1.5f;

        private float currentSpeed;
        private Vector3 previousPosition;

        private void Awake()
        {
            if (ferry == null)
                ferry = GetComponent<FerryController>();

            previousPosition = transform.position;
        }

        private void Start()
        {
            playEngineEvent?.Post(gameObject);
        }

        private void Update()
        {
            float measuredSpeed = Vector3.Distance(transform.position, previousPosition) / Time.deltaTime;
            previousPosition = transform.position;

            float target = ferry.IsCrossing ? 100f : 0f;
            float duration = ferry.IsCrossing ? rampUpDuration : rampDownDuration;

            currentSpeed = Mathf.MoveTowards(currentSpeed, target, 100f / duration * Time.deltaTime);

            AkSoundEngine.SetRTPCValue("BoatSpeed", currentSpeed, gameObject);

            _ = measuredSpeed;
        }
    }
}
