using System;
using System.Collections.Generic;
using UnityEngine;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.Core.Events;
using FiniteSkillTreeIncremental.Core.Stats;
using FiniteSkillTreeIncremental.Currencies;

namespace FiniteSkillTreeIncremental.SkillTree
{
    public struct SkillRankChangedSignal
    {
        public SkillTreeDefinition Tree;
        public SkillNodeDefinition Node;
        public int OldRank;
        public int NewRank;
    }

    public struct SkillTreeResetSignal
    {
        public SkillTreeDefinition Tree;
    }

    public class SkillTreeController : MonoBehaviour
    {
        [Header("Skill Tree Configuration")]
        [SerializeField] private SkillTreeDefinition _treeDefinition;

        private readonly Dictionary<string, SkillNodeState> _nodeStates = new Dictionary<string, SkillNodeState>();
        private readonly List<StatModifier> _activeTreeModifiers = new List<StatModifier>();

        public SkillTreeDefinition TreeDefinition => _treeDefinition;
        public IReadOnlyDictionary<string, SkillNodeState> NodeStates => _nodeStates;

        public event Action<SkillNodeDefinition, int> OnNodeRankUpdated;
        public event Action OnTreeReset;

        private void Awake()
        {
            InitializeStates();
        }

        private void Start()
        {
            ApplyAllModifiersToPipeline();
        }

        public void InitializeStates()
        {
            if (_treeDefinition == null) return;

            foreach (var node in _treeDefinition.Nodes)
            {
                if (node != null && !_nodeStates.ContainsKey(node.Id))
                {
                    var state = new SkillNodeState(node);
                    state.OnRankChanged += HandleNodeRankChanged;
                    _nodeStates[node.Id] = state;
                }
            }
        }

        public void SetTreeDefinition(SkillTreeDefinition treeDef)
        {
            _treeDefinition = treeDef;
            _nodeStates.Clear();
            InitializeStates();
            ApplyAllModifiersToPipeline();
        }

        private void HandleNodeRankChanged(SkillNodeState state, int newRank)
        {
            ApplyAllModifiersToPipeline();
            OnNodeRankUpdated?.Invoke(state.Definition, newRank);
        }

        public SkillNodeState GetNodeState(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return null;
            _nodeStates.TryGetValue(nodeId, out var state);
            return state;
        }

        public SkillNodeState GetNodeState(SkillNodeDefinition node)
        {
            if (node == null) return null;
            return GetNodeState(node.Id);
        }

