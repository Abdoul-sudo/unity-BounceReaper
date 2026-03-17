using UnityEngine;
using TMPro;

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
            }

            UpdateHPDisplay();
            _initialized = true;
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
            if (_hpText != null)
                _hpText.text = "";
        }
    }
}
