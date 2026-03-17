using UnityEngine;

namespace BounceReaper
{
    public class TurnManager : Singleton<TurnManager>
    {
        // 1. SerializeField
        [Header("References")]
        [SerializeField] private AimController _aimController;

        // 2. Private fields
        private TurnPhase _currentPhase = TurnPhase.None;
        private int _turnNumber;
        private bool _initialized;

        // 3. Properties
        public TurnPhase CurrentPhase => _currentPhase;
        public int TurnNumber => _turnNumber;

        // 4. Lifecycle
        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            _initialized = true;
            StartAimingPhase();
        }

        private void OnEnable()
        {
            GameEvents.OnAllBallsReturned += HandleAllBallsReturned;
            GameEvents.OnBlockReachedBottom += HandleGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnAllBallsReturned -= HandleAllBallsReturned;
            GameEvents.OnBlockReachedBottom -= HandleGameOver;
        }

        // 5. Public API
        public void StartAimingPhase()
        {
            if (!_initialized) return;

            _currentPhase = TurnPhase.Aiming;

            if (_aimController != null)
                _aimController.EnableAiming();

            Debug.Log($"[Turn] Phase: Aiming (turn {_turnNumber + 1})");
        }

        // 6. Private methods
        private void HandleAllBallsReturned()
        {
            _currentPhase = TurnPhase.EnemyPhase;
            _turnNumber++;

            Debug.Log($"[Turn] Turn {_turnNumber} complete — enemy phase");

            // Spawn new row of blocks
            if (GridManager.IsAvailable)
                GridManager.Instance.SpawnNewRow();

            // Back to aiming
            StartAimingPhase();
        }

        private void HandleGameOver()
        {
            _currentPhase = TurnPhase.None;

            if (_aimController != null)
                _aimController.DisableAiming();

            Debug.Log("[Turn] GAME OVER — block reached bottom!");
            GameEvents.Raise(GameEvents.OnGameStateChanged, GameState.GameOver);
        }
    }
}