        public bool IsPrerequisiteMet(SkillNodeDefinition node)
        {
            if (node == null) return false;
            if (node.Prerequisites == null || node.Prerequisites.Count == 0) return true;

            for (int i = 0; i < node.Prerequisites.Count; i++)
            {
                var prereq = node.Prerequisites[i];
                if (prereq.RequiredNode == null) continue;

                var parentState = GetNodeState(prereq.RequiredNode);
                if (parentState == null || parentState.CurrentRank < prereq.RequiredRank)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CanUpgrade(SkillNodeDefinition node)
        {
            if (node == null) return false;
            var state = GetNodeState(node);
            if (state == null || state.IsMaxRank) return false;
            if (!IsPrerequisiteMet(node)) return false;

            // Check currency cost
            var currencyMgr = CurrencyManager.Instance;
            if (currencyMgr != null && node.CostCurrency != null)
            {
                var wallet = currencyMgr.GetWallet(node.CostCurrency);
                if (wallet == null || !wallet.CanAfford(state.GetCurrentUpgradeCost()))
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryUpgradeNode(SkillNodeDefinition node)
        {
            if (!CanUpgrade(node)) return false;

            var state = GetNodeState(node);
            BigNumber cost = state.GetCurrentUpgradeCost();
            var currencyMgr = CurrencyManager.Instance;

            if (currencyMgr != null && node.CostCurrency != null)
            {
                var wallet = currencyMgr.GetWallet(node.CostCurrency);
                if (wallet != null && !wallet.TrySpend(cost))
                {
                    return false;
                }
            }

            int oldRank = state.CurrentRank;
            state.IncrementRank();

            SignalBus.Publish(new SkillRankChangedSignal
            {
                Tree = _treeDefinition,
                Node = node,
                OldRank = oldRank,
                NewRank = state.CurrentRank
            });

            return true;
        }

        public bool CanRefundNode(SkillNodeDefinition node)
        {
            if (node == null) return false;
            var state = GetNodeState(node);
            if (state == null || state.CurrentRank <= 0) return false;

            // Check if any child node currently allocated depends on this node's current rank
            if (_treeDefinition != null)
            {
                for (int i = 0; i < _treeDefinition.Nodes.Count; i++)
                {
                    var otherNode = _treeDefinition.Nodes[i];
                    if (otherNode == null || otherNode == node) continue;

                    var otherState = GetNodeState(otherNode);
                    if (otherState != null && otherState.CurrentRank > 0)
                    {
                        foreach (var prereq in otherNode.Prerequisites)
                        {
                            if (prereq.RequiredNode == node && (state.CurrentRank - 1) < prereq.RequiredRank)
                            {
                                return false; // Would break dependency
                            }
                        }
                    }
                }
            }

            return true;
        }

        public bool TryRefundNode(SkillNodeDefinition node, bool refundCurrency = true)
        {
            if (!CanRefundNode(node)) return false;

            var state = GetNodeState(node);
            int oldRank = state.CurrentRank;

            if (refundCurrency && node.CostCurrency != null)
            {
                BigNumber rankCost = node.GetCostForRank(oldRank);
                var currencyMgr = CurrencyManager.Instance;
                if (currencyMgr != null)
                {
                    var wallet = currencyMgr.GetWallet(node.CostCurrency);
                    if (wallet != null)
                    {
                        wallet.Add(rankCost);
                    }
                }
            }

            state.DecrementRank();

            SignalBus.Publish(new SkillRankChangedSignal
            {
                Tree = _treeDefinition,
                Node = node,
                OldRank = oldRank,
                NewRank = state.CurrentRank
            });

            return true;
        }

        public void RespecAll(bool refundCurrencies = true)
        {
            if (_treeDefinition == null) return;

            var currencyMgr = CurrencyManager.Instance;

            // Handle respec cost if applicable
            if (_treeDefinition.AllowRespec && _treeDefinition.RespecCost > BigNumber.Zero && currencyMgr != null && _treeDefinition.SkillPointCurrency != null)
            {
                var wallet = currencyMgr.GetWallet(_treeDefinition.SkillPointCurrency);
                if (wallet != null && !wallet.TrySpend(_treeDefinition.RespecCost))
                {
                    return; // Cannot afford respec
                }
            }

            foreach (var kvp in _nodeStates)
            {
                var state = kvp.Value;
                if (state.CurrentRank > 0)
                {
                    if (refundCurrencies && state.Definition != null && state.Definition.CostCurrency != null && currencyMgr != null)
                    {
                        BigNumber totalSpent = BigNumber.Zero;
                        for (int r = 1; r <= state.CurrentRank; r++)
                        {
                            totalSpent += state.Definition.GetCostForRank(r);
                        }
                        var wallet = currencyMgr.GetWallet(state.Definition.CostCurrency);
                        wallet?.Add(totalSpent);
                    }
                    state.Reset();
                }
            }

            ApplyAllModifiersToPipeline();
            OnTreeReset?.Invoke();
            SignalBus.Publish(new SkillTreeResetSignal { Tree = _treeDefinition });
        }

        public void ResetOnPrestige()
        {
            foreach (var kvp in _nodeStates)
            {
                var state = kvp.Value;
                if (state.Definition != null && state.Definition.ResetOnPrestige)
                {
                    state.Reset();
                }
            }

            ApplyAllModifiersToPipeline();
            OnTreeReset?.Invoke();
            SignalBus.Publish(new SkillTreeResetSignal { Tree = _treeDefinition });
        }

        public void ApplyAllModifiersToPipeline()
        {
            var currencyMgr = CurrencyManager.Instance;
            if (currencyMgr == null) return;

            var pipeline = currencyMgr.StatPipeline;
            if (pipeline == null) return;

            // Remove old modifiers registered from this tree
            pipeline.RemoveAllModifiersFromSource(this);
            _activeTreeModifiers.Clear();

            // Calculate active modifiers including Nodebuster adjacency synergy
            foreach (var kvp in _nodeStates)
            {
                var state = kvp.Value;
                if (state.CurrentRank <= 0 || state.Definition == null) continue;

                var def = state.Definition;
                double synergyMultiplier = CalculateNodeSynergyMultiplier(def);

                for (int m = 0; m < def.ModifiersPerRank.Count; m++)
                {
                    var baseMod = def.ModifiersPerRank[m];
                    if (baseMod == null) continue;

                    BigNumber totalModValue = baseMod.Value * state.CurrentRank * synergyMultiplier;
                    StatModifier activeMod;

                    if (baseMod.StatType == StatType.Custom)
                    {
                        activeMod = new StatModifier(baseMod.CustomKey, baseMod.Type, totalModValue, this, baseMod.Order);
                    }
                    else
                    {
                        activeMod = new StatModifier(baseMod.StatType, baseMod.Type, totalModValue, this, baseMod.Order);
                    }

                    _activeTreeModifiers.Add(activeMod);
                    pipeline.AddModifier(activeMod);
                }
            }
        }

        private double CalculateNodeSynergyMultiplier(SkillNodeDefinition node)
        {
            if (node.SynergyTags == null || node.SynergyTags.Count == 0 || node.AdjacentSynergyBonusPerActiveNeighbor <= 0)
            {
                return 1.0;
            }

            int matchingActiveNeighbors = 0;
            // Check connected prerequisite nodes or child nodes
            foreach (var prereq in node.Prerequisites)
            {
                if (prereq.RequiredNode != null)
                {
                    var parentState = GetNodeState(prereq.RequiredNode);
                    if (parentState != null && parentState.CurrentRank > 0 && HasSharedTag(node, prereq.RequiredNode))
                    {
                        matchingActiveNeighbors++;
                    }
                }
            }

            return 1.0 + (matchingActiveNeighbors * node.AdjacentSynergyBonusPerActiveNeighbor);
        }

        private bool HasSharedTag(SkillNodeDefinition a, SkillNodeDefinition b)
        {
            if (a.SynergyTags == null || b.SynergyTags == null) return false;
            for (int i = 0; i < a.SynergyTags.Count; i++)
            {
                string tagA = a.SynergyTags[i];
                for (int j = 0; j < b.SynergyTags.Count; j++)
                {
                    if (string.Equals(tagA, b.SynergyTags[j], StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void UnlockAllNodes()
        {
            foreach (var kvp in _nodeStates)
            {
                var state = kvp.Value;
                if (state.Definition != null)
                {
                    state.SetRank(state.Definition.MaxRank);
                }
            }
            ApplyAllModifiersToPipeline();
        }
    }
}
