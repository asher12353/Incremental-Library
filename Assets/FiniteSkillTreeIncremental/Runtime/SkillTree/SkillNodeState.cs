using System;
using UnityEngine;
using FiniteSkillTreeIncremental.Core;

namespace FiniteSkillTreeIncremental.SkillTree
{
    [Serializable]
    public class SkillNodeState
    {
        [SerializeField] private SkillNodeDefinition _definition;
        [SerializeField] private int _currentRank = 0;

        public SkillNodeDefinition Definition => _definition;
        public int CurrentRank => _currentRank;
        public bool IsUnlocked => _currentRank > 0;
        public bool IsMaxRank => _definition != null && _currentRank >= _definition.MaxRank;

        public event Action<SkillNodeState, int> OnRankChanged;

        public SkillNodeState(SkillNodeDefinition definition, int initialRank = 0)
        {
            _definition = definition;
            _currentRank = Math.Max(0, initialRank);
        }

        public BigNumber GetCurrentUpgradeCost()
        {
            if (_definition == null || IsMaxRank) return BigNumber.Zero;
            return _definition.GetCostForRank(_currentRank + 1);
        }

        public void SetRank(int rank)
        {
            int clamped = _definition != null ? Mathf.Clamp(rank, 0, _definition.MaxRank) : Math.Max(0, rank);
            if (_currentRank != clamped)
            {
                _currentRank = clamped;
                OnRankChanged?.Invoke(this, _currentRank);
            }
        }

        public void IncrementRank(int amount = 1)
        {
            SetRank(_currentRank + amount);
        }

        public void DecrementRank(int amount = 1)
        {
            SetRank(_currentRank - amount);
        }

        public void Reset()
        {
            SetRank(0);
        }
    }
}
