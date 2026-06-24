using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    public sealed class PrototypeAudioEvents : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private WeaponSystem weaponSystem;
        [SerializeField] private FerryController ferryController;
        [SerializeField] private SimpleFPSController playerController;
        [SerializeField] private bool postEvents = true;

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void ResolveReferences()
        {
            if (gameManager == null)
            {
                gameManager = GameManager.Instance != null ? GameManager.Instance : FindFirstObjectByType<GameManager>();
            }

            if (weaponSystem == null)
            {
                weaponSystem = FindFirstObjectByType<WeaponSystem>();
            }

            ferryController ??= FindFirstObjectByType<FerryController>();
            playerController ??= FindFirstObjectByType<SimpleFPSController>();
        }

        private void Subscribe()
        {
            if (weaponSystem != null)
            {
                weaponSystem.Fired += HandleWeaponFired;
                weaponSystem.HitHealth += HandleWeaponHitHealth;
            }

            if (gameManager != null)
            {
                gameManager.EnemyKilled       += HandleEnemyKilled;
                gameManager.EnemyReachedFerry += HandleEnemyReachedFerry;
                gameManager.RoundCompleted    += HandleRoundCompleted;
            }
        }

        private void Unsubscribe()
        {
            if (weaponSystem != null)
            {
                weaponSystem.Fired -= HandleWeaponFired;
                weaponSystem.HitHealth -= HandleWeaponHitHealth;
            }

            if (gameManager != null)
            {
                gameManager.EnemyKilled       -= HandleEnemyKilled;
                gameManager.EnemyReachedFerry -= HandleEnemyReachedFerry;
                gameManager.RoundCompleted    -= HandleRoundCompleted;
            }
        }

        private void HandleEnemyKilled()
        {
            Post(WwiseAudioNames.PlayEnemyKilled, gameObject);
        }

        private void HandleWeaponFired()
        {
            string eventName = WeaponEvent(weaponSystem != null ? weaponSystem.ActiveWeaponName : string.Empty);
            Post(eventName, playerController != null ? playerController.gameObject : gameObject);
        }

        private void HandleWeaponHitHealth(Health health)
        {
            SimpleEnemy enemy = health != null ? health.GetComponentInParent<SimpleEnemy>() : null;
            if (enemy == null)
            {
                return;
            }

            string eventName = enemy.MovementMode == EnemyMovementMode.Flying
                ? WwiseAudioNames.PlayPigeonHit
                : WwiseAudioNames.PlayFishHit;
            Post(eventName, enemy.gameObject);
        }

        private void HandleEnemyReachedFerry(SimpleEnemy enemy)
        {
            string eventName = enemy != null && enemy.MovementMode == EnemyMovementMode.Flying
                ? WwiseAudioNames.PlayPigeonContact
                : WwiseAudioNames.PlayFishContact;
            Post(eventName, ferryController != null ? ferryController.gameObject : gameObject);
        }

        private void HandleRoundCompleted()
        {
            Post(WwiseAudioNames.PlayHarald, playerController != null ? playerController.gameObject : gameObject);
        }

        public void SetPostingEnabled(bool isEnabled)
        {
            postEvents = isEnabled;
        }

        private void Post(string eventName, GameObject emitter)
        {
            if (!postEvents)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(eventName))
            {
                return;
            }

            WwiseAudioRuntime.Post(eventName, emitter);
        }

        private static string WeaponEvent(string weaponName)
        {
            if (weaponName.IndexOf("Harpoon", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return WwiseAudioNames.PlayHarpoon;
            }

            if (weaponName.IndexOf("Pistol", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return WwiseAudioNames.PlayPistol;
            }

            if (weaponName.IndexOf("Shotgun", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return WwiseAudioNames.PlayShotgun;
            }

            if (weaponName.IndexOf("Assault", System.StringComparison.OrdinalIgnoreCase) >= 0
                || weaponName.IndexOf("Rifle", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return WwiseAudioNames.PlayAssaultRifle;
            }

            return string.Empty;
        }
    }
}
