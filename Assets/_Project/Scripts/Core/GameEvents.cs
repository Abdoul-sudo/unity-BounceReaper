using System;
using UnityEngine;

namespace BounceReaper
{
    public static class GameEvents
    {
        // Game State
        public static Action<GameState> OnGameStateChanged;

        // Combat
        public static Action<GameObject, float> OnEnemyHit;
        public static Action<GameObject> OnEnemyKilled;

        // Wave
        public static Action<int> OnWaveComplete;
        public static Action<GameObject> OnBossSpawn;
        public static Action OnBossEscaped;

        // Economy
        public static Action<CurrencyType, int> OnCurrencyChanged;
        public static Action<string> OnUpgradePurchased;

        // Ball
        public static Action<GameObject> OnBallSpawned;

        /// <summary>
        /// Safe invoke that catches per-subscriber exceptions.
        /// Prevents one buggy listener from killing the entire event chain.
        /// </summary>
        public static void Raise<T>(Action<T> action, T arg)
        {
            if (action == null) return;
            foreach (var handler in action.GetInvocationList())
            {
                try { ((Action<T>)handler)(arg); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        public static void Raise<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            if (action == null) return;
            foreach (var handler in action.GetInvocationList())
            {
                try { ((Action<T1, T2>)handler)(arg1, arg2); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        public static void Raise(Action action)
        {
            if (action == null) return;
            foreach (var handler in action.GetInvocationList())
            {
                try { ((Action)handler)(); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            OnGameStateChanged = null;
            OnEnemyHit = null;
            OnEnemyKilled = null;
            OnWaveComplete = null;
            OnBossSpawn = null;
            OnBossEscaped = null;
            OnCurrencyChanged = null;
            OnUpgradePurchased = null;
            OnBallSpawned = null;
        }
    }
}
