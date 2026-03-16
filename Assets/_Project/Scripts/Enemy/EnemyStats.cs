using UnityEngine;

namespace BounceReaper
{
    [CreateAssetMenu(fileName = "EnemyStats", menuName = "BounceReaper/EnemyStats")]
    public class EnemyStats : ScriptableObject
    {
        [Header("Health")]
        [Range(1, 100)] [SerializeField] private int _maxHP = 1;

        [Header("Movement")]
        [Range(0f, 5f)] [SerializeField] private float _moveSpeed = 0.5f;
        [Range(0.5f, 10f)] [SerializeField] private float _directionInterval = 5f;
        [SerializeField] private bool _usePhysicsMovement;

        [Header("Visuals")]
        [Range(0.3f, 1.5f)] [SerializeField] private float _size = 0.5f;
        [SerializeField] private Color _color = Color.red;
        [SerializeField] private Sprite _sprite;

        [Header("Rewards")]
        [Range(1, 100)] [SerializeField] private int _shardReward = 1;

        public int MaxHP => _maxHP;
        public float MoveSpeed => _moveSpeed;
        public float DirectionInterval => _directionInterval;
        public bool UsePhysicsMovement => _usePhysicsMovement;
        public float Size => _size;
        public Color EnemyColor => _color;
        public Sprite EnemySprite => _sprite;
        public int ShardReward => _shardReward;

        private void OnValidate()
        {
            _maxHP = Mathf.Clamp(_maxHP, 1, 100);
            _moveSpeed = Mathf.Clamp(_moveSpeed, 0f, 5f);
            _directionInterval = Mathf.Clamp(_directionInterval, 0.5f, 10f);
            _size = Mathf.Clamp(_size, 0.3f, 1.5f);
            _shardReward = Mathf.Clamp(_shardReward, 1, 100);
        }
    }
}
