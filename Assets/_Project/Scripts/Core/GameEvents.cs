using System;
using UnityEngine;

namespace BounceReaper
{
    public static class GameEvents
    {
        // Game State
        public static Action<GameState> OnGameStateChanged;

        // Turn
        public static Action OnTurnStart;
        public static Action OnTurnEnd;
        public static Action OnAllBallsReturned;

        // Block/Combat
        public static Action<GameObject, float> OnBlockHit;
        public static Action<GameObject> OnBlockDestroyed;
        public static Action OnBlockReachedBottom;

        // Wave/Grid
        public static Action<int> OnWaveComplete;

        // Economy
        public static Action<CurrencyType, int> OnCurrencyChanged;
        public static Action<string> OnUpgradePurchased;

        // Ball
        public static Action<GameObject> OnBallSpawned;
        public static Action<Vector2> OnBallReturned; // position where ball hit the floor

        // Power-ups
        public static Action<int> OnBallCountChanged; // total ball count

        /// <summary>
        /// Safe invoke that catches per-subscriber exceptions.
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
            OnTurnStart = null;
            OnTurnEnd = null;
            OnAllBallsReturned = null;
            OnBlockHit = null;
            OnBlockDestroyed = null;
            OnBlockReachedBottom = null;
            OnWaveComplete = null;
            OnCurrencyChanged = null;
            OnUpgradePurchased = null;
            OnBallSpawned = null;
            OnBallReturned = null;
            OnBallCountChanged = null;
        }
    }
}
