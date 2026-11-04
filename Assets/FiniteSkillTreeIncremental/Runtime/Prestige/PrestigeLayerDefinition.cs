using System;
using System.Collections.Generic;
using UnityEngine;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.Core.Stats;
using FiniteSkillTreeIncremental.Currencies;

namespace FiniteSkillTreeIncremental.Prestige
{
    [CreateAssetMenu(fileName = "NewPrestigeLayer", menuName = "Incremental/Prestige Layer Definition", order = 30)]
    public class PrestigeLayerDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _layerId = "prestige_1";
        [SerializeField] private string _displayName = "Prestige / Excavation";
        [TextArea(2, 4)]
        [SerializeField] private string _description = "Reset standard progress to uncover ancient relics and empower future expeditions.";
        [SerializeField] private int _layerTier = 1;

        [Header("Currencies & Requirement")]
        [SerializeField] private CurrencyDefinition _sourceCurrency;
        [SerializeField] private CurrencyDefinition _rewardCurrency;
        [SerializeField] private BigNumber _baseRequirement = new BigNumber(1000000); // 1M

        [Header("Reward Formula")]
        [SerializeField] private PrestigeFormulaType _formulaType = PrestigeFormulaType.SquareRoot;
        [SerializeField] private double _formulaMultiplier = 1.0;
        [SerializeField] private double _customExponent = 0.5;

        [Header("Reset Behavior")]
        [SerializeField] private bool _resetStandardCurrencies = true;
        [SerializeField] private bool _resetGenerators = true;
        [SerializeField] private bool _resetSkillTrees = true;

        [Header("Milestones & Relics (Digseum-style)")]
        [SerializeField] private List<PrestigeMilestone> _milestones = new List<PrestigeMilestone>();

        public string LayerId => _layerId;
        public string DisplayName => _displayName;
        public string Description => _description;
        public int LayerTier => _layerTier;
        public CurrencyDefinition SourceCurrency => _sourceCurrency;
        public CurrencyDefinition RewardCurrency => _rewardCurrency;
        public BigNumber BaseRequirement => _baseRequirement;
        public PrestigeFormulaType FormulaType => _formulaType;
        public double FormulaMultiplier => _formulaMultiplier;
        public double CustomExponent => _customExponent;
        public bool ResetStandardCurrencies => _resetStandardCurrencies;
        public bool ResetGenerators => _resetGenerators;
        public bool ResetSkillTrees => _resetSkillTrees;
        public IReadOnlyList<PrestigeMilestone> Milestones => _milestones;

        public BigNumber CalculateReward(BigNumber sourceAmount, StatPipeline pipeline = null)
        {
            if (sourceAmount < _baseRequirement || _baseRequirement.IsZero)
            {
                return BigNumber.Zero;
            }

            BigNumber ratio = sourceAmount / _baseRequirement;
            BigNumber rawReward = BigNumber.Zero;

            switch (_formulaType)
            {
                case PrestigeFormulaType.SquareRoot:
                    rawReward = BigNumber.Sqrt(ratio) * _formulaMultiplier;
                    break;
                case PrestigeFormulaType.CubeRoot:
                    rawReward = BigNumber.Pow(ratio, 1.0 / 3.0) * _formulaMultiplier;
                    break;
                case PrestigeFormulaType.LinearRatio:
                    rawReward = ratio * _formulaMultiplier;
                    break;
                case PrestigeFormulaType.CustomExponent:
                    rawReward = BigNumber.Pow(ratio, _customExponent) * _formulaMultiplier;
                    break;
                case PrestigeFormulaType.Logarithmic:
                    double logVal = Math.Max(1.0, Math.Log10(Math.Max(1.0, ratio.ToDouble())));
                    rawReward = new BigNumber(logVal * _formulaMultiplier);
                    break;
            }

            if (pipeline != null)
            {
                BigNumber prestigeBonus = pipeline.GetMultiplier(StatType.PrestigeRewardMultiplier);
                rawReward *= prestigeBonus;
            }

            return BigNumber.Floor(rawReward);
        }
    }
}
