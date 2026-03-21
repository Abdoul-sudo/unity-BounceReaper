using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace BounceReaper
{
    public class UpgradePanel : MonoBehaviour
    {
        // 1. SerializeField
        [Header("Buttons")]
        [SerializeField] private Button _damageButton;
        [SerializeField] private TextMeshProUGUI _damageText;
        [SerializeField] private Button _speedButton;
        [SerializeField] private TextMeshProUGUI _speedText;
        [SerializeField] private Button _ballsButton;
        [SerializeField] private TextMeshProUGUI _ballsText;
        [SerializeField] private Button _skipButton;

        [Header("Panel")]
        [SerializeField] private GameObject _panel;

        // 4. Lifecycle
        private void Start()
        {
            if (_panel != null) _panel.SetActive(false);

            if (_damageButton != null) _damageButton.onClick.AddListener(OnDamageClick);
            if (_speedButton != null) _speedButton.onClick.AddListener(OnSpeedClick);
            if (_ballsButton != null) _ballsButton.onClick.AddListener(OnBallsClick);
            if (_skipButton != null) _skipButton.onClick.AddListener(OnSkipClick);
        }

        // 5. Public API
        public void Show()
        {
            if (_panel == null) return;
            _panel.SetActive(true);
            RefreshButtons();

            // Slide in from bottom
            var rect = _panel.GetComponent<RectTransform>();
            if (rect != null)
            {
                DOTween.Kill(rect);
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -500f);
                rect.DOAnchorPosY(0f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }

        public void Hide()
        {
            if (_panel == null) return;

            var rect = _panel.GetComponent<RectTransform>();
            if (rect != null)
            {
                DOTween.Kill(rect);
                rect.DOAnchorPosY(-500f, 0.2f).SetEase(Ease.InBack).SetUpdate(true)
                    .OnComplete(() => _panel.SetActive(false));
            }
            else
            {
                _panel.SetActive(false);
            }
        }

        // 6. Private methods
        private void RefreshButtons()
        {
            if (!UpgradeManager.IsAvailable) return;
            var um = UpgradeManager.Instance;

            UpdateButton(_damageButton, _damageText, um.DamageUpgrade, um);
            UpdateButton(_speedButton, _speedText, um.SpeedUpgrade, um);
            UpdateButton(_ballsButton, _ballsText, um.ExtraBallsUpgrade, um);
        }

        private void UpdateButton(Button btn, TextMeshProUGUI text, UpgradeConfig config, UpgradeManager um)
        {
            if (btn == null || text == null || config == null) return;

            int level = um.GetLevel(config);
            int cost = um.GetCost(config);
            bool maxed = level >= config.MaxLevel;
            bool canAfford = um.CanBuy(config);

            if (maxed)
            {
                text.text = $"{config.DisplayName}\nMAX";
                btn.interactable = false;
            }
            else
            {
                text.text = $"{config.DisplayName} Lv.{level}\n<size=70%>{cost} shards</size>";
                btn.interactable = canAfford;
            }
        }

        private void OnDamageClick()
        {
            if (UpgradeManager.IsAvailable)
            {
                UpgradeManager.Instance.Buy(UpgradeManager.Instance.DamageUpgrade);
                RefreshButtons();
            }
        }

        private void OnSpeedClick()
        {
            if (UpgradeManager.IsAvailable)
            {
                UpgradeManager.Instance.Buy(UpgradeManager.Instance.SpeedUpgrade);
                RefreshButtons();
            }
        }

        private void OnBallsClick()
        {
            if (UpgradeManager.IsAvailable)
            {
                UpgradeManager.Instance.Buy(UpgradeManager.Instance.ExtraBallsUpgrade);
                RefreshButtons();
            }
        }

        private void OnSkipClick()
        {
            Hide();
            // Resume the game — TurnManager will detect panel closed
            if (TurnManager.IsAvailable)
                TurnManager.Instance.StartAimingPhase();
        }
    }
}
