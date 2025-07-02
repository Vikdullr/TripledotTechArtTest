using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditorInternal;

[CustomEditor(typeof(BottomBarView))]
public class BottomBarViewEditor : Editor
{
    private SerializedProperty _scriptProp;
    private SerializedProperty _toggleEntriesProp;
    private SerializedProperty _onContentActivatedProp;
    private SerializedProperty _onFirstContentActivatedProp;
    private SerializedProperty _onClosedProp;

    private ReorderableList _reorderableList;

    private void OnEnable()
    {
        _scriptProp = serializedObject.FindProperty("m_Script");
        _toggleEntriesProp = serializedObject.FindProperty("_toggleEntries");
        _onContentActivatedProp = serializedObject.FindProperty("OnContentActivated");
        _onFirstContentActivatedProp = serializedObject.FindProperty("OnFirstContentActivated");
        _onClosedProp = serializedObject.FindProperty("OnClosed");

        _reorderableList = new ReorderableList(serializedObject, _toggleEntriesProp, true, true, true, true);

        _reorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Toggle Entries");
        };

        _reorderableList.elementHeightCallback = (int index) =>
        {
            float height = EditorGUIUtility.standardVerticalSpacing * 2;
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            SerializedProperty element = _toggleEntriesProp.GetArrayElementAtIndex(index);
            
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (element.FindPropertyRelative("configureUnityEvents").boolValue)
            {
                height += EditorGUI.GetPropertyHeight(element.FindPropertyRelative("onActivated")) + EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUI.GetPropertyHeight(element.FindPropertyRelative("onDeactivated")) + EditorGUIUtility.standardVerticalSpacing;
            }

            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (element.FindPropertyRelative("configureTweener").boolValue)
            {
                SerializedProperty tweenerSettingsProp = element.FindPropertyRelative("tweenerSettings");
                height += EditorGUI.GetPropertyHeight(tweenerSettingsProp.FindPropertyRelative("actionType")) + EditorGUIUtility.standardVerticalSpacing;
                
                var actionType = (UITweenerToggleSettings.TargetType)tweenerSettingsProp.FindPropertyRelative("actionType").enumValueIndex;
                if (actionType != UITweenerToggleSettings.TargetType.None)
                {
                    height += EditorGUI.GetPropertyHeight(tweenerSettingsProp.FindPropertyRelative("tweener")) + EditorGUIUtility.standardVerticalSpacing;
                    if (actionType == UITweenerToggleSettings.TargetType.SingleAnimation)
                    {
                        height += EditorGUI.GetPropertyHeight(tweenerSettingsProp.FindPropertyRelative("animationName")) + EditorGUIUtility.standardVerticalSpacing;
                    }
                    else if (actionType == UITweenerToggleSettings.TargetType.AnimationGroup)
                    {
                        height += EditorGUI.GetPropertyHeight(tweenerSettingsProp.FindPropertyRelative("groupName")) + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
            }
            return height;
        };

        _reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = _reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += EditorGUIUtility.standardVerticalSpacing;
            float yPos = rect.y;

            float boxHeight = _reorderableList.elementHeightCallback(index) - EditorGUIUtility.standardVerticalSpacing * 2;
            EditorGUI.HelpBox(new Rect(rect.x, yPos, rect.width, boxHeight), "", MessageType.None);

            Rect lineRect = new Rect(rect.x + 5, yPos, rect.width - 10, EditorGUIUtility.singleLineHeight);
            yPos += lineRect.height + EditorGUIUtility.standardVerticalSpacing;

            LockableToggle lockableToggle = null;
            bool currentLockState = false;
            SerializedProperty toggleFieldProp = element.FindPropertyRelative("toggle");
            if (toggleFieldProp.objectReferenceValue != null)
            {
                lockableToggle = toggleFieldProp.objectReferenceValue as LockableToggle;
                if (lockableToggle != null)
                {
                    currentLockState = lockableToggle.isLocked;
                }
            }
            
            GUIContent lockIconContent = currentLockState ? EditorGUIUtility.IconContent("IN LockButton on") : EditorGUIUtility.IconContent("IN LockButton");
            lockIconContent.tooltip = currentLockState ? "Unlock Entry" : "Lock Entry";

            Rect lockButtonRect = new Rect(lineRect.x, lineRect.y, 19, 19);
            if (GUI.Button(lockButtonRect, lockIconContent, EditorStyles.iconButton))
            {
                if (lockableToggle != null)
                {
                    Undo.RecordObject(lockableToggle, "Toggle Lock State");
                    lockableToggle.isLocked = !lockableToggle.isLocked;
                    EditorUtility.SetDirty(lockableToggle);
                }
            }
            
            Rect toggleFieldRect = new Rect(lockButtonRect.xMax + 5, lineRect.y, lineRect.width - lockButtonRect.width - 5, lineRect.height);
            EditorGUI.PropertyField(toggleFieldRect, toggleFieldProp, GUIContent.none);

            int originalIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            Rect foldoutRect = new Rect(rect.x, yPos, rect.width, EditorGUIUtility.singleLineHeight);
            
            SerializedProperty configureUnityEventsProp = element.FindPropertyRelative("configureUnityEvents");
            configureUnityEventsProp.boolValue = EditorGUI.Foldout(foldoutRect, configureUnityEventsProp.boolValue, "Configure Unity Events", true);
            yPos += foldoutRect.height + EditorGUIUtility.standardVerticalSpacing;

            if (configureUnityEventsProp.boolValue)
            {
                EditorGUI.indentLevel++;
                DrawProperty(element.FindPropertyRelative("onActivated"), rect, ref yPos);
                DrawProperty(element.FindPropertyRelative("onDeactivated"), rect, ref yPos);
                EditorGUI.indentLevel--;
            }

            foldoutRect.y = yPos;
            SerializedProperty configureTweenerProp = element.FindPropertyRelative("configureTweener");
            configureTweenerProp.boolValue = EditorGUI.Foldout(foldoutRect, configureTweenerProp.boolValue, "Configure Toggle Tweener", true);
            yPos += foldoutRect.height + EditorGUIUtility.standardVerticalSpacing;

            if (configureTweenerProp.boolValue)
            {
                EditorGUI.indentLevel++;
                SerializedProperty tweenerSettingsProp = element.FindPropertyRelative("tweenerSettings");
                SerializedProperty actionTypeProp = tweenerSettingsProp.FindPropertyRelative("actionType");
                DrawProperty(actionTypeProp, rect, ref yPos);

                UITweenerToggleSettings.TargetType actionType = (UITweenerToggleSettings.TargetType)actionTypeProp.enumValueIndex;
                if (actionType != UITweenerToggleSettings.TargetType.None)
                {
                    DrawProperty(tweenerSettingsProp.FindPropertyRelative("tweener"), rect, ref yPos);
                    if (actionType == UITweenerToggleSettings.TargetType.SingleAnimation)
                    {
                        DrawProperty(tweenerSettingsProp.FindPropertyRelative("animationName"), rect, ref yPos);
                    }
                    else if (actionType == UITweenerToggleSettings.TargetType.AnimationGroup)
                    {
                        DrawProperty(tweenerSettingsProp.FindPropertyRelative("groupName"), rect, ref yPos);
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel = originalIndent;
        };
    }

    private void DrawProperty(SerializedProperty property, Rect parentRect, ref float yPos)
    {
        float height = EditorGUI.GetPropertyHeight(property);
        Rect rect = new Rect(parentRect.x, yPos, parentRect.width, height);
        EditorGUI.PropertyField(rect, property, true);
        yPos += height + EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUI.enabled = false;
        EditorGUILayout.PropertyField(_scriptProp);
        GUI.enabled = true;

        _reorderableList.DoLayoutList();
        
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_onContentActivatedProp);
        EditorGUILayout.PropertyField(_onFirstContentActivatedProp);
        EditorGUILayout.PropertyField(_onClosedProp);

        serializedObject.ApplyModifiedProperties();
    }
}