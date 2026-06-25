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
        [SerializeField] private int baseKillReward = 10;
        [Tooltip("Scales the per-kill reward. Flat (no per-round growth) so income grows with how many you kill, not exponentially. 0.3 x base 10 = 3 gold/kill.")]
        [SerializeField, Min(0f)] private float killRewardScale = 0.3f;
        [SerializeField] private float healthScalePerRound = 0.35f;
        [SerializeField] private float fallbackSpawnRadius = 65f;
        [SerializeField] private bool useFixedSpawnHeight = true;
        [SerializeField] private float spawnHeight = 7f;
        [Tooltip("Cruising altitude for flying enemies (birds): they hold this height while approaching, well above the fish, then dive onto the ferry once in attack range. Renamed from flyingSpawnHeight so the old lower scene value is dropped and this default applies.")]
        [SerializeField] private float birdCruiseAltitude = 24f;
        [Tooltip("Random +/- vertical scatter on the cruise altitude so a swarm is not a flat sheet.")]
        [SerializeField] private float birdCruiseAltitudeJitter = 3f;

        [Header("Cluster Spawning")]
        [Tooltip("Enemies in one swarm are scattered within this radius so they flock together.")]
        [SerializeField, Min(0f)] private float clusterRadius = 5f;

        [Header("Round Progression")]
        [Tooltip("Largest a single swarm can be in round 1. The max grows by swarmSizeGrowthPerRound each round after that.")]
        [SerializeField, Min(1)] private int swarmSizeRound1Max = 1;
        [Tooltip("How much a swarm's MAX size grows per round after round 1.")]
        [SerializeField, Min(0f)] private float swarmSizeGrowthPerRound = 1f;
        [Tooltip("A swarm rolls a random count in [maxSize - swarmSizeRange, maxSize], floored at 1. With range 1: round 1 = 1, round 2 = 1-2, round 3 = 2-3, ...")]
        [SerializeField, Min(0)] private int swarmSizeRange = 1;
        [Tooltip("Hard cap on a single swarm so very late rounds stay sane.")]
        [SerializeField, Min(1)] private int swarmSizeCap = 16;
        [Tooltip("Seconds between swarm spawns in round 1 (before the first-round factor below).")]
        [SerializeField, Min(0.2f)] private float baseSwarmInterval = 2.4f;
        [Tooltip("Seconds shaved off the spawn interval each round, floored at minSwarmInterval.")]
        [SerializeField, Min(0f)] private float intervalStepPerRound = 0.18f;
        [Tooltip("Fastest the swarm interval ever gets, no matter how high the round.")]
        [SerializeField, Min(0.2f)] private float minSwarmInterval = 0.8f;
        [Tooltip("Extra slow-down on the round-1 interval only (>1 = gentler first crossing, beatable with harpoon/pistol).")]
        [SerializeField, Min(1f)] private float firstRoundIntervalFactor = 1.5f;

        [Header("Procedural Placement")]
        [Tooltip("Spawn each swarm on a ring around the ferry's CURRENT position (any angle, incl. behind) instead of fixed spawn points.")]
        [SerializeField] private bool spawnRelativeToFerry = true;
        [SerializeField] private float spawnRadius = 70f;
        [Tooltip("How far AHEAD of the moving ferry swarms spawn so they reach its side (multiplier on the computed intercept lead).")]
        [SerializeField, Min(0f)] private float spawnLeadFactor = 1.5f;
        [Tooltip("Swarms only spawn over this water surface's XZ bounds. Found by name at runtime if not assigned.")]
        [SerializeField] private Renderer waterRenderer;
        [SerializeField] private string waterObjectName = "River Water Surface";
        [Header("Enemy Speed")]
        [Tooltip("Absolute move speed (m/s) of a round-1 enemy. The ferry crosses at ~6 m/s; below that, enemies trail it.")]
        [SerializeField] private float enemyBaseSpeed = 7f;
        [SerializeField] private float enemySpeedPerRound = 0.3f;

        private readonly List<SimpleEnemy> aliveEnemies = new List<SimpleEnemy>();
        private Coroutine spawnRoutine;
        private GameManager gameManager;
        private int activeRound = 1;
        private bool isSpawning;
        private float augmentCountMultiplier = 1f;
        private float augmentHealthMultiplier = 1f;
        private float augmentSpeedMultiplier = 1f;
        private float augmentRewardMultiplier = 1f;

        public int AliveCount => aliveEnemies.Count;
        public IReadOnlyList<SimpleEnemy> AliveEnemies => aliveEnemies;
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
            // Progressive difficulty: swarms grow and arrive faster every round. Round 1 stays
            // small + slow (beatable with just harpoon/pistol); later rounds ramp up.
            // Swarm size scales with the round: the MAX grows by one per round (R1 max = 1, R2 = 2, ...)
            // and the swarm rolls a random size in [max - swarmSizeRange, max], floored at 1. With the
            // default range of 1 that gives R1 = 1, R2 = 1-2, R3 = 2-3, and so on.
            int maxSize = Mathf.Clamp(
                swarmSizeRound1Max + Mathf.FloorToInt((activeRound - 1) * swarmSizeGrowthPerRound),
                1,
                swarmSizeCap);
            int low = Mathf.Clamp(maxSize - swarmSizeRange, 1, maxSize);
            int high = maxSize;

            float interval = Mathf.Max(minSwarmInterval, baseSwarmInterval - (activeRound - 1) * intervalStepPerRound);
            if (activeRound <= 1)
            {
                // Ease the player in: the very first crossing spawns at an even slower interval.
                interval *= Mathf.Max(1f, firstRoundIntervalFactor);
            }

            while (isSpawning)
            {
                // One swarm = one enemy type, around one origin. Picking the profile per swarm (not
                // per enemy) keeps fish and birds in separate clusters at their own spawn heights,
                // so they no longer pile up on top of each other.
                EnemySpawnProfile profile = SelectProfile();
                Vector3 clusterCenter = GetClusterCenter(profile);
                int burst = Mathf.Max(1, Mathf.RoundToInt(Random.Range(low, high + 1) * augmentCountMultiplier));
                for (int i = 0; i < burst; i++)
                {
                    SpawnEnemy(clusterCenter, profile);
                }

                yield return new WaitForSeconds(interval);
            }
        }

        private SimpleEnemy SpawnEnemy(Vector3 clusterCenter, EnemySpawnProfile profile)
        {
            if (ferryTarget == null)
            {
                Debug.LogWarning("EnemySpawner is missing the ferry target.", this);
                return null;
            }

            SimpleEnemy prefab = profile != null ? profile.Prefab : enemyPrefab;
            if (prefab == null)
            {
                Debug.LogWarning("EnemySpawner has no eligible enemy prefab for this round.", this);
                return null;
            }

            Vector3 spawnPosition = ApplyClusterOffset(clusterCenter, profile);
            SimpleEnemy enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
            if (enemy.GetComponent<AkGameObj>() == null)
            {
                enemy.gameObject.AddComponent<AkGameObj>();
            }

            if (enemy.GetComponent<EnemyMovementAudio>() == null)
            {
                enemy.gameObject.AddComponent<EnemyMovementAudio>();
            }

            string profileName = profile != null && !string.IsNullOrWhiteSpace(profile.DisplayName)
                ? profile.DisplayName
                : prefab.name;
            enemy.name = $"{profileName} R{activeRound}";
            enemy.Removed += HandleEnemyRemoved;
            aliveEnemies.Add(enemy);

            float speed = (enemyBaseSpeed + (activeRound - 1) * enemySpeedPerRound) * augmentSpeedMultiplier;
            float healthMultiplier = (1f + (activeRound - 1) * healthScalePerRound) * augmentHealthMultiplier;
            int reward = Mathf.RoundToInt(Mathf.Max(baseKillReward, enemy.KillReward) * killRewardScale * augmentRewardMultiplier);
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
                // Surface enemies (fish) are pinned to the water plane.
                position.y = profile != null ? profile.FixedSpawnHeight : spawnHeight;
            }
            else if (profile != null)
            {
                // Flying enemies (birds) spawn at their cruise altitude, well above the fish, with a
                // little vertical scatter so a swarm does not form a flat sheet. They hold this height
                // while approaching and only descend once they commit to a dive (see SimpleEnemy).
                position.y = birdCruiseAltitude + Random.Range(-birdCruiseAltitudeJitter, birdCruiseAltitudeJitter);
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
        // The height comes from the swarm's profile (fish on the water, birds in the air).
        private Vector3 GetClusterCenter(EnemySpawnProfile profile)
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
                        return ApplySpawnHeight(candidate, profile);
                    }
                }

                return ApplySpawnHeight(ferryPosition, profile);
            }

            return GetSpawnPosition(profile);
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

        private void HandleEnemyRemoved(SimpleEnemy enemy)
        {
            enemy.Removed -= HandleEnemyRemoved;
            aliveEnemies.Remove(enemy);
        }
    }
}
