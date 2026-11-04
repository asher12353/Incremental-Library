using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.Currencies;

namespace FiniteSkillTreeIncremental.UI
{
    public class GeneratorCardUI : MonoBehaviour
    {
        [SerializeField] private string _generatorId;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _productionText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private int _buyAmount = 1;

        private CurrencyGenerator _generator;

        public CurrencyGenerator Generator => _generator;

        private void Start()
        {
            FindAndBindGenerator();
            if (_buyButton != null)
            {
                _buyButton.onClick.AddListener(HandleBuyClicked);
            }
        }

        public void BindGenerator(CurrencyGenerator generator)
        {
            _generator = generator;
            if (_generator != null)
            {
                _generatorId = _generator.Id;
            }
            UpdateDisplay();
        }

        private void FindAndBindGenerator()
        {
            if (_generator != null) return;

            var currencyMgr = CurrencyManager.Instance;
            if (currencyMgr == null) return;

            for (int i = 0; i < currencyMgr.Generators.Count; i++)
            {
                if (currencyMgr.Generators[i].Id == _generatorId)
                {
                    _generator = currencyMgr.Generators[i];
                    break;
                }
            }
        }

        private void Update()
        {
            if (_generator == null)
            {
                FindAndBindGenerator();
            }
            UpdateDisplay();
        }

        public void SetBuyAmount(int amount)
        {
            _buyAmount = Mathf.Max(1, amount);
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (_generator == null) return;

            var currencyMgr = CurrencyManager.Instance;
            if (currencyMgr == null) return;

            if (_titleText != null)
            {
                _titleText.text = _generator.DisplayName;
            }

            if (_levelText != null)
            {
                _levelText.text = $"Lvl {_generator.Level}";
            }

            if (_productionText != null && _generator.TargetCurrency != null)
            {
                BigNumber prod = _generator.GetProductionPerSecond(currencyMgr.StatPipeline);
                _productionText.text = $"+{_generator.TargetCurrency.Format(prod)}/s";
            }

            BigNumber cost = _generator.GetCostForBulk(_buyAmount);

            if (_costText != null && _generator.CostCurrency != null)
            {
                string countSuffix = _buyAmount > 1 ? $" (x{_buyAmount})" : "";
                _costText.text = $"Buy{countSuffix}: {_generator.CostCurrency.Format(cost)}";
            }

            if (_buyButton != null && _generator.CostCurrency != null)
            {
                var wallet = currencyMgr.GetWallet(_generator.CostCurrency);
                _buyButton.interactable = wallet != null && wallet.CanAfford(cost);
            }
        }

        private void HandleBuyClicked()
        {
            if (_generator == null || _generator.CostCurrency == null) return;

            var currencyMgr = CurrencyManager.Instance;
            if (currencyMgr == null) return;

            var wallet = currencyMgr.GetWallet(_generator.CostCurrency);
            if (wallet != null)
            {
                _generator.TryPurchase(wallet, _buyAmount, currencyMgr.StatPipeline);
                UpdateDisplay();
            }
        }
    }
}
