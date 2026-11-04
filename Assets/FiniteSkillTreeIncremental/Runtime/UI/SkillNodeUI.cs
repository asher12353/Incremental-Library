using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.SkillTree;

namespace FiniteSkillTreeIncremental.UI
{
    public class SkillNodeUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField] private Image _frameImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _glowOutline;
        [SerializeField] private TMP_Text _rankText;
        [SerializeField] private TMP_Text _costText;

        [Header("Archetype Frame Sprites/Colors")]
        [SerializeField] private Color _minorColor = new Color(0.3f, 0.5f, 0.8f);
        [SerializeField] private Color _notableColor = new Color(0.9f, 0.75f, 0.2f);
        [SerializeField] private Color _keystoneColor = new Color(0.95f, 0.35f, 0.15f);
        [SerializeField] private Color _gatewayColor = new Color(0.75f, 0.3f, 0.9f);

        [Header("State Colors")]
        [SerializeField] private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
        [SerializeField] private Color _availableColor = new Color(1f, 1f, 0.4f, 1f);
        [SerializeField] private Color _allocatedColor = new Color(0.2f, 0.8f, 1f, 1f);
        [SerializeField] private Color _maxedColor = new Color(0.2f, 0.9f, 0.3f, 1f);

        private SkillNodeDefinition _nodeDefinition;
        private SkillTreeController _controller;
        private RectTransform _rectTransform;

        public SkillNodeDefinition NodeDefinition => _nodeDefinition;
        public RectTransform RectTransform => _rectTransform ?? (_rectTransform = GetComponent<RectTransform>());

        public void Bind(SkillNodeDefinition definition, SkillTreeController controller)
        {
            _nodeDefinition = definition;
            _controller = controller;

            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            if (_nodeDefinition != null)
            {
                _rectTransform.anchoredPosition = _nodeDefinition.GraphPosition;

                if (_iconImage != null && _nodeDefinition.Icon != null)
                {
                    _iconImage.sprite = _nodeDefinition.Icon;
                    _iconImage.enabled = true;
                }

                ApplyArchetypeStyling();
            }

            UpdateDisplay();
        }

        private void ApplyArchetypeStyling()
        {
            if (_nodeDefinition == null || _frameImage == null) return;

            Vector2 baseSize = Vector2.one * 60f;
            switch (_nodeDefinition.NodeType)
            {
                case SkillNodeType.Minor:
                    baseSize = new Vector2(50f, 50f);
                    _frameImage.color = _minorColor;
                    break;
                case SkillNodeType.Notable:
                    baseSize = new Vector2(70f, 70f);
                    _frameImage.color = _notableColor;
                    break;
                case SkillNodeType.Keystone:
                    baseSize = new Vector2(85f, 85f);
                    _frameImage.color = _keystoneColor;
                    break;
                case SkillNodeType.Gateway:
                    baseSize = new Vector2(90f, 90f);
                    _frameImage.color = _gatewayColor;
                    break;
            }

            RectTransform.sizeDelta = baseSize;
        }

        private void Update()
        {
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (_nodeDefinition == null || _controller == null) return;

            var state = _controller.GetNodeState(_nodeDefinition);
            bool isPrereqMet = _controller.IsPrerequisiteMet(_nodeDefinition);
            bool canUpgrade = _controller.CanUpgrade(_nodeDefinition);
            int currentRank = state != null ? state.CurrentRank : 0;
            bool isMaxed = state != null && state.IsMaxRank;
            bool isAllocated = currentRank > 0;

            // Update rank text
            if (_rankText != null)
            {
                _rankText.text = isMaxed ? "MAX" : $"{currentRank}/{_nodeDefinition.MaxRank}";
            }

            // Update cost text
            if (_costText != null)
            {
                if (isMaxed)
                {
                    _costText.text = "";
                }
                else
                {
                    BigNumber cost = state != null ? state.GetCurrentUpgradeCost() : _nodeDefinition.BaseCost;
                    _costText.text = _nodeDefinition.CostCurrency != null ? _nodeDefinition.CostCurrency.Format(cost) : cost.ToString();
                }
            }

            // Update glow and frame tints
            if (_glowOutline != null)
            {
                if (!isPrereqMet)
                {
                    _glowOutline.color = _lockedColor;
                    _glowOutline.enabled = false;
                }
                else if (isMaxed)
                {
                    _glowOutline.color = _maxedColor;
                    _glowOutline.enabled = true;
                }
                else if (canUpgrade)
                {
                    _glowOutline.color = _availableColor;
                    _glowOutline.enabled = true;
                }
                else if (isAllocated)
                {
                    _glowOutline.color = _allocatedColor;
                    _glowOutline.enabled = true;
                }
                else
                {
                    _glowOutline.enabled = false;
                }
            }

            if (_iconImage != null)
            {
                _iconImage.color = isPrereqMet ? Color.white : new Color(0.4f, 0.4f, 0.4f, 0.5f);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_nodeDefinition == null || _controller == null) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _controller.TryUpgradeNode(_nodeDefinition);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                _controller.TryRefundNode(_nodeDefinition);
            }

            UpdateDisplay();
            if (SkillTooltipUI.Instance != null)
            {
                SkillTooltipUI.Instance.Show(_nodeDefinition, _controller.GetNodeState(_nodeDefinition), _controller);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_nodeDefinition != null && _controller != null && SkillTooltipUI.Instance != null)
            {
                SkillTooltipUI.Instance.Show(_nodeDefinition, _controller.GetNodeState(_nodeDefinition), _controller);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (SkillTooltipUI.Instance != null)
            {
                SkillTooltipUI.Instance.Hide();
            }
        }
    }
}
