using System;
using UnityEngine;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.Core.Stats;

namespace FiniteSkillTreeIncremental.Currencies
{
    [Serializable]
    public class CurrencyGenerator
    {
        [SerializeField] private string _id = "miner_1";
        [SerializeField] private string _displayName = "Basic Miner";
        [SerializeField] private CurrencyDefinition _targetCurrency;
        [SerializeField] private CurrencyDefinition _costCurrency;
        [SerializeField] private BigNumber _baseCost = new BigNumber(10);
        [SerializeField] private double _costGrowthRate = 1.15;
        [SerializeField] private BigNumber _baseProductionPerSecond = new BigNumber(1);
        [SerializeField] private int _level = 0;
        [SerializeField] private StatType _productionStat = StatType.GlobalProductionMultiplier;
        [SerializeField] private string _customStatKey = string.Empty;

        public string Id => _id;
        public string DisplayName => _displayName;
        public CurrencyDefinition TargetCurrency => _targetCurrency;
        public CurrencyDefinition CostCurrency => _costCurrency;
        public BigNumber BaseCost => _baseCost;
        public double CostGrowthRate => _costGrowthRate;
        public BigNumber BaseProductionPerSecond => _baseProductionPerSecond;
        public int Level => _level;

        public event Action<CurrencyGenerator, int> OnLevelChanged;

        public CurrencyGenerator() { }

        public CurrencyGenerator(string id, string displayName, CurrencyDefinition targetCurrency, CurrencyDefinition costCurrency, BigNumber baseCost, double costGrowthRate, BigNumber baseProduction)
        {
            _id = id;
            _displayName = displayName;
            _targetCurrency = targetCurrency;
            _costCurrency = costCurrency;
            _baseCost = baseCost;
            _costGrowthRate = costGrowthRate;
            _baseProductionPerSecond = baseProduction;
            _level = 0;
        }

        public BigNumber GetCostForLevel(int targetLevel)
        {
            if (targetLevel <= 0) return _baseCost;
            return _baseCost * BigNumber.Pow(new BigNumber(_costGrowthRate), targetLevel);
        }

        public BigNumber GetCurrentCost()
        {
            return GetCostForLevel(_level);
        }

        public BigNumber GetCostForBulk(int count)
        {
            if (count <= 1) return GetCurrentCost();
            // Sum of geometric series: a * (r^n - 1) / (r - 1)
            double r = _costGrowthRate;
            if (Math.Abs(r - 1.0) < 1e-6)
            {
                return GetCurrentCost() * count;
            }

            BigNumber currentCost = GetCurrentCost();
            double multiplier = (Math.Pow(r, count) - 1.0) / (r - 1.0);
            return currentCost * multiplier;
        }

        public BigNumber GetProductionPerSecond(StatPipeline pipeline)
        {
            if (_level <= 0 || _targetCurrency == null) return BigNumber.Zero;

            BigNumber rawProduction = _baseProductionPerSecond * _level;

            if (pipeline == null) return rawProduction;

            BigNumber globalMultiplier = pipeline.GetMultiplier(StatType.GlobalProductionMultiplier);
            BigNumber speedMultiplier = pipeline.GetMultiplier(StatType.GeneratorSpeedMultiplier);

            BigNumber specificMultiplier = BigNumber.One;
            if (_productionStat != StatType.GlobalProductionMultiplier && _productionStat != StatType.Custom)
            {
                specificMultiplier = pipeline.GetMultiplier(_productionStat);
            }
            else if (!string.IsNullOrEmpty(_customStatKey))
            {
                specificMultiplier = pipeline.GetMultiplier(_customStatKey);
            }

            return rawProduction * globalMultiplier * speedMultiplier * specificMultiplier;
        }

        public bool TryPurchase(CurrencyWallet wallet, int count = 1, StatPipeline pipeline = null)
        {
            if (wallet == null || count <= 0) return false;

            BigNumber totalCost = GetCostForBulk(count);
            if (pipeline != null)
            {
                BigNumber discount = pipeline.GetMultiplier(StatType.SkillCostReduction);
                if (discount > BigNumber.Zero && discount < BigNumber.One)
                {
                    totalCost *= (BigNumber.One - discount);
                }
            }

            if (wallet.TrySpend(totalCost))
            {
                _level += count;
                OnLevelChanged?.Invoke(this, _level);
                return true;
            }
            return false;
        }

        public void SetLevel(int level)
        {
            _level = Math.Max(0, level);
            OnLevelChanged?.Invoke(this, _level);
        }

        public void ResetLevel()
        {
            _level = 0;
            OnLevelChanged?.Invoke(this, _level);
        }
    }
}
