using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;

[CustomEditor(typeof(SnapPointSlider), true)]
[CanEditMultipleObjects]
public class SnapPointSliderEditor : SelectableEditor {
    SerializedProperty m_Direction;
    SerializedProperty m_FillRect;
    SerializedProperty m_HandleRect;
    SerializedProperty m_MinValue;
    SerializedProperty m_MaxValue;
    SerializedProperty m_WholeNumbers;
    SerializedProperty m_Value;
    SerializedProperty m_OnValueChanged;
    SerializedProperty m_SnapPointNumber;
    protected override void OnEnable()
    {
      base.OnEnable();
      m_FillRect = serializedObject.FindProperty("m_FillRect");
      m_HandleRect = serializedObject.FindProperty("m_HandleRect");
      m_Direction = serializedObject.FindProperty("m_Direction");
      m_MinValue = serializedObject.FindProperty("m_MinValue");
      m_MaxValue = serializedObject.FindProperty("m_MaxValue");
      m_WholeNumbers = serializedObject.FindProperty("m_WholeNumbers");
      m_Value = serializedObject.FindProperty("m_Value");
      m_OnValueChanged = serializedObject.FindProperty("m_OnValueChanged");
      m_SnapPointNumber = serializedObject.FindProperty("SnapPointNumber");
    }

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();
      EditorGUILayout.Space();

      serializedObject.Update();

      EditorGUILayout.PropertyField(m_FillRect);
      EditorGUILayout.PropertyField(m_HandleRect);

      if (m_FillRect.objectReferenceValue != null || m_HandleRect.objectReferenceValue != null)
      {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_Direction);
        if (EditorGUI.EndChangeCheck())
        {
          Slider.Direction direction = (Slider.Direction)m_Direction.enumValueIndex;
          foreach (var obj in serializedObject.targetObjects)
          {
            SnapPointSlider slider = obj as SnapPointSlider;
            slider.SetDirection(direction, true);
          }
        }

        EditorGUILayout.PropertyField(m_MinValue);
        EditorGUILayout.PropertyField(m_MaxValue);
        EditorGUILayout.PropertyField(m_WholeNumbers);
        EditorGUILayout.Slider(m_Value, m_MinValue.floatValue, m_MaxValue.floatValue);
        EditorGUILayout.PropertyField(m_SnapPointNumber);

        bool warning = false;
        foreach (var obj in serializedObject.targetObjects)
        {
          SnapPointSlider slider = obj as SnapPointSlider;
          SnapPointSlider.Direction dir = slider.direction;
          if (dir == Slider.Direction.LeftToRight || dir == Slider.Direction.RightToLeft)
            warning = (slider.navigation.mode != Navigation.Mode.Automatic && (slider.FindSelectableOnLeft() != null || slider.FindSelectableOnRight() != null));
          else
            warning = (slider.navigation.mode != Navigation.Mode.Automatic && (slider.FindSelectableOnDown() != null || slider.FindSelectableOnUp() != null));
        }

        if (warning)
          EditorGUILayout.HelpBox("The selected slider direction conflicts with navigation. Not all navigation options may work.", MessageType.Warning);

        // Draw the event notification options
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(m_OnValueChanged);
      }
      else
      {
        EditorGUILayout.HelpBox("Specify a RectTransform for the slider fill or the slider handle or both. Each must have a parent RectTransform that it can slide within.", MessageType.Info);
      }

      serializedObject.ApplyModifiedProperties();
    }
}
