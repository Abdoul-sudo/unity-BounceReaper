using UnityEngine;
using TMPro;
using DG.Tweening;

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
        private SpriteRenderer _spriteRenderer;
        private TextMeshPro _hpText;
        private bool _initialized;
        private Vector3 _targetScale;

        // 3. Properties
        public EnemyStats Stats => _stats;
        public EnemyHealth Health => _health;

        // 4. Lifecycle
        private void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _hpText = GetComponentInChildren<TextMeshPro>();

            Debug.Assert(_health != null, $"[Block] Missing EnemyHealth on {gameObject.name}");
        }

        private void OnDisable()
        {
            DOTween.Kill(transform);
        }

        // 5. Public API
        public void Initialize(int hp, Color color, Sprite sprite = null)
        {
            _health.Initialize(hp);

            if (_spriteRenderer != null)
            {
                if (sprite != null)
                    _spriteRenderer.sprite = sprite;
                _spriteRenderer.color = color;
                _spriteRenderer.sortingOrder = GameConstants.SortOrderEnemies;
                // Reset alpha
                var c = _spriteRenderer.color;
                c.a = 1f;
                _spriteRenderer.color = c;
            }

            UpdateHPDisplay();
            _initialized = true;

            // Spawn pop-in animation
            _targetScale = transform.localScale;
            transform.localScale = Vector3.zero;
            transform.DOScale(_targetScale, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        public void InitializeAsPickup(int hp, Color color, Sprite sprite)
        {
            Initialize(hp, color, sprite);

            // Override HP text
            if (_hpText != null)
            {
                _hpText.text = "+1";
                _hpText.fontSize = 4;
            }

            // Pulse animation loop
            DOTween.Kill(transform);
            transform.localScale = _targetScale;
            transform.DOScale(_targetScale * 1.12f, 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        public void UpdateHPDisplay()
        {
            if (_hpText != null)
            {
                _hpText.text = _health.CurrentHP.ToString();
                _hpText.sortingOrder = GameConstants.SortOrderDamageNumbers;
            }
        }

        public void ResetBlock()
        {
            _initialized = false;
            _health.ResetHealth();
            DOTween.Kill(transform);
            transform.localScale = Vector3.one;
            if (_hpText != null)
                _hpText.text = "";
        }
    }
}
