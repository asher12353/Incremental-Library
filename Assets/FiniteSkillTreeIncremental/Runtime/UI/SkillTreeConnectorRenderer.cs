using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FiniteSkillTreeIncremental.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class SkillTreeConnectorRenderer : Graphic
    {
        [System.Serializable]
        public struct ConnectionLine
        {
            public Vector2 StartPoint;
            public Vector2 EndPoint;
            public Color LineColor;
            public float Thickness;
        }

        [SerializeField] private float _defaultThickness = 4f;
        [SerializeField] private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private Color _availableColor = new Color(0.9f, 0.8f, 0.2f, 0.9f);
        [SerializeField] private Color _activeColor = new Color(0.2f, 0.8f, 1f, 1f);
        [SerializeField] private Color _maxedColor = new Color(0.2f, 0.9f, 0.3f, 1f);

        private readonly List<ConnectionLine> _lines = new List<ConnectionLine>();

        public float DefaultThickness => _defaultThickness;
        public Color LockedColor => _lockedColor;
        public Color AvailableColor => _availableColor;
        public Color ActiveColor => _activeColor;
        public Color MaxedColor => _maxedColor;

        public void ClearLines()
        {
            _lines.Clear();
            SetVerticesDirty();
        }

        public void AddLine(Vector2 start, Vector2 end, Color color, float thickness = -1f)
        {
            _lines.Add(new ConnectionLine
            {
                StartPoint = start,
                EndPoint = end,
                LineColor = color,
                Thickness = thickness > 0f ? thickness : _defaultThickness
            });
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            for (int i = 0; i < _lines.Count; i++)
            {
                var line = _lines[i];
                DrawLineSegment(vh, line.StartPoint, line.EndPoint, line.LineColor, line.Thickness);
            }
        }

        private void DrawLineSegment(VertexHelper vh, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 direction = (end - start).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * (thickness * 0.5f);

            int startIndex = vh.currentVertCount;

            UIVertex v1 = UIVertex.simpleVert;
            v1.color = color;
            v1.position = start - normal;
            vh.AddVert(v1);

            UIVertex v2 = UIVertex.simpleVert;
            v2.color = color;
            v2.position = start + normal;
            vh.AddVert(v2);

            UIVertex v3 = UIVertex.simpleVert;
            v3.color = color;
            v3.position = end + normal;
            vh.AddVert(v3);

            UIVertex v4 = UIVertex.simpleVert;
            v4.color = color;
            v4.position = end - normal;
            vh.AddVert(v4);

            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }
    }
}
