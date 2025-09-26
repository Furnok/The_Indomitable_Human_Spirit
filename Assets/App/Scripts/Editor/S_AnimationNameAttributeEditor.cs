using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System;

[CustomPropertyDrawer(typeof(S_AnimationNameAttribute))]
public class S_AnimationNameAttributeEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "Use [S_AnimatorParameter] on a string.");
            EditorGUI.EndProperty();
            return;
        }

        // Get the target object (MonoBehaviour) where this property is used
        UnityEngine.Object target = property.serializedObject.targetObject;
        Animator animator = null;

        // Try to find Animator in the same GameObject
        if (target is MonoBehaviour mb)
        {
            animator = mb.GetComponent<Animator>();
        }

        if (animator == null)
        {
            EditorGUI.LabelField(position, label.text, "No Animator found on this GameObject.");
            EditorGUI.EndProperty();
            return;
        }

        AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;

        if (controller == null)
        {
            EditorGUI.LabelField(position, label.text, "No AnimatorController assigned.");
            EditorGUI.EndProperty();
            return;
        }

        // Get all parameter names
        string[] parameterNames = Array.ConvertAll(controller.parameters, p => p.name);

        // Find the index of the current value
        int selectedIndex = Mathf.Max(0, Array.IndexOf(parameterNames, property.stringValue));

        // Draw the popup
        selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, parameterNames);

        // Update the selected value
        property.stringValue = parameterNames[selectedIndex];

        EditorGUI.EndProperty();
    }
}