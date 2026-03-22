using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
            if (_panel == null || choices == null || choices.Length == 0)
            {
                Debug.LogWarning("[LevelUp] Cannot show — panel or choices null");
                // Fallback: consume level up and resume
                if (SkillManager.IsAvailable) SkillManager.Instance.ConsumeLevelUp();
                if (TurnManager.IsAvailable) TurnManager.Instance.StartAimingPhase();
                return;
            }

            _currentChoices = choices;
            _panel.SetActive(true);

            if (_levelText != null && SkillManager.IsAvailable)
                _levelText.text = $"LEVEL {SkillManager.Instance.CurrentLevel}!";

            for (int i = 0; i < _skillButtons.Length; i++)
            {
                if (i < choices.Length)
                {
                    _skillButtons[i].gameObject.SetActive(true);
                    var skill = choices[i];

                    if (i < _skillTexts.Length && _skillTexts[i] != null)
                    {
                        int stacks = SkillManager.IsAvailable ? SkillManager.Instance.GetStacks(skill.Type) : 0;
                        string stackText = stacks > 0 ? $" (x{stacks + 1})" : "";
                        _skillTexts[i].text = $"{skill.DisplayName}{stackText}\n<size=70%>{skill.Description}</size>";
                    }

                    if (i < _skillImages.Length && _skillImages[i] != null)
                        _skillImages[i].color = skill.SkillColor;

                    int index = i;
                    _skillButtons[i].onClick.RemoveAllListeners();
                    _skillButtons[i].onClick.AddListener(() => OnSkillChosen(index));
                }
                else if (i < _skillButtons.Length)
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

            // Check if another level up is pending (multiple level ups at once)
            if (SkillManager.IsAvailable && SkillManager.Instance.LevelUpPending)
            {
                var newChoices = SkillManager.Instance.GetRandomSkillChoices(3);
                Show(newChoices);
            }
            else if (TurnManager.IsAvailable)
            {
                TurnManager.Instance.StartAimingPhase();
            }

            Debug.Log($"[LevelUp] Chose: {skill.DisplayName}");
        }
    }
}
