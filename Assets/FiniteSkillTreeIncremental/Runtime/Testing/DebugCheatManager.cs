using UnityEngine;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.Currencies;
using FiniteSkillTreeIncremental.Prestige;
using FiniteSkillTreeIncremental.Save;
using FiniteSkillTreeIncremental.SkillTree;

namespace FiniteSkillTreeIncremental.Testing
{
    public class DebugCheatManager : MonoBehaviour
    {
        public static DebugCheatManager Instance { get; private set; }

        [SerializeField] private float _currentTimeScale = 1.0f;

        public float CurrentTimeScale => _currentTimeScale;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void SetTimeScale(float scale)
        {
            _currentTimeScale = Mathf.Clamp(scale, 0.1f, 100.0f);
            Time.timeScale = _currentTimeScale;
        }

        public void FastForwardSeconds(float seconds)
        {
            var currencyMgr = CurrencyManager.Instance;
            if (currencyMgr != null)
            {
                currencyMgr.TickGenerators(seconds);
            }
        }

        public void GrantCurrency(string currencyId, BigNumber amount)
        {
            var currencyMgr = CurrencyManager.Instance;
            if (currencyMgr == null) return;

            var wallet = currencyMgr.GetWallet(currencyId);
            wallet?.Add(amount);
        }

        public void GrantAllCurrencies(BigNumber amount)
        {
            var currencyMgr = CurrencyManager.Instance;
            if (currencyMgr == null) return;

            for (int i = 0; i < currencyMgr.RegisteredCurrencies.Count; i++)
            {
                var cur = currencyMgr.RegisteredCurrencies[i];
                if (cur != null)
                {
                    var wallet = currencyMgr.GetWallet(cur);
                    wallet?.Add(amount);
                }
            }
        }

        public void UnlockAllSkills()
        {
            var trees = FindObjectsByType<SkillTreeController>(FindObjectsSortMode.None);
            for (int i = 0; i < trees.Length; i++)
            {
                trees[i].UnlockAllNodes();
            }
        }

        public void RespecAllSkills()
        {
            var trees = FindObjectsByType<SkillTreeController>(FindObjectsSortMode.None);
            for (int i = 0; i < trees.Length; i++)
            {
                trees[i].RespecAll(true);
            }
        }

        public void ForcePrestige()
        {
            var prestigeMgr = PrestigeManager.Instance;
            if (prestigeMgr != null && prestigeMgr.Layers.Count > 0)
            {
                var layer = prestigeMgr.Layers[0];
                // Temporarily give enough source currency if needed
                var currencyMgr = CurrencyManager.Instance;
                if (currencyMgr != null && layer.SourceCurrency != null)
                {
                    var wallet = currencyMgr.GetWallet(layer.SourceCurrency);
                    if (wallet != null && wallet.Amount < layer.BaseRequirement)
                    {
                        wallet.SetAmount(layer.BaseRequirement * 2);
                    }
                }
                prestigeMgr.ExecutePrestige(layer);
            }
        }

        public void WipeSave()
        {
            var saveMgr = SaveManager.Instance;
            saveMgr?.WipeSave();
        }

        public void SaveNow()
        {
            var saveMgr = SaveManager.Instance;
            saveMgr?.SaveGame();
        }

        public void LoadNow()
        {
            var saveMgr = SaveManager.Instance;
            saveMgr?.LoadGame();
        }
    }
}
