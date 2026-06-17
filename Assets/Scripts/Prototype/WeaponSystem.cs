using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Owns the player's weapons and the firing input. Holds one or more
    /// <see cref="Weapon"/> instances, drives the active one, and forwards its
    /// fire/hit events so listeners (HUD, audio) do not care which weapon is active.
    /// </summary>
    public sealed class WeaponSystem : MonoBehaviour
    {
        [SerializeField] private Camera fireCamera;
        [SerializeField] private Transform ignoredRoot;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private List<Weapon> weapons = new List<Weapon>();
        [SerializeField] private int startWeaponIndex;
        [SerializeField] private string fireActionPath = "Player/Attack";
        [SerializeField] private string nextWeaponActionPath = "Player/Next";
        [SerializeField] private string previousWeaponActionPath = "Player/Previous";

        private int activeIndex;
        private bool fireHeld;
        private InputAction fireAction;
        private InputAction nextAction;
        private InputAction previousAction;

        public event Action Fired;
        public event Action<Health> HitHealth;
        public event Action<Weapon> WeaponChanged;

        public bool InputEnabled { get; private set; } = true;
        public int WeaponCount => weapons.Count;
        public int ActiveIndex => activeIndex;
        public Weapon ActiveWeapon => activeIndex >= 0 && activeIndex < weapons.Count ? weapons[activeIndex] : null;
        public float ActiveDamage => ActiveWeapon != null ? ActiveWeapon.Damage : 0f;
        public float ActiveShotsPerSecond => ActiveWeapon != null ? ActiveWeapon.ShotsPerSecond : 0f;
        public string ActiveWeaponName => ActiveWeapon != null ? ActiveWeapon.DisplayName : "None";

        private void Awake()
        {
            if (fireCamera == null)
            {
                fireCamera = GetComponentInParent<Camera>();
            }

            activeIndex = weapons.Count == 0 ? 0 : Mathf.Clamp(startWeaponIndex, 0, weapons.Count - 1);
        }

        private void OnEnable()
        {
            BindActions();
            SubscribeWeapon(ActiveWeapon);
        }

        private void OnDisable()
        {
            if (fireAction != null)
            {
                fireAction.performed -= HandleFirePerformed;
                fireAction.canceled -= HandleFireCanceled;
            }

            if (nextAction != null)
            {
                nextAction.performed -= HandleNext;
            }

            if (previousAction != null)
            {
                previousAction.performed -= HandlePrevious;
            }

            UnsubscribeWeapon(ActiveWeapon);
            fireHeld = false;
        }

        private void Update()
        {
            if (InputEnabled && fireHeld)
            {
                ActiveWeapon?.Fire(fireCamera, ignoredRoot, hitMask);
            }
        }

        public void SetInputEnabled(bool isEnabled)
        {
            InputEnabled = isEnabled;
            if (!isEnabled)
            {
                fireHeld = false;
            }
        }

        public void AddDamageToActive(float amount)
        {
            ActiveWeapon?.AddDamage(amount);
        }

        public void MultiplyActiveCooldown(float multiplier)
        {
            ActiveWeapon?.MultiplyCooldown(multiplier);
        }

        public void AddRicochetToActive(int bounces)
        {
            ActiveWeapon?.AddRicochet(bounces);
        }

        public void SwitchTo(int index)
        {
            if (weapons.Count == 0)
            {
                return;
            }

            int wrapped = ((index % weapons.Count) + weapons.Count) % weapons.Count;
            if (wrapped == activeIndex)
            {
                return;
            }

            UnsubscribeWeapon(ActiveWeapon);
            activeIndex = wrapped;
            SubscribeWeapon(ActiveWeapon);
            WeaponChanged?.Invoke(ActiveWeapon);
        }

        private void BindActions()
        {
            fireAction ??= PrototypeInputActions.Find(fireActionPath);
            if (fireAction != null)
            {
                fireAction.performed += HandleFirePerformed;
                fireAction.canceled += HandleFireCanceled;
                fireAction.Enable();
            }

            nextAction ??= PrototypeInputActions.Find(nextWeaponActionPath);
            if (nextAction != null)
            {
                nextAction.performed += HandleNext;
                nextAction.Enable();
            }

            previousAction ??= PrototypeInputActions.Find(previousWeaponActionPath);
            if (previousAction != null)
            {
                previousAction.performed += HandlePrevious;
                previousAction.Enable();
            }
        }

        private void HandleFirePerformed(InputAction.CallbackContext context)
        {
            fireHeld = true;
            if (InputEnabled)
            {
                ActiveWeapon?.Fire(fireCamera, ignoredRoot, hitMask);
            }
        }

        private void HandleFireCanceled(InputAction.CallbackContext context)
        {
            fireHeld = false;
        }

        private void HandleNext(InputAction.CallbackContext context)
        {
            if (InputEnabled)
            {
                SwitchTo(activeIndex + 1);
            }
        }

        private void HandlePrevious(InputAction.CallbackContext context)
        {
            if (InputEnabled)
            {
                SwitchTo(activeIndex - 1);
            }
        }

        private void SubscribeWeapon(Weapon weapon)
        {
            if (weapon == null)
            {
                return;
            }

            weapon.Fired += HandleActiveFired;
            weapon.HitHealth += HandleActiveHitHealth;
        }

        private void UnsubscribeWeapon(Weapon weapon)
        {
            if (weapon == null)
            {
                return;
            }

            weapon.Fired -= HandleActiveFired;
            weapon.HitHealth -= HandleActiveHitHealth;
        }

        private void HandleActiveFired()
        {
            Fired?.Invoke();
        }

        private void HandleActiveHitHealth(Health health)
        {
            HitHealth?.Invoke(health);
        }
    }
}
