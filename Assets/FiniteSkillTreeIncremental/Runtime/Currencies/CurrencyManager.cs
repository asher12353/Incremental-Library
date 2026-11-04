using System;
using System.Collections.Generic;
using UnityEngine;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.Core.Events;
using FiniteSkillTreeIncremental.Core.Stats;

namespace FiniteSkillTreeIncremental.Currencies
{
    public struct CurrencyChangedSignal
    {
        public CurrencyDefinition Currency;
        public BigNumber NewBalance;
        public BigNumber Delta;
    }

    [DefaultExecutionOrder(-50)]
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        [Header("Registered Currencies")]
        [SerializeField] private List<CurrencyDefinition> _registeredCurrencies = new List<CurrencyDefinition>();

        [Header("Generators")]
        [SerializeField] private List<CurrencyGenerator> _generators = new List<CurrencyGenerator>();

        private readonly Dictionary<string, CurrencyWallet> _walletsById = new Dictionary<string, CurrencyWallet>();
        private StatPipeline _statPipeline;

        public StatPipeline StatPipeline => _statPipeline ?? (_statPipeline = new StatPipeline());
        public IReadOnlyList<CurrencyDefinition> RegisteredCurrencies => _registeredCurrencies;
        public IReadOnlyList<CurrencyGenerator> Generators => _generators;

        public event Action OnInitialized;

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

            InitializeWallets();
        }

        private void Start()
        {
            OnInitialized?.Invoke();
        }

        public void InitializeWallets()
        {
            if (_statPipeline == null)
            {
                _statPipeline = new StatPipeline();
            }

            foreach (var cur in _registeredCurrencies)
            {
                if (cur != null && !_walletsById.ContainsKey(cur.Id))
                {
                    var wallet = new CurrencyWallet(cur);
                    wallet.OnBalanceChanged += HandleWalletBalanceChanged;
                    _walletsById[cur.Id] = wallet;
                }
            }
        }

        private void HandleWalletBalanceChanged(CurrencyDefinition def, BigNumber newAmount, BigNumber delta)
        {
            SignalBus.Publish(new CurrencyChangedSignal
            {
                Currency = def,
                NewBalance = newAmount,
                Delta = delta
            });
        }

        private void Update()
        {
            TickGenerators(Time.deltaTime);
        }

        public void TickGenerators(float deltaTime)
        {
            if (deltaTime <= 0f) return;

            // Recalculate production rates
            var productionByCurrency = new Dictionary<string, BigNumber>();

            for (int i = 0; i < _generators.Count; i++)
            {
                var gen = _generators[i];
                if (gen.Level > 0 && gen.TargetCurrency != null)
                {
                    BigNumber rate = gen.GetProductionPerSecond(StatPipeline);
                    string cId = gen.TargetCurrency.Id;
                    if (!productionByCurrency.TryGetValue(cId, out var currentRate))
                    {
                        currentRate = BigNumber.Zero;
                    }
                    productionByCurrency[cId] = currentRate + rate;

                    // Produce currency
                    if (_walletsById.TryGetValue(cId, out var wallet))
                    {
                        BigNumber produced = rate * (double)deltaTime;
                        if (produced > BigNumber.Zero)
                        {
                            wallet.Add(produced);
                        }
                    }
                }
            }

            // Update rate per second on all wallets
            foreach (var kvp in _walletsById)
            {
                if (productionByCurrency.TryGetValue(kvp.Key, out var rate))
                {
                    kvp.Value.SetRatePerSecond(rate);
                }
                else
                {
                    kvp.Value.SetRatePerSecond(BigNumber.Zero);
                }
            }
        }

        public CurrencyWallet GetWallet(string currencyId)
        {
            if (string.IsNullOrEmpty(currencyId)) return null;
            _walletsById.TryGetValue(currencyId, out var wallet);
            return wallet;
        }

        public CurrencyWallet GetWallet(CurrencyDefinition definition)
        {
            if (definition == null) return null;
            return GetWallet(definition.Id);
        }

        public void RegisterCurrency(CurrencyDefinition def)
        {
            if (def == null || _walletsById.ContainsKey(def.Id)) return;
            _registeredCurrencies.Add(def);
            var wallet = new CurrencyWallet(def);
            wallet.OnBalanceChanged += HandleWalletBalanceChanged;
            _walletsById[def.Id] = wallet;
        }

        public void RegisterGenerator(CurrencyGenerator gen)
        {
            if (gen == null || _generators.Contains(gen)) return;
            _generators.Add(gen);
        }

        public void ManualClickEarn(CurrencyDefinition currency, BigNumber baseClickAmount)
        {
            if (currency == null) return;
            var wallet = GetWallet(currency);
            if (wallet == null) return;

            BigNumber clickMultiplier = StatPipeline.GetMultiplier(StatType.ClickPowerMultiplier);
            BigNumber globalMultiplier = StatPipeline.GetMultiplier(StatType.GlobalProductionMultiplier);
            BigNumber totalEarned = baseClickAmount * clickMultiplier * globalMultiplier;

            wallet.Add(totalEarned);
        }

        public void ResetNonPrestigeCurrenciesAndGenerators()
        {
            foreach (var kvp in _walletsById)
            {
                if (kvp.Value.Definition != null && !kvp.Value.Definition.IsPrestigeCurrency)
                {
                    kvp.Value.ResetToStarting();
                }
            }

            for (int i = 0; i < _generators.Count; i++)
            {
                _generators[i].ResetLevel();
            }
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
