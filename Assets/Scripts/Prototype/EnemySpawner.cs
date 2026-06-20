using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    [System.Serializable]
    public sealed class EnemySpawnProfile
    {
        [SerializeField] private string displayName = "Enemy";
        [SerializeField] private SimpleEnemy prefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField, Min(1)] private int firstRound = 1;
        [SerializeField, Min(0f)] private float spawnWeight = 1f;
        [SerializeField] private bool useFixedSpawnHeight;
        [SerializeField] private float fixedSpawnHeight;

        public string DisplayName => displayName;
        public SimpleEnemy Prefab => prefab;
        public Transform[] SpawnPoints => spawnPoints;
        public int FirstRound => Mathf.Max(1, firstRound);
        public float SpawnWeight => Mathf.Max(0f, spawnWeight);
        public bool UseFixedSpawnHeight => useFixedSpawnHeight;
        public float FixedSpawnHeight => fixedSpawnHeight;

        public void Configure(
            string profileName,
            SimpleEnemy enemyPrefab,
            Transform[] points,
            int unlockRound,
            float weight,
            bool useFixedHeight,
            float spawnHeight)
        {
            displayName = profileName;
            prefab = enemyPrefab;
            spawnPoints = points;
            firstRound = Mathf.Max(1, unlockRound);
            spawnWeight = Mathf.Max(0f, weight);
            useFixedSpawnHeight = useFixedHeight;
            fixedSpawnHeight = spawnHeight;
        }
    }

    public sealed class EnemySpawner : MonoBehaviour
    {
        [Header("Enemy Types")]
        [SerializeField] private EnemySpawnProfile[] enemyProfiles;

        [Header("Legacy Fish Setup")]
        [SerializeField] private SimpleEnemy enemyPrefab;
        [SerializeField] private FerryDamageTarget ferryTarget;
        [SerializeField] private FerryController ferryController;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnInterval = 1.6f;
        [SerializeField, Range(0f, 1f)] private float spawnStartProgress = 0.05f;
        [SerializeField, Range(0f, 1f)] private float spawnEndProgress = 0.9f;
        [SerializeField] private int baseEnemiesPerRound = 8;
        [SerializeField] private int extraEnemiesPerRound = 5;
        [SerializeField] private int maxAliveEnemies = 14;
        [SerializeField] private int baseKillReward = 10;
        [SerializeField] private float healthScalePerRound = 0.35f;
        [SerializeField] private float speedScalePerRound = 0.15f;
        [SerializeField] private float spawnDelayReductionPerRound = 0.18f;
        [SerializeField] private float fallbackSpawnRadius = 65f;
        [SerializeField] private bool useFixedSpawnHeight = true;
        [SerializeField] private float spawnHeight = 7f;

        [Header("Cluster Spawning")]
        [Tooltip("Each swarm spawns a random count in this range, scattered within clusterRadius so they flock.")]
        [SerializeField, Min(1)] private int minSwarmSize = 3;
        [SerializeField, Min(1)] private int maxSwarmSize = 10;
        [SerializeField, Min(0f)] private float clusterRadius = 5f;
        [Tooltip("Show an on-screen warning when a spawned swarm is at least this big.")]
        [SerializeField, Min(1)] private int bigSwarmWarningThreshold = 12;

        [Header("Procedural Placement")]
        [Tooltip("Spawn each swarm on a ring around the ferry's CURRENT position (any angle, incl. behind) instead of fixed spawn points.")]
        [SerializeField] private bool spawnRelativeToFerry = true;
        [SerializeField] private float spawnRadius = 70f;
        [SerializeField] private float spawnRadiusJitter = 10f;
        [Tooltip("No swarm spawns within this half-angle (deg) of the ferry's forward direction.")]
        [SerializeField, Range(0f, 120f)] private float frontExclusionAngle = 55f;
        [Tooltip("Add the ferry's velocity along the spawn line to each enemy's speed, so swarms spawning behind catch up at the same time as those ahead.")]
        [SerializeField] private bool catchUpSpeedCompensation = true;
        [Tooltip("0 = no catch-up help (behind swarms stay slow), 1 = full equalization (behind swarms very fast).")]
        [SerializeField, Range(0f, 1f)] private float catchUpStrength = 0.25f;
        [Tooltip("Global multiplier on enemy move speed. Below 1 makes all swarms slower.")]
        [SerializeField, Range(0.2f, 2f)] private float enemySpeedScale = 0.6f;

        [Header("Testing Flood (turn off later)")]
        [Tooltip("Ignores crossing pacing and pours swarms in continuously up to floodAliveCap. Test-only.")]
        [SerializeField] private bool floodForTesting = true;
        [SerializeField] private int floodAliveCap = 24;
        [SerializeField] private int floodPerRound = 500;

        [Header("Adaptive Escalation")]
        [Tooltip("If at least this fraction of a swarm is killed within the time window, the next swarm doubles in size and speed. Testing value; production ~0.5.")]
        [SerializeField, Range(0f, 1f)] private float escalationKillFraction = 0.1f;
        [Tooltip("Time window in crossing-progress units (0-1) measured from when the swarm spawned. Testing value; production ~0.5.")]
        [SerializeField, Range(0f, 1f)] private float escalationTimeWindow = 0.9f;
        [SerializeField, Min(1f)] private float maxSwarmMultiplier = 8f;

        private readonly List<SimpleEnemy> aliveEnemies = new List<SimpleEnemy>();
        private Coroutine spawnRoutine;
        private GameManager gameManager;
        private int activeRound = 1;
        private bool isSpawning;
        private float augmentCountMultiplier = 1f;
        private float augmentHealthMultiplier = 1f;

        private float swarmSizeMultiplier = 1f;
        private float swarmSpeedMultiplier = 1f;
        private int roundTargetCount = 1;
        private readonly HashSet<SimpleEnemy> currentWaveMembers = new HashSet<SimpleEnemy>();
        private int currentWaveCount;
        private int currentWaveKills;
        private float currentWaveStartProgress;
        private bool currentWaveEscalated;

        public int AliveCount => aliveEnemies.Count;
        public EnemySpawnProfile[] EnemyProfiles => enemyProfiles;

        public void Configure(SimpleEnemy prefab, FerryDamageTarget target, Transform[] points, GameManager manager)
        {
            enemyPrefab = prefab;
            ferryTarget = target;
            spawnPoints = points;
            gameManager = manager;
            ferryController = target != null ? target.GetComponentInParent<FerryController>() : null;
        }

        public void ConfigureProfiles(EnemySpawnProfile[] profiles, FerryDamageTarget target, GameManager manager)
        {
            enemyProfiles = profiles;
            ferryTarget = target;
            gameManager = manager;
            ferryController = target != null ? target.GetComponentInParent<FerryController>() : null;
        }

        public void BeginRound(int round, GameManager manager)
        {
            gameManager = manager;
            activeRound = Mathf.Max(1, round);
            if (ferryController == null && ferryTarget != null)
            {
                ferryController = ferryTarget.GetComponentInParent<FerryController>();
            }

            StopRound(true);
            ResetEscalation();
            isSpawning = true;
            spawnRoutine = StartCoroutine(SpawnRound());
        }

        public void StopRound(bool clearAlive)
        {
            isSpawning = false;
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }

            if (clearAlive)
            {
                ClearAliveEnemies();
            }
        }

        public void ClearAliveEnemies()
        {
            for (int i = aliveEnemies.Count - 1; i >= 0; i--)
            {
                SimpleEnemy enemy = aliveEnemies[i];
                if (enemy != null)
                {
                    enemy.Removed -= HandleEnemyRemoved;
                    Destroy(enemy.gameObject);
                }
            }

            aliveEnemies.Clear();
        }

        public void AddCountMultiplier(float multiplier)
        {
            augmentCountMultiplier = Mathf.Max(0.1f, augmentCountMultiplier * multiplier);
        }

        public void AddHealthMultiplier(float multiplier)
        {
            augmentHealthMultiplier = Mathf.Max(0.1f, augmentHealthMultiplier * multiplier);
        }

        public void ResetAugments()
        {
            augmentCountMultiplier = 1f;
            augmentHealthMultiplier = 1f;
        }

        private IEnumerator SpawnRound()
        {
            roundTargetCount = floodForTesting
                ? Mathf.Max(1, floodPerRound)
                : Mathf.Max(1, Mathf.RoundToInt((baseEnemiesPerRound + (activeRound - 1) * extraEnemiesPerRound) * augmentCountMultiplier));
            int aliveCap = floodForTesting ? Mathf.Max(1, floodAliveCap) : maxAliveEnemies;
            int spawned = 0;
            float startProgress = Mathf.Clamp01(spawnStartProgress);
            float endProgress = Mathf.Clamp(spawnEndProgress, startProgress, 1f);

            while (isSpawning && spawned < roundTargetCount)
            {
                float targetProgress = roundTargetCount <= 1
                    ? startProgress
                    : Mathf.Lerp(startProgress, endProgress, spawned / (float)(roundTargetCount - 1));

                bool gateOpen = floodForTesting || GetCrossingProgress() >= targetProgress;
                if (!gateOpen || aliveEnemies.Count >= aliveCap)
                {
                    yield return null;
                    continue;
                }

                // Drop a whole swarm around one procedurally chosen origin so it forms immediately.
                // Size is randomized per wave, then scaled by the escalation multiplier.
                Vector3 clusterCenter = GetClusterCenter();
                int low = Mathf.Min(minSwarmSize, maxSwarmSize);
                int high = Mathf.Max(minSwarmSize, maxSwarmSize);
                int burst = Mathf.Max(1, Mathf.RoundToInt(Random.Range(low, high + 1) * swarmSizeMultiplier));
                if (burst >= bigSwarmWarningThreshold)
                {
                    gameManager?.ShowSwarmWarning(burst);
                }

                BeginWave();
                for (int i = 0; i < burst && spawned < roundTargetCount && aliveEnemies.Count < aliveCap; i++)
                {
                    SimpleEnemy spawnedEnemy = SpawnEnemy(clusterCenter);
                    if (spawnedEnemy != null)
                    {
                        currentWaveMembers.Add(spawnedEnemy);
                        currentWaveCount++;
                    }

                    spawned++;
                }

                yield return new WaitForSeconds(floodForTesting ? 0.4f : GetSpawnDelay());
            }
        }

        private SimpleEnemy SpawnEnemy(Vector3? clusterCenter = null)
        {
            if (ferryTarget == null)
            {
                Debug.LogWarning("EnemySpawner is missing the ferry target.", this);
                return null;
            }

            EnemySpawnProfile profile = SelectProfile();
            SimpleEnemy prefab = profile != null ? profile.Prefab : enemyPrefab;
            if (prefab == null)
            {
                Debug.LogWarning("EnemySpawner has no eligible enemy prefab for this round.", this);
                return null;
            }

            Vector3 spawnPosition = clusterCenter.HasValue
                ? ApplyClusterOffset(clusterCenter.Value, profile)
                : GetSpawnPosition(profile);
            SimpleEnemy enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
            string profileName = profile != null && !string.IsNullOrWhiteSpace(profile.DisplayName)
                ? profile.DisplayName
                : prefab.name;
            enemy.name = $"{profileName} R{activeRound}";
            enemy.Removed += HandleEnemyRemoved;
            aliveEnemies.Add(enemy);

            float speedMultiplier = (1f + (activeRound - 1) * speedScalePerRound) * swarmSpeedMultiplier * enemySpeedScale;
            float healthMultiplier = (1f + (activeRound - 1) * healthScalePerRound) * augmentHealthMultiplier;
            int reward = Mathf.Max(baseKillReward, enemy.KillReward) + (activeRound - 1) * 2;
            float speedBonus = ComputeCatchUpBonus(spawnPosition);
            enemy.Initialize(ferryTarget, gameManager, reward, speedMultiplier, healthMultiplier, speedBonus);
            return enemy;
        }

        private EnemySpawnProfile SelectProfile()
        {
            if (enemyProfiles == null || enemyProfiles.Length == 0)
            {
                return null;
            }

            float totalWeight = 0f;
            for (int i = 0; i < enemyProfiles.Length; i++)
            {
                EnemySpawnProfile profile = enemyProfiles[i];
                if (IsEligible(profile))
                {
                    totalWeight += profile.SpawnWeight;
                }
            }

            if (totalWeight <= 0f)
            {
                return null;
            }

            float selection = Random.value * totalWeight;
            for (int i = 0; i < enemyProfiles.Length; i++)
            {
                EnemySpawnProfile profile = enemyProfiles[i];
                if (!IsEligible(profile))
                {
                    continue;
                }

                selection -= profile.SpawnWeight;
                if (selection <= 0f)
                {
                    return profile;
                }
            }

            return null;
        }

        private bool IsEligible(EnemySpawnProfile profile)
        {
            return profile != null
                && profile.Prefab != null
                && profile.SpawnWeight > 0f
                && activeRound >= profile.FirstRound;
        }

        private Vector3 GetSpawnPosition(EnemySpawnProfile profile)
        {
            Transform[] points = profile != null ? profile.SpawnPoints : spawnPoints;
            if (points != null && points.Length > 0)
            {
                int startIndex = Random.Range(0, points.Length);
                for (int i = 0; i < points.Length; i++)
                {
                    Transform point = points[(startIndex + i) % points.Length];
                    if (point != null && IsAheadOrBesideFerry(point.position))
                    {
                        return ApplySpawnHeight(point.position, profile);
                    }
                }
            }

            Vector3 center = ferryTarget != null ? ferryTarget.transform.position : transform.position;
            Transform ferryTransform = ferryTarget != null ? ferryTarget.transform : transform;
            Vector3 forward = Vector3.ProjectOnPlane(ferryTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(ferryTransform.right, Vector3.up).normalized;
            float forwardDistance = fallbackSpawnRadius * Random.Range(0.65f, 1f);
            float sideDistance = fallbackSpawnRadius * Random.Range(-0.8f, 0.8f);
            return ApplySpawnHeight(center + forward * forwardDistance + right * sideDistance, profile);
        }

        private bool IsAheadOrBesideFerry(Vector3 position)
        {
            if (ferryTarget == null)
            {
                return true;
            }

            Transform ferryTransform = ferryTarget.transform;
            Vector3 forward = Vector3.ProjectOnPlane(ferryTransform.forward, Vector3.up);
            Vector3 offset = Vector3.ProjectOnPlane(position - ferryTransform.position, Vector3.up);
            if (forward.sqrMagnitude < 0.001f || offset.sqrMagnitude < 0.001f)
            {
                return true;
            }

            return Vector3.Dot(forward.normalized, offset.normalized) >= 0f;
        }

        private Vector3 ApplySpawnHeight(Vector3 position, EnemySpawnProfile profile)
        {
            bool fixedHeight = profile != null ? profile.UseFixedSpawnHeight : useFixedSpawnHeight;
            if (fixedHeight)
            {
                position.y = profile != null ? profile.FixedSpawnHeight : spawnHeight;
            }

            return position;
        }

        private Vector3 ApplyClusterOffset(Vector3 center, EnemySpawnProfile profile)
        {
            Vector2 offset = Random.insideUnitCircle * clusterRadius;
            return ApplySpawnHeight(center + new Vector3(offset.x, 0f, offset.y), profile);
        }

        // Procedural swarm origin: a point on a ring around the ferry's CURRENT position
        // (any angle, including behind) so swarms keep appearing around the moving ferry.
        private Vector3 GetClusterCenter()
        {
            if (spawnRelativeToFerry && ferryTarget != null)
            {
                Vector3 ferryForward = Vector3.ProjectOnPlane(ferryTarget.transform.forward, Vector3.up).normalized;
                float frontCos = Mathf.Cos(frontExclusionAngle * Mathf.Deg2Rad);
                Vector3 direction = Vector3.forward;
                for (int attempt = 0; attempt < 12; attempt++)
                {
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

                    // Reject directions inside the forbidden cone right in front of the ferry.
                    if (ferryForward.sqrMagnitude < 0.0001f || Vector3.Dot(direction, ferryForward) < frontCos)
                    {
                        break;
                    }
                }

                float radius = Mathf.Max(1f, spawnRadius + Random.Range(-spawnRadiusJitter, spawnRadiusJitter));
                Vector3 center = ferryTarget.transform.position + direction * radius;
                return ApplySpawnHeight(center, null);
            }

            return GetSpawnPosition(null);
        }

        // Adds the ferry's velocity component along the enemy->ferry line so that, regardless
        // of spawn angle, every enemy's closing speed is base + ferrySpeed -> the same catch-up
        // time as one spawned directly ahead. Enemies behind the ferry get the biggest boost.
        private float ComputeCatchUpBonus(Vector3 spawnPosition)
        {
            if (!catchUpSpeedCompensation || ferryController == null || !ferryController.IsCrossing || ferryTarget == null)
            {
                return 0f;
            }

            Vector3 ferryVelocity = ferryController.Velocity;
            float ferrySpeed = ferryVelocity.magnitude;
            if (ferrySpeed < 0.01f)
            {
                return 0f;
            }

            Vector3 toFerry = ferryTarget.transform.position - spawnPosition;
            toFerry.y = 0f;
            if (toFerry.sqrMagnitude < 0.0001f)
            {
                return 0f;
            }

            Vector3 lineDirection = toFerry.normalized;
            Vector3 ferryDirection = ferryVelocity / ferrySpeed;
            return Mathf.Max(0f, catchUpStrength * ferrySpeed * (1f + Vector3.Dot(ferryDirection, lineDirection)));
        }

        private void BeginWave()
        {
            currentWaveMembers.Clear();
            currentWaveCount = 0;
            currentWaveKills = 0;
            currentWaveStartProgress = GetCrossingProgress();
            currentWaveEscalated = false;
        }

        // If the player clears enough of the current swarm quickly, the next swarm doubles
        // in size and speed (capped by maxSwarmMultiplier). Triggers once per wave.
        private void TryEscalateNextWave()
        {
            if (currentWaveEscalated || currentWaveCount <= 0)
            {
                return;
            }

            float killedFraction = currentWaveKills / (float)currentWaveCount;
            float elapsed = GetCrossingProgress() - currentWaveStartProgress;
            if (killedFraction >= escalationKillFraction && elapsed <= escalationTimeWindow)
            {
                currentWaveEscalated = true;
                swarmSizeMultiplier = Mathf.Min(maxSwarmMultiplier, swarmSizeMultiplier * 2f);
                swarmSpeedMultiplier = Mathf.Min(maxSwarmMultiplier, swarmSpeedMultiplier * 2f);
                roundTargetCount += Mathf.Max(1, Mathf.RoundToInt(maxSwarmSize * swarmSizeMultiplier));
                Debug.Log($"[Swarm] Cleared fast - next swarm x{swarmSizeMultiplier:0} size / x{swarmSpeedMultiplier:0} speed.", this);
            }
        }

        private void ResetEscalation()
        {
            swarmSizeMultiplier = 1f;
            swarmSpeedMultiplier = 1f;
            currentWaveMembers.Clear();
            currentWaveCount = 0;
            currentWaveKills = 0;
            currentWaveEscalated = false;
        }

        private float GetSpawnDelay()
        {
            return Mathf.Max(0.35f, spawnInterval - (activeRound - 1) * spawnDelayReductionPerRound);
        }

        private float GetCrossingProgress()
        {
            return ferryController != null && ferryController.IsCrossing
                ? ferryController.Progress
                : 0f;
        }

        private void HandleEnemyRemoved(SimpleEnemy enemy)
        {
            enemy.Removed -= HandleEnemyRemoved;
            aliveEnemies.Remove(enemy);

            if (currentWaveMembers.Remove(enemy) && enemy.WasKilledByDamage)
            {
                currentWaveKills++;
                TryEscalateNextWave();
            }
        }
    }
}
