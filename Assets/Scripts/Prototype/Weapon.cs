using System;
using System.Collections.Generic;
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
        [SerializeField] private WeaponTracer tracer;
        [SerializeField] private float ricochetRange = 25f;
        [SerializeField] private UnityEvent fired = new UnityEvent();
        [SerializeField] private UnityEvent hitSomething = new UnityEvent();
        [SerializeField] private UnityEvent hitHealth = new UnityEvent();

        private float currentDamage;
        private float currentCooldown;
        private float nextFireTime;
        private int ricochetBounces;
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

        public void AddRicochet(int bounces)
        {
            ricochetBounces += Mathf.Max(0, bounces);
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
                if (definition.FireMode == WeaponFireMode.Projectile)
                {
                    FireProjectile(fireCamera, ignoredRoot, hitMask);
                }
                else
                {
                    FireSingleRay(fireCamera, ignoredRoot, hitMask);
                }
            }

            return true;
        }

        private void FireProjectile(Camera fireCamera, Transform ignoredRoot, LayerMask hitMask)
        {
            Vector3 direction = GetShotDirection(fireCamera);
            Vector3 origin = fireCamera.transform.position + direction * 0.5f;

            GameObject projectileObject = new GameObject($"{DisplayName} Projectile");
            projectileObject.transform.position = origin;

            Projectile projectile = projectileObject.AddComponent<Projectile>();
            projectile.Initialize(
                direction * definition.ProjectileSpeed,
                definition.ProjectileGravity,
                currentDamage,
                definition.ProjectileLifetime,
                ignoredRoot,
                hitMask);
        }

        private void FireSingleRay(Camera fireCamera, Transform ignoredRoot, LayerMask hitMask)
        {
            Vector3 direction = GetShotDirection(fireCamera);
            Ray ray = new Ray(fireCamera.transform.position, direction);

            bool didHit = TryFindHit(ray, ignoredRoot, hitMask, out RaycastHit hit);
            Vector3 endPoint = didHit ? hit.point : ray.origin + direction * definition.Range;
            ShowTracer(fireCamera, endPoint);

            if (!didHit)
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

                if (ricochetBounces > 0)
                {
                    Ricochet(hit.point, health.GetComponentInParent<SimpleEnemy>());
                }
            }
        }

        private void Ricochet(Vector3 fromPoint, SimpleEnemy firstHit)
        {
            HashSet<SimpleEnemy> alreadyHit = new HashSet<SimpleEnemy>();
            if (firstHit != null)
            {
                alreadyHit.Add(firstHit);
            }

            Vector3 from = fromPoint;
            for (int i = 0; i < ricochetBounces; i++)
            {
                SimpleEnemy next = FindNearestEnemy(from, alreadyHit);
                if (next == null)
                {
                    break;
                }

                Health nextHealth = next.GetComponent<Health>();
                if (nextHealth != null)
                {
                    nextHealth.Damage(currentDamage);
                    HitHealth?.Invoke(nextHealth);
                }

                Vector3 to = next.transform.position;
                tracer?.Show(from, to);
                alreadyHit.Add(next);
                from = to;
            }
        }

        private SimpleEnemy FindNearestEnemy(Vector3 point, HashSet<SimpleEnemy> exclude)
        {
            SimpleEnemy nearest = null;
            float nearestSqr = ricochetRange * ricochetRange;

            foreach (SimpleEnemy enemy in FindObjectsByType<SimpleEnemy>(FindObjectsSortMode.None))
            {
                if (enemy == null || exclude.Contains(enemy))
                {
                    continue;
                }

                float sqr = (enemy.transform.position - point).sqrMagnitude;
                if (sqr <= nearestSqr)
                {
                    nearestSqr = sqr;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        private void ShowTracer(Camera fireCamera, Vector3 endPoint)
        {
            if (tracer == null)
            {
                return;
            }

            // A tracer exactly on the view axis is seen edge-on (nearly invisible) and is
            // clipped by the near plane. Start a touch ahead of and below the eye so it is
            // visible at every angle, while staying horizontally centered under the crosshair
            // and converging on the actual aim/hit point.
            Transform cameraTransform = fireCamera.transform;
            Vector3 muzzle = cameraTransform.position
                + cameraTransform.forward * 0.3f
                - cameraTransform.up * 0.12f;
            tracer.Show(muzzle, endPoint);
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
