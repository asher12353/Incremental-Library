using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FiniteSkillTreeIncremental.SkillTree;

namespace FiniteSkillTreeIncremental.UI
{
    public class SkillTreeUI : MonoBehaviour, IDragHandler, IScrollHandler
    {
        [Header("References")]
        [SerializeField] private SkillTreeController _controller;
        [SerializeField] private RectTransform _contentContainer;
        [SerializeField] private SkillNodeUI _nodePrefab;
        [SerializeField] private SkillTreeConnectorRenderer _connectorRenderer;

        [Header("Pan & Zoom Settings")]
        [SerializeField] private float _minZoom = 0.5f;
        [SerializeField] private float _maxZoom = 2.0f;
        [SerializeField] private float _zoomSpeed = 0.1f;
        [SerializeField] private Vector2 _panLimits = new Vector2(2000f, 2000f);

        private readonly List<SkillNodeUI> _spawnedNodes = new List<SkillNodeUI>();
        private float _currentZoom = 1.0f;

        public SkillTreeController Controller => _controller;

        private void Start()
        {
            if (_controller == null)
            {
                _controller = FindFirstObjectByType<SkillTreeController>();
            }

            RebuildTree();
        }

        public void BindController(SkillTreeController controller)
        {
            _controller = controller;
            RebuildTree();
        }

        public void RebuildTree()
        {
            // Clear old nodes
            for (int i = 0; i < _spawnedNodes.Count; i++)
            {
                if (_spawnedNodes[i] != null)
                {
                    Destroy(_spawnedNodes[i].gameObject);
                }
            }
            _spawnedNodes.Clear();

            if (_controller == null || _controller.TreeDefinition == null || _contentContainer == null)
            {
                return;
            }

            var tree = _controller.TreeDefinition;

            // Instantiate Node UIs
            if (_nodePrefab != null)
            {
                for (int i = 0; i < tree.Nodes.Count; i++)
                {
                    var nodeDef = tree.Nodes[i];
                    if (nodeDef == null) continue;

                    var nodeUI = Instantiate(_nodePrefab, _contentContainer);
                    nodeUI.Bind(nodeDef, _controller);
                    _spawnedNodes.Add(nodeUI);
                }
            }

            RedrawConnectors();
        }

        public void RedrawConnectors()
        {
            if (_connectorRenderer == null || _controller == null || _controller.TreeDefinition == null)
            {
                return;
            }

            _connectorRenderer.ClearLines();
            var tree = _controller.TreeDefinition;

            var nodeMap = new Dictionary<string, SkillNodeUI>();
            for (int i = 0; i < _spawnedNodes.Count; i++)
            {
                if (_spawnedNodes[i] != null && _spawnedNodes[i].NodeDefinition != null)
                {
                    nodeMap[_spawnedNodes[i].NodeDefinition.Id] = _spawnedNodes[i];
                }
            }

            for (int i = 0; i < tree.Nodes.Count; i++)
            {
                var targetNode = tree.Nodes[i];
                if (targetNode == null) continue;

                var targetState = _controller.GetNodeState(targetNode);
                bool isTargetMaxed = targetState != null && targetState.IsMaxRank;
                bool isTargetActive = targetState != null && targetState.CurrentRank > 0;

                for (int p = 0; p < targetNode.Prerequisites.Count; p++)
                {
                    var prereq = targetNode.Prerequisites[p];
                    if (prereq.RequiredNode == null) continue;

                    var parentDef = prereq.RequiredNode;
                    var parentState = _controller.GetNodeState(parentDef);
                    bool isParentActive = parentState != null && parentState.CurrentRank >= prereq.RequiredRank;

                    Color lineColor = _connectorRenderer.LockedColor;
                    if (isTargetMaxed)
                    {
                        lineColor = _connectorRenderer.MaxedColor;
                    }
                    else if (isTargetActive)
                    {
                        lineColor = _connectorRenderer.ActiveColor;
                    }
                    else if (isParentActive)
                    {
                        lineColor = _connectorRenderer.AvailableColor;
                    }

                    Vector2 startPos = parentDef.GraphPosition;
                    Vector2 endPos = targetNode.GraphPosition;

                    _connectorRenderer.AddLine(startPos, endPos, lineColor, _connectorRenderer.DefaultThickness);
                }
            }
        }

        private void Update()
        {
            // Regularly update line connectors when node states change
            RedrawConnectors();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_contentContainer == null) return;

            Vector2 newPos = _contentContainer.anchoredPosition + eventData.delta;
            newPos.x = Mathf.Clamp(newPos.x, -_panLimits.x, _panLimits.x);
            newPos.y = Mathf.Clamp(newPos.y, -_panLimits.y, _panLimits.y);
            _contentContainer.anchoredPosition = newPos;
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (_contentContainer == null) return;

            _currentZoom += eventData.scrollDelta.y * _zoomSpeed;
            _currentZoom = Mathf.Clamp(_currentZoom, _minZoom, _maxZoom);
            _contentContainer.localScale = Vector3.one * _currentZoom;
        }

        public void ResetView()
        {
            if (_contentContainer != null)
            {
                _contentContainer.anchoredPosition = Vector2.zero;
                _currentZoom = 1.0f;
                _contentContainer.localScale = Vector3.one;
            }
        }
    }
}
