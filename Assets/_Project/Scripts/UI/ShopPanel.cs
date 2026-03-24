using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BounceReaper
{
    public class ShopPanel : Singleton<ShopPanel>
    {
        [Header("Panel")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;

        [Header("Upgrade Buttons")]
        [SerializeField] private Button _damageBtn;
        [SerializeField] private TextMeshProUGUI _damageTxt;
        [SerializeField] private Button _extraBallBtn;
        [SerializeField] private TextMeshProUGUI _extraBallTxt;
        [SerializeField] private Button _fireBtn;
        [SerializeField] private TextMeshProUGUI _fireTxt;
        [SerializeField] private Button _shieldBtn;
        [SerializeField] private TextMeshProUGUI _shieldTxt;
        [SerializeField] private Button _poisonBtn;
        [SerializeField] private TextMeshProUGUI _poisonTxt;

        // In-run state (resets each run)
        private int _damageLevel;
        private int _extraBallLevel;
        private int _fireCount;
        private int _shieldCount;
        private int _poisonStacks;
        private bool _isOpen;

        // Cost config
        private const int DamageBaseCost = 5;
        private const float DamageCostScale = 1.5f;
        private const int ExtraBallBaseCost = 15;
        private const float ExtraBallCostScale = 2f;
        private const int FireCost = 30;
        private const int ShieldCost = 20;
        private const int PoisonCost = 25;

        // Properties
        public float DamageBonus => _damageLevel;
        public int ShieldCount => _shieldCount;
        public int PoisonStacks => _poisonStacks;
        public bool IsOpen => _isOpen;

        protected override void Awake()
        {
            base.Awake();
            if (_panel != null) _panel.SetActive(false);
        }

        private void OnEnable()
        {
            if (_openButton != null) _openButton.onClick.AddListener(Open);
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            if (_damageBtn != null) _damageBtn.onClick.AddListener(BuyDamage);
            if (_extraBallBtn != null) _extraBallBtn.onClick.AddListener(BuyExtraBall);
            if (_fireBtn != null) _fireBtn.onClick.AddListener(BuyFire);
            if (_shieldBtn != null) _shieldBtn.onClick.AddListener(BuyShield);
            if (_poisonBtn != null) _poisonBtn.onClick.AddListener(BuyPoison);
        }

        public void Open()
        {
            if (_panel == null) return;
            _panel.SetActive(true);
            _panel.transform.SetAsLastSibling();
            _isOpen = true;
            Refresh();
        }

        public void Close()
        {
            if (_panel != null) _panel.SetActive(false);
            _isOpen = false;
        }

        public void ResetRun()
        {
            _damageLevel = 0;
            _extraBallLevel = 0;
            _fireCount = 0;
            _shieldCount = 0;
            _poisonStacks = 0;
            Close();
        }

        public void UseShield()
        {
            if (_shieldCount > 0) _shieldCount--;
        }

        public bool RollFireBall()
        {
            if (_fireCount <= 0) return false;
            return Random.value < (0.25f * _fireCount);
        }

        private void BuyDamage()
        {
            int cost = GetDamageCost();
            if (!TrySpend(cost)) return;
            _damageLevel++;
            Debug.Log($"[Shop] Damage +1 (Lv.{_damageLevel})");
            Refresh();
        }

        private void BuyExtraBall()
        {
            int cost = GetExtraBallCost();
            if (!TrySpend(cost)) return;
            _extraBallLevel++;
            if (BallManager.IsAvailable) BallManager.Instance.AddBalls(1);
            Debug.Log($"[Shop] Extra Ball +1 (Lv.{_extraBallLevel})");
            Refresh();
        }

        private void BuyFire()
        {
            if (!TrySpend(FireCost)) return;
            _fireCount++;
            Debug.Log($"[Shop] Fire Ball x{_fireCount}");
            Refresh();
        }

        private void BuyShield()
        {
            if (!TrySpend(ShieldCost)) return;
            _shieldCount++;
            Debug.Log($"[Shop] Shield x{_shieldCount}");
            Refresh();
        }

        private void BuyPoison()
        {
            if (!TrySpend(PoisonCost)) return;
            _poisonStacks++;
            Debug.Log($"[Shop] Poison x{_poisonStacks}");
            Refresh();
        }

        private bool TrySpend(int cost)
        {
            if (!CurrencyManager.IsAvailable) return false;
            return CurrencyManager.Instance.SpendShards(cost);
        }

        private void Refresh()
        {
            int shards = CurrencyManager.IsAvailable ? CurrencyManager.Instance.Shards : 0;
            RefreshBtn(_damageBtn, _damageTxt, $"Damage +1\n(Lv.{_damageLevel})", GetDamageCost(), shards);
            RefreshBtn(_extraBallBtn, _extraBallTxt, $"Extra Ball\n(Lv.{_extraBallLevel})", GetExtraBallCost(), shards);
            RefreshBtn(_fireBtn, _fireTxt, $"Fire Ball\n(x{_fireCount})", FireCost, shards);
            RefreshBtn(_shieldBtn, _shieldTxt, $"Shield\n(x{_shieldCount})", ShieldCost, shards);
            RefreshBtn(_poisonBtn, _poisonTxt, $"Poison\n(x{_poisonStacks})", PoisonCost, shards);
        }

        private void RefreshBtn(Button btn, TextMeshProUGUI txt, string label, int cost, int shards)
        {
            if (btn == null || txt == null) return;
            bool canAfford = shards >= cost;
            string color = canAfford ? "#4CFF4C" : "#FF4C4C";
            txt.text = $"{label}\n<size=70%><color={color}>{cost}</color></size>";
            btn.interactable = canAfford;
        }

        private int GetDamageCost() => Mathf.RoundToInt(DamageBaseCost * Mathf.Pow(DamageCostScale, _damageLevel));
        private int GetExtraBallCost() => Mathf.RoundToInt(ExtraBallBaseCost * Mathf.Pow(ExtraBallCostScale, _extraBallLevel));
    }
}
