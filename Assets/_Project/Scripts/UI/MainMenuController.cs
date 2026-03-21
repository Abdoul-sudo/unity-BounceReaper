using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

namespace BounceReaper
{
    public class MainMenuController : MonoBehaviour
    {
        // 1. SerializeField
        [Header("UI")]
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _tapText;
        [SerializeField] private TextMeshProUGUI _bestWaveText;

        // 2. Private fields
        private bool _waitingForTap;

        // 4. Lifecycle
        private void Start()
        {
            ShowMenu();
        }

        private void Update()
        {
            if (!_waitingForTap) return;

            var mouse = Mouse.current;
            var touch = Touchscreen.current;

            bool tapped = (mouse != null && mouse.leftButton.wasPressedThisFrame)
                       || (touch != null && touch.primaryTouch.press.wasPressedThisFrame);

            if (tapped)
            {
                StartGame();
            }
        }

        // 5. Public API
        public void ShowMenu()
        {
            if (_menuPanel == null) return;

            _menuPanel.SetActive(true);
            _waitingForTap = true;

            // Best wave
            if (_bestWaveText != null)
            {
                int best = 0;
                if (SaveManager.IsAvailable && SaveManager.Instance.Data != null)
                    best = SaveManager.Instance.Data.highestWave;
                _bestWaveText.text = best > 0 ? $"Best: Wave {best}" : "";
            }

            // Tap text pulse
            if (_tapText != null)
            {
                _tapText.DOFade(0.4f, 0.8f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);
            }

            // Title scale in
            if (_titleText != null)
            {
                _titleText.transform.localScale = Vector3.zero;
                _titleText.transform.DOScale(Vector3.one, 0.5f)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
            }
        }

        // 6. Private methods
        private void StartGame()
        {
            _waitingForTap = false;

            if (_tapText != null)
                DOTween.Kill(_tapText);

            // Fade out menu
            if (_menuPanel != null)
            {
                var img = _menuPanel.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    img.DOFade(0f, 0.3f).SetUpdate(true);
                }

                // Fade all texts
                foreach (var tmp in _menuPanel.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    tmp.DOFade(0f, 0.3f).SetUpdate(true);
                }

                DOVirtual.DelayedCall(0.35f, () =>
                {
                    _menuPanel.SetActive(false);

                    // Start the game
                    if (TurnManager.IsAvailable)
                        TurnManager.Instance.StartGame();
                }).SetUpdate(true);
            }
        }
    }
}
