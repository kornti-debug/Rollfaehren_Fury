using System.Collections.Generic;
using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Placeholder thrown projectile: flies along a gravity parabola, raycasts its
    /// own path each step to hit <see cref="Health"/>, then despawns. Builds its own
    /// placeholder visual (stretched cube + trail) so no projectile asset is needed yet.
    /// Spawned by <see cref="Weapon"/> when the fire mode is Projectile.
    /// </summary>
    public sealed class Projectile : MonoBehaviour
    {
        private Vector3 velocity;
        private float gravity;
        private float damage;
        private float lifeRemaining;
        private Transform ignoredRoot;
        private LayerMask hitMask;
        private bool initialized;
        private int ricochetBounces;
        private float ricochetRange = 25f;

        public void Initialize(Vector3 startVelocity, float gravityStrength, float impactDamage, float lifetime, Transform ignored, LayerMask mask, int bounces = 0)
        {
            velocity = startVelocity;
            gravity = Mathf.Max(0f, gravityStrength);
            damage = impactDamage;
            lifeRemaining = Mathf.Max(0.1f, lifetime);
            ignoredRoot = ignored;
            hitMask = mask;
            ricochetBounces = Mathf.Max(0, bounces);
            initialized = true;

            BuildVisual();
            OrientAlongVelocity();
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            lifeRemaining -= Time.deltaTime;
            if (lifeRemaining <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            velocity += Vector3.down * gravity * Time.deltaTime;

            Vector3 start = transform.position;
            Vector3 step = velocity * Time.deltaTime;
            float distance = step.magnitude;

            if (distance > 0f
                && Physics.Raycast(start, step.normalized, out RaycastHit hit, distance, hitMask, QueryTriggerInteraction.Collide)
                && !ShouldIgnore(hit.collider))
            {
                Health health = hit.collider.GetComponentInParent<Health>();
                if (health != null)
                {
                    health.Damage(damage);

                    if (ricochetBounces > 0)
                    {
                        Ricochet(hit.point, health.GetComponentInParent<SimpleEnemy>());
                    }
                }

                transform.position = hit.point;
                Destroy(gameObject);
                return;
            }

            transform.position = start + step;
            OrientAlongVelocity();
        }

        // Harpoon Ricochet upgrade: after the primary hit, chain damage to the nearest enemies.
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
                    nextHealth.Damage(damage);
                }

                alreadyHit.Add(next);
                from = next.transform.position;
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

        private bool ShouldIgnore(Collider hitCollider)
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

        private void OrientAlongVelocity()
        {
            if (velocity.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
            }
        }

        private void BuildVisual()
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "ProjectileVisual";
            visual.transform.SetParent(transform, false);
            visual.transform.localScale = new Vector3(0.08f, 0.08f, 0.7f);

            Collider visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
            {
                Destroy(visualCollider);
            }

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null)
                {
                    shader = Shader.Find("Sprites/Default");
                }

                Material material = new Material(shader) { color = new Color(0.85f, 0.78f, 0.35f) };
                renderer.sharedMaterial = material;
            }

            TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.4f;
            trail.startWidth = 0.12f;
            trail.endWidth = 0f;
            trail.numCapVertices = 0;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;

            Shader trailShader = Shader.Find("Sprites/Default");
            if (trailShader != null)
            {
                trail.material = new Material(trailShader);
            }

            trail.startColor = new Color(1f, 0.85f, 0.35f, 0.9f);
            trail.endColor = new Color(1f, 0.85f, 0.35f, 0f);
        }
    }
}
