using UnityEditor;
using UnityEngine;
using FiniteSkillTreeIncremental.Core;

namespace FiniteSkillTreeIncremental.Editor
{
    [CustomPropertyDrawer(typeof(BigNumber))]
    public class BigNumberDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var mantissaProp = property.FindPropertyRelative("_mantissa");
            var exponentProp = property.FindPropertyRelative("_exponent");

            double mantissa = mantissaProp.doubleValue;
            long exponent = exponentProp.longValue;

            BigNumber num = new BigNumber(mantissa, exponent);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            float width = position.width;
            float fieldWidth = (width - 10f) / 2f;

            Rect strRect = new Rect(position.x, position.y, fieldWidth, position.height);
            Rect parsedRect = new Rect(position.x + fieldWidth + 10f, position.y, fieldWidth, position.height);

            string currentStr = num.ToString(BigNumberNotation.Standard, 2);
            string newStr = EditorGUI.TextField(strRect, currentStr);

            if (newStr != currentStr)
            {
                if (BigNumber.TryParse(newStr, out BigNumber parsed))
                {
                    mantissaProp.doubleValue = parsed.Mantissa;
                    exponentProp.longValue = parsed.Exponent;
                }
            }

            GUI.enabled = false;
            EditorGUI.TextField(parsedRect, $"{mantissa:F3}e{exponent}");
            GUI.enabled = true;

            EditorGUI.EndProperty();
        }
    }
}
