using System;
using System.Collections.Generic;
using UnityEngine;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.Core.Events;
using FiniteSkillTreeIncremental.Core.Stats;
using FiniteSkillTreeIncremental.Currencies;
using FiniteSkillTreeIncremental.SkillTree;

namespace FiniteSkillTreeIncremental.Prestige
{
    public struct PrestigeExecutedSignal
    {
        public PrestigeLayerDefinition Layer;
        public BigNumber RewardAmount;
        public int TotalPrestiges;
    }

    [DefaultExecutionOrder(-40)]
    public class PrestigeManager : MonoBehaviour
    {
        public static PrestigeManager Instance { get; private set; }

        [Header("Prestige Layers")]
        [SerializeField] private List<PrestigeLayerDefinition> _layers = new List<PrestigeLayerDefinition>();

        [Header("Runtime Stats")]
        [SerializeField] private int _totalPrestiges = 0;
        [SerializeField] private BigNumber _lifetimePrestigeCurrency = BigNumber.Zero;
        [SerializeField] private float _currentRunTimeSeconds = 0f;
        [SerializeField] private float _fastestPrestigeTimeSeconds = -1f;

        private readonly List<StatModifier> _activeMilestoneModifiers = new List<StatModifier>();
        private readonly HashSet<string> _unlockedMilestones = new HashSet<string>();

        public int TotalPrestiges => _totalPrestiges;
        public BigNumber LifetimePrestigeCurrency => _lifetimePrestigeCurrency;
        public float CurrentRunTimeSeconds => _currentRunTimeSeconds;
        public float FastestPrestigeTimeSeconds => _fastestPrestigeTimeSeconds;
        public IReadOnlyList<PrestigeLayerDefinition> Layers => _layers;
        public IReadOnlyCollection<string> UnlockedMilestones => _unlockedMilestones;

        public event Action<PrestigeLayerDefinition, BigNumber> OnPrestigeExecuted;
        public event Action<PrestigeMilestone> OnMilestoneUnlocked;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            EvaluateMilestones();
        }

        private void Update()
        {
            _currentRunTimeSeconds += Time.deltaTime;
        }

        public BigNumber GetPendingReward(PrestigeLayerDefinition layer)
        {
            if (layer == null || layer.SourceCurrency == null) return BigNumber.Zero;

            var currencyMgr = CurrencyManager.Instance;
            if (currencyMgr == null) return BigNumber.Zero;

            var wallet = currencyMgr.GetWallet(layer.SourceCurrency);
            if (wallet == null) return BigNumber.Zero;

            return layer.CalculateReward(wallet.Amount, currencyMgr.StatPipeline);
        }

        public bool CanPrestige(PrestigeLayerDefinition layer)
        {
            if (layer == null) return false;
            return GetPendingReward(layer) > BigNumber.Zero;
        }

        public bool ExecutePrestige(PrestigeLayerDefinition layer)
        {
            if (!CanPrestige(layer)) return false;

            var currencyMgr = CurrencyManager.Instance;
            if (currencyMgr == null) return false;

            BigNumber reward = GetPendingReward(layer);
            if (reward.IsZero) return false;

            // 1. Award prestige currency
            if (layer.RewardCurrency != null)
            {
                var rewardWallet = currencyMgr.GetWallet(layer.RewardCurrency);
                if (rewardWallet != null)
                {
                    rewardWallet.Add(reward);
                }
                _lifetimePrestigeCurrency += reward;
            }

            // 2. Update stats
            _totalPrestiges++;
            if (_fastestPrestigeTimeSeconds < 0f || _currentRunTimeSeconds < _fastestPrestigeTimeSeconds)
            {
                _fastestPrestigeTimeSeconds = _currentRunTimeSeconds;
            }
            _currentRunTimeSeconds = 0f;

            // 3. Reset standard currencies & generators if configured
            if (layer.ResetStandardCurrencies || layer.ResetGenerators)
            {
                currencyMgr.ResetNonPrestigeCurrenciesAndGenerators();
            }

            // 4. Reset skill trees if configured
            if (layer.ResetSkillTrees)
            {
                var skillTrees = FindObjectsByType<SkillTreeController>(FindObjectsSortMode.None);
                for (int i = 0; i < skillTrees.Length; i++)
                {
                    skillTrees[i].ResetOnPrestige();
                }
            }

            // 5. Evaluate and apply new permanent milestones
            EvaluateMilestones();

            // 6. Notify events & signal bus
            OnPrestigeExecuted?.Invoke(layer, reward);
            SignalBus.Publish(new PrestigeExecutedSignal
            {
                Layer = layer,
                RewardAmount = reward,
                TotalPrestiges = _totalPrestiges
            });

            return true;
        }

        public void EvaluateMilestones()
        {
            var currencyMgr = CurrencyManager.Instance;
            if (currencyMgr == null) return;
            var pipeline = currencyMgr.StatPipeline;
            if (pipeline == null) return;

            bool newlyUnlocked = false;

            for (int l = 0; l < _layers.Count; l++)
            {
                var layer = _layers[l];
                if (layer == null) continue;

                for (int m = 0; m < layer.Milestones.Count; m++)
                {
                    var milestone = layer.Milestones[m];
                    if (milestone == null) continue;

                    if (!_unlockedMilestones.Contains(milestone.Id) && milestone.IsUnlocked(_totalPrestiges, _lifetimePrestigeCurrency))
                    {
                        _unlockedMilestones.Add(milestone.Id);
                        newlyUnlocked = true;
                        OnMilestoneUnlocked?.Invoke(milestone);
                    }
                }
            }

            if (newlyUnlocked || _activeMilestoneModifiers.Count == 0)
            {
                pipeline.RemoveAllModifiersFromSource(this);
                _activeMilestoneModifiers.Clear();

                for (int l = 0; l < _layers.Count; l++)
                {
                    var layer = _layers[l];
                    if (layer == null) continue;

                    for (int m = 0; m < layer.Milestones.Count; m++)
                    {
                        var milestone = layer.Milestones[m];
                        if (milestone != null && _unlockedMilestones.Contains(milestone.Id))
                        {
                            for (int modIdx = 0; modIdx < milestone.Modifiers.Count; modIdx++)
                            {
                                var baseMod = milestone.Modifiers[modIdx];
                                if (baseMod != null)
                                {
                                    StatModifier mod = baseMod.StatType == StatType.Custom
                                        ? new StatModifier(baseMod.CustomKey, baseMod.Type, baseMod.Value, this, baseMod.Order)
                                        : new StatModifier(baseMod.StatType, baseMod.Type, baseMod.Value, this, baseMod.Order);
                                    _activeMilestoneModifiers.Add(mod);
                                    pipeline.AddModifier(mod);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void LoadStats(int totalPrestiges, BigNumber lifetimePrestigeCurrency, float currentRunTime, float fastestTime, IEnumerable<string> unlockedMilestones)
        {
            _totalPrestiges = totalPrestiges;
            _lifetimePrestigeCurrency = lifetimePrestigeCurrency;
            _currentRunTimeSeconds = currentRunTime;
            _fastestPrestigeTimeSeconds = fastestTime;

            _unlockedMilestones.Clear();
            if (unlockedMilestones != null)
            {
                foreach (var id in unlockedMilestones)
                {
                    _unlockedMilestones.Add(id);
                }
            }

            EvaluateMilestones();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
