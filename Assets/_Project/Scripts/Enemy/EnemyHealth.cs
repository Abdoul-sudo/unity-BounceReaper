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

        [Header("Death")]
        [SerializeField] private float _deathDuration = 0.2f;

        // 2. Private fields
        private int _currentHP;
        private int _shardReward;
        private bool _isDead;
        private bool _dying;
        private EnemyController _controller;
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;

        // 3. Properties
        public int CurrentHP => _currentHP;
        public int ShardReward => _shardReward;
        public bool IsDead => _isDead;

        // 4. Lifecycle
        private void Awake()
        {
            _controller = GetComponent<EnemyController>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnDisable()
        {
            DOTween.Kill(transform);
            if (_spriteRenderer != null)
                DOTween.Kill(_spriteRenderer);
        }

        // 5. Public API
        public void Initialize(int maxHP, int shardReward = 0)
        {
            _currentHP = maxHP;
            _shardReward = shardReward > 0 ? shardReward : maxHP;
            _isDead = false;
            _dying = false;
            if (_spriteRenderer != null)
                _originalColor = _spriteRenderer.color;
        }

        public void TakeDamage(float damage, Vector2 hitDirection)
        {
            if (_isDead || _dying) return;

            _currentHP -= Mathf.RoundToInt(damage);

            GameEvents.Raise(GameEvents.OnBlockHit, gameObject, damage);

            // Hit flash: white → original color
            if (_spriteRenderer != null)
            {
                DOTween.Kill(_spriteRenderer);
                _spriteRenderer.color = Color.white;
                _spriteRenderer.DOColor(_originalColor, 0.1f).SetUpdate(true);
            }

            // Knockback
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
            _dying = true;

            // Award currency + XP
            if (CurrencyManager.IsAvailable && _shardReward > 0)
                CurrencyManager.Instance.AddShards(_shardReward);
            if (SkillManager.IsAvailable)
                SkillManager.Instance.AddXP(_shardReward);

            // Death animation: scale up → shrink to 0 + fade
            DOTween.Kill(transform);
            if (_spriteRenderer != null)
                DOTween.Kill(_spriteRenderer);

            var seq = DOTween.Sequence();
            seq.Append(transform.DOScale(transform.localScale * 1.2f, _deathDuration * 0.3f).SetEase(Ease.OutQuad));
            seq.Append(transform.DOScale(Vector3.zero, _deathDuration * 0.7f).SetEase(Ease.InBack));
            if (_spriteRenderer != null)
                seq.Join(_spriteRenderer.DOFade(0f, _deathDuration * 0.7f));
            seq.SetUpdate(true);
            seq.OnComplete(() =>
            {
                _dying = false;
                GameEvents.Raise(GameEvents.OnBlockDestroyed, gameObject);
            });
        }

        public void ResetHealth()
        {
            _currentHP = 0;
            _shardReward = 0;
            _isDead = false;
            _dying = false;
            DOTween.Kill(transform);
            if (_spriteRenderer != null)
            {
                DOTween.Kill(_spriteRenderer);
                _spriteRenderer.color = Color.white;
                var c = _spriteRenderer.color;
                c.a = 1f;
                _spriteRenderer.color = c;
            }
        }
    }
}
