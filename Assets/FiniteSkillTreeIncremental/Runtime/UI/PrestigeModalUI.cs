using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.Currencies;
using FiniteSkillTreeIncremental.Prestige;

namespace FiniteSkillTreeIncremental.UI
{
    public class PrestigeModalUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private PrestigeLayerDefinition _prestigeLayer;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _requirementText;
        [SerializeField] private TMP_Text _pendingRewardText;
        [SerializeField] private TMP_Text _totalPrestigesText;
        [SerializeField] private Button _prestigeButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _openButton;

        private void Start()
        {
            if (_prestigeButton != null)
            {
                _prestigeButton.onClick.AddListener(HandlePrestigeClicked);
            }
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Hide);
            }
            if (_openButton != null)
            {
                _openButton.onClick.AddListener(Show);
            }

            UpdateDisplay();
        }

        private void Update()
        {
            if (_panel != null && _panel.activeSelf)
            {
                UpdateDisplay();
            }
        }

        public void SetPrestigeLayer(PrestigeLayerDefinition layer)
        {
            _prestigeLayer = layer;
            UpdateDisplay();
        }

        public void Show()
        {
            if (_panel != null)
            {
                _panel.SetActive(true);
                UpdateDisplay();
            }
        }

        public void Hide()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }

        public void UpdateDisplay()
        {
            var prestigeMgr = PrestigeManager.Instance;
            var currencyMgr = CurrencyManager.Instance;

            if (_prestigeLayer == null && prestigeMgr != null && prestigeMgr.Layers.Count > 0)
            {
                _prestigeLayer = prestigeMgr.Layers[0];
            }

            if (_prestigeLayer == null) return;

            if (_titleText != null)
            {
                _titleText.text = _prestigeLayer.DisplayName;
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = _prestigeLayer.Description;
            }

            if (_requirementText != null && _prestigeLayer.SourceCurrency != null)
            {
                string req = _prestigeLayer.SourceCurrency.Format(_prestigeLayer.BaseRequirement);
                _requirementText.text = $"Req: {req} {_prestigeLayer.SourceCurrency.DisplayName}";
            }

            BigNumber pending = prestigeMgr != null ? prestigeMgr.GetPendingReward(_prestigeLayer) : BigNumber.Zero;

            if (_pendingRewardText != null && _prestigeLayer.RewardCurrency != null)
            {
                string rewardStr = _prestigeLayer.RewardCurrency.Format(pending);
                _pendingRewardText.text = $"+{rewardStr} {_prestigeLayer.RewardCurrency.DisplayName}";
            }

            if (_totalPrestigesText != null && prestigeMgr != null)
            {
                _totalPrestigesText.text = $"Total Prestiges: {prestigeMgr.TotalPrestiges}";
            }

            if (_prestigeButton != null)
            {
                _prestigeButton.interactable = pending > BigNumber.Zero;
            }
        }

        private void HandlePrestigeClicked()
        {
            var prestigeMgr = PrestigeManager.Instance;
            if (prestigeMgr != null && _prestigeLayer != null)
            {
                if (prestigeMgr.ExecutePrestige(_prestigeLayer))
                {
                    UpdateDisplay();
                }
            }
        }
    }
}
