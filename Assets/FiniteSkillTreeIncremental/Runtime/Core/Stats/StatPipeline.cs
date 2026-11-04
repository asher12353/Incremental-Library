using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiniteSkillTreeIncremental.Core.Stats
{
    /// <summary>
    /// Central manager for calculating stat values with Flat, PercentAdd, and PercentMult modifiers.
    /// Provides caching for performance and dispatches change events.
    /// </summary>
    public class StatPipeline
    {
        private readonly Dictionary<string, List<StatModifier>> _modifiersByKey = new Dictionary<string, List<StatModifier>>();
        private readonly Dictionary<string, BigNumber> _cachedMultipliers = new Dictionary<string, BigNumber>();
        private readonly HashSet<string> _dirtyKeys = new HashSet<string>();

        public event Action<string> OnStatChanged;

        public void AddModifier(StatModifier modifier)
        {
            if (modifier == null) return;
            string key = modifier.GetEffectiveKey();

            if (!_modifiersByKey.TryGetValue(key, out var list))
            {
                list = new List<StatModifier>();
                _modifiersByKey[key] = list;
            }

            list.Add(modifier);
            list.Sort((a, b) => a.Order.CompareTo(b.Order));
            _dirtyKeys.Add(key);
            OnStatChanged?.Invoke(key);
        }

        public void AddModifiers(IEnumerable<StatModifier> modifiers)
        {
            if (modifiers == null) return;
            var changedKeys = new HashSet<string>();
            foreach (var mod in modifiers)
            {
                if (mod == null) continue;
                string key = mod.GetEffectiveKey();
                if (!_modifiersByKey.TryGetValue(key, out var list))
                {
                    list = new List<StatModifier>();
                    _modifiersByKey[key] = list;
                }
                list.Add(mod);
                list.Sort((a, b) => a.Order.CompareTo(b.Order));
                _dirtyKeys.Add(key);
                changedKeys.Add(key);
            }

            foreach (var key in changedKeys)
            {
                OnStatChanged?.Invoke(key);
            }
        }

        public bool RemoveModifier(StatModifier modifier)
        {
            if (modifier == null) return false;
            string key = modifier.GetEffectiveKey();

            if (_modifiersByKey.TryGetValue(key, out var list))
            {
                if (list.Remove(modifier))
                {
                    _dirtyKeys.Add(key);
                    OnStatChanged?.Invoke(key);
                    return true;
                }
            }
            return false;
        }

        public int RemoveAllModifiersFromSource(object source)
        {
            if (source == null) return 0;
            int removedCount = 0;
            var changedKeys = new HashSet<string>();

            foreach (var kvp in _modifiersByKey)
            {
                var list = kvp.Value;
                int count = list.RemoveAll(m => ReferenceEquals(m.Source, source) || Equals(m.Source, source));
                if (count > 0)
                {
                    removedCount += count;
                    _dirtyKeys.Add(kvp.Key);
                    changedKeys.Add(kvp.Key);
                }
            }

            foreach (var key in changedKeys)
            {
                OnStatChanged?.Invoke(key);
            }

            return removedCount;
        }

        public void ClearAll()
        {
            _modifiersByKey.Clear();
            _cachedMultipliers.Clear();
            _dirtyKeys.Clear();
            OnStatChanged?.Invoke(string.Empty);
        }

        public BigNumber GetMultiplier(StatType statType)
        {
            return CalculateValue(statType.ToString(), BigNumber.One);
        }

        public BigNumber GetMultiplier(string customKey)
        {
            return CalculateValue($"Custom_{customKey}", BigNumber.One);
        }

        public BigNumber CalculateValue(StatType statType, BigNumber baseValue)
        {
            return CalculateValue(statType.ToString(), baseValue);
        }

        public BigNumber CalculateValue(string key, BigNumber baseValue)
        {
            if (!_modifiersByKey.TryGetValue(key, out var list) || list.Count == 0)
            {
                return baseValue;
            }

            // Calculation formula: (Base + Flat) * (1 + Sum(PercentAdd)) * Product(1 + PercentMult)
            BigNumber sumFlat = BigNumber.Zero;
            BigNumber sumPercentAdd = BigNumber.Zero;
            BigNumber productPercentMult = BigNumber.One;

            for (int i = 0; i < list.Count; i++)
            {
                var mod = list[i];
                switch (mod.Type)
                {
                    case ModifierType.Flat:
                        sumFlat += mod.Value;
                        break;
                    case ModifierType.PercentAdd:
                        sumPercentAdd += mod.Value;
                        break;
                    case ModifierType.PercentMult:
                        productPercentMult *= (BigNumber.One + mod.Value);
                        break;
                }
            }

            BigNumber result = (baseValue + sumFlat) * (BigNumber.One + sumPercentAdd) * productPercentMult;
            return BigNumber.Max(BigNumber.Zero, result);
        }
    }
}
