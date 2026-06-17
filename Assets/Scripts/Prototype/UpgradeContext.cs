namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// The set of systems an <see cref="UpgradeDefinition"/> may modify when applied.
    /// Built by <see cref="GameManager"/> on purchase and passed to <c>Apply</c>.
    /// </summary>
    public sealed class UpgradeContext
    {
        public UpgradeContext(WeaponSystem weaponSystem, Health ferryHealth)
        {
            WeaponSystem = weaponSystem;
            FerryHealth = ferryHealth;
        }

        public WeaponSystem WeaponSystem { get; }
        public Health FerryHealth { get; }
    }
}
