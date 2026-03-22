using System.Collections.Generic;
using UnityEngine;

namespace BounceReaper
{
    public class SkillManager : Singleton<SkillManager>
    {
        // 1. SerializeField
        [Header("Skills Pool")]
        [SerializeField] private SkillConfig[] _allSkills;

        [Header("XP Curve")]
        [SerializeField] private int _baseXP = 10;
        [SerializeField] private float _xpScaling = 1.5f;

        // 2. Private fields
        private Dictionary<SkillType, int> _activeSkills = new();
        private int _currentXP;
        private int _currentLevel;
        private int _xpToNextLevel;
        private bool _levelUpPending;

        // 3. Properties
        public int CurrentXP => _currentXP;
        public int CurrentLevel => _currentLevel;
        public int XPToNextLevel => _xpToNextLevel;
        public float XPProgress => _xpToNextLevel > 0 ? (float)_currentXP / _xpToNextLevel : 0f;
        public bool LevelUpPending => _levelUpPending;

        // 4. Lifecycle
        protected override void Awake()
        {
            base.Awake();
            ResetRun();
        }

        // 5. Public API
        public void AddXP(int amount)
        {
            _currentXP += amount;

            while (_currentXP >= _xpToNextLevel)
            {
                _currentXP -= _xpToNextLevel;
                _currentLevel++;
                _xpToNextLevel = GetXPForLevel(_currentLevel + 1);
                _levelUpPending = true;
                Debug.Log($"[Skill] LEVEL UP! Level {_currentLevel}");
            }
        }

        public void ConsumeLevelUp()
        {
            _levelUpPending = false;
        }

        public SkillConfig[] GetRandomSkillChoices(int count = 3)
        {
            var available = new List<SkillConfig>();
            foreach (var skill in _allSkills)
            {
                int stacks = GetStacks(skill.Type);
                if (stacks < skill.MaxStacks)
                    available.Add(skill);
            }

            // Weighted random selection
            var choices = new List<SkillConfig>();
            var pool = new List<SkillConfig>(available);

            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                float totalWeight = 0f;
                foreach (var s in pool) totalWeight += s.Weight;

                float roll = Random.Range(0f, totalWeight);
                float cumulative = 0f;

                for (int j = 0; j < pool.Count; j++)
                {
                    cumulative += pool[j].Weight;
                    if (roll <= cumulative)
                    {
                        choices.Add(pool[j]);
                        pool.RemoveAt(j);
                        break;
                    }
                }
            }

            return choices.ToArray();
        }

        public void ApplySkill(SkillConfig skill)
        {
            if (!_activeSkills.ContainsKey(skill.Type))
                _activeSkills[skill.Type] = 0;

            _activeSkills[skill.Type]++;
            Debug.Log($"[Skill] Applied {skill.DisplayName} (x{_activeSkills[skill.Type]})");

            // Immediate effects
            if (skill.Type == SkillType.ExtraBall && BallManager.IsAvailable)
                BallManager.Instance.AddBalls(1);
        }

        public int GetStacks(SkillType type)
        {
            return _activeSkills.TryGetValue(type, out int count) ? count : 0;
        }

        public float GetDamageBonus()
        {
            return GetStacks(SkillType.DamageUp);
        }

        public bool RollFireBall()
        {
            int stacks = GetStacks(SkillType.FireBall);
            if (stacks <= 0) return false;
            return Random.value < (0.25f * stacks);
        }

        public int GetShieldCount()
        {
            return GetStacks(SkillType.Shield);
        }

        public void UseShield()
        {
            if (_activeSkills.ContainsKey(SkillType.Shield) && _activeSkills[SkillType.Shield] > 0)
                _activeSkills[SkillType.Shield]--;
        }

        public int GetPoisonStacks()
        {
            return GetStacks(SkillType.Poison);
        }

        public void ResetRun()
        {
            _activeSkills.Clear();
            _currentXP = 0;
            _currentLevel = 0;
            _levelUpPending = false;
            _xpToNextLevel = GetXPForLevel(1);
        }

        // 6. Private methods
        private int GetXPForLevel(int level)
        {
            return Mathf.RoundToInt(_baseXP * Mathf.Pow(_xpScaling, level - 1));
        }
    }
}
