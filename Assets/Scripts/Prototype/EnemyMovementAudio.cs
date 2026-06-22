using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [RequireComponent(typeof(SimpleEnemy))]
    [RequireComponent(typeof(AkGameObj))]
    public sealed class EnemyMovementAudio : MonoBehaviour
    {
        private SimpleEnemy enemy;
        private bool movementPlaying;

        private void Awake()
        {
            enemy = GetComponent<SimpleEnemy>();
        }

        private void Update()
        {
            if (movementPlaying || enemy == null || !WwiseAudioRuntime.IsReady)
            {
                return;
            }

            WwiseAudioRuntime.Post(GetPlayEvent(), gameObject);
            movementPlaying = true;
        }

        private void OnDisable()
        {
            if (!movementPlaying)
            {
                return;
            }

            WwiseAudioRuntime.Post(GetStopEvent(), gameObject);
            movementPlaying = false;
        }

        private string GetPlayEvent()
        {
            return enemy.MovementMode == EnemyMovementMode.Flying
                ? WwiseAudioNames.PlayPigeonMovement
                : WwiseAudioNames.PlayFishMovement;
        }

        private string GetStopEvent()
        {
            return enemy.MovementMode == EnemyMovementMode.Flying
                ? WwiseAudioNames.StopPigeonMovement
                : WwiseAudioNames.StopFishMovement;
        }
    }
}
