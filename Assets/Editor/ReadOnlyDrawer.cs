using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Disable the GUI to make the field uneditable
        GUI.enabled = false;

        // Draw the property field normally
        EditorGUI.PropertyField(position, property, label, true);

        // Re-enable the GUI for subsequent fields
        GUI.enabled = true;
    }
}
