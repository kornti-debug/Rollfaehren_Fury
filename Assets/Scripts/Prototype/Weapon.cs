using System;
using UnityEngine;
using UnityEngine.Events;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Runtime weapon driven by a <see cref="WeaponDefinition"/>. It does not read
    /// input itself; <see cref="WeaponSystem"/> owns input and calls <see cref="Fire"/>
    /// on the active weapon. Upgrades modify runtime copies of the stats, never the asset.
    /// </summary>
    public sealed class Weapon : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition definition;
        [SerializeField] private UnityEvent fired = new UnityEvent();
        [SerializeField] private UnityEvent hitSomething = new UnityEvent();
        [SerializeField] private UnityEvent hitHealth = new UnityEvent();

        private float currentDamage;
        private float currentCooldown;
        private float nextFireTime;
        private bool statsInitialized;

        public event Action Fired;
        public event Action<RaycastHit> HitSomething;
        public event Action<Health> HitHealth;

        public WeaponDefinition Definition => definition;
        public string DisplayName => definition != null ? definition.DisplayName : name;
        public float Damage => currentDamage;
        public float FireCooldown => currentCooldown;
        public float ShotsPerSecond => currentCooldown <= 0f ? 0f : 1f / currentCooldown;

        private void Awake()
        {
            EnsureStats();
        }

        /// <summary>Copies definition stats into runtime fields once.</summary>
        public void EnsureStats()
        {
            if (statsInitialized || definition == null)
            {
                return;
            }

            currentDamage = definition.Damage;
            currentCooldown = definition.FireCooldown;
            statsInitialized = true;
        }

        public void AddDamage(float amount)
        {
            EnsureStats();
            currentDamage = Mathf.Max(1f, currentDamage + amount);
        }

        public void MultiplyCooldown(float multiplier)
        {
            EnsureStats();
            currentCooldown = Mathf.Max(0.05f, currentCooldown * multiplier);
        }

        public bool CanFire()
        {
            return Time.time >= nextFireTime;
        }

        public bool Fire(Camera fireCamera, Transform ignoredRoot, LayerMask hitMask)
        {
            if (definition == null || fireCamera == null || !CanFire())
            {
                return false;
            }

            EnsureStats();
            nextFireTime = Time.time + currentCooldown;
            fired.Invoke();
            Fired?.Invoke();

            int pellets = definition.PelletsPerShot;
            for (int i = 0; i < pellets; i++)
            {
                FireSingleRay(fireCamera, ignoredRoot, hitMask);
            }

            return true;
        }

        private void FireSingleRay(Camera fireCamera, Transform ignoredRoot, LayerMask hitMask)
        {
            Vector3 direction = GetShotDirection(fireCamera);
            Ray ray = new Ray(fireCamera.transform.position, direction);
            Debug.DrawRay(ray.origin, ray.direction * definition.Range, Color.yellow, 0.08f);

            if (!TryFindHit(ray, ignoredRoot, hitMask, out RaycastHit hit))
            {
                return;
            }

            hitSomething.Invoke();
            HitSomething?.Invoke(hit);

            Health health = hit.collider.GetComponentInParent<Health>();
            if (health != null)
            {
                health.Damage(currentDamage);
                hitHealth.Invoke();
                HitHealth?.Invoke(health);
            }
        }

        private Vector3 GetShotDirection(Camera fireCamera)
        {
            Vector3 forward = fireCamera.transform.forward;
            float spread = definition.SpreadAngle;
            if (spread <= 0f)
            {
                return forward;
            }

            Quaternion offset = Quaternion.Euler(
                UnityEngine.Random.Range(-spread, spread),
                UnityEngine.Random.Range(-spread, spread),
                0f);
            return offset * forward;
        }

        private bool TryFindHit(Ray ray, Transform ignoredRoot, LayerMask hitMask, out RaycastHit selectedHit)
        {
            float radius = definition.AimAssistRadius;
            RaycastHit[] hits = radius > 0f
                ? Physics.SphereCastAll(ray, radius, definition.Range, hitMask, QueryTriggerInteraction.Collide)
                : Physics.RaycastAll(ray, definition.Range, hitMask, QueryTriggerInteraction.Collide);

            Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

            foreach (RaycastHit hit in hits)
            {
                if (ShouldIgnoreHit(hit.collider, ignoredRoot))
                {
                    continue;
                }

                selectedHit = hit;
                return true;
            }

            selectedHit = default;
            return false;
        }

        private static bool ShouldIgnoreHit(Collider hitCollider, Transform ignoredRoot)
        {
            if (hitCollider == null)
            {
                return true;
            }

            if (ignoredRoot != null && hitCollider.transform.IsChildOf(ignoredRoot))
            {
                return true;
            }

            if (hitCollider.GetComponentInParent<SimpleFPSController>() != null)
            {
                return true;
            }

            return hitCollider.GetComponentInParent<FerryDamageTarget>() != null;
        }
    }
}
