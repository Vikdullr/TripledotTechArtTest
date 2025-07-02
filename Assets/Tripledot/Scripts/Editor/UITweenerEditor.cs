using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UITweener))]
public class UITweenerEditor : Editor
{
    SerializedProperty _scriptProp;
    SerializedProperty tweenAnimationsProp;

    void OnEnable()
    {
        _scriptProp = serializedObject.FindProperty("m_Script");
        tweenAnimationsProp = serializedObject.FindProperty("tweenAnimations");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUI.enabled = false;
        EditorGUILayout.PropertyField(_scriptProp);
        GUI.enabled = true;

        EditorGUILayout.PropertyField(tweenAnimationsProp, true);

        serializedObject.ApplyModifiedProperties();
    }
}