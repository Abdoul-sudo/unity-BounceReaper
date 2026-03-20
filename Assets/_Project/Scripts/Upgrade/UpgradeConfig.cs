using UnityEngine;

namespace BounceReaper
{
    [CreateAssetMenu(fileName = "UpgradeConfig", menuName = "BounceReaper/UpgradeConfig")]
    public class UpgradeConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _upgradeId;
        [SerializeField] private string _displayName;

        [Header("Cost")]
        [SerializeField] private int _baseCost = 10;
        [SerializeField] private float _costScale = 1.5f;

        [Header("Limits")]
        [SerializeField] private int _maxLevel = 20;

        [Header("Effect")]
        [SerializeField] private float _effectPerLevel = 1f;

        public string UpgradeId => _upgradeId;
        public string DisplayName => _displayName;
        public int BaseCost => _baseCost;
        public float CostScale => _costScale;
        public int MaxLevel => _maxLevel;
        public float EffectPerLevel => _effectPerLevel;

        public int GetCost(int currentLevel)
        {
            return Mathf.RoundToInt(_baseCost * Mathf.Pow(_costScale, currentLevel));
        }

        public float GetTotalEffect(int level)
        {
            return _effectPerLevel * level;
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_upgradeId) && !string.IsNullOrEmpty(_displayName))
                _upgradeId = _displayName.ToLower().Replace(" ", "_");
            _baseCost = Mathf.Max(1, _baseCost);
            _costScale = Mathf.Max(1.1f, _costScale);
            _maxLevel = Mathf.Max(1, _maxLevel);
        }
    }
}
