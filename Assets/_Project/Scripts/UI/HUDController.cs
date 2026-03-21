using UnityEngine;
using TMPro;
using DG.Tweening;

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

            int shards = CurrencyManager.IsAvailable ? CurrencyManager.Instance.Shards : 0;
            int balls = BallManager.IsAvailable ? BallManager.Instance.BallCount : 1;
            UpdateShards(shards);
            UpdateWave(0);
            UpdateBallCount(balls);
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
            if (_gameOverPanel == null) return;

            _gameOverPanel.SetActive(true);

            // Animate: fade in panel + scale punch title
            var panelImg = _gameOverPanel.GetComponent<UnityEngine.UI.Image>();
            if (panelImg != null)
            {
                var c = panelImg.color;
                panelImg.color = new Color(c.r, c.g, c.b, 0);
                panelImg.DOFade(c.a, 0.4f).SetUpdate(true);
            }

            // Scale in the title
            var title = _gameOverPanel.transform.Find("Title");
            if (title != null)
            {
                title.localScale = Vector3.zero;
                title.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetUpdate(true)
                    .OnComplete(() => title.DOShakePosition(0.3f, 5f).SetUpdate(true));
            }

            if (_gameOverScoreText != null)
            {
                int wave = TurnManager.IsAvailable ? TurnManager.Instance.TurnNumber : 0;
                int shards = CurrencyManager.IsAvailable ? CurrencyManager.Instance.Shards : 0;
                int bestWave = 0;
                bool isNewRecord = false;

                if (SaveManager.IsAvailable && SaveManager.Instance.Data != null)
                {
                    bestWave = SaveManager.Instance.Data.highestWave;
                    isNewRecord = wave >= bestWave && wave > 0;
                }

                string recordText = isNewRecord ? "\n<color=#FFD700><size=120%>NEW RECORD!</size></color>" : $"\nBest: Wave {bestWave}";
                _gameOverScoreText.text = $"Wave {wave}\n{shards} Shards{recordText}";

                _gameOverScoreText.transform.localScale = Vector3.zero;
                _gameOverScoreText.transform.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack).SetDelay(0.3f).SetUpdate(true);

                if (isNewRecord)
                {
                    _gameOverScoreText.transform.DOShakePosition(0.5f, 3f)
                        .SetDelay(0.7f).SetUpdate(true);
                }
            }
        }
    }
}
