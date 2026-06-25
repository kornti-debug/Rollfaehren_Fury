using System.Collections.Generic;
using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Boids-style flocking locomotion for <see cref="SimpleEnemy"/>. When present it takes
    /// over movement: the enemy still seeks the ferry (so rounds keep resolving), but blends
    /// in separation / alignment / cohesion with nearby enemies so the group moves as a swarm.
    /// SimpleEnemy keeps owning health, contact damage and rewards, so EnemySpawner and
    /// GameManager are unaffected — they still only know about SimpleEnemy.
    /// </summary>
    [RequireComponent(typeof(SimpleEnemy))]
    public sealed class SwarmMovement : MonoBehaviour
    {
        [Header("Neighbourhood")]
        [SerializeField] private float neighborRadius = 6f;
        [SerializeField] private float separationRadius = 2.2f;

        [Header("Steering weights")]
        [SerializeField, Range(0f, 4f)] private float seekWeight = 1.6f;
        [SerializeField, Range(0f, 4f)] private float separationWeight = 1.8f;
        [SerializeField, Range(0f, 4f)] private float alignmentWeight = 0.7f;
        [SerializeField, Range(0f, 4f)] private float cohesionWeight = 0.6f;

        [Header("Motion")]
        [SerializeField, Range(0.5f, 20f)] private float turnResponsiveness = 4f;
        [SerializeField, Range(0f, 1f)] private float wander = 0.15f;
        [Tooltip("How sharply a diving flyer snaps onto its plunge line. Higher = a more committed, direct dive.")]
        [SerializeField, Range(0.5f, 20f)] private float diveTurnResponsiveness = 8f;

        // Shared registry so each agent finds neighbours without scanning the whole scene.
        private static readonly List<SwarmMovement> Flock = new List<SwarmMovement>();

        private SimpleEnemy enemy;
        private Vector3 velocity;

        public Vector3 Velocity => velocity;

        // Clears the registry on every Play start so disabling domain reload can't leave stale agents.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetFlock()
        {
            Flock.Clear();
        }

        private void Awake()
        {
            enemy = GetComponent<SimpleEnemy>();
        }

        private void OnEnable()
        {
            if (!Flock.Contains(this))
            {
                Flock.Add(this);
            }
        }

        private void OnDisable()
        {
            Flock.Remove(this);
        }

        private void Update()
        {
            if (enemy == null || !enemy.CanMove)
            {
                return;
            }

            Vector3 position = transform.position;

            // Committed dive (a flying enemy in attack range): ignore the flock and plunge straight at
            // the ferry's aim point, a touch faster, so birds crash onto it instead of circling above.
            if (enemy.MovementMode == EnemyMovementMode.Flying && enemy.IsDiving)
            {
                Vector3 toFerry = enemy.TargetPosition - position;
                float diveSpeed = enemy.ActiveMoveSpeed * enemy.DiveSpeedMultiplier;
                Vector3 dived = toFerry.sqrMagnitude > 0.0001f ? toFerry.normalized * diveSpeed : velocity;
                velocity = Vector3.Lerp(velocity, dived, Mathf.Clamp01(diveTurnResponsiveness * Time.deltaTime));
                transform.position += velocity * Time.deltaTime;

                if (enemy.FaceTarget && velocity.sqrMagnitude > 0.0001f)
                {
                    transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
                }

                return;
            }

            Vector3 separation = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            Vector3 cohesion = Vector3.zero;
            int neighbors = 0;
            int separationCount = 0;

            for (int i = 0; i < Flock.Count; i++)
            {
                SwarmMovement other = Flock[i];
                if (other == null || other == this)
                {
                    continue;
                }

                Vector3 offset = position - other.transform.position;
                float distance = offset.magnitude;
                if (distance > neighborRadius || distance < 0.0001f)
                {
                    continue;
                }

                alignment += other.velocity;
                cohesion += other.transform.position;
                neighbors++;

                if (distance < separationRadius)
                {
                    separation += offset / distance; // push away, stronger the closer they are
                    separationCount++;
                }
            }

            Vector3 toTarget = enemy.TargetPosition - position;
            Vector3 steer = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized * seekWeight : Vector3.zero;

            if (separationCount > 0)
            {
                steer += (separation / separationCount).normalized * separationWeight;
            }

            if (neighbors > 0)
            {
                Vector3 averageVelocity = alignment / neighbors;
                if (averageVelocity.sqrMagnitude > 0.0001f)
                {
                    steer += averageVelocity.normalized * alignmentWeight;
                }

                Vector3 toCenter = (cohesion / neighbors) - position;
                if (toCenter.sqrMagnitude > 0.0001f)
                {
                    steer += toCenter.normalized * cohesionWeight;
                }
            }

            if (wander > 0f)
            {
                steer += Random.insideUnitSphere * wander;
            }

            // Fish on the water and cruising birds both stay on a level plane (birds only descend once
            // they commit to a dive, handled above), so we flatten steering on Y for both.
            steer.y = 0f;

            float speed = enemy.ActiveMoveSpeed;
            Vector3 desiredVelocity = steer.sqrMagnitude > 0.0001f ? steer.normalized * speed : velocity;
            velocity = Vector3.Lerp(velocity, desiredVelocity, Mathf.Clamp01(turnResponsiveness * Time.deltaTime));

            velocity.y = 0f;

            transform.position += velocity * Time.deltaTime;

            if (enemy.FaceTarget && velocity.sqrMagnitude > 0.0001f)
            {
                Vector3 facing = new Vector3(velocity.x, 0f, velocity.z);
                if (facing.sqrMagnitude > 0.0001f)
                {
                    transform.rotation = Quaternion.LookRotation(facing.normalized, Vector3.up);
                }
            }
        }
    }
}
