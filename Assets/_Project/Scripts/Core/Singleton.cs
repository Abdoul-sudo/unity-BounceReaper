using UnityEngine;

namespace BounceReaper
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate {typeof(T).Name} on {gameObject.name} — destroying");
                Destroy(gameObject);
                return;
            }
            Instance = this as T;
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnApplicationQuit()
        {
            SingletonHelper.IsQuitting = true;
        }

        public static bool IsAvailable => Instance != null && !SingletonHelper.IsQuitting;
    }

    public static class SingletonHelper
    {
        public static bool IsQuitting { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetQuittingFlag()
        {
            IsQuitting = false;
        }
    }
}
