using UnityEngine;

public class UITweenerOnEnable : MonoBehaviour
{
    public UITweenerToggleSettings settings;

    private void OnEnable()
    {
        if (settings == null || settings.tweener == null || settings.actionType == UITweenerToggleSettings.TargetType.None)
        {
            return;
        }

        UITweener tweener = settings.tweener;
        string animName = settings.animationName;
        string groupName = settings.groupName;

        switch (settings.actionType)
        {
            case UITweenerToggleSettings.TargetType.SingleAnimation:
                if (!string.IsNullOrEmpty(animName))
                {
                    tweener.PlayAnimation(animName);
                }
                break;
            case UITweenerToggleSettings.TargetType.AnimationGroup:
                if (!string.IsNullOrEmpty(groupName))
                {
                    tweener.PlayAnimationGroup(groupName);
                }
                break;
        }
    }
}