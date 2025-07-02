using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(TweenAnimation))]
public class TweenAnimationDrawer : PropertyDrawer
{
    private const float WARNING_BOX_HEIGHT = 40f;
    private const float AXIS_TOGGLE_WIDTH = 30f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty animationNameProp = property.FindPropertyRelative("animationName");
        SerializedProperty enabledProp = property.FindPropertyRelative("enabled");
        SerializedProperty operationTypeProp = property.FindPropertyRelative("operationType");

        Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        string foldoutLabel = string.IsNullOrEmpty(animationNameProp.stringValue) ? "Tween Animation" : animationNameProp.stringValue;
        foldoutLabel += $" ({(AnimationOperationType)operationTypeProp.enumValueIndex})";
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, foldoutLabel, true);

        float yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            DrawProperty(position, property, ref yOffset, "animationName", "Name");
            DrawProperty(position, property, ref yOffset, "animationGroup", "Group Name");
            DrawProperty(position, property, ref yOffset, "enabled", "Enabled");
            DrawProperty(position, property, ref yOffset, "operationType", "Type");
            
            if (enabledProp.boolValue)
            {
                DrawProperty(position, property, ref yOffset, "duration", "Duration");
                DrawProperty(position, property, ref yOffset, "delay", "Delay");
                DrawProperty(position, property, ref yOffset, "easingType", "Easing");
                DrawProperty(position, property, ref yOffset, "mirrorEaseOnRewind", "Mirror Ease On Rewind");
                DrawProperty(position, property, ref yOffset, "unrewindable", "Unrewindable");
                DrawProperty(position, property, ref yOffset, "snapToEndState", "Snap To End State");

                SerializedProperty easingTypeProp = property.FindPropertyRelative("easingType");
                if ((EasingType)easingTypeProp.enumValueIndex == EasingType.Custom)
                {
                    DrawProperty(position, property, ref yOffset, "customEaseCurve", "Custom Ease Curve");
                }

                AnimationOperationType currentType = (AnimationOperationType)operationTypeProp.enumValueIndex;
                switch (currentType)
                {
                    case AnimationOperationType.Move:
                        DrawAxisToggles(position, property, ref yOffset, true, true, false);
                        DrawProperty(position, property, ref yOffset, "targetAnchoredPosition", "Target Position");
                        DrawProperty(position, property, ref yOffset, "targetRectTransformOverride", "Target Override (Rect)");
                        break;
                    case AnimationOperationType.MoveToTarget:
                        DrawAxisToggles(position, property, ref yOffset, true, true, true);
                        DrawProperty(position, property, ref yOffset, "moveToTargetTransform", "Destination Target (Rect)");
                        DrawProperty(position, property, ref yOffset, "chaseTarget", "Chase Target");
                        DrawProperty(position, property, ref yOffset, "targetRectTransformOverride", "Source Override (Rect)");
                        break;
                    case AnimationOperationType.Scale:
                        DrawAxisToggles(position, property, ref yOffset, true, true, true);
                        DrawProperty(position, property, ref yOffset, "targetScale", "Target Scale");
                        DrawProperty(position, property, ref yOffset, "targetRectTransformOverride", "Target Override (Rect)");
                        break;
                    case AnimationOperationType.Rotate:
                        DrawAxisToggles(position, property, ref yOffset, true, true, true);
                        DrawProperty(position, property, ref yOffset, "targetEulerAngles", "Target Rotation (Euler)");
                        DrawProperty(position, property, ref yOffset, "targetRectTransformOverride", "Target Override (Rect)");
                        break;
                    case AnimationOperationType.Fade:
                        DrawProperty(position, property, ref yOffset, "targetAlpha", "Target Alpha");
                        DrawProperty(position, property, ref yOffset, "targetGameObjectOverride", "Target Override (GO)");
                        break;
                    case AnimationOperationType.Custom:
                        DrawProperty(position, property, ref yOffset, "customAnimationType", "Custom Value Type");
                        SerializedProperty customTypeProp = property.FindPropertyRelative("customAnimationType");
                        switch((CustomAnimationType)customTypeProp.enumValueIndex)
                        {
                            case CustomAnimationType.Float:
                                DrawProperty(position, property, ref yOffset, "customStartFloat", "Start Value");
                                DrawProperty(position, property, ref yOffset, "customEndFloat", "End Value");
                                DrawUnityEventWithWarning(position, property, ref yOffset, "onUpdateFloat", "Float");
                                break;
                            case CustomAnimationType.Color:
                                DrawProperty(position, property, ref yOffset, "customStartColor", "Start Value");
                                DrawProperty(position, property, ref yOffset, "customEndColor", "End Value");
                                DrawUnityEventWithWarning(position, property, ref yOffset, "onUpdateColor", "Color");
                                break;
                        }
                        break;
                }
            }
            EditorGUI.indentLevel--;
        }
        EditorGUI.EndProperty();
    }
    
    private void DrawUnityEventWithWarning(Rect position, SerializedProperty parentProperty, ref float yOffset, string propertyName, string dynamicTypeName)
    {
        SerializedProperty unityEventProp = parentProperty.FindPropertyRelative(propertyName);
        DrawProperty(position, parentProperty, ref yOffset, propertyName, $"On Update ({dynamicTypeName})");

        if (IsUnityEventStatic(unityEventProp))
        {
            string warningText = $"Warning: A static function is selected. Please select a function from the 'Dynamic {dynamicTypeName}' section for the tween to work correctly.";
            Rect warningRect = new Rect(position.x, position.y + yOffset, position.width, WARNING_BOX_HEIGHT);
            EditorGUI.HelpBox(warningRect, warningText, MessageType.Warning);
            yOffset += WARNING_BOX_HEIGHT + EditorGUIUtility.standardVerticalSpacing;
        }
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        if (property.isExpanded)
        {
            totalHeight += GetSimplePropHeight(property, "animationName");
            totalHeight += GetSimplePropHeight(property, "animationGroup");
            totalHeight += GetSimplePropHeight(property, "enabled");
            totalHeight += GetSimplePropHeight(property, "operationType");

            if (property.FindPropertyRelative("enabled").boolValue)
            {
                totalHeight += GetSimplePropHeight(property, "duration");
                totalHeight += GetSimplePropHeight(property, "delay");
                totalHeight += GetSimplePropHeight(property, "easingType");
                totalHeight += GetSimplePropHeight(property, "mirrorEaseOnRewind");
                totalHeight += GetSimplePropHeight(property, "unrewindable");
                totalHeight += GetSimplePropHeight(property, "snapToEndState");

                if ((EasingType)property.FindPropertyRelative("easingType").enumValueIndex == EasingType.Custom)
                {
                    totalHeight += GetSimplePropHeight(property, "customEaseCurve");
                }
                
                AnimationOperationType currentType = (AnimationOperationType)property.FindPropertyRelative("operationType").enumValueIndex;
                switch (currentType)
                {
                    case AnimationOperationType.Move:
                    case AnimationOperationType.MoveToTarget:
                    case AnimationOperationType.Scale:
                    case AnimationOperationType.Rotate:
                        totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        break;
                }

                switch (currentType)
                {
                    case AnimationOperationType.Move:
                        totalHeight += GetSimplePropHeight(property, "targetAnchoredPosition");
                        totalHeight += GetSimplePropHeight(property, "targetRectTransformOverride");
                        break;
                    case AnimationOperationType.MoveToTarget:
                        totalHeight += GetSimplePropHeight(property, "moveToTargetTransform");
                        totalHeight += GetSimplePropHeight(property, "chaseTarget");
                        totalHeight += GetSimplePropHeight(property, "targetRectTransformOverride");
                        break;
                    case AnimationOperationType.Scale:
                        totalHeight += GetSimplePropHeight(property, "targetScale");
                        totalHeight += GetSimplePropHeight(property, "targetRectTransformOverride");
                        break;
                    case AnimationOperationType.Rotate:
                        totalHeight += GetSimplePropHeight(property, "targetEulerAngles");
                        totalHeight += GetSimplePropHeight(property, "targetRectTransformOverride");
                        break;
                    case AnimationOperationType.Fade:
                        totalHeight += GetSimplePropHeight(property, "targetAlpha");
                        totalHeight += GetSimplePropHeight(property, "targetGameObjectOverride");
                        break;
                    case AnimationOperationType.Custom:
                        totalHeight += GetSimplePropHeight(property, "customAnimationType");
                        CustomAnimationType customType = (CustomAnimationType)property.FindPropertyRelative("customAnimationType").enumValueIndex;
                        switch(customType)
                        {
                            case CustomAnimationType.Float:
                                totalHeight += GetSimplePropHeight(property, "customStartFloat");
                                totalHeight += GetSimplePropHeight(property, "customEndFloat");
                                totalHeight += GetSimplePropHeight(property, "onUpdateFloat");
                                if (IsUnityEventStatic(property.FindPropertyRelative("onUpdateFloat")))
                                    totalHeight += WARNING_BOX_HEIGHT + EditorGUIUtility.standardVerticalSpacing;
                                break;
                            case CustomAnimationType.Color:
                                totalHeight += GetSimplePropHeight(property, "customStartColor");
                                totalHeight += GetSimplePropHeight(property, "customEndColor");
                                totalHeight += GetSimplePropHeight(property, "onUpdateColor");
                                if (IsUnityEventStatic(property.FindPropertyRelative("onUpdateColor")))
                                    totalHeight += WARNING_BOX_HEIGHT + EditorGUIUtility.standardVerticalSpacing;
                                break;
                        }
                        break;
                }
            }
        }
        return totalHeight;
    }
    
    private void DrawProperty(Rect position, SerializedProperty parentProperty, ref float yOffset, string propertyName, string label)
    {
        SerializedProperty specificProp = parentProperty.FindPropertyRelative(propertyName);
        Rect propRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUI.GetPropertyHeight(specificProp));
        EditorGUI.PropertyField(propRect, specificProp, new GUIContent(label), true);
        yOffset += EditorGUI.GetPropertyHeight(specificProp, true) + EditorGUIUtility.standardVerticalSpacing;
    }
    
    private void DrawAxisToggles(Rect position, SerializedProperty parentProperty, ref float yOffset, bool showX, bool showY, bool showZ)
    {
        Rect controlRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
        
        Rect labelRect = new Rect(controlRect.x, controlRect.y, EditorGUIUtility.labelWidth, controlRect.height);
        EditorGUI.LabelField(labelRect, "Ignore Axis");

        Rect toggleRect = new Rect(controlRect.x + EditorGUIUtility.labelWidth, controlRect.y, controlRect.width - EditorGUIUtility.labelWidth, controlRect.height);
        
        float currentX = toggleRect.x;
        if (showX)
        {
            SerializedProperty propX = parentProperty.FindPropertyRelative("ignoreX");
            propX.boolValue = GUI.Toggle(new Rect(currentX, toggleRect.y, AXIS_TOGGLE_WIDTH, toggleRect.height), propX.boolValue, "X");
            currentX += AXIS_TOGGLE_WIDTH;
        }
        if (showY)
        {
            SerializedProperty propY = parentProperty.FindPropertyRelative("ignoreY");
            propY.boolValue = GUI.Toggle(new Rect(currentX, toggleRect.y, AXIS_TOGGLE_WIDTH, toggleRect.height), propY.boolValue, "Y");
            currentX += AXIS_TOGGLE_WIDTH;
        }
        if (showZ)
        {
            SerializedProperty propZ = parentProperty.FindPropertyRelative("ignoreZ");
            propZ.boolValue = GUI.Toggle(new Rect(currentX, toggleRect.y, AXIS_TOGGLE_WIDTH, toggleRect.height), propZ.boolValue, "Z");
        }

        yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }

    private float GetSimplePropHeight(SerializedProperty parentProperty, string propertyName)
    {
        return EditorGUI.GetPropertyHeight(parentProperty.FindPropertyRelative(propertyName), true) + EditorGUIUtility.standardVerticalSpacing;
    }

    private bool IsUnityEventStatic(SerializedProperty unityEventProp)
    {
        SerializedProperty persistentCalls = unityEventProp.FindPropertyRelative("m_PersistentCalls.m_Calls");
        if (persistentCalls == null || !persistentCalls.isArray) return false;
        for (int i = 0; i < persistentCalls.arraySize; i++)
        {
            SerializedProperty listener = persistentCalls.GetArrayElementAtIndex(i);
            SerializedProperty mode = listener.FindPropertyRelative("m_Mode");
            SerializedProperty target = listener.FindPropertyRelative("m_Target");
            SerializedProperty methodName = listener.FindPropertyRelative("m_MethodName");
            bool isConfigured = target.objectReferenceValue != null && !string.IsNullOrEmpty(methodName.stringValue);
            if (!isConfigured) continue;
            if (mode.enumValueIndex > 1) return true;
        }
        return false;
    }
}