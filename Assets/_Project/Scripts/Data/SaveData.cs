using System.Collections.Generic;

namespace BounceReaper
{
    /// <summary>
    /// Serialized via Newtonsoft Json.NET (NOT JsonUtility).
    /// Public fields required for JSON serialization.
    /// lastPlayTimestamp = Unix UTC seconds (DateTimeOffset.UtcNow.ToUnixTimeSeconds).
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        public int version = 1;
        public int shards;
        public int souls;
        public int highestWave;
        public Dictionary<string, int> upgradeLevels = new Dictionary<string, int>();
        public long lastPlayTimestamp;
    }
}
