using System;
using UnityEngine;
using UnityEngine.Events;

namespace RollfaehrenFury.Prototype
{
    public enum EnemyMovementMode
    {
        Surface,
        Flying
    }

    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Health))]
    public sealed class SimpleEnemy : MonoBehaviour
    {
        [SerializeField] private EnemyMovementMode movementMode = EnemyMovementMode.Surface;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float contactDamage = 10f;
        [SerializeField] private int killReward = 10;
        [SerializeField] private bool faceTarget = true;
        [SerializeField] private GameObject contactEffectPrefab;
        [SerializeField] private float contactEffectDuration = 1.25f;
        [SerializeField] private UnityEvent reachedFerry = new UnityEvent();
        [SerializeField] private UnityEvent diedFromDamage = new UnityEvent();

        private FerryDamageTarget ferryTarget;
        private GameManager gameManager;
        private Health health;
        private bool rewardOnDeath = true;
        private bool hasHitFerry;
        private float activeMoveSpeed;

        public event Action<SimpleEnemy> Removed;

        public int KillReward => killReward;
        public EnemyMovementMode MovementMode => movementMode;

        private void Awake()
        {
            health = GetComponent<Health>();
            activeMoveSpeed = moveSpeed;
            health.Died += HandleDied;
        }

        private void OnEnable()
        {
            rewardOnDeath = true;
            hasHitFerry = false;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= HandleDied;
            }

            Removed?.Invoke(this);
        }

        private void Update()
        {
            if (ferryTarget == null || hasHitFerry)
            {
                return;
            }

            Vector3 targetPosition = ferryTarget.AimPoint.position;
            Vector3 direction = targetPosition - transform.position;
            if (movementMode == EnemyMovementMode.Surface)
            {
                direction.y = 0f;
            }

            if (direction.sqrMagnitude < 0.01f)
            {
                return;
            }

            Vector3 step = direction.normalized * activeMoveSpeed * Time.deltaTime;
            transform.position += step;

            if (faceTarget)
            {
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }

        public void Initialize(FerryDamageTarget target, GameManager manager, int reward, float speedMultiplier, float healthMultiplier)
        {
            ferryTarget = target;
            gameManager = manager;
            killReward = Mathf.Max(0, reward);
            activeMoveSpeed = Mathf.Max(0.1f, moveSpeed * speedMultiplier);

            if (health == null)
            {
                health = GetComponent<Health>();
            }

            health.SetMaxHealth(health.MaxHealth * Mathf.Max(0.1f, healthMultiplier), true);
        }

        private void OnTriggerEnter(Collider other)
        {
            FerryDamageTarget target = other.GetComponentInParent<FerryDamageTarget>();
            if (target != null)
            {
                HitFerry(target);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            FerryDamageTarget target = collision.collider.GetComponentInParent<FerryDamageTarget>();
            if (target != null)
            {
                HitFerry(target);
            }
        }

        private void HitFerry(FerryDamageTarget target)
        {
            if (hasHitFerry)
            {
                return;
            }

            hasHitFerry = true;
            rewardOnDeath = false;
            target.ApplyEnemyDamage(contactDamage);
            gameManager?.RegisterEnemyReachedFerry(this, contactDamage);
            reachedFerry.Invoke();

            if (contactEffectPrefab != null)
            {
                GameObject effect = Instantiate(contactEffectPrefab, transform.position, transform.rotation);
                Destroy(effect, Mathf.Max(0.1f, contactEffectDuration));
            }

            Destroy(gameObject);
        }

        private void HandleDied(Health deadHealth)
        {
            if (rewardOnDeath)
            {
                gameManager?.RegisterEnemyKilled(killReward);
                diedFromDamage.Invoke();
            }

            Destroy(gameObject);
        }
    }
}
