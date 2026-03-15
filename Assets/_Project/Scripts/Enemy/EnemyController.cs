using UnityEngine;

namespace BounceReaper
{
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(Collider2D))]
    public class EnemyController : MonoBehaviour
    {
        // 1. SerializeField
        [Header("Config")]
        [SerializeField] private EnemyStats _stats;

        // 2. Private fields
        private EnemyHealth _health;
        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private Vector2 _moveDirection;
        private float _directionTimer;
        private bool _initialized;

        // 3. Properties
        public EnemyStats Stats => _stats;
        public EnemyHealth Health => _health;

        // 4. Lifecycle
        private void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            Debug.Assert(_health != null, $"[Enemy] Missing EnemyHealth on {gameObject.name}");
        }

        private void Update()
        {
            if (!_initialized || _health.IsDead) return;

            if (_stats.UsePhysicsMovement) return;

            _directionTimer -= Time.deltaTime;
            if (_directionTimer <= 0f)
            {
                PickRandomDirection();
                _directionTimer = _stats.DirectionInterval;
            }

            transform.Translate(_moveDirection * (_stats.MoveSpeed * Time.deltaTime));
        }

        // 5. Public API
        public void Initialize(EnemyStats stats)
        {
            Debug.Assert(stats != null, "[Enemy] EnemyStats is null in Initialize");
            _stats = stats;
            _health.Initialize(stats);

            // Visuals
            transform.localScale = Vector3.one * stats.Size;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = stats.EnemyColor;
                _spriteRenderer.sortingOrder = GameConstants.SortOrderEnemies;
            }

            // Movement
            if (stats.UsePhysicsMovement && _rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Dynamic;
                _rb.gravityScale = 0f;
                _rb.linearDamping = 0f;
                _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                _rb.freezeRotation = true;

                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                _rb.linearVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * stats.MoveSpeed;
            }
            else if (_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.linearVelocity = Vector2.zero;
            }

            PickRandomDirection();
            _directionTimer = stats.DirectionInterval;
            _initialized = true;
        }

        public void ResetEnemy()
        {
            _initialized = false;
            _health.ResetHealth();

            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }

            transform.localScale = Vector3.one;
        }

        // 6. Private methods
        private void PickRandomDirection()
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            _moveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }
}
