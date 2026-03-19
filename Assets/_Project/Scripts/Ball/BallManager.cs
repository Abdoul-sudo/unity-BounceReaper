using System.Collections;
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
        [SerializeField] private GameConfig _gameConfig;

        [Header("Firing")]
        [SerializeField] private float _fireDelay = 0.08f;
        [SerializeField] private float _floorY = -4.5f;

        [Header("Pool")]
        [SerializeField] [Range(1, 10)] private int _poolWarmup = 5;
        [SerializeField] [Range(10, 100)] private int _poolMaxSize = 60;

        // 2. Private fields
        private ObjectPool<BallController> _pool;
        private System.Collections.Generic.List<BallController> _returnedBalls = new(16);
        private int _ballCount = 1;
        private int _ballsInFlight;
        private int _ballsReturned;
        private Vector2 _launchPosition;
        private Vector2 _nextLaunchPosition;
        private bool _firstBallReturned;
        private bool _initialized;
        private Coroutine _fireCoroutine;

        // Cached
        private WaitForSeconds _waitFireDelay;

        // 3. Properties
        public int BallCount => _ballCount;
        public int BallsInFlight => _ballsInFlight;
        public Vector2 LaunchPosition => _launchPosition;
        public bool IsFiring => _ballsInFlight > 0;

        // 4. Lifecycle
        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        private void OnEnable()
        {
            GameEvents.OnBallReturned += HandleBallReturned;
        }

        private void OnDisable()
        {
            GameEvents.OnBallReturned -= HandleBallReturned;
        }

        // 5. Public API
        public void FireBalls(Vector2 direction)
        {
            if (!_initialized)
            {
                Debug.LogError("[Ball] BallManager not initialized! Check SO references in Inspector.");
                return;
            }
            if (_ballsInFlight > 0) return;

            // Collect returned balls from previous turn
            CollectReturnedBalls();

            _ballsReturned = 0;
            _firstBallReturned = false;
            _nextLaunchPosition = _launchPosition;

            if (_fireCoroutine != null) StopCoroutine(_fireCoroutine);
            _fireCoroutine = StartCoroutine(FireVolleyCoroutine(direction));

            GameEvents.Raise(GameEvents.OnTurnStart);
            Debug.Log($"[Ball] Firing {_ballCount} balls");
        }

        public void SetLaunchPosition(Vector2 pos)
        {
            _launchPosition = new Vector2(pos.x, _floorY);
        }

        public void AddBalls(int count)
        {
            _ballCount += count;
            GameEvents.Raise(GameEvents.OnBallCountChanged, _ballCount);
            Debug.Log($"[Ball] Ball count: {_ballCount}");
        }

        public void SetBallCount(int count)
        {
            _ballCount = Mathf.Max(1, count);
            GameEvents.Raise(GameEvents.OnBallCountChanged, _ballCount);
        }

        // 6. Private methods
        private void Init()
        {
            Debug.Assert(_defaultStats != null, "[Ball] DefaultStats SO not assigned");
            Debug.Assert(_ballPrefab != null, "[Ball] BallPrefab not assigned");
            Debug.Assert(_gameConfig != null, "[Ball] GameConfig not assigned");

            _waitFireDelay = new WaitForSeconds(_fireDelay);
            _launchPosition = new Vector2(0, _floorY);

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
            var warmup = new BallController[_poolWarmup];
            for (int i = 0; i < _poolWarmup; i++)
                warmup[i] = _pool.Get();
            for (int i = 0; i < _poolWarmup; i++)
                _pool.Release(warmup[i]);

            _initialized = true;
            Debug.Log($"[Ball] Initialized — {_ballCount} balls, pool warmed up");

            // Show the ball at launch position at start
            ShowBallAtLaunchPosition();
        }

        private void ShowBallAtLaunchPosition()
        {
            var ball = _pool.Get();
            ball.transform.position = new Vector3(_launchPosition.x, _launchPosition.y, 0);
            ball.gameObject.SetActive(true);
            ball.ResetBall();
            _returnedBalls.Add(ball);
        }

        private IEnumerator FireVolleyCoroutine(Vector2 direction)
        {
            _ballsInFlight = _ballCount;
            _returnedBalls.Clear();

            for (int i = 0; i < _ballCount; i++)
            {
                var ball = _pool.Get();
                ball.transform.position = new Vector3(_launchPosition.x, _launchPosition.y, 0);
                ball.gameObject.SetActive(true);
                ball.Initialize(_defaultStats);
                ball.SetFloorY(_floorY);
                ball.Launch(direction);

                if (i < _ballCount - 1)
                    yield return _waitFireDelay;
            }
        }

        private void HandleBallReturned(Vector2 returnPos)
        {
            // Find the ball that just returned
            var balls = GetComponentsInChildren<BallController>(false);
            foreach (var b in balls)
            {
                if (b.HasReturned && !_returnedBalls.Contains(b))
                {
                    _returnedBalls.Add(b);

                    // First ball stays visible, all others hide immediately
                    if (!_firstBallReturned)
                    {
                        _firstBallReturned = true;
                        _nextLaunchPosition = new Vector2(
                            Mathf.Clamp(returnPos.x, -2.5f, 2.5f),
                            _floorY
                        );
                        // Keep this ball visible at the landing spot
                        b.transform.position = new Vector3(_nextLaunchPosition.x, _floorY, 0);
                    }
                    else
                    {
                        // Hide subsequent balls — only show one
                        b.gameObject.SetActive(false);
                    }
                    break;
                }
            }

            _ballsReturned++;

            if (_ballsReturned >= _ballsInFlight)
            {
                AllBallsReturned();
            }
        }

        private void AllBallsReturned()
        {
            _ballsInFlight = 0;
            _launchPosition = _nextLaunchPosition;

            Debug.Log($"[Ball] All balls returned. Next launch at x={_launchPosition.x:F1}");
            GameEvents.Raise(GameEvents.OnAllBallsReturned);
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

        private void CollectReturnedBalls()
        {
            foreach (var ball in _returnedBalls)
            {
                if (ball != null && ball.gameObject.activeSelf)
                {
                    ball.ResetBall();
                    _pool.Release(ball);
                }
            }
            _returnedBalls.Clear();
        }
    }
}
