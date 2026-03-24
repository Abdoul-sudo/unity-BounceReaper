using UnityEngine;

namespace BounceReaper
{
    public class CurrencyManager : Singleton<CurrencyManager>
    {
        // 2. Private fields
        private int _shards;
        private int _shardsThisTurn;
        private bool _initialized;

        // 3. Properties
        public int Shards => _shards;
        public int ShardsThisTurn => _shardsThisTurn;

        // 4. Lifecycle
        protected override void Awake()
        {
            base.Awake();
            LoadFromSave();
            _initialized = true;
        }

        private void OnEnable()
        {
            GameEvents.OnAllBallsReturned += HandleTurnEnd;
        }

        private void OnDisable()
        {
            GameEvents.OnAllBallsReturned -= HandleTurnEnd;
        }

        // 5. Public API
        public void AddShards(int amount)
        {
            if (amount <= 0) return;
            _shards += amount;
            _shardsThisTurn += amount;
            GameEvents.Raise(GameEvents.OnCurrencyChanged, CurrencyType.Shards, _shards);
        }

        public bool SpendShards(int amount)
        {
            if (amount <= 0 || _shards < amount) return false;
            _shards -= amount;
            GameEvents.Raise(GameEvents.OnCurrencyChanged, CurrencyType.Shards, _shards);
            return true;
        }

        public bool CanAfford(int amount)
        {
            return _shards >= amount;
        }

        public void ResetShards()
        {
            _shards = 0;
            _shardsThisTurn = 0;
            GameEvents.Raise(GameEvents.OnCurrencyChanged, CurrencyType.Shards, _shards);
        }

        // 6. Private methods
        private void HandleTurnEnd()
        {
            if (_shardsThisTurn > 0)
            {
                Debug.Log($"[Currency] +{_shardsThisTurn} shards this turn (total: {_shards})");
                _shardsThisTurn = 0;
            }
            SaveToFile();
        }

        private void LoadFromSave()
        {
            if (SaveManager.IsAvailable && SaveManager.Instance.Data != null)
            {
                _shards = SaveManager.Instance.Data.shards;
                Debug.Log($"[Currency] Loaded {_shards} shards from save");
            }
        }

        private void SaveToFile()
        {
            if (SaveManager.IsAvailable && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.shards = _shards;
                SaveManager.Instance.Save();
            }
        }
    }
}
