using System;
using UnityEngine;
using UnityEngine.Events;

namespace RollfaehrenFury.Prototype
{
    public sealed class Health : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool resetOnAwake = true;
        [SerializeField] private UnityEvent<float, float> healthChanged = new UnityEvent<float, float>();
        [SerializeField] private UnityEvent died = new UnityEvent();

        private float currentHealth;
        private bool isDead;

        public event Action<Health> Died;
        public event Action<Health, float, float> HealthChanged;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float Normalized => maxHealth <= 0f ? 0f : currentHealth / maxHealth;
        public bool IsAlive => !isDead && currentHealth > 0f;

        private void Awake()
        {
            if (resetOnAwake || currentHealth <= 0f)
            {
                ResetHealth();
            }
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1f, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        public void ResetHealth()
        {
            isDead = false;
            currentHealth = maxHealth;
            RaiseHealthChanged();
        }

        public void SetMaxHealth(float value, bool refill)
        {
            maxHealth = Mathf.Max(1f, value);
            currentHealth = refill ? maxHealth : Mathf.Clamp(currentHealth, 0f, maxHealth);
            if (currentHealth > 0f)
            {
                isDead = false;
            }

            RaiseHealthChanged();
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || isDead)
            {
                return;
            }

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            RaiseHealthChanged();
        }

        public void Damage(float amount)
        {
            if (amount <= 0f || isDead)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            RaiseHealthChanged();

            if (currentHealth <= 0f)
            {
                isDead = true;
                died.Invoke();
                Died?.Invoke(this);
            }
        }

        private void RaiseHealthChanged()
        {
            healthChanged.Invoke(currentHealth, maxHealth);
            HealthChanged?.Invoke(this, currentHealth, maxHealth);
        }
    }
}
