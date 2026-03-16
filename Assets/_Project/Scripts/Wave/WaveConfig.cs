using UnityEngine;

namespace BounceReaper
{
    [CreateAssetMenu(fileName = "WaveConfig", menuName = "BounceReaper/WaveConfig")]
    public class WaveConfig : ScriptableObject
    {
        [Header("Scaling")]
        [Range(1, 10)] [SerializeField] private int _baseEnemyCount = 3;
        [Range(0.5f, 3f)] [SerializeField] private float _enemyCountScaling = 1.5f;

        [Header("Spawn")]
        [Range(0.1f, 2f)] [SerializeField] private float _spawnInterval = 0.4f;
        [Range(0.5f, 5f)] [SerializeField] private float _timeBetweenWaves = 2f;

        [Header("Boss")]
        [Range(5, 20)] [SerializeField] private int _bossEveryNWaves = 10;
        [Range(15f, 90f)] [SerializeField] private float _bossTimerDuration = 45f;
        [Range(1f, 5f)] [SerializeField] private float _bossHPMultiplier = 3f;

        [Header("Enemy Type Unlocks (wave number)")]
        [Range(1, 20)] [SerializeField] private int _unlockSquare = 3;
        [Range(1, 20)] [SerializeField] private int _unlockHexagon = 5;
        [Range(1, 20)] [SerializeField] private int _unlockDiamond = 8;

        [Header("Spawn Zone")]
        [Range(0.1f, 0.5f)] [SerializeField] private float _spawnZoneTopPercent = 0.3f;
        [Range(0.2f, 1f)] [SerializeField] private float _spawnPadding = 0.5f;

        public int BaseEnemyCount => _baseEnemyCount;
        public float EnemyCountScaling => _enemyCountScaling;
        public float SpawnInterval => _spawnInterval;
        public float TimeBetweenWaves => _timeBetweenWaves;
        public int BossEveryNWaves => _bossEveryNWaves;
        public float BossTimerDuration => _bossTimerDuration;
        public float BossHPMultiplier => _bossHPMultiplier;
        public int UnlockSquare => _unlockSquare;
        public int UnlockHexagon => _unlockHexagon;
        public int UnlockDiamond => _unlockDiamond;
        public float SpawnZoneTopPercent => _spawnZoneTopPercent;
        public float SpawnPadding => _spawnPadding;

        public int GetEnemyCount(int waveNumber)
        {
            return Mathf.RoundToInt(_baseEnemyCount + waveNumber * _enemyCountScaling);
        }

        public bool IsBossWave(int waveNumber)
        {
            return waveNumber > 0 && waveNumber % _bossEveryNWaves == 0;
        }

        private void OnValidate()
        {
            _baseEnemyCount = Mathf.Clamp(_baseEnemyCount, 1, 10);
            _enemyCountScaling = Mathf.Clamp(_enemyCountScaling, 0.5f, 3f);
            _spawnInterval = Mathf.Clamp(_spawnInterval, 0.1f, 2f);
            _timeBetweenWaves = Mathf.Clamp(_timeBetweenWaves, 0.5f, 5f);
            _bossEveryNWaves = Mathf.Clamp(_bossEveryNWaves, 5, 20);
            _bossTimerDuration = Mathf.Clamp(_bossTimerDuration, 15f, 90f);
            _bossHPMultiplier = Mathf.Clamp(_bossHPMultiplier, 1f, 5f);
            if (_unlockSquare < 1) _unlockSquare = 1;
            if (_unlockHexagon <= _unlockSquare) _unlockHexagon = _unlockSquare + 1;
            if (_unlockDiamond <= _unlockHexagon) _unlockDiamond = _unlockHexagon + 1;
        }
    }
}
