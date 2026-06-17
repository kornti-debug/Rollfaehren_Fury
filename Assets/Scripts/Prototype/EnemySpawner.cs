using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RollfaehrenFury.Prototype
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private SimpleEnemy enemyPrefab;
        [SerializeField] private FerryDamageTarget ferryTarget;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnInterval = 1.6f;
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

        public void Configure(SimpleEnemy prefab, FerryDamageTarget target, Transform[] points, GameManager manager)
        {
            enemyPrefab = prefab;
            ferryTarget = target;
            spawnPoints = points;
            gameManager = manager;
        }

        public void BeginRound(int round, GameManager manager)
        {
            gameManager = manager;
            activeRound = Mathf.Max(1, round);
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

            while (isSpawning && spawned < targetCount)
            {
                if (aliveEnemies.Count < maxAliveEnemies)
                {
                    SpawnEnemy();
                    spawned++;
                }

                yield return new WaitForSeconds(GetSpawnDelay());
            }
        }

        private void SpawnEnemy()
        {
            if (enemyPrefab == null || ferryTarget == null)
            {
                Debug.LogWarning("EnemySpawner is missing an enemy prefab or ferry target.", this);
                return;
            }

            Vector3 spawnPosition = GetSpawnPosition();
            SimpleEnemy enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemy.name = $"Prototype Enemy R{activeRound}";
            enemy.Removed += HandleEnemyRemoved;
            aliveEnemies.Add(enemy);

            float speedMultiplier = 1f + (activeRound - 1) * speedScalePerRound;
            float healthMultiplier = (1f + (activeRound - 1) * healthScalePerRound) * augmentHealthMultiplier;
            int reward = baseKillReward + (activeRound - 1) * 2;
            enemy.Initialize(ferryTarget, gameManager, reward, speedMultiplier, healthMultiplier);
        }

        private Vector3 GetSpawnPosition()
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
                if (point != null)
                {
                    return ApplySpawnHeight(point.position);
                }
            }

            Vector2 randomCircle = Random.insideUnitCircle.normalized * fallbackSpawnRadius;
            Vector3 center = ferryTarget != null ? ferryTarget.transform.position : transform.position;
            return ApplySpawnHeight(center + new Vector3(randomCircle.x, 0f, randomCircle.y));
        }

        private Vector3 ApplySpawnHeight(Vector3 position)
        {
            if (useFixedSpawnHeight)
            {
                position.y = spawnHeight;
            }

            return position;
        }

        private float GetSpawnDelay()
        {
            return Mathf.Max(0.35f, spawnInterval - (activeRound - 1) * spawnDelayReductionPerRound);
        }

        private void HandleEnemyRemoved(SimpleEnemy enemy)
        {
            enemy.Removed -= HandleEnemyRemoved;
            aliveEnemies.Remove(enemy);
        }
    }
}
