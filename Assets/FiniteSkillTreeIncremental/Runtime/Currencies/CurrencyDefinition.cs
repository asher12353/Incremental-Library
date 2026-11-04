using UnityEngine;
using FiniteSkillTreeIncremental.Core;

namespace FiniteSkillTreeIncremental.Currencies
{
    [CreateAssetMenu(fileName = "NewCurrency", menuName = "Incremental/Currency Definition", order = 10)]
    public class CurrencyDefinition : ScriptableObject
    {
        [SerializeField] private string _id = "gold";
        [SerializeField] private string _displayName = "Gold";
        [TextArea(2, 4)]
        [SerializeField] private string _description = "Primary economic currency.";
        [SerializeField] private Sprite _icon;
        [SerializeField] private BigNumberNotation _defaultNotation = BigNumberNotation.Standard;
        [SerializeField] private int _displayDecimals = 2;
        [SerializeField] private bool _hasCap = false;
        [SerializeField] private BigNumber _maxCap = BigNumber.MaxValue;
        [SerializeField] private BigNumber _startingAmount = BigNumber.Zero;
        [SerializeField] private bool _isPrestigeCurrency = false;

        public string Id => _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public BigNumberNotation DefaultNotation => _defaultNotation;
        public int DisplayDecimals => _displayDecimals;
        public bool HasCap => _hasCap;
        public BigNumber MaxCap => _maxCap;
        public BigNumber StartingAmount => _startingAmount;
        public bool IsPrestigeCurrency => _isPrestigeCurrency;

        public string Format(BigNumber amount)
        {
            return amount.ToString(_defaultNotation, _displayDecimals);
        }

        public string Format(BigNumber amount, BigNumberNotation notation, int decimals = -1)
        {
            return amount.ToString(notation, decimals < 0 ? _displayDecimals : decimals);
        }
    }
}
