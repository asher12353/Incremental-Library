using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.Core.Stats;
using FiniteSkillTreeIncremental.Currencies;
using FiniteSkillTreeIncremental.Prestige;
using FiniteSkillTreeIncremental.SkillTree;
using FiniteSkillTreeIncremental.UI;

namespace FiniteSkillTreeIncremental.Samples
{
    public class DemoManager : MonoBehaviour
    {
        [Header("Currencies")]
        [SerializeField] private CurrencyDefinition _goldCurrency;
        [SerializeField] private CurrencyDefinition _gemCurrency;
        [SerializeField] private CurrencyDefinition _prestigeShardCurrency;

        [Header("Gameplay Controls")]
        [SerializeField] private Button _manualClickButton;
        [SerializeField] private TMP_Text _clickButtonText;
        [SerializeField] private Button _respecButton;
        [SerializeField] private Button _prestigeModalOpenButton;

        [Header("UI Views")]
        [SerializeField] private CurrencyDisplayUI _goldDisplay;
        [SerializeField] private CurrencyDisplayUI _gemDisplay;
        [SerializeField] private CurrencyDisplayUI _shardsDisplay;
        [SerializeField] private SkillTreeUI _skillTreeUI;
        [SerializeField] private PrestigeModalUI _prestigeModalUI;

        private void Start()
        {
            if (_manualClickButton != null)
            {
                _manualClickButton.onClick.AddListener(HandleManualClick);
            }

            if (_respecButton != null)
            {
                _respecButton.onClick.AddListener(HandleRespec);
            }

            UpdateClickButtonText();
        }

        private void Update()
        {
            UpdateClickButtonText();
        }

        private void UpdateClickButtonText()
        {
            if (_clickButtonText == null || _goldCurrency == null) return;

            var currencyMgr = CurrencyManager.Instance;
            if (currencyMgr == null) return;

            BigNumber baseClick = BigNumber.One;
            BigNumber clickMult = currencyMgr.StatPipeline.GetMultiplier(StatType.ClickPowerMultiplier);
            BigNumber globalMult = currencyMgr.StatPipeline.GetMultiplier(StatType.GlobalProductionMultiplier);
            BigNumber currentClickPower = baseClick * clickMult * globalMult;

            _clickButtonText.text = $"Mine Gold (+{_goldCurrency.Format(currentClickPower)})";
        }

        private void HandleManualClick()
        {
            var currencyMgr = CurrencyManager.Instance;
            if (currencyMgr != null && _goldCurrency != null)
            {
                currencyMgr.ManualClickEarn(_goldCurrency, BigNumber.One);
            }
        }

        private void HandleRespec()
        {
            var skillTree = FindFirstObjectByType<SkillTreeController>();
            if (skillTree != null)
            {
                skillTree.RespecAll(true);
            }
        }
    }
}
