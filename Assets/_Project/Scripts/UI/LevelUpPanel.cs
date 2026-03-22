using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace BounceReaper
{
    public class LevelUpPanel : MonoBehaviour
    {
        // 1. SerializeField
        [Header("Panel")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _levelText;

        [Header("Skill Buttons")]
        [SerializeField] private Button[] _skillButtons;
        [SerializeField] private TextMeshProUGUI[] _skillTexts;
        [SerializeField] private Image[] _skillImages;

        // 2. Private fields
        private SkillConfig[] _currentChoices;

        // 4. Lifecycle
        private void Start()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        // 5. Public API
        public void Show(SkillConfig[] choices)
        {
            if (_panel == null || choices == null || choices.Length == 0) return;

            _currentChoices = choices;
            _panel.SetActive(true);

            if (_levelText != null && SkillManager.IsAvailable)
                _levelText.text = $"LEVEL {SkillManager.Instance.CurrentLevel}!";

            // Setup buttons
            for (int i = 0; i < _skillButtons.Length; i++)
            {
                if (i < choices.Length)
                {
                    _skillButtons[i].gameObject.SetActive(true);
                    var skill = choices[i];

                    if (_skillTexts[i] != null)
                    {
                        int stacks = SkillManager.IsAvailable ? SkillManager.Instance.GetStacks(skill.Type) : 0;
                        string stackText = stacks > 0 ? $" (x{stacks + 1})" : "";
                        _skillTexts[i].text = $"{skill.DisplayName}{stackText}\n<size=70%>{skill.Description}</size>";
                    }

                    if (_skillImages[i] != null)
                        _skillImages[i].color = skill.SkillColor;

                    int index = i; // capture for closure
                    _skillButtons[i].onClick.RemoveAllListeners();
                    _skillButtons[i].onClick.AddListener(() => OnSkillChosen(index));

                    // Pop-in animation
                    _skillButtons[i].transform.localScale = Vector3.zero;
                    _skillButtons[i].transform.DOScale(Vector3.one, 0.2f)
                        .SetDelay(i * 0.1f)
                        .SetEase(Ease.OutBack)
                        .SetUpdate(true);
                }
                else
                {
                    _skillButtons[i].gameObject.SetActive(false);
                }
            }

            Debug.Log($"[LevelUp] Showing {choices.Length} skill choices");
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        // 6. Private methods
        private void OnSkillChosen(int index)
        {
            if (_currentChoices == null || index >= _currentChoices.Length) return;

            var skill = _currentChoices[index];

            if (SkillManager.IsAvailable)
            {
                SkillManager.Instance.ApplySkill(skill);
                SkillManager.Instance.ConsumeLevelUp();
            }

            Hide();

            if (TurnManager.IsAvailable)
                TurnManager.Instance.StartAimingPhase();

            Debug.Log($"[LevelUp] Chose: {skill.DisplayName}");
        }
    }
}
