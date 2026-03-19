using UnityEngine;
using DG.Tweening;

namespace BounceReaper
{
    public class EnemyHealth : MonoBehaviour
    {
        // 1. SerializeField
        [Header("Knockback")]
        [SerializeField] private float _knockbackStrength = 0.1f;
        [SerializeField] private float _knockbackDuration = 0.1f;

        // 2. Private fields
        private int _currentHP;
        private int _shardReward;
        private bool _isDead;
        private EnemyController _controller;

        // 3. Properties
        public int CurrentHP => _currentHP;
        public int ShardReward => _shardReward;
        public bool IsDead => _isDead;

        // 4. Lifecycle
        private void Awake()
        {
            _controller = GetComponent<EnemyController>();
        }

        private void OnDisable()
        {
            DOTween.Kill(transform);
        }

        // 5. Public API
        public void Initialize(int maxHP, int shardReward = 0)
        {
            _currentHP = maxHP;
            _shardReward = shardReward > 0 ? shardReward : maxHP;
            _isDead = false;
        }

        public void TakeDamage(float damage, Vector2 hitDirection)
        {
            if (_isDead) return;

            _currentHP -= Mathf.RoundToInt(damage);

            GameEvents.Raise(GameEvents.OnBlockHit, gameObject, damage);

            // Knockback via DOTween
            DOTween.Kill(transform);
            transform.DOPunchPosition(
                (Vector3)hitDirection.normalized * _knockbackStrength,
                _knockbackDuration,
                vibrato: 0,
                elasticity: 0
            ).SetUpdate(true);

            // Update HP display
            if (_controller != null)
                _controller.UpdateHPDisplay();

            if (_currentHP <= 0)
            {
                Die();
            }
        }

        // 6. Private methods
        private void Die()
        {
            _isDead = true;
            // Award currency before the event (GridManager may reset reward in its handler)
            if (CurrencyManager.IsAvailable && _shardReward > 0)
                CurrencyManager.Instance.AddShards(_shardReward);
            GameEvents.Raise(GameEvents.OnBlockDestroyed, gameObject);
        }

        public void ResetHealth()
        {
            _currentHP = 0;
            _shardReward = 0;
            _isDead = false;
            DOTween.Kill(transform);
        }
    }
}
