using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [RequireComponent(typeof(AkGameObj))]
    public sealed class FerryAudio : MonoBehaviour
    {
        [SerializeField] private FerryController ferry;
        [SerializeField, Min(0f)] private float rampUpDuration = 2f;
        [SerializeField, Min(0f)] private float rampDownDuration = 1.5f;

        private float currentSpeed;
        private bool initializedLoops;
        private bool wasCrossing;

        private void Awake()
        {
            if (ferry == null)
            {
                ferry = GetComponent<FerryController>();
            }
        }

        private void Update()
        {
            if (ferry == null || !WwiseAudioRuntime.IsReady)
            {
                return;
            }

            if (!initializedLoops)
            {
                initializedLoops = true;
                wasCrossing = ferry.IsCrossing;
                if (wasCrossing)
                {
                    EnterMovingAudio(false);
                }
                else
                {
                    EnterDockedAudio(false);
                }
            }
            else if (ferry.IsCrossing != wasCrossing)
            {
                wasCrossing = ferry.IsCrossing;
                if (wasCrossing)
                {
                    EnterMovingAudio(true);
                }
                else
                {
                    EnterDockedAudio(true);
                }
            }

            float target = ferry.IsCrossing ? 100f : 0f;
            float duration = ferry.IsCrossing ? rampUpDuration : rampDownDuration;
            float changeRate = duration <= 0.001f ? 1000f : 100f / duration;
            currentSpeed = Mathf.MoveTowards(currentSpeed, target, changeRate * Time.deltaTime);
            WwiseAudioRuntime.SetRtpc("BoatSpeed", currentSpeed, gameObject);
        }

        private void OnDisable()
        {
            StopAllLoops();
            initializedLoops = false;
            currentSpeed = 0f;
        }

        private void EnterMovingAudio(bool playSteering)
        {
            WwiseAudioRuntime.Post(WwiseAudioNames.StopBoatStanding, gameObject);
            WwiseAudioRuntime.Post(WwiseAudioNames.PlayBoatEngine, gameObject);
            WwiseAudioRuntime.Post(WwiseAudioNames.PlayBoatMoving, gameObject);
            if (playSteering)
            {
                WwiseAudioRuntime.Post(WwiseAudioNames.PlayBoatSteering, gameObject);
            }
        }

        private void EnterDockedAudio(bool playSteering)
        {
            WwiseAudioRuntime.Post(WwiseAudioNames.StopBoatEngine, gameObject);
            WwiseAudioRuntime.Post(WwiseAudioNames.StopBoatMoving, gameObject);
            WwiseAudioRuntime.Post(WwiseAudioNames.PlayBoatStanding, gameObject);
            WwiseAudioRuntime.SetRtpc("BoatSpeed", 0f, gameObject);
            if (playSteering)
            {
                WwiseAudioRuntime.Post(WwiseAudioNames.PlayBoatSteering, gameObject);
            }
        }

        private void StopAllLoops()
        {
            if (!WwiseAudioRuntime.IsReady)
            {
                return;
            }

            WwiseAudioRuntime.Post(WwiseAudioNames.StopBoatEngine, gameObject);
            WwiseAudioRuntime.Post(WwiseAudioNames.StopBoatMoving, gameObject);
            WwiseAudioRuntime.Post(WwiseAudioNames.StopBoatStanding, gameObject);
        }
    }
}
