using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Lockable Toggle", 36)]
    public class LockableToggle : Toggle
    {
        [Tooltip("If true, the toggle will not change its value and will invoke the OnLockedPressed event instead.")]
        public bool isLocked = false;

        [Tooltip("Event that fires when the toggle is pressed while in a locked state.")]
        public UnityEvent OnLockedPressed;

        private void InternalToggle()
        {
            if (!IsActive() || !IsInteractable())
                return;

            if (isLocked)
            {
                OnLockedPressed.Invoke();
                return;
            }
            
            isOn = !isOn;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            InternalToggle();
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            InternalToggle();
        }
    }
}