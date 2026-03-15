using UnityEngine;
using UnityEngine.Pool;

namespace BounceReaper
{
    public class BallManager : Singleton<BallManager>
    {
        // 1. SerializeField
        [Header("Config")]
        [SerializeField] private BallStats _defaultStats;
        [SerializeField] private BallController _ballPrefab;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private GameConfig _gameConfig;

        [Header("Startup")]
        [SerializeField] [Range(0, 5)] private int _startingBalls = 1;

        [Header("Pool")]
        [SerializeField] [Range(1, 10)] private int _poolWarmup = 3;
        [SerializeField] [Range(5, 50)] private int _poolMaxSize = 30;

        // 2. Private fields
        private ObjectPool<BallController> _pool;
        private int _activeBallCount;
        private bool _initialized;

        // 3. Properties
        public int ActiveBallCount => _activeBallCount;

        // 4. Lifecycle
        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        private void OnEnable()
        {
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }

        // 5. Public API
        public void SpawnBall()
        {
            if (!_initialized) return;

            if (_activeBallCount >= _gameConfig.MaxVisualBalls)
            {
                Debug.LogWarning($"[Ball] Max visual balls reached ({_gameConfig.MaxVisualBalls})");
                return;
            }

            var ball = _pool.Get();
            ball.transform.position = _spawnPoint != null ? _spawnPoint.position : Vector3.zero;
            ball.gameObject.SetActive(true);
            ball.Initialize(_defaultStats);

            _activeBallCount++;

            GameEvents.Raise(GameEvents.OnBallSpawned, ball.gameObject);

            Debug.Log($"[Ball] Spawned — {_activeBallCount} active");
        }

        public void DespawnBall(BallController ball)
        {
            if (ball == null) return;

            ball.ResetBall();
            _pool.Release(ball);
            _activeBallCount = Mathf.Max(0, _activeBallCount - 1);
        }

        public void DespawnAll()
        {
            var balls = GetComponentsInChildren<BallController>(true);
            foreach (var ball in balls)
            {
                if (ball.gameObject.activeSelf)
                    DespawnBall(ball);
            }
        }

        // 6. Private methods
        private void Init()
        {
            Debug.Assert(_defaultStats != null, "[Ball] DefaultStats SO not assigned");
            Debug.Assert(_ballPrefab != null, "[Ball] BallPrefab not assigned");
            Debug.Assert(_gameConfig != null, "[Ball] GameConfig not assigned");

            _pool = new ObjectPool<BallController>(
                createFunc: CreateBall,
                actionOnGet: null,
                actionOnRelease: OnReleaseBall,
                actionOnDestroy: OnDestroyBall,
                collectionCheck: true,
                defaultCapacity: _poolWarmup,
                maxSize: _poolMaxSize
            );

            // Warm-up
            var warmupBalls = new BallController[_poolWarmup];
            for (int i = 0; i < _poolWarmup; i++)
                warmupBalls[i] = _pool.Get();
            for (int i = 0; i < _poolWarmup; i++)
                _pool.Release(warmupBalls[i]);

            _initialized = true;
            Debug.Log($"[Ball] Initialized — pool warmed up with {_poolWarmup} balls");

            for (int i = 0; i < _startingBalls; i++)
                SpawnBall();
        }

        private BallController CreateBall()
        {
            var ball = Instantiate(_ballPrefab, transform);
            ball.gameObject.SetActive(false);
            return ball;
        }

        private void OnReleaseBall(BallController ball)
        {
            ball.gameObject.SetActive(false);
        }

        private void OnDestroyBall(BallController ball)
        {
            if (ball != null)
                Destroy(ball.gameObject);
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.Defeat || state == GameState.Victory || state == GameState.MainMenu)
            {
                DespawnAll();
            }
        }
    }
}
