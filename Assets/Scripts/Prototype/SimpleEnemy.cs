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
        [SerializeField] private bool useSwarmMovement = true;

        [Header("Catch-up Ramp")]
        [Tooltip("While behind the ferry's travel direction, this enemy keeps accelerating so it eventually reaches the boat.")]
        [SerializeField] private bool behindRampEnabled = true;
        [SerializeField] private float behindRampPerSecond = 0.5f;
        [SerializeField] private float maxRampSpeed = 16f;
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
        private SwarmMovement swarmMovement;
        private bool rewardOnDeath = true;
        private bool hasHitFerry;
        private float activeMoveSpeed;

        public event Action<SimpleEnemy> Removed;

        public int KillReward => killReward;
        public EnemyMovementMode MovementMode => movementMode;

        // Exposed so an attached SwarmMovement can drive locomotion while this
        // component keeps owning health, contact damage and rewards.
        public bool CanMove => ferryTarget != null && !hasHitFerry;
        public Vector3 TargetPosition => ferryTarget != null ? ferryTarget.AimPoint.position : transform.position;
        public float ActiveMoveSpeed => activeMoveSpeed;
        public bool FaceTarget => faceTarget;

        // True when the player killed this enemy (vs. it reaching the ferry); used by the
        // spawner's adaptive escalation to measure how fast a swarm gets cleared.
        public bool WasKilledByDamage { get; private set; }

        private void Awake()
        {
            health = GetComponent<Health>();
            activeMoveSpeed = moveSpeed;
            health.Died += HandleDied;

            if (useSwarmMovement)
            {
                swarmMovement = GetComponent<SwarmMovement>();
                if (swarmMovement == null)
                {
                    swarmMovement = gameObject.AddComponent<SwarmMovement>();
                }
            }
        }

        private void OnEnable()
        {
            rewardOnDeath = true;
            hasHitFerry = false;
            WasKilledByDamage = false;
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
            RampSpeedIfBehind();

            if (swarmMovement != null && swarmMovement.enabled)
            {
                return; // SwarmMovement owns locomotion when present.
            }

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

        // While behind the ferry (relative to its travel direction), keep accelerating so the
        // enemy is never left behind for good and eventually forces the player to deal with it.
        private void RampSpeedIfBehind()
        {
            if (!behindRampEnabled || ferryTarget == null || hasHitFerry)
            {
                return;
            }

            Transform ferry = ferryTarget.transform;
            Vector3 forward = Vector3.ProjectOnPlane(ferry.forward, Vector3.up);
            Vector3 offset = Vector3.ProjectOnPlane(transform.position - ferry.position, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f)
            {
                return;
            }

            if (Vector3.Dot(forward, offset) < 0f)
            {
                activeMoveSpeed = Mathf.Min(maxRampSpeed, activeMoveSpeed + behindRampPerSecond * Time.deltaTime);
            }
        }

        public void Initialize(FerryDamageTarget target, GameManager manager, int reward, float speedMultiplier, float healthMultiplier, float speedBonus = 0f)
        {
            ferryTarget = target;
            gameManager = manager;
            killReward = Mathf.Max(0, reward);
            activeMoveSpeed = Mathf.Max(0.1f, moveSpeed * speedMultiplier + speedBonus);

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
            WasKilledByDamage = true;

            if (rewardOnDeath)
            {
                gameManager?.RegisterEnemyKilled(killReward);
                diedFromDamage.Invoke();
            }

            Destroy(gameObject);
        }
    }
}
