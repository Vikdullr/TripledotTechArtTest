using UnityEngine;

[System.Serializable]
public class UITweenerToggleSettings
{
    public enum TargetType { None, SingleAnimation, AnimationGroup }
    public TargetType actionType = TargetType.None;
    public UITweener tweener;
    public string animationName;
    public string groupName;
}