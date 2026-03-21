using UnityEngine;

namespace BounceReaper
{
    public class TurnManager : Singleton<TurnManager>
    {
        // 1. SerializeField
        [Header("References")]
        [SerializeField] private AimController _aimController;
        [SerializeField] private UpgradePanel _upgradePanel;

        [Header("Menu")]
        [SerializeField] private MainMenuController _mainMenu;

        // 2. Private fields
        private TurnPhase _currentPhase = TurnPhase.None;
        private int _turnNumber;
        private bool _initialized;
        private bool _gameStarted;
        private bool _gameOver;

        // 3. Properties
        public TurnPhase CurrentPhase => _currentPhase;
        public int TurnNumber => _turnNumber;
        public bool IsGameOver => _gameOver;

        // 4. Lifecycle
        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            _initialized = true;
            _gameOver = false;
            _gameStarted = false;
            // Don't auto-start — wait for MainMenu tap
        }

        public void StartGame()
        {
            if (_gameStarted) return;
            _gameStarted = true;

            if (GridManager.IsAvailable)
                GridManager.Instance.SpawnInitialRows();

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
            if (!_initialized || _gameOver) return;

            _currentPhase = TurnPhase.Aiming;

            if (_upgradePanel != null)
                _upgradePanel.Hide();

            if (_aimController != null)
                _aimController.EnableAiming();

            Debug.Log($"[Turn] Phase: Aiming (turn {_turnNumber + 1})");
        }

        // 6. Private methods
        private void HandleAllBallsReturned()
        {
            if (_gameOver) return;

            _currentPhase = TurnPhase.EnemyPhase;
            _turnNumber++;

            Debug.Log($"[Turn] Turn {_turnNumber} complete — enemy phase");

            // Spawn new row of blocks
            if (GridManager.IsAvailable)
                GridManager.Instance.SpawnNewRow();

            // Update best wave
            if (SaveManager.IsAvailable && SaveManager.Instance.Data != null)
            {
                if (_turnNumber > SaveManager.Instance.Data.highestWave)
                    SaveManager.Instance.Data.highestWave = _turnNumber;
            }

            if (_gameOver) return;

            // Show upgrade panel between turns
            if (_upgradePanel != null && _upgradePanel.gameObject != null)
            {
                _upgradePanel.Show();
                Debug.Log("[Turn] Upgrade panel shown — waiting for player");
                // Player will click Skip or buy upgrades, then UpgradePanel calls StartAimingPhase()
            }
            else
            {
                Debug.Log("[Turn] No upgrade panel — going straight to aiming");
                StartAimingPhase();
            }
        }

        private void HandleGameOver()
        {
            if (_gameOver) return;

            _gameOver = true;
            _currentPhase = TurnPhase.None;

            if (_aimController != null)
                _aimController.DisableAiming();
            if (_upgradePanel != null)
                _upgradePanel.Hide();

            Debug.Log($"[Turn] GAME OVER at turn {_turnNumber}!");
            GameEvents.Raise(GameEvents.OnGameStateChanged, GameState.GameOver);
        }
    }
}
