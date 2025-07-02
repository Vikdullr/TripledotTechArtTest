using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(LockableToggle), true)]
    [CanEditMultipleObjects]
    public class LockableToggleEditor : ToggleEditor
    {
        SerializedProperty m_IsLockedProperty;
        SerializedProperty m_OnLockedPressedProperty;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_IsLockedProperty = serializedObject.FindProperty("isLocked");
            m_OnLockedPressedProperty = serializedObject.FindProperty("OnLockedPressed");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_IsLockedProperty);
            EditorGUILayout.PropertyField(m_OnLockedPressedProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}