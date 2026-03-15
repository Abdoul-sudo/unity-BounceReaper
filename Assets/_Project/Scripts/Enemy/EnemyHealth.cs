using UnityEngine;
using DG.Tweening;

namespace BounceReaper
{
    public class EnemyHealth : MonoBehaviour
    {
        // 1. SerializeField
        [Header("Knockback")]
        [SerializeField] private float _knockbackStrength = 0.3f;
        [SerializeField] private float _knockbackDuration = 0.15f;

        // 2. Private fields
        private int _currentHP;
        private EnemyStats _stats;
        private bool _isDead;

        // 3. Properties
        public int CurrentHP => _currentHP;
        public bool IsDead => _isDead;

        // 4. Lifecycle
        private void OnDisable()
        {
            DOTween.Kill(transform);
        }

        // 5. Public API
        public void Initialize(EnemyStats stats)
        {
            Debug.Assert(stats != null, "[Enemy] EnemyStats is null in Initialize");
            _stats = stats;
            _currentHP = stats.MaxHP;
            _isDead = false;
        }

        public void TakeDamage(float damage, Vector2 hitDirection)
        {
            if (_isDead) return;

            _currentHP -= Mathf.RoundToInt(damage);

            GameEvents.Raise(GameEvents.OnEnemyHit, gameObject, damage);

            // Knockback via DOTween
            DOTween.Kill(transform);
            transform.DOPunchPosition(
                (Vector3)hitDirection.normalized * _knockbackStrength,
                _knockbackDuration,
                vibrato: 0,
                elasticity: 0
            ).SetUpdate(true);

            if (_currentHP <= 0)
            {
                Die();
            }
        }

        // 6. Private methods
        private void Die()
        {
            _isDead = true;
            GameEvents.Raise(GameEvents.OnEnemyKilled, gameObject);
        }

        public void ResetHealth()
        {
            _currentHP = 0;
            _isDead = false;
            DOTween.Kill(transform);
        }
    }
}
