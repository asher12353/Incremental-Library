using System;
using UnityEngine;
using FiniteSkillTreeIncremental.Core;

namespace FiniteSkillTreeIncremental.Currencies
{
    [Serializable]
    public class CurrencyWallet
    {
        [SerializeField] private CurrencyDefinition _definition;
        [SerializeField] private BigNumber _amount = BigNumber.Zero;
        [SerializeField] private BigNumber _lifetimeEarned = BigNumber.Zero;
        [SerializeField] private BigNumber _ratePerSecond = BigNumber.Zero;

        public CurrencyDefinition Definition => _definition;
        public BigNumber Amount => _amount;
        public BigNumber LifetimeEarned => _lifetimeEarned;
        public BigNumber RatePerSecond => _ratePerSecond;

        public event Action<CurrencyDefinition, BigNumber, BigNumber> OnBalanceChanged;

        public CurrencyWallet(CurrencyDefinition definition)
        {
            _definition = definition;
            _amount = definition != null ? definition.StartingAmount : BigNumber.Zero;
            _lifetimeEarned = _amount;
            _ratePerSecond = BigNumber.Zero;
        }

        public bool CanAfford(BigNumber cost)
        {
            if (cost.IsNegative || cost.IsZero) return true;
            return _amount >= cost;
        }

        public bool TrySpend(BigNumber cost)
        {
            if (cost.IsNegative || cost.IsZero) return true;
            if (_amount < cost) return false;

            _amount -= cost;
            OnBalanceChanged?.Invoke(_definition, _amount, -cost);
            return true;
        }

        public void Add(BigNumber amount)
        {
            if (amount.IsNegative || amount.IsZero) return;

            _amount += amount;
            _lifetimeEarned += amount;

            if (_definition != null && _definition.HasCap && _amount > _definition.MaxCap)
            {
                _amount = _definition.MaxCap;
            }

            OnBalanceChanged?.Invoke(_definition, _amount, amount);
        }

        public void SetAmount(BigNumber amount)
        {
            BigNumber delta = amount - _amount;
            _amount = BigNumber.Max(BigNumber.Zero, amount);
            if (delta.IsPositive)
            {
                _lifetimeEarned += delta;
            }
            if (_definition != null && _definition.HasCap && _amount > _definition.MaxCap)
            {
                _amount = _definition.MaxCap;
            }
            OnBalanceChanged?.Invoke(_definition, _amount, delta);
        }

        public void SetRatePerSecond(BigNumber rate)
        {
            _ratePerSecond = BigNumber.Max(BigNumber.Zero, rate);
        }

        public void ResetToStarting()
        {
            BigNumber oldAmount = _amount;
            _amount = _definition != null ? _definition.StartingAmount : BigNumber.Zero;
            _ratePerSecond = BigNumber.Zero;
            OnBalanceChanged?.Invoke(_definition, _amount, _amount - oldAmount);
        }
    }
}
