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
        public int ActiveAmmo => ActiveWeapon != null ? ActiveWeapon.CurrentAmmo : 0;
        public int ActiveMagazineSize => ActiveWeapon != null ? ActiveWeapon.MagazineSize : 0;
        public int ActiveReserveAmmo => ActiveWeapon != null ? ActiveWeapon.ReserveAmmo : 0;
        public bool ActiveHasInfiniteAmmo => ActiveWeapon == null || ActiveWeapon.HasInfiniteAmmo;
        public bool ActiveIsReloading => ActiveWeapon != null && ActiveWeapon.IsReloading;
        public float ActiveReloadProgress => ActiveWeapon != null ? ActiveWeapon.ReloadProgress01 : 1f;

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
            RefreshEquipped();
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
            HandleReloadInput();

            // Held-fire only continues for automatic weapons; semi-auto fires once per press
            // (handled in HandleFirePerformed), so the pistol needs a click per shot.
            if (fireHeld && ActiveWeapon != null && ActiveWeapon.IsAutomatic)
            {
                ActiveWeapon.Fire(fireCamera, ignoredRoot, hitMask);
            }
        }

        private void HandleReloadInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                ActiveWeapon?.Reload();
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

        public void MultiplyDamageToActive(float factor)
        {
            ActiveWeapon?.MultiplyDamage(factor);
        }

        public void MultiplyActiveCooldown(float multiplier)
        {
            ActiveWeapon?.MultiplyCooldown(multiplier);
        }

        public void AddRicochetToActive(int bounces)
        {
            ActiveWeapon?.AddRicochet(bounces);
        }

        public void AddMagazineSizeToActive(int amount)
        {
            ActiveWeapon?.AddMagazineSize(amount);
        }

        public void AddReserveMagazinesToActive(int magazines)
        {
            ActiveWeapon?.AddReserveMagazines(magazines);
        }

        public void MultiplyActiveReloadDuration(float multiplier)
        {
            ActiveWeapon?.MultiplyReloadDuration(multiplier);
        }

        /// <summary>Augment hook: shortens reload time on every weapon.</summary>
        public void MultiplyAllReloadDuration(float multiplier)
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                weapons[i]?.MultiplyReloadDuration(multiplier);
            }
        }

        /// <summary>Augment hook: every weapon gains a timed damage boost after each reload.</summary>
        public void EnableReloadDamageBuffOnAll(float multiplier, float duration)
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                weapons[i]?.EnableReloadDamageBuff(multiplier, duration);
            }
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
            ActiveWeapon?.SetEquipped(false);
            activeIndex = wrapped;
            ActiveWeapon?.SetEquipped(true);
            SubscribeWeapon(ActiveWeapon);
            WeaponChanged?.Invoke(ActiveWeapon);
        }

        /// <summary>Marks only the active weapon as equipped so holstered weapons pause their reload.</summary>
        private void RefreshEquipped()
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                weapons[i]?.SetEquipped(i == activeIndex);
            }
        }

        /// <summary>Tops every weapon's magazine and reserve back to full (e.g. restock at the dock).</summary>
        public void RefillAllAmmo()
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                weapons[i]?.RefillAmmo();
            }
        }

        /// <summary>Resets every weapon to its definition defaults for a fresh run (clears upgrades + augment buffs, refills ammo).</summary>
        public void ResetAllWeapons()
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                weapons[i]?.ResetStats();
            }
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
