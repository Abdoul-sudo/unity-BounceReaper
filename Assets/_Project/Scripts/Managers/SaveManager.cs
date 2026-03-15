using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace BounceReaper
{
    public class SaveManager : Singleton<SaveManager>
    {
        // 1. SerializeField
        [Header("Debug")]
        [SerializeField] private bool _logSaveOperations = true;

        // 2. Private fields
        private SaveData _currentData;
        private string _savePath;
        private bool _initialized;

        // 3. Properties
        public SaveData Data => _currentData;

        // 4. Lifecycle
        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && _initialized)
            {
                Save();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && _initialized)
            {
                Save();
            }
        }

        // 5. Public API
        public void Save()
        {
            if (!_initialized) return;

            _currentData.lastPlayTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            try
            {
                var json = JsonConvert.SerializeObject(_currentData, Formatting.Indented);
                File.WriteAllText(_savePath, json);

                if (_logSaveOperations)
                    Debug.Log($"[Save] Saved to {_savePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Save] Write failed: {e.Message}");
            }
        }

        public void Load()
        {
            if (!File.Exists(_savePath))
            {
                if (_logSaveOperations)
                    Debug.Log("[Save] No save file found, using defaults");
                _currentData = new SaveData();
                return;
            }

            try
            {
                var json = File.ReadAllText(_savePath);
                _currentData = JsonConvert.DeserializeObject<SaveData>(json);

                if (_currentData == null)
                {
                    Debug.LogWarning("[Save] Deserialized null, using defaults");
                    _currentData = new SaveData();
                    return;
                }

                if (_currentData.version != GameConstants.SaveVersion)
                {
                    Migrate(_currentData);
                }

                if (_logSaveOperations)
                    Debug.Log($"[Save] Loaded v{_currentData.version} — wave {_currentData.highestWave}, {_currentData.shards} shards");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Save] Read failed: {e.Message}");
                _currentData = new SaveData();
            }
        }

        public void DeleteSave()
        {
            if (File.Exists(_savePath))
            {
                File.Delete(_savePath);
                Debug.Log("[Save] Save file deleted");
            }
            _currentData = new SaveData();
        }

        // 6. Private methods
        private void Init()
        {
            _savePath = Path.Combine(Application.persistentDataPath, GameConstants.SaveFileName);
            Load();
            _initialized = true;

            Debug.Assert(_currentData != null, "[Save] SaveData is null after init");
        }

        private void Migrate(SaveData data)
        {
            Debug.Log($"[Save] Migrating from v{data.version} to v{GameConstants.SaveVersion}");

            // v1 → v2 migration goes here when needed
            // if (data.version < 2) { /* migrate */ data.version = 2; }

            data.version = GameConstants.SaveVersion;
        }
    }
}
