namespace RollfaehrenFury.Prototype
{
    /// <summary>
    /// Systems a round-end <see cref="AugmentDefinition"/> may modify when applied.
    /// Built by <see cref="AugmentSystem"/> on pick.
    /// </summary>
    public sealed class AugmentContext
    {
        public AugmentContext(GameManager gameManager, EnemySpawner spawner)
        {
            GameManager = gameManager;
            Spawner = spawner;
        }

        public GameManager GameManager { get; }
        public EnemySpawner Spawner { get; }
    }
}
