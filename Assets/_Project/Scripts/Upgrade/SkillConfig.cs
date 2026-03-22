using UnityEngine;

namespace BounceReaper
{
    public enum SkillType
    {
        DamageUp,
        ExtraBall,
        FireBall,
        Shield,
        Poison
    }

    [CreateAssetMenu(fileName = "SkillConfig", menuName = "BounceReaper/SkillConfig")]
    public class SkillConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private SkillType _type;
        [SerializeField] private string _displayName;
        [SerializeField] [TextArea] private string _description;
        [SerializeField] private Color _color = Color.white;

        [Header("Pool")]
        [Range(0.1f, 1f)] [SerializeField] private float _weight = 1f;
        [Range(1, 10)] [SerializeField] private int _maxStacks = 10;

        public SkillType Type => _type;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Color SkillColor => _color;
        public float Weight => _weight;
        public int MaxStacks => _maxStacks;
    }
}
