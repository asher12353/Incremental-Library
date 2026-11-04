using System.Collections.Generic;
using UnityEngine;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.Core.Stats;
using FiniteSkillTreeIncremental.Currencies;

namespace FiniteSkillTreeIncremental.SkillTree
{
    [CreateAssetMenu(fileName = "NewSkillNode", menuName = "Incremental/Skill Node Definition", order = 20)]
    public class SkillNodeDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _id = "node_1";
        [SerializeField] private string _title = "Node Title";
        [TextArea(2, 4)]
        [SerializeField] private string _description = "Increases production by 5%.";
        [SerializeField] private Sprite _icon;
        [SerializeField] private SkillNodeType _nodeType = SkillNodeType.Minor;
        [SerializeField] private Vector2 _graphPosition = Vector2.zero;

        [Header("Rank & Costs")]
        [SerializeField] private int _maxRank = 5;
        [SerializeField] private CurrencyDefinition _costCurrency;
        [SerializeField] private BigNumber _baseCost = new BigNumber(100);
        [SerializeField] private double _costGrowthFactor = 1.5;
        [SerializeField] private int _skillPointCost = 1;

        [Header("Prerequisites & Rules")]
        [SerializeField] private List<SkillNodePrerequisite> _prerequisites = new List<SkillNodePrerequisite>();
        [SerializeField] private bool _resetOnPrestige = true;
        [SerializeField] private int _requiredPrestigeTier = 0;

        [Header("Modifiers & Synergies (Nodebuster / PoE)")]
        [SerializeField] private List<StatModifier> _modifiersPerRank = new List<StatModifier>();
        [SerializeField] private List<string> _synergyTags = new List<string>();
        [SerializeField] private double _adjacentSynergyBonusPerActiveNeighbor = 0.05; // +5% modifier efficiency per active neighbor sharing tags

        public string Id => _id;
        public string Title => _title;
        public string Description => _description;
        public Sprite Icon => _icon;
        public SkillNodeType NodeType => _nodeType;
        public Vector2 GraphPosition => _graphPosition;
        public int MaxRank => _maxRank <= 0 ? 1 : _maxRank;
        public CurrencyDefinition CostCurrency => _costCurrency;
        public BigNumber BaseCost => _baseCost;
        public double CostGrowthFactor => _costGrowthFactor;
        public int SkillPointCost => _skillPointCost;
        public IReadOnlyList<SkillNodePrerequisite> Prerequisites => _prerequisites;
        public bool ResetOnPrestige => _resetOnPrestige;
        public int RequiredPrestigeTier => _requiredPrestigeTier;
        public IReadOnlyList<StatModifier> ModifiersPerRank => _modifiersPerRank;
        public IReadOnlyList<string> SynergyTags => _synergyTags;
        public double AdjacentSynergyBonusPerActiveNeighbor => _adjacentSynergyBonusPerActiveNeighbor;

        public BigNumber GetCostForRank(int rank)
        {
            if (rank <= 1) return _baseCost;
            return _baseCost * BigNumber.Pow(new BigNumber(_costGrowthFactor), rank - 1);
        }

        public void SetGraphPosition(Vector2 pos)
        {
            _graphPosition = pos;
        }
    }
}
