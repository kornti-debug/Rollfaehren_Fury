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
        private uint enginePlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
        private uint movingWaterPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
        private uint standingWaterPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;

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
            WwiseAudioRuntime.StopPlaying(ref standingWaterPlayingId);
            enginePlayingId = StartLoop(WwiseAudioNames.PlayBoatEngine, enginePlayingId);
            movingWaterPlayingId = StartLoop(WwiseAudioNames.PlayBoatMoving, movingWaterPlayingId);
            if (playSteering)
            {
                WwiseAudioRuntime.Post(WwiseAudioNames.PlayBoatSteering, gameObject);
            }
        }

        private void EnterDockedAudio(bool playSteering)
        {
            WwiseAudioRuntime.StopPlaying(ref enginePlayingId);
            WwiseAudioRuntime.StopPlaying(ref movingWaterPlayingId);
            standingWaterPlayingId = StartLoop(WwiseAudioNames.PlayBoatStanding, standingWaterPlayingId);
            WwiseAudioRuntime.SetRtpc("BoatSpeed", 0f, gameObject);
            if (playSteering)
            {
                WwiseAudioRuntime.Post(WwiseAudioNames.PlayBoatSteering, gameObject);
            }
        }

        private void StopAllLoops()
        {
            WwiseAudioRuntime.StopPlaying(ref enginePlayingId);
            WwiseAudioRuntime.StopPlaying(ref movingWaterPlayingId);
            WwiseAudioRuntime.StopPlaying(ref standingWaterPlayingId);
        }

        private uint StartLoop(string eventName, uint currentPlayingId)
        {
            return currentPlayingId != AkUnitySoundEngine.AK_INVALID_PLAYING_ID
                ? currentPlayingId
                : WwiseAudioRuntime.Post(eventName, gameObject);
        }
    }
}
