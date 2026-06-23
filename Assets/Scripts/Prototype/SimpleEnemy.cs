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

        [Header("Flying Dive (birds)")]
        [Tooltip("Flying enemies commit to a dive at the ferry once within this HORIZONTAL distance (m). Until then they cruise at spawn altitude.")]
        [SerializeField, Min(0f)] private float diveRange = 14f;
        [Tooltip("Speed multiplier while diving. 1 = exactly the cruising/fish speed; >1 = a faster plunge.")]
        [SerializeField, Min(1f)] private float diveSpeedMultiplier = 1.4f;

        [Header("Rear Catch-up (small)")]
        [Tooltip("Enemies behind the ferry slowly ramp their speed up so they don't fall behind forever. Kept gentle.")]
        [SerializeField] private bool rearRampEnabled = true;
        [SerializeField] private float rearRampPerSecond = 0.2f;
        [SerializeField] private float rearRampMaxSpeed = 8f;
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
        private bool isDiving;

        public event Action<SimpleEnemy> Removed;

        public int KillReward => killReward;
        public EnemyMovementMode MovementMode => movementMode;

        // Exposed so an attached SwarmMovement can drive locomotion while this
        // component keeps owning health, contact damage and rewards.
        public bool CanMove => ferryTarget != null && !hasHitFerry;
        public Vector3 TargetPosition => ferryTarget != null ? ferryTarget.AimPoint.position : transform.position;
        public float ActiveMoveSpeed => activeMoveSpeed;
        public bool FaceTarget => faceTarget;

        // Flying enemies only: true once the bird has committed to its plunge onto the ferry.
        public bool IsDiving => isDiving;
        public float DiveSpeedMultiplier => Mathf.Max(1f, diveSpeedMultiplier);

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
            isDiving = false;
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
            RampRearSpeed();
            UpdateDiveState();

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
            float speed = activeMoveSpeed;
            if (movementMode == EnemyMovementMode.Surface)
            {
                direction.y = 0f;
            }
            else if (isDiving)
            {
                // Committed plunge: head straight at the ferry (downward), a touch faster.
                speed *= DiveSpeedMultiplier;
            }
            else
            {
                // Cruise: hold altitude until in dive range so birds approach high, then plunge.
                direction.y = 0f;
            }

            if (direction.sqrMagnitude < 0.01f)
            {
                return;
            }

            Vector3 step = direction.normalized * speed * Time.deltaTime;
            transform.position += step;

            if (faceTarget)
            {
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }

        // Flying enemies cruise at altitude, then commit to a dive once within diveRange (horizontal)
        // of the ferry. Once committed it stays committed so they plunge instead of pulling back up.
        private void UpdateDiveState()
        {
            if (movementMode != EnemyMovementMode.Flying || ferryTarget == null || hasHitFerry)
            {
                isDiving = false;
                return;
            }

            if (isDiving)
            {
                return;
            }

            Vector3 toTarget = ferryTarget.AimPoint.position - transform.position;
            float horizontalDistance = new Vector2(toTarget.x, toTarget.z).magnitude;
            if (horizontalDistance <= Mathf.Max(0f, diveRange))
            {
                isDiving = true;
            }
        }

        // Small catch-up: enemies behind the ferry slowly ramp up so they keep pace instead of
        // falling behind forever. Deliberately gentle (low rate, cap just above ferry speed).
        private void RampRearSpeed()
        {
            if (!rearRampEnabled || ferryTarget == null || hasHitFerry || activeMoveSpeed >= rearRampMaxSpeed)
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
                activeMoveSpeed = Mathf.Min(rearRampMaxSpeed, activeMoveSpeed + rearRampPerSecond * Time.deltaTime);
            }
        }

        public void Initialize(FerryDamageTarget target, GameManager manager, int reward, float speed, float healthMultiplier)
        {
            ferryTarget = target;
            gameManager = manager;
            killReward = Mathf.Max(0, reward);
            activeMoveSpeed = Mathf.Max(0.1f, speed);

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
