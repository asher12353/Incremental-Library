using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.Core.Events;
using FiniteSkillTreeIncremental.Currencies;
using FiniteSkillTreeIncremental.Prestige;
using FiniteSkillTreeIncremental.SkillTree;

namespace FiniteSkillTreeIncremental.Save
{
    public struct OfflineEarningsSignal
    {
        public double SecondsOffline;
        public Dictionary<string, BigNumber> Earnings;
    }

    [DefaultExecutionOrder(-30)]
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SaveKeyPrefix = "IncrementalSave_";

        [Header("Save Settings")]
        [SerializeField] private string _saveSlot = "default";
        [SerializeField] private bool _autoSaveEnabled = true;
        [SerializeField] private float _autoSaveIntervalSeconds = 30f;
        [SerializeField] private bool _saveToFile = false;

        [Header("Offline Progress")]
        [SerializeField] private bool _calculateOfflineProgress = true;
        [SerializeField] private float _maxOfflineHours = 24f;
        [SerializeField] private float _offlineEfficiency = 1.0f; // 100%

        private float _autoSaveTimer = 0f;

        public event Action<GameSaveData> OnGameSaved;
        public event Action<GameSaveData> OnGameLoaded;
        public event Action<double, Dictionary<string, BigNumber>> OnOfflineProgressGranted;

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
            LoadGame();
        }

        private void Update()
        {
            if (!_autoSaveEnabled) return;

            _autoSaveTimer += Time.deltaTime;
            if (_autoSaveTimer >= _autoSaveIntervalSeconds)
            {
                _autoSaveTimer = 0f;
                SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGame();
            }
        }

        public void SaveGame()
        {
            var data = new GameSaveData
            {
                saveVersion = 1,
                timestampUnixUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            // 1. Save Currencies & Wallets
            var currencyMgr = CurrencyManager.Instance;
            if (currencyMgr != null)
            {
                foreach (var cur in currencyMgr.RegisteredCurrencies)
                {
                    if (cur == null) continue;
                    var wallet = currencyMgr.GetWallet(cur);
                    if (wallet != null)
                    {
                        var entry = new CurrencySaveEntry { currencyId = cur.Id };
                        entry.SetAmount(wallet.Amount);
                        entry.SetLifetime(wallet.LifetimeEarned);
                        data.currencies.Add(entry);
                    }
                }

                // 2. Save Generators
                foreach (var gen in currencyMgr.Generators)
                {
                    if (gen == null) continue;
                    data.generators.Add(new GeneratorSaveEntry
                    {
                        generatorId = gen.Id,
                        level = gen.Level
                    });
                }
            }

            // 3. Save Skill Trees
            var skillTrees = FindObjectsByType<SkillTreeController>(FindObjectsSortMode.None);
            for (int t = 0; t < skillTrees.Length; t++)
            {
                var tree = skillTrees[t];
                if (tree == null) continue;

                foreach (var kvp in tree.NodeStates)
                {
                    if (kvp.Value != null && kvp.Value.CurrentRank > 0)
                    {
                        data.skills.Add(new SkillNodeSaveEntry
                        {
                            nodeId = kvp.Key,
                            rank = kvp.Value.CurrentRank
                        });
                    }
                }
            }

            // 4. Save Prestige
            var prestigeMgr = PrestigeManager.Instance;
            if (prestigeMgr != null)
            {
                data.prestige.totalPrestiges = prestigeMgr.TotalPrestiges;
                data.prestige.SetLifetimePrestigeCurrency(prestigeMgr.LifetimePrestigeCurrency);
                data.prestige.currentRunTimeSeconds = prestigeMgr.CurrentRunTimeSeconds;
                data.prestige.fastestPrestigeTimeSeconds = prestigeMgr.FastestPrestigeTimeSeconds;
                data.prestige.unlockedMilestones = new List<string>(prestigeMgr.UnlockedMilestones);
            }

            string json = JsonUtility.ToJson(data, true);

            if (_saveToFile)
            {
                string path = GetSaveFilePath();
                File.WriteAllText(path, json);
            }
            else
            {
                PlayerPrefs.SetString(SaveKeyPrefix + _saveSlot, json);
                PlayerPrefs.Save();
            }

            OnGameSaved?.Invoke(data);
        }

        public bool LoadGame()
        {
            string json = null;

            if (_saveToFile)
            {
                string path = GetSaveFilePath();
                if (File.Exists(path))
                {
                    json = File.ReadAllText(path);
                }
            }
            else
            {
                string key = SaveKeyPrefix + _saveSlot;
                if (PlayerPrefs.HasKey(key))
                {
                    json = PlayerPrefs.GetString(key);
                }
            }

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                var data = JsonUtility.FromJson<GameSaveData>(json);
                if (data == null) return false;

                ApplySaveData(data);
                OnGameLoaded?.Invoke(data);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Failed to load save: {ex.Message}");
                return false;
            }
        }

        private void ApplySaveData(GameSaveData data)
        {
            var currencyMgr = CurrencyManager.Instance;
            var prestigeMgr = PrestigeManager.Instance;

            // 1. Load Prestige first so milestones apply modifiers
            if (prestigeMgr != null && data.prestige != null)
            {
                prestigeMgr.LoadStats(
                    data.prestige.totalPrestiges,
                    data.prestige.GetLifetimePrestigeCurrency(),
                    data.prestige.currentRunTimeSeconds,
                    data.prestige.fastestPrestigeTimeSeconds,
                    data.prestige.unlockedMilestones
                );
            }

            // 2. Load Skills
            var skillTrees = FindObjectsByType<SkillTreeController>(FindObjectsSortMode.None);
            var skillMap = new Dictionary<string, int>();
            for (int i = 0; i < data.skills.Count; i++)
            {
                skillMap[data.skills[i].nodeId] = data.skills[i].rank;
            }

            for (int t = 0; t < skillTrees.Length; t++)
            {
                var tree = skillTrees[t];
                if (tree == null) continue;

                foreach (var kvp in tree.NodeStates)
                {
                    if (skillMap.TryGetValue(kvp.Key, out int rank))
                    {
                        kvp.Value.SetRank(rank);
                    }
                    else
                    {
                        kvp.Value.Reset();
                    }
                }
                tree.ApplyAllModifiersToPipeline();
            }

            // 3. Load Generators
            var genMap = new Dictionary<string, int>();
            for (int i = 0; i < data.generators.Count; i++)
            {
                genMap[data.generators[i].generatorId] = data.generators[i].level;
            }

            if (currencyMgr != null)
            {
                for (int i = 0; i < currencyMgr.Generators.Count; i++)
                {
                    var gen = currencyMgr.Generators[i];
                    if (genMap.TryGetValue(gen.Id, out int lvl))
                    {
                        gen.SetLevel(lvl);
                    }
                }

                // 4. Load Currencies
                var curMap = new Dictionary<string, CurrencySaveEntry>();
                for (int i = 0; i < data.currencies.Count; i++)
                {
                    curMap[data.currencies[i].currencyId] = data.currencies[i];
                }

                foreach (var cur in currencyMgr.RegisteredCurrencies)
                {
                    var wallet = currencyMgr.GetWallet(cur);
                    if (wallet != null && curMap.TryGetValue(cur.Id, out var entry))
                    {
                        wallet.SetAmount(entry.GetAmount());
                    }
                }

                // 5. Calculate Offline Progress
                if (_calculateOfflineProgress && data.timestampUnixUtc > 0)
                {
                    long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    double offlineSeconds = Math.Max(0, now - data.timestampUnixUtc);
                    double maxSeconds = _maxOfflineHours * 3600.0;
                    double effectiveSeconds = Math.Min(offlineSeconds, maxSeconds);

                    if (effectiveSeconds > 5.0) // If offline for more than 5 seconds
                    {
                        ApplyOfflineProgress(effectiveSeconds * _offlineEfficiency);
                    }
                }
            }
        }

        private void ApplyOfflineProgress(double effectiveSeconds)
        {
            var currencyMgr = CurrencyManager.Instance;
            if (currencyMgr == null) return;

            var earnings = new Dictionary<string, BigNumber>();

            for (int i = 0; i < currencyMgr.Generators.Count; i++)
            {
                var gen = currencyMgr.Generators[i];
                if (gen.Level > 0 && gen.TargetCurrency != null)
                {
                    BigNumber rate = gen.GetProductionPerSecond(currencyMgr.StatPipeline);
                    BigNumber earned = rate * effectiveSeconds;

                    if (earned > BigNumber.Zero)
                    {
                        string cId = gen.TargetCurrency.Id;
                        if (!earnings.TryGetValue(cId, out var currentEarned))
                        {
                            currentEarned = BigNumber.Zero;
                        }
                        earnings[cId] = currentEarned + earned;

                        var wallet = currencyMgr.GetWallet(cId);
                        wallet?.Add(earned);
                    }
                }
            }

            OnOfflineProgressGranted?.Invoke(effectiveSeconds, earnings);
            SignalBus.Publish(new OfflineEarningsSignal
            {
                SecondsOffline = effectiveSeconds,
                Earnings = earnings
            });
        }

        public void WipeSave()
        {
            if (_saveToFile)
            {
                string path = GetSaveFilePath();
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            else
            {
                PlayerPrefs.DeleteKey(SaveKeyPrefix + _saveSlot);
                PlayerPrefs.Save();
            }

            // Reset in-memory state
            var currencyMgr = CurrencyManager.Instance;
            if (currencyMgr != null)
            {
                foreach (var cur in currencyMgr.RegisteredCurrencies)
                {
                    var wallet = currencyMgr.GetWallet(cur);
                    wallet?.ResetToStarting();
                }
                for (int i = 0; i < currencyMgr.Generators.Count; i++)
                {
                    currencyMgr.Generators[i].ResetLevel();
                }
            }

            var skillTrees = FindObjectsByType<SkillTreeController>(FindObjectsSortMode.None);
            for (int i = 0; i < skillTrees.Length; i++)
            {
                skillTrees[i].RespecAll(false);
            }

            var prestigeMgr = PrestigeManager.Instance;
            if (prestigeMgr != null)
            {
                prestigeMgr.LoadStats(0, BigNumber.Zero, 0f, -1f, null);
            }
        }

        private string GetSaveFilePath()
        {
            return Path.Combine(Application.persistentDataPath, $"save_{_saveSlot}.json");
        }
    }
}
