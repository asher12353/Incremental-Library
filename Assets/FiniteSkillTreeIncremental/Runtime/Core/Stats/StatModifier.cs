using System;
using UnityEngine;

namespace FiniteSkillTreeIncremental.Core.Stats
{
    [Serializable]
    public class StatModifier
    {
        [SerializeField] private StatType _statType = StatType.GlobalProductionMultiplier;
        [SerializeField] private string _customKey = string.Empty;
        [SerializeField] private ModifierType _type = ModifierType.PercentAdd;
        [SerializeField] private BigNumber _value = BigNumber.Zero;
        [SerializeField] private int _order = 0;

        public StatType StatType => _statType;
        public string CustomKey => _customKey;
        public ModifierType Type => _type;
        public BigNumber Value => _value;
        public int Order => _order;
        public object Source { get; set; }

        public StatModifier() { }

        public StatModifier(StatType statType, ModifierType type, BigNumber value, object source = null, int order = 0)
        {
            _statType = statType;
            _type = type;
            _value = value;
            Source = source;
            _order = order == 0 ? (int)type : order;
        }

        public StatModifier(string customKey, ModifierType type, BigNumber value, object source = null, int order = 0)
        {
            _statType = StatType.Custom;
            _customKey = customKey;
            _type = type;
            _value = value;
            Source = source;
            _order = order == 0 ? (int)type : order;
        }

        public string GetEffectiveKey()
        {
            return _statType == StatType.Custom ? $"Custom_{_customKey}" : _statType.ToString();
        }
    }
}
