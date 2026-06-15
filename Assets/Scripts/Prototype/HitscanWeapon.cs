using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace RollfaehrenFury.Prototype
{
    public sealed class HitscanWeapon : MonoBehaviour
    {
        [SerializeField] private Camera fireCamera;
        [SerializeField] private float damage = 25f;
        [SerializeField] private float range = 100f;
        [SerializeField] private float fireCooldown = 0.2f;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private UnityEvent fired = new UnityEvent();
        [SerializeField] private UnityEvent hitSomething = new UnityEvent();
        [SerializeField] private UnityEvent hitHealth = new UnityEvent();

        private float nextFireTime;

        public event Action Fired;
        public event Action<RaycastHit> HitSomething;
        public event Action<Health> HitHealth;

        public bool InputEnabled { get; private set; } = true;
        public float Damage => damage;
        public float FireCooldown => fireCooldown;

        private void Awake()
        {
            if (fireCamera == null)
            {
                fireCamera = GetComponentInParent<Camera>();
            }
        }

        private void Update()
        {
            if (!InputEnabled || Mouse.current == null)
            {
                return;
            }

            if (Mouse.current.leftButton.isPressed)
            {
                TryFire();
            }
        }

        public void SetInputEnabled(bool isEnabled)
        {
            InputEnabled = isEnabled;
        }

        public void AddDamage(float amount)
        {
            damage = Mathf.Max(1f, damage + amount);
        }

        public void MultiplyCooldown(float multiplier)
        {
            fireCooldown = Mathf.Max(0.05f, fireCooldown * multiplier);
        }

        public bool TryFire()
        {
            if (Time.time < nextFireTime || fireCamera == null)
            {
                return false;
            }

            nextFireTime = Time.time + fireCooldown;
            fired.Invoke();
            Fired?.Invoke();

            Ray ray = new Ray(fireCamera.transform.position, fireCamera.transform.forward);
            Debug.DrawRay(ray.origin, ray.direction * range, Color.yellow, 0.08f);

            if (!Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Collide))
            {
                return true;
            }

            hitSomething.Invoke();
            HitSomething?.Invoke(hit);

            Health health = hit.collider.GetComponentInParent<Health>();
            if (health != null)
            {
                health.Damage(damage);
                hitHealth.Invoke();
                HitHealth?.Invoke(health);
            }

            return true;
        }
    }
}
