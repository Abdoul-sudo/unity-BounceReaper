using System.Collections.Generic;
using UnityEngine;

namespace BounceReaper
{
    public class UpgradeManager : Singleton<UpgradeManager>
    {
        // 1. SerializeField
        [Header("Upgrades")]
        [SerializeField] private UpgradeConfig _damageUpgrade;
        [SerializeField] private UpgradeConfig _speedUpgrade;
        [SerializeField] private UpgradeConfig _extraBallsUpgrade;

        // 2. Private fields
        private Dictionary<string, int> _levels = new Dictionary<string, int>();
        private bool _initialized;

        // 3. Properties
        public UpgradeConfig DamageUpgrade => _damageUpgrade;
        public UpgradeConfig SpeedUpgrade => _speedUpgrade;
        public UpgradeConfig ExtraBallsUpgrade => _extraBallsUpgrade;

        // 4. Lifecycle
        protected override void Awake()
        {
            base.Awake();
            LoadFromSave();
            _initialized = true;
        }

        // 5. Public API
        public int GetLevel(UpgradeConfig config)
        {
            if (config == null) return 0;
            return _levels.TryGetValue(config.UpgradeId, out int level) ? level : 0;
        }

        public int GetCost(UpgradeConfig config)
        {
            return config.GetCost(GetLevel(config));
        }

        public bool CanBuy(UpgradeConfig config)
        {
            if (config == null) return false;
            int level = GetLevel(config);
            if (level >= config.MaxLevel) return false;
            return CurrencyManager.IsAvailable && CurrencyManager.Instance.CanAfford(GetCost(config));
        }

        public bool Buy(UpgradeConfig config)
        {
            if (!CanBuy(config)) return false;

            int cost = GetCost(config);
            if (!CurrencyManager.Instance.SpendShards(cost)) return false;

            int newLevel = GetLevel(config) + 1;
            _levels[config.UpgradeId] = newLevel;

            ApplyUpgrade(config, newLevel);
            SaveToFile();

            Debug.Log($"[Upgrade] {config.DisplayName} → Lv.{newLevel} (cost: {cost})");
            GameEvents.Raise(GameEvents.OnUpgradePurchased, config.UpgradeId);
            return true;
        }

        public float GetDamageBonus()
        {
            return _damageUpgrade != null ? _damageUpgrade.GetTotalEffect(GetLevel(_damageUpgrade)) : 0;
        }

        public float GetSpeedBonus()
        {
            return _speedUpgrade != null ? _speedUpgrade.GetTotalEffect(GetLevel(_speedUpgrade)) : 0;
        }

        // 6. Private methods
        private void ApplyUpgrade(UpgradeConfig config, int level)
        {
            if (config == _extraBallsUpgrade && BallManager.IsAvailable)
            {
                // Extra balls = set starting ball count
                BallManager.Instance.SetBallCount(1 + level);
            }
            // Damage and speed are read dynamically via GetDamageBonus/GetSpeedBonus
        }

        private void ApplyAllUpgrades()
        {
            // Apply extra balls on load
            if (_extraBallsUpgrade != null && BallManager.IsAvailable)
            {
                int ballLevel = GetLevel(_extraBallsUpgrade);
                if (ballLevel > 0)
                    BallManager.Instance.SetBallCount(1 + ballLevel);
            }
        }

        private void Start()
        {
            ApplyAllUpgrades();
        }

        private void LoadFromSave()
        {
            if (SaveManager.IsAvailable && SaveManager.Instance.Data != null)
            {
                var saved = SaveManager.Instance.Data.upgradeLevels;
                if (saved != null)
                    _levels = new Dictionary<string, int>(saved);
                Debug.Log($"[Upgrade] Loaded {_levels.Count} upgrades from save");
            }
        }

        private void SaveToFile()
        {
            if (SaveManager.IsAvailable && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.upgradeLevels = new Dictionary<string, int>(_levels);
                SaveManager.Instance.Save();
            }
        }
    }
}
