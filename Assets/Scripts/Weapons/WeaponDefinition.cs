using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    public enum WeaponFireMode
    {
        Hitscan,
        Spread,
        Projectile
    }

    /// <summary>
    /// Data-driven definition of a weapon. A single runtime <see cref="Weapon"/>
    /// reads one of these and fires according to <see cref="FireMode"/>. Stats are
    /// copied to runtime fields on the weapon, so upgrades never mutate this asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Rollfaehren Fury/Weapon Definition", fileName = "WeaponDefinition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Weapon";
        [SerializeField] private WeaponFireMode fireMode = WeaponFireMode.Hitscan;
        [SerializeField] private float damage = 25f;
        [SerializeField] private float range = 250f;
        [SerializeField] private float fireCooldown = 0.2f;
        [SerializeField] private float aimAssistRadius = 0.45f;
        [SerializeField, Min(1)] private int pelletsPerShot = 1;
        [SerializeField] private float spreadAngle = 0f;

        [Header("Firing & Reload")]
        [Tooltip("When true, holding fire keeps shooting at the cooldown. When false the weapon is semi-auto: one shot per press.")]
        [SerializeField] private bool automatic = false;
        [Tooltip("Rounds per magazine. 0 (or less) = unlimited ammo, no reload, no reserve.")]
        [SerializeField, Min(0)] private int magazineSize = 0;
        [Tooltip("Seconds to refill an empty magazine.")]
        [SerializeField, Min(0.1f)] private float reloadTime = 1.5f;
        [Tooltip("Spare magazines carried in reserve (beyond the loaded one). Total reserve ammo = this x magazineSize. Ignored for unlimited weapons.")]
        [SerializeField, Min(0)] private int reserveMagazines = 6;

        [Header("Progression")]
        [Tooltip("Owned automatically when a new run begins.")]
        [SerializeField] private bool initiallyUnlocked;
        [Tooltip("Money required to unlock this weapon in the shop.")]
        [SerializeField, Min(0)] private int unlockPrice;
        [Tooltip("Earliest round preparation in which this weapon can be unlocked.")]
        [SerializeField, Min(1)] private int minimumUnlockRound = 1;

        [Header("Projectile (fire mode = Projectile)")]
        [SerializeField] private float projectileSpeed = 40f;
        [SerializeField] private float projectileGravity = 18f;
        [SerializeField] private float projectileLifetime = 4f;

        public string DisplayName => displayName;
        public WeaponFireMode FireMode => fireMode;
        public float Damage => damage;
        public float Range => range;
        public float FireCooldown => fireCooldown;
        public float AimAssistRadius => aimAssistRadius;
        public int PelletsPerShot => Mathf.Max(1, pelletsPerShot);
        public float SpreadAngle => Mathf.Max(0f, spreadAngle);
        public bool Automatic => automatic;
        public int MagazineSize => Mathf.Max(0, magazineSize);
        public float ReloadTime => Mathf.Max(0.1f, reloadTime);
        public int ReserveMagazines => Mathf.Max(0, reserveMagazines);
        public bool InitiallyUnlocked => initiallyUnlocked;
        public int UnlockPrice => Mathf.Max(0, unlockPrice);
        public int MinimumUnlockRound => Mathf.Max(1, minimumUnlockRound);
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileGravity => projectileGravity;
        public float ProjectileLifetime => projectileLifetime;
    }
}
