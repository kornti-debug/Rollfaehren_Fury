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

        private int activeIndex;
        private bool fireHeld;
        private InputAction fireAction;

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

            UnsubscribeWeapon(ActiveWeapon);
            fireHeld = false;
        }

        private void Update()
        {
            if (!InputEnabled)
            {
                return;
            }

            HandleSwitchInput();

            if (fireHeld)
            {
                ActiveWeapon?.Fire(fireCamera, ignoredRoot, hitMask);
            }
        }

        private void HandleSwitchInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.digit1Key.wasPressedThisFrame) SwitchTo(0);
                else if (keyboard.digit2Key.wasPressedThisFrame) SwitchTo(1);
                else if (keyboard.digit3Key.wasPressedThisFrame) SwitchTo(2);
                else if (keyboard.digit4Key.wasPressedThisFrame) SwitchTo(3);
            }

            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                float scroll = mouse.scroll.ReadValue().y;
                if (scroll > 0.01f)
                {
                    SwitchTo(activeIndex + 1);
                }
                else if (scroll < -0.01f)
                {
                    SwitchTo(activeIndex - 1);
                }
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
