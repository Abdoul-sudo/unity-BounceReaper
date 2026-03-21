using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

namespace BounceReaper
{
    public class AudioManager : Singleton<AudioManager>
    {
        // 1. SerializeField
        [Header("SFX Clips")]
        [SerializeField] private AudioClip _hitClip;
        [SerializeField] private AudioClip _destroyClip;
        [SerializeField] private AudioClip _bonusClip;
        [SerializeField] private AudioClip _shotClip;
        [SerializeField] private AudioClip _gameOverClip;
        [SerializeField] private AudioClip _upgradeClip;

        [Header("Settings")]
        [SerializeField] [Range(0f, 1f)] private float _sfxVolume = 0.7f;
        [SerializeField] [Range(0f, 0.2f)] private float _pitchVariation = 0.1f;
        [SerializeField] [Range(0.02f, 0.2f)] private float _throttleTime = 0.05f;
        [SerializeField] [Range(1, 16)] private int _poolSize = 8;

        [Header("Music")]
        [SerializeField] private AudioClip _musicClip;
        [SerializeField] [Range(0f, 1f)] private float _musicVolume = 0.25f;

        // 2. Private fields
        private ObjectPool<AudioSource> _pool;
        private AudioSource _musicSource;
        private float _lastHitTime;
        private bool _initialized;

        // 4. Lifecycle
        protected override void Awake()
        {
            base.Awake();
            InitPool();
            InitMusic();
        }

        private void OnEnable()
        {
            GameEvents.OnBlockHit += HandleBlockHit;
            GameEvents.OnBlockDestroyed += HandleBlockDestroyed;
            GameEvents.OnTurnStart += HandleTurnStart;
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnUpgradePurchased += HandleUpgrade;
            GameEvents.OnBallCountChanged += HandleBallCountChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnBlockHit -= HandleBlockHit;
            GameEvents.OnBlockDestroyed -= HandleBlockDestroyed;
            GameEvents.OnTurnStart -= HandleTurnStart;
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnUpgradePurchased -= HandleUpgrade;
            GameEvents.OnBallCountChanged -= HandleBallCountChanged;
        }

        // 5. Public API
        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null || !_initialized) return;

            var source = _pool.Get();
            source.gameObject.SetActive(true);
            source.clip = clip;
            source.volume = _sfxVolume * volumeScale;
            source.pitch = 1f + Random.Range(-_pitchVariation, _pitchVariation);
            source.Play();

            // Return to pool when done
            StartCoroutine(ReturnAfterPlay(source, clip.length + 0.1f));
        }

        // 6. Private methods
        private void InitMusic()
        {
            if (_musicClip == null) return;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.clip = _musicClip;
            _musicSource.volume = _musicVolume;
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.Play();
            Debug.Log("[Audio] Music started");
        }

        private void InitPool()
        {
            _pool = new ObjectPool<AudioSource>(
                createFunc: () =>
                {
                    var go = new GameObject("SFX_Source");
                    go.transform.SetParent(transform);
                    var src = go.AddComponent<AudioSource>();
                    src.playOnAwake = false;
                    go.SetActive(false);
                    return src;
                },
                actionOnRelease: src => src.gameObject.SetActive(false),
                defaultCapacity: _poolSize,
                maxSize: _poolSize * 2
            );
            _initialized = true;
        }

        private System.Collections.IEnumerator ReturnAfterPlay(AudioSource source, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (source != null && source.gameObject.activeSelf)
                _pool.Release(source);
        }

        private void HandleBlockHit(GameObject go, float dmg)
        {
            // Throttle hit sounds
            if (Time.unscaledTime - _lastHitTime < _throttleTime) return;
            _lastHitTime = Time.unscaledTime;
            PlaySFX(_hitClip, 0.5f);
        }

        private void HandleBlockDestroyed(GameObject go)
        {
            PlaySFX(_destroyClip, 0.8f);
        }

        private void HandleTurnStart()
        {
            PlaySFX(_shotClip, 0.6f);
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                PlaySFX(_gameOverClip);
                // Fade out music
                if (_musicSource != null)
                    _musicSource.DOFade(0f, 0.5f).SetUpdate(true);
            }
        }

        private void HandleUpgrade(string id)
        {
            PlaySFX(_upgradeClip);
        }

        private void HandleBallCountChanged(int count)
        {
            PlaySFX(_bonusClip, 0.7f);
        }
    }
}
