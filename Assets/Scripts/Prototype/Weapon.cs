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

        private int magazineSize;
        private float reloadDuration;
        private int currentAmmo;
        private int currentReserve;
        private int maxReserve;
        private bool isReloading;
        private float reloadRemaining;
        private bool isEquipped = true;

        private float reloadDamageMultiplier = 1f;
        private float reloadDamageDuration;
        private float damageBuffUntil;

        public event Action Fired;
        public event Action<RaycastHit> HitSomething;
        public event Action<Health> HitHealth;
        public event Action ReloadStarted;
        public event Action ReloadCompleted;

        public WeaponDefinition Definition => definition;
        public string DisplayName => definition != null ? definition.DisplayName : name;
        public float Damage => currentDamage;
        public float FireCooldown => currentCooldown;
        public float ShotsPerSecond => currentCooldown <= 0f ? 0f : 1f / currentCooldown;
        public bool IsAutomatic => definition != null && definition.Automatic;

        /// <summary>True when this weapon never runs out of ammo (magazine size 0).</summary>
        public bool HasInfiniteAmmo { get { EnsureStats(); return magazineSize <= 0; } }
        public int MagazineSize { get { EnsureStats(); return magazineSize; } }
        /// <summary>Rounds left in the magazine; -1 for an infinite-ammo weapon.</summary>
        public int CurrentAmmo { get { EnsureStats(); return magazineSize <= 0 ? -1 : currentAmmo; } }
        /// <summary>Rounds left in reserve (outside the magazine); -1 for an infinite-ammo weapon.</summary>
        public int ReserveAmmo { get { EnsureStats(); return magazineSize <= 0 ? -1 : currentReserve; } }
        public int MaxReserveAmmo { get { EnsureStats(); return maxReserve; } }
        /// <summary>True when the magazine and reserve are both topped up (or the weapon is unlimited).</summary>
        public bool IsAmmoFull { get { EnsureStats(); return magazineSize <= 0 || (currentAmmo >= magazineSize && currentReserve >= maxReserve); } }
        public bool IsReloading => isReloading;
        /// <summary>True while this is the selected weapon — used to show/hide its first-person model.</summary>
        public bool IsEquipped => isEquipped;

        /// <summary>0 at reload start, 1 when the magazine is ready again (1 while not reloading).</summary>
        public float ReloadProgress01
        {
            get
            {
                if (!isReloading || reloadDuration <= 0f)
                {
                    return 1f;
                }

                return Mathf.Clamp01(1f - reloadRemaining / reloadDuration);
            }
        }

        private void Awake()
        {
            EnsureStats();
        }

        private void Update()
        {
            // The reload only advances while this weapon is the equipped one, so switching to
            // another gun and back pauses the reload instead of finishing it in the background.
            if (isReloading && isEquipped)
            {
                reloadRemaining -= Time.deltaTime;
                if (reloadRemaining <= 0f)
                {
                    CompleteReload();
                }
            }
        }

        /// <summary>Marks whether this weapon is currently selected. Reloads pause while holstered.</summary>
        public void SetEquipped(bool equipped)
        {
            isEquipped = equipped;
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
            magazineSize = definition.MagazineSize;
            reloadDuration = definition.ReloadTime;
            currentAmmo = magazineSize;
            maxReserve = magazineSize * definition.ReserveMagazines;
            currentReserve = maxReserve;
            statsInitialized = true;
        }

        public void MultiplyDamage(float factor)
        {
            EnsureStats();
            currentDamage = Mathf.Max(1f, currentDamage * Mathf.Max(0.01f, factor));
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
            EnsureStats();
            return Time.time >= nextFireTime
                && !isReloading
                && (magazineSize <= 0 || currentAmmo > 0);
        }

        /// <summary>Begins a reload if the magazine is not full, not already reloading, and reserve ammo is left.</summary>
        public void Reload()
        {
            EnsureStats();
            if (magazineSize <= 0 || isReloading || currentAmmo >= magazineSize || currentReserve <= 0)
            {
                return;
            }

            isReloading = true;
            reloadRemaining = Mathf.Max(0.05f, reloadDuration);
            ReloadStarted?.Invoke();
        }

        private void CompleteReload()
        {
            int needed = magazineSize - currentAmmo;
            int take = Mathf.Min(needed, currentReserve);
            currentAmmo += take;
            currentReserve -= take;
            isReloading = false;

            // Reload Fury augment: finishing a reload grants a timed damage boost.
            if (reloadDamageMultiplier > 1f && reloadDamageDuration > 0f)
            {
                damageBuffUntil = Time.time + reloadDamageDuration;
            }

            ReloadCompleted?.Invoke();
        }

        /// <summary>Enables a timed damage boost that triggers each time a reload completes.</summary>
        public void EnableReloadDamageBuff(float multiplier, float duration)
        {
            reloadDamageMultiplier = Mathf.Max(1f, multiplier);
            reloadDamageDuration = Mathf.Max(0f, duration);
        }

        /// <summary>Damage actually dealt this instant, including the post-reload buff while it lasts.</summary>
        private float EffectiveDamage => Time.time < damageBuffUntil ? currentDamage * reloadDamageMultiplier : currentDamage;

        /// <summary>Tops the magazine and reserve back up to full and cancels any reload (e.g. restock at the dock).</summary>
        public void RefillAmmo()
        {
            EnsureStats();
            isReloading = false;
            currentAmmo = magazineSize;
            currentReserve = maxReserve;
        }

        /// <summary>Restores the weapon to its definition defaults for a fresh run: clears upgrades, buffs, and refills ammo.</summary>
        public void ResetStats()
        {
            statsInitialized = false;
            isReloading = false;
            reloadRemaining = 0f;
            reloadDamageMultiplier = 1f;
            reloadDamageDuration = 0f;
            damageBuffUntil = 0f;
            nextFireTime = 0f;
            ricochetBounces = 0;
            EnsureStats();
        }

        public void AddMagazineSize(int amount)
        {
            EnsureStats();
            if (magazineSize <= 0)
            {
                return; // unlimited weapons have no magazine to grow
            }

            magazineSize = Mathf.Max(1, magazineSize + amount);
        }

        public void AddReserveMagazines(int magazines)
        {
            EnsureStats();
            if (magazineSize <= 0)
            {
                return;
            }

            int extra = Mathf.Max(0, magazines) * magazineSize;
            maxReserve += extra;
            currentReserve += extra;
        }

        public void MultiplyReloadDuration(float multiplier)
        {
            EnsureStats();
            reloadDuration = Mathf.Max(0.1f, reloadDuration * multiplier);
        }

        public bool Fire(Camera fireCamera, Transform ignoredRoot, LayerMask hitMask)
        {
            if (definition == null || fireCamera == null)
            {
                return false;
            }

            EnsureStats();

            // Empty magazine: start the reload instead of firing (auto-reload on a dry trigger).
            if (magazineSize > 0 && currentAmmo <= 0)
            {
                Reload();
                return false;
            }

            if (!CanFire())
            {
                return false;
            }

            nextFireTime = Time.time + currentCooldown;

            if (magazineSize > 0)
            {
                currentAmmo--;
            }

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

            // Firing the last round kicks off the reload immediately, so the HUD bar appears at once.
            if (magazineSize > 0 && currentAmmo <= 0)
            {
                Reload();
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
                EffectiveDamage,
                definition.ProjectileLifetime,
                ignoredRoot,
                hitMask,
                ricochetBounces);
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
                health.Damage(EffectiveDamage);
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
                    nextHealth.Damage(EffectiveDamage);
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
