using UnityEngine;

namespace BounceReaper
{
    [CreateAssetMenu(fileName = "BallStats", menuName = "BounceReaper/BallStats")]
    public class BallStats : ScriptableObject
    {
        [Header("Movement")]
        [Range(1f, 20f)] [SerializeField] private float _baseSpeed = 8f;
        [Range(1f, 15f)] [SerializeField] private float _minSpeed = 6f;
        [Range(5f, 30f)] [SerializeField] private float _maxSpeed = 15f;

        [Header("Combat")]
        [Range(0.1f, 100f)] [SerializeField] private float _baseDamage = 1f;

        public float BaseSpeed => _baseSpeed;
        public float MinSpeed => _minSpeed;
        public float MaxSpeed => _maxSpeed;
        public float BaseDamage => _baseDamage;

        private void OnValidate()
        {
            _baseSpeed = Mathf.Clamp(_baseSpeed, 1f, 20f);
            _minSpeed = Mathf.Clamp(_minSpeed, 1f, 15f);
            _maxSpeed = Mathf.Clamp(_maxSpeed, 5f, 30f);
            _baseDamage = Mathf.Clamp(_baseDamage, 0.1f, 100f);

            if (_minSpeed > _maxSpeed) _minSpeed = _maxSpeed;
            if (_baseSpeed < _minSpeed) _baseSpeed = _minSpeed;
            if (_baseSpeed > _maxSpeed) _baseSpeed = _maxSpeed;
        }
    }
}
