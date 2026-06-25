using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    public sealed class FishGeigerAudio : MonoBehaviour
    {
        [Header("Distance Thresholds (metres)")]
        [SerializeField, Min(0f)] private float closeRange  = 25f;
        [SerializeField, Min(0f)] private float mediumRange = 50f;

        [Header("Wwise")]
        [SerializeField] private string switchGroup = "FishProximity";
        [SerializeField] private string switchClose = "Close";
        [SerializeField] private string switchMedium = "Medium";
        [SerializeField] private string switchFar = "Far";

        private Transform playerTransform;
        private uint playingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
        private string currentSwitch = string.Empty;

        private void Awake()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        private void Update()
        {
            // Post the sound once Wwise is ready, same pattern as EnemyMovementAudio.
            if (playingId == AkUnitySoundEngine.AK_INVALID_PLAYING_ID && WwiseAudioRuntime.IsReady)
                playingId = WwiseAudioRuntime.Post(WwiseAudioNames.PlayFishRadioactive, gameObject);

            if (playerTransform == null)
                return;

            float dist = Vector3.Distance(transform.position, playerTransform.position);
            string target = dist <= closeRange ? switchClose
                          : dist <= mediumRange ? switchMedium
                          : switchFar;

            if (target == currentSwitch)
                return;

            currentSwitch = target;
            AkSoundEngine.SetSwitch(switchGroup, target, gameObject);
        }

        private void OnDisable()
        {
            WwiseAudioRuntime.StopPlaying(ref playingId);
        }
    }
}
