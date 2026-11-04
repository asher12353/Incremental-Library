using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.Core.Stats;
using FiniteSkillTreeIncremental.Currencies;

namespace FiniteSkillTreeIncremental.Editor
{
    public class SimulationWindow : EditorWindow
    {
        private CurrencyDefinition _primaryCurrency;
        private List<CurrencyGenerator> _simulationGenerators = new List<CurrencyGenerator>();
        private float _simulateDurationHours = 24f;
        private float _tickStepSeconds = 1.0f;
        private bool _autoBuyUpgrades = true;

        private Vector2 _scrollPos;
        private readonly List<string> _simulationLogs = new List<string>();

        [MenuItem("Tools/Incremental/Economy Balance Simulator")]
        public static void ShowWindow()
        {
            var window = GetWindow<SimulationWindow>("Economy Simulator");
            window.minSize = new Vector2(500, 450);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Incremental Economy & Idle Simulation Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _primaryCurrency = (CurrencyDefinition)EditorGUILayout.ObjectField("Primary Currency", _primaryCurrency, typeof(CurrencyDefinition), false);
            _simulateDurationHours = EditorGUILayout.FloatField("Simulate Duration (Hours)", _simulateDurationHours);
            _tickStepSeconds = EditorGUILayout.FloatField("Tick Step (Seconds)", _tickStepSeconds);
            _autoBuyUpgrades = EditorGUILayout.Toggle("Simulate Optimal Auto-Purchases", _autoBuyUpgrades);

            EditorGUILayout.Space();

            if (GUILayout.Button("Run Fast Simulation", GUILayout.Height(32)))
            {
                RunSimulation();
            }

            EditorGUILayout.Space();
            GUILayout.Label("Simulation Log & Milestone Progression:", EditorStyles.boldLabel);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            if (_simulationLogs.Count == 0)
            {
                EditorGUILayout.HelpBox("Configure parameters and click 'Run Fast Simulation'.", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < _simulationLogs.Count; i++)
                {
                    EditorGUILayout.LabelField(_simulationLogs[i], EditorStyles.wordWrappedLabel);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void RunSimulation()
        {
            _simulationLogs.Clear();

            if (_primaryCurrency == null)
            {
                _simulationLogs.Add("[ERROR] Please select a primary currency to simulate.");
                return;
            }

            // Setup test environment
            var wallet = new CurrencyWallet(_primaryCurrency);
            var pipeline = new StatPipeline();

            // Default generator setup if empty
            var gen1 = new CurrencyGenerator("miner_1", "Miner T1", _primaryCurrency, _primaryCurrency, new BigNumber(10), 1.15, new BigNumber(1));
            var gen2 = new CurrencyGenerator("miner_2", "Miner T2", _primaryCurrency, _primaryCurrency, new BigNumber(100), 1.18, new BigNumber(10));
            var gen3 = new CurrencyGenerator("miner_3", "Miner T3", _primaryCurrency, _primaryCurrency, new BigNumber(1000), 1.22, new BigNumber(120));

            gen1.SetLevel(1); // Player starts with 1 miner

            var gens = new List<CurrencyGenerator> { gen1, gen2, gen3 };

            double totalSeconds = Mathf.Max(1f, _simulateDurationHours) * 3600.0;
            double currentSimTime = 0.0;
            float step = Mathf.Max(0.1f, _tickStepSeconds);

            double logIntervalSeconds = totalSeconds / 10.0;
            double nextLogTime = logIntervalSeconds;

            _simulationLogs.Add($"--- SIMULATION STARTED (Duration: {_simulateDurationHours:F1}h, Step: {step:F1}s) ---");
            _simulationLogs.Add($"[Hour 0.0] Initial Balance: {_primaryCurrency.Format(wallet.Amount)} | Rate: {_primaryCurrency.Format(gen1.GetProductionPerSecond(pipeline))}/s");

            while (currentSimTime < totalSeconds)
            {
                currentSimTime += step;

                // Calculate production
                BigNumber stepProduction = BigNumber.Zero;
                for (int i = 0; i < gens.Count; i++)
                {
                    stepProduction += gens[i].GetProductionPerSecond(pipeline) * step;
                }
                wallet.Add(stepProduction);

                // Auto-purchase highest ROI generator if enabled
                if (_autoBuyUpgrades)
                {
                    for (int i = gens.Count - 1; i >= 0; i--)
                    {
                        var gen = gens[i];
                        if (wallet.CanAfford(gen.GetCurrentCost()))
                        {
                            gen.TryPurchase(wallet, 1, pipeline);
                        }
                    }
                }

                if (currentSimTime >= nextLogTime || currentSimTime >= totalSeconds)
                {
                    double hoursElapsed = currentSimTime / 3600.0;
                    BigNumber totalRate = BigNumber.Zero;
                    for (int i = 0; i < gens.Count; i++)
                    {
                        totalRate += gens[i].GetProductionPerSecond(pipeline);
                    }

                    string genLevels = $"[T1: {gen1.Level}, T2: {gen2.Level}, T3: {gen3.Level}]";
                    _simulationLogs.Add($"[Hour {hoursElapsed:F1}] Balance: {_primaryCurrency.Format(wallet.Amount)} | Rate: {_primaryCurrency.Format(totalRate)}/s | Gens: {genLevels}");
                    nextLogTime += logIntervalSeconds;
                }
            }

            _simulationLogs.Add($"--- SIMULATION FINISHED. Lifetime Earned: {_primaryCurrency.Format(wallet.LifetimeEarned)} ---");
        }
    }
}
