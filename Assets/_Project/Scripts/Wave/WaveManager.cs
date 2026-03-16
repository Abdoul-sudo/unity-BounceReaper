using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace BounceReaper
{
    public class WaveManager : Singleton<WaveManager>
    {
        // 1. SerializeField
        [Header("Config")]
        [SerializeField] private WaveConfig _config;
        [SerializeField] private EnemyController _enemyPrefab;

        [Header("Enemy Types")]
        [SerializeField] private EnemyStats _triangleStats;
        [SerializeField] private EnemyStats _squareStats;
        [SerializeField] private EnemyStats _hexagonStats;
        [SerializeField] private EnemyStats _diamondStats;

        [Header("Arena Bounds")]
        [SerializeField] private float _arenaHalfWidth = 2.75f;
        [SerializeField] private float _arenaHalfHeight = 4.75f;

        [Header("Pool")]
        [SerializeField] [Range(5, 30)] private int _poolWarmup = 5;
        [SerializeField] [Range(10, 60)] private int _poolMaxSize = 40;

        // 2. Private fields
        private ObjectPool<EnemyController> _pool;
        private List<EnemyController> _activeEnemies = new List<EnemyController>(32);
        private int _currentWave;
        private int _spawnedThisWave;
        private int _totalForWave;
        private bool _isSpawning;
        private bool _isBossWave;
        private float _bossTimer;
        private GameObject _currentBoss;
        private Coroutine _spawnCoroutine;
        private bool _initialized;

        // Cached WaitForSeconds
        private WaitForSeconds _waitSpawnInterval;
        private WaitForSeconds _waitBetweenWaves;

        // 3. Properties
        public int CurrentWave => _currentWave;
        public int AliveCount => _activeEnemies.Count;
        public bool IsBossWave => _isBossWave;
        public float BossTimer => _bossTimer;

        // 4. Lifecycle
        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        private void OnEnable()
        {
            GameEvents.OnEnemyKilled += HandleEnemyKilled;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyKilled -= HandleEnemyKilled;
        }

        private void Update()
        {
            if (!_initialized) return;

            if (_isBossWave && _currentBoss != null)
            {
                _bossTimer -= Time.deltaTime;
                if (_bossTimer <= 0f)
                {
                    BossEscaped();
                }
            }
        }

        // 5. Public API
        public void StartWaves()
        {
            if (!_initialized) return;
            _currentWave = 0;
            StartNextWave();
        }

        public void StartNextWave()
        {
            if (!_initialized) return;

            _currentWave++;
            _isBossWave = _config.IsBossWave(_currentWave);
            _totalForWave = _isBossWave ? 1 : _config.GetEnemyCount(_currentWave);
            _spawnedThisWave = 0;

            Debug.Log($"[Wave] Wave {_currentWave} starting — {_totalForWave} enemies{(_isBossWave ? " (BOSS)" : "")}");

            if (_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = StartCoroutine(SpawnWaveCoroutine());
        }

        public void DespawnAll()
        {
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = _activeEnemies[i];
                if (enemy != null)
                {
                    enemy.ResetEnemy();
                    _pool.Release(enemy);
                }
            }
            _activeEnemies.Clear();
            _currentBoss = null;
            _isBossWave = false;
            _isSpawning = false;
        }

        // 6. Private methods
        private void Init()
        {
            Debug.Assert(_config != null, "[Wave] WaveConfig not assigned");
            Debug.Assert(_enemyPrefab != null, "[Wave] Enemy prefab not assigned");
            Debug.Assert(_triangleStats != null, "[Wave] Triangle stats not assigned");

            _waitSpawnInterval = new WaitForSeconds(_config.SpawnInterval);
            _waitBetweenWaves = new WaitForSeconds(_config.TimeBetweenWaves);

            _pool = new ObjectPool<EnemyController>(
                createFunc: CreateEnemy,
                actionOnGet: null,
                actionOnRelease: OnReleaseEnemy,
                actionOnDestroy: OnDestroyEnemy,
                collectionCheck: true,
                defaultCapacity: _poolWarmup,
                maxSize: _poolMaxSize
            );

            // Warm-up
            var warmup = new EnemyController[_poolWarmup];
            for (int i = 0; i < _poolWarmup; i++)
                warmup[i] = _pool.Get();
            for (int i = 0; i < _poolWarmup; i++)
                _pool.Release(warmup[i]);

            _initialized = true;
            Debug.Log($"[Wave] Initialized — pool warmed up with {_poolWarmup} enemies");

            // Auto-start
            StartWaves();
        }

        private IEnumerator SpawnWaveCoroutine()
        {
            _isSpawning = true;

            for (int i = 0; i < _totalForWave; i++)
            {
                SpawnEnemy();
                _spawnedThisWave++;

                if (i < _totalForWave - 1)
                    yield return _waitSpawnInterval;
            }

            _isSpawning = false;

            if (_isBossWave)
            {
                _bossTimer = _config.BossTimerDuration;
                GameEvents.Raise(GameEvents.OnBossSpawn, _currentBoss);
                Debug.Log($"[Wave] BOSS spawned — {_config.BossTimerDuration}s timer");
            }
        }

        private void SpawnEnemy()
        {
            var enemy = _pool.Get();
            if (enemy == null)
            {
                Debug.LogWarning("[Wave] Pool exhausted, skipping spawn");
                return;
            }

            enemy.transform.position = GetSpawnPosition();
            enemy.gameObject.SetActive(true);

            var stats = _isBossWave ? _diamondStats : PickEnemyType();
            enemy.Initialize(stats);

            // Set layer
            int enemyLayer = LayerMask.NameToLayer(GameConstants.LayerEnemy);
            if (enemyLayer >= 0) enemy.gameObject.layer = enemyLayer;

            _activeEnemies.Add(enemy);

            if (_isBossWave)
                _currentBoss = enemy.gameObject;
        }

        private EnemyStats PickEnemyType()
        {
            var available = new List<EnemyStats>(4) { _triangleStats };

            if (_currentWave >= _config.UnlockSquare && _squareStats != null)
                available.Add(_squareStats);
            if (_currentWave >= _config.UnlockHexagon && _hexagonStats != null)
                available.Add(_hexagonStats);
            if (_currentWave >= _config.UnlockDiamond && _diamondStats != null)
                available.Add(_diamondStats);

            return available[Random.Range(0, available.Count)];
        }

        private Vector3 GetSpawnPosition()
        {
            float padding = _config.SpawnPadding;
            float topZone = _config.SpawnZoneTopPercent;

            float xMin = -_arenaHalfWidth + padding;
            float xMax = _arenaHalfWidth - padding;
            float yMin = _arenaHalfHeight * (1f - topZone * 2f);
            float yMax = _arenaHalfHeight - padding;

            return new Vector3(
                Random.Range(xMin, xMax),
                Random.Range(yMin, yMax),
                0f
            );
        }

        private void HandleEnemyKilled(GameObject enemyGO)
        {
            var enemy = enemyGO.GetComponent<EnemyController>();
            if (enemy == null) return;

            _activeEnemies.Remove(enemy);
            enemy.ResetEnemy();
            _pool.Release(enemy);

            // Boss killed
            if (_isBossWave && enemyGO == _currentBoss)
            {
                _currentBoss = null;
                _isBossWave = false;
                Debug.Log($"[Wave] BOSS killed!");
            }

            // Check wave complete: all spawned AND all dead
            if (!_isSpawning && _spawnedThisWave >= _totalForWave && _activeEnemies.Count == 0)
            {
                WaveComplete();
            }
        }

        private void WaveComplete()
        {
            Debug.Log($"[Wave] Wave {_currentWave} complete!");
            GameEvents.Raise(GameEvents.OnWaveComplete, _currentWave);

            StartCoroutine(WaitAndStartNextWave());
        }

        private IEnumerator WaitAndStartNextWave()
        {
            yield return _waitBetweenWaves;
            StartNextWave();
        }

        private void BossEscaped()
        {
            Debug.Log("[Wave] Boss escaped!");
            _isBossWave = false;

            if (_currentBoss != null)
            {
                var enemy = _currentBoss.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    _activeEnemies.Remove(enemy);
                    enemy.ResetEnemy();
                    _pool.Release(enemy);
                }
                _currentBoss = null;
            }

            GameEvents.Raise(GameEvents.OnBossEscaped);

            // Continue to next wave even if boss escaped
            if (_activeEnemies.Count == 0 && !_isSpawning)
                WaveComplete();
        }

        private EnemyController CreateEnemy()
        {
            var enemy = Instantiate(_enemyPrefab, transform);
            enemy.gameObject.SetActive(false);
            return enemy;
        }

        private void OnReleaseEnemy(EnemyController enemy)
        {
            enemy.gameObject.SetActive(false);
        }

        private void OnDestroyEnemy(EnemyController enemy)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
    }
}
