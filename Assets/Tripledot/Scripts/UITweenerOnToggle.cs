using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class UITweenerOnToggle : MonoBehaviour
{
    public UITweenerToggleSettings settings;

    private Toggle _toggle;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(HandleToggleValueChanged);
    }

    private void OnDestroy()
    {
        if (_toggle != null)
        {
            _toggle.onValueChanged.RemoveListener(HandleToggleValueChanged);
        }
    }

    private void HandleToggleValueChanged(bool isOn)
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
                    tweener.ToggleAnimation(animName, isOn);
                }
                break;
            case UITweenerToggleSettings.TargetType.AnimationGroup:
                if (!string.IsNullOrEmpty(groupName))
                {
                    tweener.ToggleAnimationGroup(groupName, isOn);
                }
                break;
        }
    }
}