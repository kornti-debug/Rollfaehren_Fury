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

        private readonly List<SimpleEnemy> aliveEnemies = new List<SimpleEnemy>();
        private Coroutine spawnRoutine;
        private GameManager gameManager;
        private int activeRound = 1;
        private bool isSpawning;
        private float augmentCountMultiplier = 1f;
        private float augmentHealthMultiplier = 1f;

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

        public void ResetAugments()
        {
            augmentCountMultiplier = 1f;
            augmentHealthMultiplier = 1f;
        }

        private IEnumerator SpawnRound()
        {
            int targetCount = Mathf.Max(1, Mathf.RoundToInt((baseEnemiesPerRound + (activeRound - 1) * extraEnemiesPerRound) * augmentCountMultiplier));
            int spawned = 0;
            float startProgress = Mathf.Clamp01(spawnStartProgress);
            float endProgress = Mathf.Clamp(spawnEndProgress, startProgress, 1f);

            while (isSpawning && spawned < targetCount)
            {
                float targetProgress = targetCount <= 1
                    ? startProgress
                    : Mathf.Lerp(startProgress, endProgress, spawned / (float)(targetCount - 1));

                if (GetCrossingProgress() < targetProgress || aliveEnemies.Count >= maxAliveEnemies)
                {
                    yield return null;
                    continue;
                }

                SpawnEnemy();
                spawned++;
                yield return new WaitForSeconds(GetSpawnDelay());
            }
        }

        private void SpawnEnemy()
        {
            if (ferryTarget == null)
            {
                Debug.LogWarning("EnemySpawner is missing the ferry target.", this);
                return;
            }

            EnemySpawnProfile profile = SelectProfile();
            SimpleEnemy prefab = profile != null ? profile.Prefab : enemyPrefab;
            if (prefab == null)
            {
                Debug.LogWarning("EnemySpawner has no eligible enemy prefab for this round.", this);
                return;
            }

            Vector3 spawnPosition = GetSpawnPosition(profile);
            SimpleEnemy enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
            string profileName = profile != null && !string.IsNullOrWhiteSpace(profile.DisplayName)
                ? profile.DisplayName
                : prefab.name;
            enemy.name = $"{profileName} R{activeRound}";
            enemy.Removed += HandleEnemyRemoved;
            aliveEnemies.Add(enemy);

            float speedMultiplier = 1f + (activeRound - 1) * speedScalePerRound;
            float healthMultiplier = (1f + (activeRound - 1) * healthScalePerRound) * augmentHealthMultiplier;
            int reward = Mathf.Max(baseKillReward, enemy.KillReward) + (activeRound - 1) * 2;
            enemy.Initialize(ferryTarget, gameManager, reward, speedMultiplier, healthMultiplier);
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
