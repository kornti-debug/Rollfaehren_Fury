using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [RequireComponent(typeof(SimpleEnemy))]
    [RequireComponent(typeof(AkGameObj))]
    public sealed class EnemyMovementAudio : MonoBehaviour
    {
        private SimpleEnemy enemy;
        private uint movementPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
        private uint divePlayingId     = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
        private bool divePosted;

        private void Awake()
        {
            enemy = GetComponent<SimpleEnemy>();
        }

        private void Update()
        {
            if (enemy == null || !WwiseAudioRuntime.IsReady)
                return;

            if (enemy.MovementMode == EnemyMovementMode.Flying)
            {
                // Post flap loop once on spawn.
                if (movementPlayingId == AkUnitySoundEngine.AK_INVALID_PLAYING_ID)
                    movementPlayingId = WwiseAudioRuntime.Post(WwiseAudioNames.PlayPigeonMovement, gameObject);

                // When dive commits: stop flap, fire stuka one-shot.
                if (enemy.IsDiving && !divePosted)
                {
                    divePosted = true;
                    WwiseAudioRuntime.StopPlaying(ref movementPlayingId);
                    divePlayingId = WwiseAudioRuntime.Post(WwiseAudioNames.PlayPigeonDive, gameObject);
                }
            }
            // Fish swim sound commented out — Geiger covers it.
            // else if (movementPlayingId == AkUnitySoundEngine.AK_INVALID_PLAYING_ID)
            //     movementPlayingId = WwiseAudioRuntime.Post(WwiseAudioNames.PlayFishMovement, gameObject);
        }

        private void OnDisable()
        {
            WwiseAudioRuntime.StopPlaying(ref movementPlayingId);
            WwiseAudioRuntime.StopPlaying(ref divePlayingId);
        }

    }
}
