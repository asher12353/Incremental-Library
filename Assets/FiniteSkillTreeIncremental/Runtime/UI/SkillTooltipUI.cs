using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.SkillTree;

namespace FiniteSkillTreeIncremental.UI
{
    public class SkillTooltipUI : MonoBehaviour
    {
        public static SkillTooltipUI Instance { get; private set; }

        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _typeText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _rankText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private TMP_Text _synergyText;
        [SerializeField] private Vector2 _offset = new Vector2(15f, -15f);

        private RectTransform _rectTransform;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            _rectTransform = GetComponent<RectTransform>();
            Hide();
        }

        private void Update()
        {
            if (_panel != null && _panel.activeSelf)
            {
                FollowMouse();
            }
        }

        private void FollowMouse()
        {
            if (UnityEngine.InputSystem.Mouse.current != null)
            {
                Vector2 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
                transform.position = mousePos + _offset;
            }
        }

        public void Show(SkillNodeDefinition node, SkillNodeState state, SkillTreeController controller)
        {
            if (node == null || _panel == null) return;

            if (_titleText != null)
            {
                _titleText.text = node.Title;
            }

            if (_typeText != null)
            {
                _typeText.text = $"[{node.NodeType.ToString().ToUpper()}]";
                switch (node.NodeType)
                {
                    case SkillNodeType.Minor:
                        _typeText.color = new Color(0.7f, 0.8f, 1f);
                        break;
                    case SkillNodeType.Notable:
                        _typeText.color = new Color(1f, 0.85f, 0.3f);
                        break;
                    case SkillNodeType.Keystone:
                        _typeText.color = new Color(1f, 0.4f, 0.2f);
                        break;
                    case SkillNodeType.Gateway:
                        _typeText.color = new Color(0.8f, 0.4f, 1f);
                        break;
                }
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = node.Description;
            }

            int currentRank = state != null ? state.CurrentRank : 0;
            if (_rankText != null)
            {
                _rankText.text = $"Rank: {currentRank} / {node.MaxRank}";
            }

            if (_costText != null)
            {
                if (state != null && state.IsMaxRank)
                {
                    _costText.text = "<color=#4CAF50>MAX LEVEL</color>";
                }
                else
                {
                    BigNumber cost = node.GetCostForRank(currentRank + 1);
                    string curName = node.CostCurrency != null ? node.CostCurrency.DisplayName : "Points";
                    string formatted = node.CostCurrency != null ? node.CostCurrency.Format(cost) : cost.ToString();
                    _costText.text = $"Cost: {formatted} {curName}";
                }
            }

            if (_synergyText != null)
            {
                if (node.SynergyTags != null && node.SynergyTags.Count > 0)
                {
                    string tags = string.Join(", ", node.SynergyTags);
                    _synergyText.text = $"Tags: {tags} (+{(node.AdjacentSynergyBonusPerActiveNeighbor * 100):0}% synergy/neighbor)";
                    _synergyText.gameObject.SetActive(true);
                }
                else
                {
                    _synergyText.gameObject.SetActive(false);
                }
            }

            _panel.SetActive(true);
            FollowMouse();
        }

        public void Hide()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }
    }
}
