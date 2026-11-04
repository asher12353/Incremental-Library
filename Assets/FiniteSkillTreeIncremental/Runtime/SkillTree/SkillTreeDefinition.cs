using System.Collections.Generic;
using UnityEngine;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.Currencies;

namespace FiniteSkillTreeIncremental.SkillTree
{
    [CreateAssetMenu(fileName = "NewSkillTree", menuName = "Incremental/Skill Tree Definition", order = 21)]
    public class SkillTreeDefinition : ScriptableObject
    {
        [SerializeField] private string _treeId = "main_tree";
        [SerializeField] private string _treeName = "Main Skill Tree";
        [TextArea(2, 3)]
        [SerializeField] private string _description = "Mastery and upgrade constellation.";
        [SerializeField] private List<SkillNodeDefinition> _nodes = new List<SkillNodeDefinition>();
        [SerializeField] private CurrencyDefinition _skillPointCurrency;
        [SerializeField] private bool _allowRespec = true;
        [SerializeField] private BigNumber _respecCost = BigNumber.Zero; // 0 for free respec

        public string TreeId => _treeId;
        public string TreeName => _treeName;
        public string Description => _description;
        public IReadOnlyList<SkillNodeDefinition> Nodes => _nodes;
        public CurrencyDefinition SkillPointCurrency => _skillPointCurrency;
        public bool AllowRespec => _allowRespec;
        public BigNumber RespecCost => _respecCost;

        public void AddNode(SkillNodeDefinition node)
        {
            if (node != null && !_nodes.Contains(node))
            {
                _nodes.Add(node);
            }
        }
    }
}
