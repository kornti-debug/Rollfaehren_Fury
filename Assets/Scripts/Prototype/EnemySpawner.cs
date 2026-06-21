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
        [SerializeField, Min(1)] private int maxSwarmSize = 8;
        [SerializeField, Min(0f)] private float clusterRadius = 5f;
        [Tooltip("Seconds between swarm spawns.")]
        [SerializeField, Min(0.1f)] private float swarmInterval = 1.5f;

        [Header("Procedural Placement")]
        [Tooltip("Spawn each swarm on a ring around the ferry's CURRENT position (any angle, incl. behind) instead of fixed spawn points.")]
        [SerializeField] private bool spawnRelativeToFerry = true;
        [SerializeField] private float spawnRadius = 55f;
        [SerializeField] private float spawnRadiusJitter = 10f;
        [Tooltip("How far AHEAD of the moving ferry swarms spawn so they reach its side (multiplier on the computed intercept lead).")]
        [SerializeField, Min(0f)] private float spawnLeadFactor = 1.5f;
        [Tooltip("Swarms only spawn over this water surface's XZ bounds. Found by name at runtime if not assigned.")]
        [SerializeField] private Renderer waterRenderer;
        [SerializeField] private string waterObjectName = "River Water Surface";
        [Header("Enemy Speed")]
        [Tooltip("Absolute move speed (m/s) of a round-1 enemy. The ferry crosses at ~6 m/s; below that, enemies trail it.")]
        [SerializeField] private float enemyBaseSpeed = 7f;
        [SerializeField] private float enemySpeedPerRound = 0.3f;

        [Header("Testing Flood (turn off later)")]
        [Tooltip("Ignores crossing pacing and pours swarms in continuously up to floodAliveCap. Test-only.")]
        [SerializeField] private bool floodForTesting = true;
        [SerializeField] private int floodAliveCap = 31;
        [SerializeField] private int floodPerRound = 500;

        private readonly List<SimpleEnemy> aliveEnemies = new List<SimpleEnemy>();
        private Coroutine spawnRoutine;
        private GameManager gameManager;
        private int activeRound = 1;
        private bool isSpawning;
        private float augmentCountMultiplier = 1f;
        private float augmentHealthMultiplier = 1f;
        private float augmentSpeedMultiplier = 1f;
        private float augmentRewardMultiplier = 1f;

        private int roundTargetCount = 1;

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

        public void AddSpeedMultiplier(float multiplier)
        {
            augmentSpeedMultiplier = Mathf.Max(0.1f, augmentSpeedMultiplier * multiplier);
        }

        public void AddRewardMultiplier(float multiplier)
        {
            augmentRewardMultiplier = Mathf.Max(0.1f, augmentRewardMultiplier * multiplier);
        }

        public void ResetAugments()
        {
            augmentCountMultiplier = 1f;
            augmentHealthMultiplier = 1f;
            augmentSpeedMultiplier = 1f;
            augmentRewardMultiplier = 1f;
        }

        private IEnumerator SpawnRound()
        {
            roundTargetCount = floodForTesting
                ? Mathf.Max(1, floodPerRound)
                : Mathf.Max(1, Mathf.RoundToInt((baseEnemiesPerRound + (activeRound - 1) * extraEnemiesPerRound) * augmentCountMultiplier));
            int aliveCap = floodForTesting ? Mathf.Max(1, floodAliveCap) : maxAliveEnemies;
            int spawned = 0;
            Debug.Log($"[SpeedCheck] round {activeRound}: enemy speed = {enemyBaseSpeed + (activeRound - 1) * enemySpeedPerRound:0.0} m/s | ferry = {(ferryController != null ? ferryController.CurrentSpeed : 0f):0.0} m/s", this);
            float startProgress = Mathf.Clamp01(spawnStartProgress);
            float endProgress = Mathf.Clamp(spawnEndProgress, startProgress, 1f);

            while (isSpawning && (floodForTesting || spawned < roundTargetCount))
            {
                float targetProgress = roundTargetCount <= 1
                    ? startProgress
                    : Mathf.Lerp(startProgress, endProgress, spawned / (float)(roundTargetCount - 1));

                bool gateOpen = floodForTesting || GetCrossingProgress() >= targetProgress;
                if (!gateOpen || (!floodForTesting && aliveEnemies.Count >= aliveCap))
                {
                    yield return null;
                    continue;
                }

                // Drop a whole swarm around one procedurally chosen origin so it forms immediately.
                Vector3 clusterCenter = GetClusterCenter();
                int low = Mathf.Min(minSwarmSize, maxSwarmSize);
                int high = Mathf.Max(minSwarmSize, maxSwarmSize);
                int burst = Random.Range(low, high + 1);
                for (int i = 0; i < burst && (floodForTesting || (spawned < roundTargetCount && aliveEnemies.Count < aliveCap)); i++)
                {
                    SpawnEnemy(clusterCenter);
                    spawned++;
                }

                yield return new WaitForSeconds(floodForTesting ? swarmInterval : GetSpawnDelay());
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

            float speed = (enemyBaseSpeed + (activeRound - 1) * enemySpeedPerRound) * augmentSpeedMultiplier;
            float healthMultiplier = (1f + (activeRound - 1) * healthScalePerRound) * augmentHealthMultiplier;
            int reward = Mathf.RoundToInt((Mathf.Max(baseKillReward, enemy.KillReward) + (activeRound - 1) * 2) * augmentRewardMultiplier);
            enemy.Initialize(ferryTarget, gameManager, reward, speed, healthMultiplier);
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
                Vector3 ferryPosition = ferryTarget.transform.position;

                // Travel direction from the ferry's velocity; fall back to its facing while docked.
                Vector3 forward = ferryController != null ? ferryController.Velocity : Vector3.zero;
                forward = Vector3.ProjectOnPlane(forward, Vector3.up);
                float ferrySpeed = forward.magnitude;
                if (ferrySpeed > 0.01f)
                {
                    forward /= ferrySpeed;
                }
                else
                {
                    forward = Vector3.ProjectOnPlane(ferryTarget.transform.forward, Vector3.up).normalized;
                    if (forward.sqrMagnitude < 0.0001f)
                    {
                        forward = Vector3.forward;
                    }
                }

                Vector3 right = Vector3.Cross(Vector3.up, forward);

                float side = Random.value < 0.5f ? 1f : -1f;
                float lateral = spawnRadius * Random.Range(0.5f, 1f);
                // Lead: the ferry advances this far while an enemy (speed enemyBaseSpeed) crosses to
                // its side, so spawning that far ahead makes them converge on the SIDE, not behind.
                float lead = ferrySpeed * (lateral / Mathf.Max(0.1f, enemyBaseSpeed)) * spawnLeadFactor;

                Vector3 offset = forward * lead + right * (side * lateral);
                float distance = offset.magnitude;
                Vector3 direction = distance > 0.001f ? offset / distance : right * side;

                // Walk in along the spawn line until it sits on the water.
                for (float r = distance; r >= 8f; r -= 10f)
                {
                    Vector3 candidate = ferryPosition + direction * r;
                    if (IsOverWater(candidate))
                    {
                        return ApplySpawnHeight(candidate, null);
                    }
                }

                return ApplySpawnHeight(ferryPosition, null);
            }

            return GetSpawnPosition(null);
        }

        private Renderer GetWaterRenderer()
        {
            if (waterRenderer == null && !string.IsNullOrEmpty(waterObjectName))
            {
                GameObject waterObject = GameObject.Find(waterObjectName);
                if (waterObject != null)
                {
                    waterRenderer = waterObject.GetComponent<Renderer>();
                }
            }

            return waterRenderer;
        }

        private bool IsOverWater(Vector3 position)
        {
            Renderer water = GetWaterRenderer();
            if (water == null)
            {
                return true; // No water reference -> don't block spawning.
            }

            Bounds bounds = water.bounds;
            const float margin = 12f; // keep spawns off the shoreline / dock edges
            return position.x >= bounds.min.x + margin && position.x <= bounds.max.x - margin
                && position.z >= bounds.min.z + margin && position.z <= bounds.max.z - margin;
        }

        private Vector3 ClampToWater(Vector3 position)
        {
            Renderer water = GetWaterRenderer();
            if (water == null)
            {
                return position;
            }

            Bounds bounds = water.bounds;
            const float margin = 4f;
            position.x = Mathf.Clamp(position.x, bounds.min.x + margin, bounds.max.x - margin);
            position.z = Mathf.Clamp(position.z, bounds.min.z + margin, bounds.max.z - margin);
            return position;
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
        }
    }
}
