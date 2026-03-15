using UnityEngine;

namespace BounceReaper
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class BallController : MonoBehaviour
    {
        // 1. SerializeField
        [Header("Config")]
        [SerializeField] private BallStats _stats;

        // 2. Private fields
        private Rigidbody2D _rb;
        private int _enemyLayer;
        private bool _initialized;

        // 3. Properties
        public BallStats Stats => _stats;

        // 4. Lifecycle
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _enemyLayer = LayerMask.NameToLayer(GameConstants.LayerEnemy);

            Debug.Assert(_rb != null, $"[Ball] Missing Rigidbody2D on {gameObject.name}");
        }

        private void FixedUpdate()
        {
            if (!_initialized) return;
            ClampSpeed();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!_initialized) return;
            if (collision.gameObject.layer != _enemyLayer) return;

            float damage = _stats.BaseDamage;
            GameEvents.Raise(GameEvents.OnEnemyHit, collision.gameObject, damage);
        }

        private void OnDisable()
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            // DOTween cleanup for pooled objects
            // DG.Tweening.DOTween.Kill(gameObject);
            #endif
        }

        // 5. Public API
        public void Initialize(BallStats stats)
        {
            Debug.Assert(stats != null, "[Ball] BallStats is null in Initialize");
            _stats = stats;
            _initialized = true;

            LaunchInRandomDirection();
        }

        public void ResetBall()
        {
            _initialized = false;
            if (_rb != null)
                _rb.linearVelocity = Vector2.zero;
        }

        // 6. Private methods
        private void LaunchInRandomDirection()
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            _rb.linearVelocity = direction * _stats.BaseSpeed;
        }

        private void ClampSpeed()
        {
            float speed = _rb.linearVelocity.magnitude;

            if (speed < _stats.MinSpeed && speed > 0.01f)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * _stats.MinSpeed;
            }
            else if (speed > _stats.MaxSpeed)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * _stats.MaxSpeed;
            }
        }
    }
}
