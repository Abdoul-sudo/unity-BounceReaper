using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace BounceReaper
{
    public class GridManager : Singleton<GridManager>
    {
        // 1. SerializeField
        [Header("Grid")]
        [SerializeField] private int _columns = 5;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private float _topY = 4f;
        [SerializeField] private float _gameOverY = -3.5f;

        [Header("HP Scaling")]
        [SerializeField] private int _baseHP = 1;
        [SerializeField] private float _hpScaling = 1.5f;

        [Header("Power-ups")]
        [SerializeField] [Range(0, 3)] private int _ballPickupsPerRow = 1;
        [SerializeField] private Color _pickupColor = new Color(0.2f, 1f, 0.4f);

        [Header("Visuals")]
        [SerializeField] private Sprite _blockSprite;

        [Header("Prefab")]
        [SerializeField] private EnemyController _blockPrefab;

        [Header("Pool")]
        [SerializeField] [Range(5, 50)] private int _poolWarmup = 10;
        [SerializeField] [Range(20, 100)] private int _poolMaxSize = 60;

        // 2. Private fields
        private ObjectPool<EnemyController> _pool;
        private List<EnemyController> _activeBlocks = new List<EnemyController>(64);
        private int _currentWave;
        private float _gridStartX;
        private bool _initialized;

        // Color gradient for HP
        private static readonly Color[] HPColors = {
            new Color(0.3f, 0.8f, 0.3f), // green - low HP
            new Color(0.3f, 0.6f, 1f),   // blue
            new Color(1f, 0.8f, 0.2f),   // yellow
            new Color(1f, 0.4f, 0.1f),   // orange
            new Color(1f, 0.2f, 0.2f),   // red
            new Color(0.8f, 0.2f, 1f),   // purple - high HP
        };

        // 3. Properties
        public int CurrentWave => _currentWave;
        public int AliveCount => _activeBlocks.Count;

        // 4. Lifecycle
        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        private void OnEnable()
        {
            GameEvents.OnBlockDestroyed += HandleBlockDestroyed;
        }

        private void OnDisable()
        {
            GameEvents.OnBlockDestroyed -= HandleBlockDestroyed;
        }

        // 5. Public API
        public void SpawnNewRow()
        {
            _currentWave++;

            // Move all existing blocks down
            for (int i = _activeBlocks.Count - 1; i >= 0; i--)
            {
                var block = _activeBlocks[i];
                if (block == null || !block.gameObject.activeSelf)
                {
                    _activeBlocks.RemoveAt(i);
                    continue;
                }
                block.transform.position += Vector3.down * _cellSize;

                // Check game over
                if (block.transform.position.y <= _gameOverY)
                {
                    Debug.Log("[Grid] Block reached bottom — GAME OVER");
                    GameEvents.Raise(GameEvents.OnBlockReachedBottom);
                    return;
                }
            }

            // Spawn new row at top
            int hpForWave = GetHPForWave(_currentWave);

            for (int col = 0; col < _columns; col++)
            {
                // Random chance to skip a cell (30% empty)
                if (Random.value < 0.3f) continue;

                // Random HP variation (80% - 120% of wave HP)
                int blockHP = Mathf.Max(1, Mathf.RoundToInt(hpForWave * Random.Range(0.8f, 1.2f)));

                SpawnBlock(col, _topY, blockHP);
            }

            // Spawn +1 ball pickups (50% chance per row, in empty column)
            if (Random.value < 0.5f)
            {
                var usedCols = new System.Collections.Generic.HashSet<int>();
                foreach (var b in _activeBlocks)
                {
                    if (Mathf.Approximately(b.transform.position.y, _topY))
                        usedCols.Add(Mathf.RoundToInt((b.transform.position.x - _gridStartX) / _cellSize));
                }
                for (int attempt = 0; attempt < _columns; attempt++)
                {
                    int randomCol = Random.Range(0, _columns);
                    if (!usedCols.Contains(randomCol))
                    {
                        SpawnPickup(randomCol, _topY);
                        break;
                    }
                }
            }

            Debug.Log($"[Grid] Wave {_currentWave} — {_activeBlocks.Count} blocks, HP base: {hpForWave}");
            GameEvents.Raise(GameEvents.OnWaveComplete, _currentWave);
        }

        public void DespawnAll()
        {
            for (int i = _activeBlocks.Count - 1; i >= 0; i--)
            {
                var block = _activeBlocks[i];
                if (block != null)
                {
                    block.ResetBlock();
                    _pool.Release(block);
                }
            }
            _activeBlocks.Clear();
            _currentWave = 0;
        }

        // 6. Private methods
        private void Init()
        {
            Debug.Assert(_blockPrefab != null, "[Grid] Block prefab not assigned");

            _gridStartX = -(_columns - 1) * _cellSize * 0.5f;

            _pool = new ObjectPool<EnemyController>(
                createFunc: CreateBlock,
                actionOnGet: null,
                actionOnRelease: OnReleaseBlock,
                actionOnDestroy: OnDestroyBlock,
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
            Debug.Log("[Grid] Initialized");
        }

        public void SpawnInitialRows(int count = 3)
        {
            for (int row = 0; row < count; row++)
                SpawnNewRow();
        }

        private void SpawnBlock(int col, float y, int hp)
        {
            var block = _pool.Get();
            float x = _gridStartX + col * _cellSize;
            block.transform.position = new Vector3(x, y, 0);
            block.transform.localScale = Vector3.one * (_cellSize * 0.85f);
            block.gameObject.SetActive(true);

            int enemyLayer = LayerMask.NameToLayer(GameConstants.LayerEnemy);
            if (enemyLayer >= 0) block.gameObject.layer = enemyLayer;

            Color color = GetColorForHP(hp);
            block.Initialize(hp, color, _blockSprite);

            _activeBlocks.Add(block);
        }

        private int GetHPForWave(int wave)
        {
            return Mathf.RoundToInt(_baseHP + wave * _hpScaling);
        }

        private Color GetColorForHP(int hp)
        {
            int index = Mathf.Clamp(hp / 3, 0, HPColors.Length - 1);
            return HPColors[index];
        }

        private void SpawnPickup(int col, float y)
        {
            var pickup = _pool.Get();
            float x = _gridStartX + col * _cellSize;
            pickup.transform.position = new Vector3(x, y, 0);
            pickup.transform.localScale = Vector3.one * (_cellSize * 0.85f);
            pickup.gameObject.SetActive(true);

            int enemyLayer = LayerMask.NameToLayer(GameConstants.LayerEnemy);
            if (enemyLayer >= 0) pickup.gameObject.layer = enemyLayer;

            pickup.gameObject.name = "Pickup_Ball";
            pickup.InitializeAsPickup(1, _pickupColor, _blockSprite);

            _activeBlocks.Add(pickup);
        }

        private void HandleBlockDestroyed(GameObject blockGO)
        {
            var block = blockGO.GetComponent<EnemyController>();
            if (block == null) return;

            // Check if it's a +1 ball pickup
            if (blockGO.name == "Pickup_Ball" && BallManager.IsAvailable)
            {
                BallManager.Instance.AddBalls(1);
                Debug.Log("[Grid] +1 ball pickup collected!");
            }

            _activeBlocks.Remove(block);
            block.ResetBlock();
            blockGO.name = "Block_Base"; // reset name
            _pool.Release(block);
        }

        private EnemyController CreateBlock()
        {
            var block = Instantiate(_blockPrefab, transform);
            block.gameObject.SetActive(false);
            return block;
        }

        private void OnReleaseBlock(EnemyController block)
        {
            block.gameObject.SetActive(false);
        }

        private void OnDestroyBlock(EnemyController block)
        {
            if (block != null)
                Destroy(block.gameObject);
        }
    }
}
