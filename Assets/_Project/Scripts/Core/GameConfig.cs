using UnityEngine;

namespace BounceReaper
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "BounceReaper/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Game")]
        [Range(30, 120)] [SerializeField] private int _targetFrameRate = 60;
        [Range(1, 50)] [SerializeField] private int _maxVisualBalls = 30;

        [Header("VFX Budget")]
        [Range(1, 20)] [SerializeField] private int _maxDamageNumbers = 8;
        [Range(1, 10)] [SerializeField] private int _maxDeathVFX = 4;

        [Header("Audio")]
        [Range(1, 16)] [SerializeField] private int _audioPoolSize = 8;
        [Range(0.01f, 0.2f)] [SerializeField] private float _sfxThrottleTime = 0.05f;
        [Range(0f, 0.2f)] [SerializeField] private float _pitchRandomization = 0.1f;

        [Header("Combat")]
        [Range(0.05f, 0.5f)] [SerializeField] private float _hitstopCooldown = 0.2f;
        [Range(0.01f, 0.1f)] [SerializeField] private float _hitstopDuration = 0.02f;
        [Range(0.5f, 3f)] [SerializeField] private float _juiceComboDuration = 1f;

        // Read-only public access
        public int TargetFrameRate => _targetFrameRate;
        public int MaxVisualBalls => _maxVisualBalls;
        public int MaxDamageNumbers => _maxDamageNumbers;
        public int MaxDeathVFX => _maxDeathVFX;
        public int AudioPoolSize => _audioPoolSize;
        public float SFXThrottleTime => _sfxThrottleTime;
        public float PitchRandomization => _pitchRandomization;
        public float HitstopCooldown => _hitstopCooldown;
        public float HitstopDuration => _hitstopDuration;
        public float JuiceComboDuration => _juiceComboDuration;

        private void OnValidate()
        {
            _targetFrameRate = Mathf.Clamp(_targetFrameRate, 30, 120);
            _maxVisualBalls = Mathf.Clamp(_maxVisualBalls, 1, 50);
            _maxDamageNumbers = Mathf.Clamp(_maxDamageNumbers, 1, 20);
            _maxDeathVFX = Mathf.Clamp(_maxDeathVFX, 1, 10);
            _audioPoolSize = Mathf.Clamp(_audioPoolSize, 1, 16);
            _sfxThrottleTime = Mathf.Clamp(_sfxThrottleTime, 0.01f, 0.2f);
            _pitchRandomization = Mathf.Clamp(_pitchRandomization, 0f, 0.2f);
            _hitstopCooldown = Mathf.Clamp(_hitstopCooldown, 0.05f, 0.5f);
            _hitstopDuration = Mathf.Clamp(_hitstopDuration, 0.01f, 0.1f);
            _juiceComboDuration = Mathf.Clamp(_juiceComboDuration, 0.5f, 3f);
        }
    }
}
