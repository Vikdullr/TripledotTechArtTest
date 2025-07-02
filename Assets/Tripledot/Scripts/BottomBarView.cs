using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class BottomBarView : MonoBehaviour
{
    [SerializeField]
    private List<BottomBarToggleEntry> _toggleEntries;

    private int _activeToggleCount = 0;
    private bool _isSwitchingToggles = false;

    [Header("Events")]
    public UnityEvent<Toggle> OnContentActivated;
    public UnityEvent<Toggle> OnFirstContentActivated;
    public UnityEvent OnClosed;

    private void Start()
    {
        _activeToggleCount = 0;
        if (_toggleEntries == null) _toggleEntries = new List<BottomBarToggleEntry>();

        foreach (var entry in _toggleEntries)
        {
            if (entry.toggle == null) continue;

            if (entry.toggle.isOn)
            {
                _activeToggleCount++;
            }
            entry.toggle.onValueChanged.AddListener((isOn) => OnToggleValueChanged(entry, isOn));
        }
    }

    private void LateUpdate()
    {
        if (_isSwitchingToggles && _activeToggleCount == 0)
        {
            OnClosed?.Invoke();
        }

        _isSwitchingToggles = false;
    }

    private void OnDestroy()
    {
        if (_toggleEntries == null) return;

        foreach (var entry in _toggleEntries)
        {
            if (entry.toggle != null)
            {
                entry.toggle.onValueChanged.RemoveAllListeners();
            }
        }
    }

    private void OnToggleValueChanged(BottomBarToggleEntry changedEntry, bool isOn)
    {
        Toggle changedToggle = changedEntry.toggle;

        if (isOn)
        {
            if (!_isSwitchingToggles && _activeToggleCount == 0)
            {
                OnFirstContentActivated?.Invoke(changedToggle);
            }
            _activeToggleCount++;
            OnContentActivated?.Invoke(changedToggle);

            changedEntry.onActivated?.Invoke();
        }
        else
        {
            _isSwitchingToggles = true;
            _activeToggleCount--;
            if (_activeToggleCount < 0)
            {
                _activeToggleCount = 0;
            }

            changedEntry.onDeactivated?.Invoke();
        }

        if (changedEntry.tweenerSettings != null &&
            changedEntry.tweenerSettings.tweener != null &&
            changedEntry.tweenerSettings.actionType != UITweenerToggleSettings.TargetType.None)
        {
            UITweener tweener = changedEntry.tweenerSettings.tweener;
            string animName = changedEntry.tweenerSettings.animationName;
            string groupName = changedEntry.tweenerSettings.groupName;

            switch (changedEntry.tweenerSettings.actionType)
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

    public void AlignToSelectedToggle(GameObject panelToAlign)
    {
        if (panelToAlign == null) return;

        foreach (var entry in _toggleEntries)
        {
            if (entry.toggle != null && entry.toggle.isOn)
            {
                Vector3 panelPosition = panelToAlign.transform.position;
                panelPosition.x = entry.toggle.transform.position.x;
                panelToAlign.transform.position = panelPosition;
                break;
            }
        }
    }

    public void SetToggleLockState(int index, bool isLocked)
    {
        if (index < 0 || index >= _toggleEntries.Count)
        {
            Debug.LogWarning($"Index {index} is out of bounds for Toggle Entries.");
            return;
        }

        if (_toggleEntries[index].toggle is LockableToggle lockableToggle)
        {
            lockableToggle.isLocked = isLocked;
        }
    }
}