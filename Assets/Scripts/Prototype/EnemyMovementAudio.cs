using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [RequireComponent(typeof(SimpleEnemy))]
    [RequireComponent(typeof(AkGameObj))]
    public sealed class EnemyMovementAudio : MonoBehaviour
    {
        private SimpleEnemy enemy;
        private uint movementPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;

        private void Awake()
        {
            enemy = GetComponent<SimpleEnemy>();
        }

        private void Update()
        {
            if (movementPlayingId != AkUnitySoundEngine.AK_INVALID_PLAYING_ID
                || enemy == null
                || !WwiseAudioRuntime.IsReady)
            {
                return;
            }

            movementPlayingId = WwiseAudioRuntime.Post(GetPlayEvent(), gameObject);
        }

        private void OnDisable()
        {
            WwiseAudioRuntime.StopPlaying(ref movementPlayingId);
        }

        private string GetPlayEvent()
        {
            return enemy.MovementMode == EnemyMovementMode.Flying
                ? WwiseAudioNames.PlayPigeonMovement
                : WwiseAudioNames.PlayFishMovement;
        }

    }
}
