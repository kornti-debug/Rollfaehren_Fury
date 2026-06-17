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
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileGravity => projectileGravity;
        public float ProjectileLifetime => projectileLifetime;
    }
}
