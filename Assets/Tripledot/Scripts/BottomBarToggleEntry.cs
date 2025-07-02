using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class BottomBarToggleEntry
{
    public LockableToggle toggle;
    public bool configureUnityEvents;
    public UnityEvent onActivated;
    public UnityEvent onDeactivated;
    public bool configureTweener;
    public UITweenerToggleSettings tweenerSettings;
}