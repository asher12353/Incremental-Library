using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.Currencies;

namespace FiniteSkillTreeIncremental.UI
{
    public class CurrencyDisplayUI : MonoBehaviour
    {
        [SerializeField] private CurrencyDefinition _currency;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private TMP_Text _rateText;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private BigNumberNotation _overrideNotation = BigNumberNotation.Standard;
        [SerializeField] private bool _useCurrencyDefaultNotation = true;

        public CurrencyDefinition Currency => _currency;

        private void Start()
        {
            if (_currency != null)
            {
                if (_iconImage != null && _currency.Icon != null)
                {
                    _iconImage.sprite = _currency.Icon;
                    _iconImage.enabled = true;
                }
                if (_nameText != null)
                {
                    _nameText.text = _currency.DisplayName;
                }
            }
            UpdateDisplay();
        }

        private void Update()
        {
            UpdateDisplay();
        }

        public void SetCurrency(CurrencyDefinition def)
        {
            _currency = def;
            if (_iconImage != null)
            {
                _iconImage.sprite = def != null ? def.Icon : null;
                _iconImage.enabled = def != null && def.Icon != null;
            }
            if (_nameText != null && def != null)
            {
                _nameText.text = def.DisplayName;
            }
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (_currency == null) return;

            var currencyMgr = CurrencyManager.Instance;
            if (currencyMgr == null) return;

            var wallet = currencyMgr.GetWallet(_currency);
            if (wallet == null) return;

            var notation = _useCurrencyDefaultNotation ? _currency.DefaultNotation : _overrideNotation;
            int decimals = _currency.DisplayDecimals;

            if (_amountText != null)
            {
                _amountText.text = wallet.Amount.ToString(notation, decimals);
            }

            if (_rateText != null)
            {
                if (wallet.RatePerSecond > BigNumber.Zero)
                {
                    _rateText.text = $"+{wallet.RatePerSecond.ToString(notation, decimals)}/s";
                }
                else
                {
                    _rateText.text = string.Empty;
                }
            }
        }
    }
}
