using UnityEngine;

namespace BounceReaper
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }
        private static bool _isQuitting;

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate {typeof(T).Name} on {gameObject.name} — destroying");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        public static bool IsAvailable => Instance != null && !_isQuitting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetQuittingFlag()
        {
            _isQuitting = false;
        }
    }
}
