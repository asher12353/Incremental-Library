using System;
using System.Collections.Generic;
using UnityEngine;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.Core.Stats;

namespace FiniteSkillTreeIncremental.Prestige
{
    public enum PrestigeFormulaType
    {
        SquareRoot,
        CubeRoot,
        LinearRatio,
        Logarithmic,
        CustomExponent
    }

    [Serializable]
    public class PrestigeMilestone
    {
        [SerializeField] private string _id = "milestone_1";
        [SerializeField] private string _title = "First Expedition";
        [TextArea(2, 3)]
        [SerializeField] private string _description = "Permanent +25% Global Production.";
        [SerializeField] private int _requiredPrestigeCount = 1;
        [SerializeField] private BigNumber _requiredLifetimePrestigeCurrency = BigNumber.Zero;
        [SerializeField] private List<StatModifier> _modifiers = new List<StatModifier>();

        public string Id => _id;
        public string Title => _title;
        public string Description => _description;
        public int RequiredPrestigeCount => _requiredPrestigeCount;
        public BigNumber RequiredLifetimePrestigeCurrency => _requiredLifetimePrestigeCurrency;
        public IReadOnlyList<StatModifier> Modifiers => _modifiers;

        public bool IsUnlocked(int prestigeCount, BigNumber lifetimePrestigeCurrency)
        {
            if (prestigeCount < _requiredPrestigeCount) return false;
            if (_requiredLifetimePrestigeCurrency > BigNumber.Zero && lifetimePrestigeCurrency < _requiredLifetimePrestigeCurrency) return false;
            return true;
        }
    }
}
