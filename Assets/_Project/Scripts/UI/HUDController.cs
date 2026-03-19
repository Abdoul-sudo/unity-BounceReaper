using UnityEngine;
using TMPro;

namespace BounceReaper
{
    public class HUDController : MonoBehaviour
    {
        // 1. SerializeField
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _shardsText;
        [SerializeField] private TextMeshProUGUI _waveText;
        [SerializeField] private TextMeshProUGUI _ballCountText;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverScoreText;

        // 4. Lifecycle
        private void Start()
        {
            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(false);

            UpdateShards(0);
            UpdateWave(0);
            UpdateBallCount(1);
        }

        private void OnEnable()
        {
            GameEvents.OnCurrencyChanged += HandleCurrencyChanged;
            GameEvents.OnWaveComplete += HandleWaveComplete;
            GameEvents.OnBallCountChanged += HandleBallCountChanged;
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnCurrencyChanged -= HandleCurrencyChanged;
            GameEvents.OnWaveComplete -= HandleWaveComplete;
            GameEvents.OnBallCountChanged -= HandleBallCountChanged;
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }

        // 5. Public API
        public void OnRestartButton()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        // 6. Private methods
        private void UpdateShards(int amount)
        {
            if (_shardsText != null)
                _shardsText.text = $"{amount}";
        }

        private void UpdateWave(int wave)
        {
            if (_waveText != null)
                _waveText.text = $"Wave {wave}";
        }

        private void UpdateBallCount(int count)
        {
            if (_ballCountText != null)
                _ballCountText.text = $"x{count}";
        }

        private void HandleCurrencyChanged(CurrencyType type, int amount)
        {
            if (type == CurrencyType.Shards)
                UpdateShards(amount);
        }

        private void HandleWaveComplete(int wave)
        {
            UpdateWave(wave);
        }

        private void HandleBallCountChanged(int count)
        {
            UpdateBallCount(count);
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
                ShowGameOver();
        }

        private void ShowGameOver()
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);

                if (_gameOverScoreText != null)
                {
                    int wave = GridManager.IsAvailable ? GridManager.Instance.CurrentWave : 0;
                    int shards = CurrencyManager.IsAvailable ? CurrencyManager.Instance.Shards : 0;
                    _gameOverScoreText.text = $"Wave {wave}\n{shards} Shards";
                }
            }
        }
    }
}
