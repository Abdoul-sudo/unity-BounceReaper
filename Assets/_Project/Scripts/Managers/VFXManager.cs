using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;
using TMPro;

namespace BounceReaper
{
    public class VFXManager : Singleton<VFXManager>
    {
        // 1. SerializeField
        [Header("Screen Shake")]
        [SerializeField] private float _shakeStrength = 0.15f;
        [SerializeField] private float _shakeDuration = 0.15f;

        [Header("Damage Numbers")]
        [SerializeField] private TextMeshPro _damageNumberPrefab;
        [SerializeField] [Range(1, 20)] private int _damagePoolSize = 8;

        // 2. Private fields
        private Camera _camera;
        private ObjectPool<TextMeshPro> _damagePool;
        private bool _initialized;

        // 4. Lifecycle
        protected override void Awake()
        {
            base.Awake();
            _camera = Camera.main;
            InitPools();
        }

        private void OnEnable()
        {
            GameEvents.OnBlockHit += HandleBlockHit;
            GameEvents.OnBlockDestroyed += HandleBlockDestroyed;
        }

        private void OnDisable()
        {
            GameEvents.OnBlockHit -= HandleBlockHit;
            GameEvents.OnBlockDestroyed -= HandleBlockDestroyed;
        }

        // 5. Public API
        public void ShakeCamera()
        {
            if (_camera != null)
            {
                DOTween.Kill(_camera.transform);
                _camera.transform.DOShakePosition(_shakeDuration, _shakeStrength, vibrato: 10)
                    .SetUpdate(true);
            }
        }

        public void SpawnDamageNumber(Vector3 position, float damage)
        {
            if (!_initialized || _damagePool == null) return;

            var tmp = _damagePool.Get();
            tmp.transform.position = position + Vector3.up * 0.3f;
            tmp.gameObject.SetActive(true);
            tmp.text = Mathf.RoundToInt(damage).ToString();
            tmp.color = Color.white;
            tmp.sortingOrder = GameConstants.SortOrderDamageNumbers;

            // Animate: float up + fade out
            var seq = DOTween.Sequence();
            seq.Join(tmp.transform.DOMoveY(position.y + 1.2f, 0.6f).SetEase(Ease.OutQuad));
            seq.Join(tmp.DOFade(0f, 0.6f).SetEase(Ease.InQuad));
            seq.SetUpdate(true);
            seq.OnComplete(() =>
            {
                tmp.gameObject.SetActive(false);
                _damagePool.Release(tmp);
            });
        }

        public void PunchBlock(Transform blockTransform)
        {
            if (blockTransform == null) return;
            DOTween.Kill(blockTransform);
            blockTransform.DOPunchScale(Vector3.one * 0.15f, 0.1f, vibrato: 0)
                .SetUpdate(true);
        }

        // 6. Private methods
        private void InitPools()
        {
            if (_damageNumberPrefab != null)
            {
                _damagePool = new ObjectPool<TextMeshPro>(
                    createFunc: () =>
                    {
                        var tmp = Instantiate(_damageNumberPrefab, transform);
                        tmp.gameObject.SetActive(false);
                        return tmp;
                    },
                    actionOnRelease: tmp => tmp.gameObject.SetActive(false),
                    defaultCapacity: _damagePoolSize,
                    maxSize: _damagePoolSize * 2
                );
            }
            _initialized = true;
        }

        private void HandleBlockHit(GameObject blockGO, float damage)
        {
            SpawnDamageNumber(blockGO.transform.position, damage);
            PunchBlock(blockGO.transform);
        }

        private void HandleBlockDestroyed(GameObject blockGO)
        {
            ShakeCamera();
        }
    }
}
