using System;
using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    public sealed class PrototypeAudioEvents : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private WeaponSystem weaponSystem;
        [SerializeField] private bool postEvents;
        [SerializeField] private string weaponShootEvent = "Play_Weapon_Shoot";
        [SerializeField] private string enemyHitEvent = "Play_Enemy_Hit";
        [SerializeField] private string enemyDeathEvent = "Play_Enemy_Death";
        [SerializeField] private string ferryDamageEvent = "Play_Ferry_Damage";
        [SerializeField] private string roundCompleteEvent = "Play_Round_Complete";
        [SerializeField] private string gameOverEvent = "Play_Game_Over";
        [SerializeField] private string upgradeBoughtEvent = "Play_UI_Upgrade";

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
                gameManager.EnemyKilled += HandleEnemyKilled;
                gameManager.FerryDamaged += HandleFerryDamaged;
                gameManager.RoundCompleted += HandleRoundCompleted;
                gameManager.GameOverReached += HandleGameOverReached;
                gameManager.UpgradeBought += HandleUpgradeBought;
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
                gameManager.EnemyKilled -= HandleEnemyKilled;
                gameManager.FerryDamaged -= HandleFerryDamaged;
                gameManager.RoundCompleted -= HandleRoundCompleted;
                gameManager.GameOverReached -= HandleGameOverReached;
                gameManager.UpgradeBought -= HandleUpgradeBought;
            }
        }

        private void HandleWeaponFired()
        {
            Post(weaponShootEvent, weaponSystem != null ? weaponSystem.gameObject : gameObject);
        }

        private void HandleWeaponHitHealth(Health health)
        {
            Post(enemyHitEvent, health != null ? health.gameObject : gameObject);
        }

        private void HandleEnemyKilled()
        {
            Post(enemyDeathEvent, gameObject);
        }

        private void HandleFerryDamaged()
        {
            Post(ferryDamageEvent, gameObject);
        }

        private void HandleRoundCompleted()
        {
            Post(roundCompleteEvent, gameObject);
        }

        private void HandleGameOverReached()
        {
            Post(gameOverEvent, gameObject);
        }

        private void HandleUpgradeBought()
        {
            Post(upgradeBoughtEvent, gameObject);
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

            try
            {
                AkUnitySoundEngine.PostEvent(eventName, emitter);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Wwise event '{eventName}' could not be posted yet: {exception.Message}", emitter);
            }
        }
    }
}
