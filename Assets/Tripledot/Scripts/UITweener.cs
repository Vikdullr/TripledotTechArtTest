using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum EasingType
{
    Linear,
    EaseInQuad, EaseOutQuad, EaseInOutQuad,
    EaseInCubic, EaseOutCubic, EaseInOutCubic,
    EaseInSine, EaseOutSine, EaseInOutSine,
    Custom
}

public enum AnimationOperationType
{
    Move,
    MoveToTarget,
    Scale,
    Rotate,
    Fade,
    Custom
}

public enum CustomAnimationType
{
    Float,
    Color
}

[System.Serializable]
public class TweenAnimation
{
    public string animationName = "New Tween";
    public string animationGroup = "";
    public bool enabled = true;
    public AnimationOperationType operationType = AnimationOperationType.Move;
    public float duration = 1.0f;
    [Min(0f)]
    public float delay = 0f;
    public EasingType easingType = EasingType.Linear;
    public bool mirrorEaseOnRewind = false;
    public bool unrewindable = false;
    public bool snapToEndState = true;
    public AnimationCurve customEaseCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    public bool ignoreX = false;
    public bool ignoreY = false;
    public bool ignoreZ = false;

    public Vector2 targetAnchoredPosition;
    public RectTransform moveToTargetTransform;
    public bool chaseTarget = false;
    public Vector3 targetScale = Vector3.one;
    public Vector3 targetEulerAngles;
    [Range(0f, 1f)]
    public float targetAlpha = 1.0f;

    public RectTransform targetRectTransformOverride;
    public GameObject targetGameObjectOverride;

    public CustomAnimationType customAnimationType = CustomAnimationType.Float;
    public float customStartFloat;
    public float customEndFloat;
    public Color customStartColor = Color.white;
    public Color customEndColor = Color.white;
    public UnityEvent<float> onUpdateFloat;
    public UnityEvent<Color> onUpdateColor;

    [System.NonSerialized] public bool isInitialStateCaptured = false;
    [System.NonSerialized] public Vector2? previewInitialAnchoredPosition;
    [System.NonSerialized] public Vector3? previewInitialWorldPosition;
    [System.NonSerialized] public Vector3? previewInitialScale;
    [System.NonSerialized] public Vector3? previewInitialEulerAngles;
    [System.NonSerialized] public float? previewInitialAlpha;

    [System.NonSerialized] public float? lastKnownFloat;
    [System.NonSerialized] public Color? lastKnownColor;
}


public class UITweener : MonoBehaviour
{
    public List<TweenAnimation> tweenAnimations = new List<TweenAnimation>();

    private enum AnimationDirection { None, Forward, Rewind }
    private Dictionary<TweenAnimation, AnimationDirection> _animationDirections = new Dictionary<TweenAnimation, AnimationDirection>();

    private RectTransform baseRectTransform;
    private CanvasGroup baseCanvasGroup;
    private Image baseImage;
    private Dictionary<TweenAnimation, Coroutine> _activeCoroutines = new Dictionary<TweenAnimation, Coroutine>();

    private Dictionary<string, float> _groupStartTimes = new Dictionary<string, float>();
    private Dictionary<string, float> _animationStartTimes = new Dictionary<string, float>();

    void Awake()
    {
        baseRectTransform = GetComponent<RectTransform>();
        baseCanvasGroup = GetComponent<CanvasGroup>();
        baseImage = GetComponent<Image>();
    }

    private void RestoreInitialState(TweenAnimation anim)
    {
        if (!anim.isInitialStateCaptured) return;

        RectTransform rt;
        switch (anim.operationType)
        {
            case AnimationOperationType.Move:
                rt = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (rt && anim.previewInitialAnchoredPosition.HasValue)
                {
                    Vector2 currentPos = rt.anchoredPosition;
                    Vector2 initialPos = anim.previewInitialAnchoredPosition.Value;
                    rt.anchoredPosition = new Vector2(
                        anim.ignoreX ? currentPos.x : initialPos.x,
                        anim.ignoreY ? currentPos.y : initialPos.y
                    );
                }
                break;
            case AnimationOperationType.MoveToTarget:
                rt = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (rt && anim.previewInitialWorldPosition.HasValue)
                {
                    Vector3 currentPos = rt.position;
                    Vector3 initialPos = anim.previewInitialWorldPosition.Value;
                    rt.position = new Vector3(
                        anim.ignoreX ? currentPos.x : initialPos.x,
                        anim.ignoreY ? currentPos.y : initialPos.y,
                        anim.ignoreZ ? currentPos.z : initialPos.z
                    );
                }
                break;
            case AnimationOperationType.Scale:
                rt = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (rt && anim.previewInitialScale.HasValue)
                {
                    Vector3 currentScale = rt.localScale;
                    Vector3 initialScale = anim.previewInitialScale.Value;
                    rt.localScale = new Vector3(
                        anim.ignoreX ? currentScale.x : initialScale.x,
                        anim.ignoreY ? currentScale.y : initialScale.y,
                        anim.ignoreZ ? currentScale.z : initialScale.z
                    );
                }
                break;
            case AnimationOperationType.Rotate:
                rt = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (rt && anim.previewInitialEulerAngles.HasValue)
                {
                    Vector3 currentAngles = rt.localEulerAngles;
                    Vector3 initialAngles = anim.previewInitialEulerAngles.Value;
                    rt.localEulerAngles = new Vector3(
                        anim.ignoreX ? currentAngles.x : initialAngles.x,
                        anim.ignoreY ? currentAngles.y : initialAngles.y,
                        anim.ignoreZ ? currentAngles.z : initialAngles.z
                    );
                }
                break;
            case AnimationOperationType.Fade:
                Component fadeTarget = GetFadeTarget(anim);
                if (fadeTarget && anim.previewInitialAlpha.HasValue)
                    SetAlpha(fadeTarget, anim.previewInitialAlpha.Value);
                break;
            case AnimationOperationType.Custom:
                switch (anim.customAnimationType)
                {
                    case CustomAnimationType.Float:
                        anim.lastKnownFloat = anim.customStartFloat;
                        anim.onUpdateFloat.Invoke(anim.customStartFloat);
                        break;
                    case CustomAnimationType.Color:
                        anim.lastKnownColor = anim.customStartColor;
                        anim.onUpdateColor.Invoke(anim.customStartColor);
                        break;
                }
                break;
        }
    }

    private void StopAndClearCoroutine(TweenAnimation anim, bool restoreStateOnInterrupt)
    {
        if (_activeCoroutines.TryGetValue(anim, out Coroutine runningCoroutine))
        {
            if (runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
                if (restoreStateOnInterrupt)
                {
                    RestoreInitialState(anim);
                }
            }
            _activeCoroutines.Remove(anim);
            _animationDirections.Remove(anim);
        }
    }

    public void StopAllTweens()
    {
        var animationsToStop = new List<TweenAnimation>(_activeCoroutines.Keys);
        foreach (var anim in animationsToStop)
        {
            StopAndClearCoroutine(anim, true);
        }
    }

    public void ToggleAnimation(string animationName, bool playForward)
    {
        TweenAnimation anim = tweenAnimations.Find(a => a.animationName == animationName);
        if (anim != null)
        {
            if (playForward) PlayAnimation(animationName);
            else RewindAnimation(animationName);
        }
        else
        {
            Debug.LogWarning($"UITweener: Animation with name '{animationName}' not found for toggle.", this);
        }
    }

    public void ToggleAnimationGroup(string groupName, bool playForward)
    {
        if (playForward)
        {
            PlayAnimationGroup(groupName);
        }
        else
        {
            RewindAnimationGroup(groupName);
        }
    }

    public void PlayAnimation(string animationName)
    {
        TweenAnimation animToPlay = tweenAnimations.Find(anim => anim.animationName == animationName);
        if (animToPlay != null)
        {
            _animationStartTimes[animationName] = Time.time;
            StartForwardTween(animToPlay);
        }
        else
        {
            Debug.LogWarning($"UITweener: Animation with name '{animationName}' not found.", this);
        }
    }

    public void PlayAnimationGroup(string groupName)
    {
        List<TweenAnimation> groupAnims = tweenAnimations.FindAll(anim => anim.animationGroup == groupName && !string.IsNullOrEmpty(groupName));
        if (groupAnims.Count > 0)
        {
            _groupStartTimes[groupName] = Time.time;
            foreach (var anim in groupAnims)
            {
                StartForwardTween(anim, anim.delay);
            }
        }
        else
        {
            Debug.LogWarning($"UITweener: Animation group with name '{groupName}' not found.", this);
        }
    }

    public void RewindAnimation(string animationName)
    {
        if (!_animationStartTimes.TryGetValue(animationName, out float startTime))
        {
            Debug.LogWarning($"Cannot rewind '{animationName}': it has not been played.", this);
            return;
        }

        TweenAnimation animToRewind = tweenAnimations.Find(anim => anim.animationName == animationName);
        if (animToRewind != null)
        {
            if (animToRewind.unrewindable) return;

            float elapsedTime = Time.time - startTime;
            if (elapsedTime < animToRewind.delay) return;

            float timeActive = elapsedTime - animToRewind.delay;
            float rewindDuration = Mathf.Min(timeActive, animToRewind.duration);

            StartRewindTween(animToRewind, 0f, rewindDuration);
        }
        else
        {
            Debug.LogWarning($"UITweener: Animation with name '{animationName}' not found for rewind.", this);
        }
    }

    public void RewindAnimationGroup(string groupName)
    {
        HandleRewindGroup(groupName);
    }

    private void HandleRewindGroup(string groupName)
    {
        if (!_groupStartTimes.TryGetValue(groupName, out float startTime))
        {
            Debug.LogWarning($"Cannot rewind group '{groupName}': it has not been played.", this);
            return;
        }

        List<TweenAnimation> groupAnims = tweenAnimations.FindAll(anim => anim.animationGroup == groupName && !string.IsNullOrEmpty(groupName));
        if (groupAnims.Count > 0)
        {
            foreach (var anim in groupAnims)
            {
                StopAndClearCoroutine(anim, false);
            }

            float totalAnimationTime = 0f;
            foreach (var anim in groupAnims)
            {
                if (anim.enabled)
                {
                    totalAnimationTime = Mathf.Max(totalAnimationTime, anim.delay + anim.duration);
                }
            }

            float elapsedTimeSinceStart = Time.time - startTime;
            float rewindTriggerTime = Mathf.Min(elapsedTimeSinceStart, totalAnimationTime);

            foreach (var anim in groupAnims)
            {
                if (!anim.enabled || anim.unrewindable)
                {
                    continue;
                }

                if (rewindTriggerTime < anim.delay)
                {
                    continue;
                }

                if (!anim.isInitialStateCaptured)
                {
                    Debug.LogWarning($"Cannot rewind '{anim.animationName}': initial state was not captured. Skipping.", this);
                    continue;
                }

                float timeActive = rewindTriggerTime - anim.delay;
                float rewindDuration = Mathf.Min(timeActive, anim.duration);
                float rewindDelay = rewindTriggerTime - (anim.delay + rewindDuration);

                StartRewindTween(anim, rewindDelay, rewindDuration);
            }
        }
        else
        {
            Debug.LogWarning($"UITweener: Animation group with name '{groupName}' not found for rewind.", this);
        }
    }

    private bool IsAnimationAtInitialState(TweenAnimation anim)
    {
        if (!anim.isInitialStateCaptured) return true;

        const float tolerance = 0.001f;
        RectTransform rt;

        switch (anim.operationType)
        {
            case AnimationOperationType.Move:
                rt = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (rt && anim.previewInitialAnchoredPosition.HasValue)
                    return Vector2.Distance(rt.anchoredPosition, anim.previewInitialAnchoredPosition.Value) < tolerance;
                break;
            case AnimationOperationType.MoveToTarget:
                rt = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (rt && anim.previewInitialWorldPosition.HasValue)
                    return Vector3.Distance(rt.position, anim.previewInitialWorldPosition.Value) < tolerance;
                break;
            case AnimationOperationType.Scale:
                rt = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (rt && anim.previewInitialScale.HasValue)
                    return Vector3.Distance(rt.localScale, anim.previewInitialScale.Value) < tolerance;
                break;
            case AnimationOperationType.Rotate:
                rt = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (rt && anim.previewInitialEulerAngles.HasValue)
                    return Vector3.Distance(rt.localEulerAngles, anim.previewInitialEulerAngles.Value) < tolerance;
                break;
            case AnimationOperationType.Fade:
                Component fadeTarget = GetFadeTarget(anim);
                if (fadeTarget && anim.previewInitialAlpha.HasValue)
                    return Mathf.Abs(GetAlpha(fadeTarget) - anim.previewInitialAlpha.Value) < tolerance;
                break;
            case AnimationOperationType.Custom:
                return true;
        }

        return true;
    }

    private void CaptureInitialState(TweenAnimation anim)
    {
        if (!anim.enabled) return;

        RectTransform rt;
        switch (anim.operationType)
        {
            case AnimationOperationType.Move:
            case AnimationOperationType.MoveToTarget:
                rt = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (rt)
                {
                    anim.previewInitialAnchoredPosition = rt.anchoredPosition;
                    anim.previewInitialWorldPosition = rt.position;
                }
                break;
            case AnimationOperationType.Scale:
                rt = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (rt) anim.previewInitialScale = rt.localScale;
                break;
            case AnimationOperationType.Rotate:
                rt = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (rt) anim.previewInitialEulerAngles = rt.localEulerAngles;
                break;
            case AnimationOperationType.Fade:
                Component fadeTarget = GetFadeTarget(anim);
                if (fadeTarget) anim.previewInitialAlpha = GetAlpha(fadeTarget);
                break;
            case AnimationOperationType.Custom:
                break;
        }
        anim.isInitialStateCaptured = true;
    }

    private void StartForwardTween(TweenAnimation anim, float? overrideDelay = null)
    {
        if (!anim.enabled) return;

        _animationDirections.TryGetValue(anim, out AnimationDirection currentDirection);
        bool restoreOnInterrupt = (currentDirection == AnimationDirection.Forward);
        StopAndClearCoroutine(anim, restoreOnInterrupt);

        if (!anim.isInitialStateCaptured)
        {
            CaptureInitialState(anim);
        }

        float delayToUse = overrideDelay ?? anim.delay;
        Coroutine newCoroutine = null;
        RectTransform sourceRT;
        Vector3 targetVec3;

        switch (anim.operationType)
        {
            case AnimationOperationType.Move:
                sourceRT = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (sourceRT != null)
                {
                    Vector2 startPos = sourceRT.anchoredPosition;
                    Vector2 targetPos = anim.targetAnchoredPosition;
                    if (anim.ignoreX) targetPos.x = startPos.x;
                    if (anim.ignoreY) targetPos.y = startPos.y;
                    newCoroutine = StartCoroutine(AnimateMove(sourceRT, startPos, targetPos, anim.duration, delayToUse, anim, false));
                }
                break;
            case AnimationOperationType.MoveToTarget:
                if (anim.moveToTargetTransform != null)
                {
                    sourceRT = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                    if (sourceRT != null)
                    {
                        Vector3 startPos = sourceRT.position;
                        if (anim.chaseTarget)
                        {
                            newCoroutine = StartCoroutine(AnimateMoveWorld_Follow(sourceRT, startPos, anim.moveToTargetTransform, anim.duration, delayToUse, anim, false));
                        }
                        else
                        {
                            Vector3 targetPos = anim.moveToTargetTransform.position;
                            if (anim.ignoreX) targetPos.x = startPos.x;
                            if (anim.ignoreY) targetPos.y = startPos.y;
                            if (anim.ignoreZ) targetPos.z = startPos.z;
                            newCoroutine = StartCoroutine(AnimateMoveWorld(sourceRT, startPos, targetPos, anim.duration, delayToUse, anim, false));
                        }
                    }
                }
                break;
            case AnimationOperationType.Scale:
                sourceRT = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (sourceRT != null)
                {
                    Vector3 startScale = sourceRT.localScale;
                    targetVec3 = anim.targetScale;
                    if (anim.ignoreX) targetVec3.x = startScale.x;
                    if (anim.ignoreY) targetVec3.y = startScale.y;
                    if (anim.ignoreZ) targetVec3.z = startScale.z;
                    newCoroutine = StartCoroutine(AnimateScale(sourceRT, startScale, targetVec3, anim.duration, delayToUse, anim, false));
                }
                break;
            case AnimationOperationType.Rotate:
                sourceRT = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (sourceRT != null)
                {
                    Vector3 startEuler = sourceRT.localEulerAngles;
                    targetVec3 = anim.targetEulerAngles;
                    if (anim.ignoreX) targetVec3.x = startEuler.x;
                    if (anim.ignoreY) targetVec3.y = startEuler.y;
                    if (anim.ignoreZ) targetVec3.z = startEuler.z;
                    newCoroutine = StartCoroutine(AnimateRotation(sourceRT, startEuler, targetVec3, anim.duration, delayToUse, anim, false));
                }
                break;
            case AnimationOperationType.Fade:
                Component fadeTarget = GetFadeTarget(anim);
                if (fadeTarget != null)
                {
                    newCoroutine = StartCoroutine(AnimateAlpha(fadeTarget, GetAlpha(fadeTarget), anim.targetAlpha, anim.duration, delayToUse, anim, false));
                }
                break;
            case AnimationOperationType.Custom:
                switch (anim.customAnimationType)
                {
                    case CustomAnimationType.Float:
                        float startFloat = anim.lastKnownFloat ?? anim.customStartFloat;
                        newCoroutine = StartCoroutine(AnimateCustomFloat(startFloat, anim.customEndFloat, anim.duration, delayToUse, anim, false));
                        break;
                    case CustomAnimationType.Color:
                        Color startColor = anim.lastKnownColor ?? anim.customStartColor;
                        newCoroutine = StartCoroutine(AnimateCustomColor(startColor, anim.customEndColor, anim.duration, delayToUse, anim, false));
                        break;
                }
                break;
        }

        if (newCoroutine != null)
        {
            _activeCoroutines[anim] = newCoroutine;
            _animationDirections[anim] = AnimationDirection.Forward;
        }
    }

    private void StartRewindTween(TweenAnimation anim, float? customRewindDelay = null, float? customRewindDuration = null)
    {
        if (!anim.enabled || anim.unrewindable) return;

        StopAndClearCoroutine(anim, false);

        if (!anim.isInitialStateCaptured)
        {
            Debug.LogWarning($"Cannot rewind '{anim.animationName}': it has not been played forward yet.", this);
            return;
        }

        Coroutine newCoroutine = null;
        bool mirror = anim.mirrorEaseOnRewind;
        float delay = customRewindDelay ?? 0f;
        float duration = customRewindDuration ?? anim.duration;

        RectTransform rt;

        switch (anim.operationType)
        {
            case AnimationOperationType.Move:
                rt = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (rt && anim.previewInitialAnchoredPosition.HasValue)
                    newCoroutine = StartCoroutine(AnimateMove(rt, rt.anchoredPosition, anim.previewInitialAnchoredPosition.Value, duration, delay, anim, mirror));
                break;
            case AnimationOperationType.MoveToTarget:
                rt = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (rt && anim.previewInitialWorldPosition.HasValue)
                    newCoroutine = StartCoroutine(AnimateMoveWorld(rt, rt.position, anim.previewInitialWorldPosition.Value, duration, delay, anim, mirror));
                break;
            case AnimationOperationType.Scale:
                rt = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (rt && anim.previewInitialScale.HasValue)
                    newCoroutine = StartCoroutine(AnimateScale(rt, rt.localScale, anim.previewInitialScale.Value, duration, delay, anim, mirror));
                break;
            case AnimationOperationType.Rotate:
                rt = anim.targetRectTransformOverride != null ? anim.targetRectTransformOverride : baseRectTransform;
                if (rt && anim.previewInitialEulerAngles.HasValue)
                    newCoroutine = StartCoroutine(AnimateRotation(rt, rt.localEulerAngles, anim.previewInitialEulerAngles.Value, duration, delay, anim, mirror));
                break;
            case AnimationOperationType.Fade:
                Component fadeTarget = GetFadeTarget(anim);
                if (fadeTarget && anim.previewInitialAlpha.HasValue)
                    newCoroutine = StartCoroutine(AnimateAlpha(fadeTarget, GetAlpha(fadeTarget), anim.previewInitialAlpha.Value, duration, delay, anim, mirror));
                break;
            case AnimationOperationType.Custom:
                switch (anim.customAnimationType)
                {
                    case CustomAnimationType.Float:
                        float startFloat = anim.lastKnownFloat ?? anim.customEndFloat;
                        newCoroutine = StartCoroutine(AnimateCustomFloat(startFloat, anim.customStartFloat, duration, delay, anim, mirror));
                        break;
                    case CustomAnimationType.Color:
                        Color startColor = anim.lastKnownColor ?? anim.customEndColor;
                        newCoroutine = StartCoroutine(AnimateCustomColor(startColor, anim.customStartColor, duration, delay, anim, mirror));
                        break;
                }
                break;
        }

        if (newCoroutine != null)
        {
            _activeCoroutines[anim] = newCoroutine;
            _animationDirections[anim] = AnimationDirection.Rewind;
        }
    }

    public void PlayConfiguredAnimationsRuntime()
    {
        foreach (var anim in tweenAnimations)
        {
            StartForwardTween(anim);
        }
    }

    public Component GetFadeTarget(TweenAnimation anim)
    {
        GameObject targetGO = anim.targetGameObjectOverride != null ? anim.targetGameObjectOverride : this.gameObject;
        
        CanvasGroup cg = targetGO.GetComponent<CanvasGroup>();
        if (cg != null) return cg;

        Image img = targetGO.GetComponent<Image>();
        if (img != null) return img;
        
        if (anim.targetGameObjectOverride == null)
        {
             if (baseCanvasGroup != null) return baseCanvasGroup;
             if (baseImage != null) return baseImage;
        }

        return null;
    }
    
    private EasingType GetMirroredEasing(EasingType easing)
    {
        switch (easing)
        {
            case EasingType.EaseInQuad: return EasingType.EaseOutQuad;
            case EasingType.EaseOutQuad: return EasingType.EaseInQuad;
            case EasingType.EaseInCubic: return EasingType.EaseOutCubic;
            case EasingType.EaseOutCubic: return EasingType.EaseInCubic;
            case EasingType.EaseInSine: return EasingType.EaseOutSine;
            case EasingType.EaseOutSine: return EasingType.EaseInSine;
            default: return easing;
        }
    }

    private float GetEasedValue(float t, EasingType easing, AnimationCurve customCurve, bool mirror)
    {
        if (mirror)
        {
            if (easing == EasingType.Custom)
            {
                return 1.0f - customCurve.Evaluate(1.0f - t);
            }
            easing = GetMirroredEasing(easing);
        }

        switch (easing)
        {
            case EasingType.Linear: return t;
            case EasingType.EaseInQuad: return t * t;
            case EasingType.EaseOutQuad: return t * (2f - t);
            case EasingType.EaseInOutQuad: return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
            case EasingType.EaseInCubic: return t * t * t;
            case EasingType.EaseOutCubic: return (--t) * t * t + 1f;
            case EasingType.EaseInOutCubic: return t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;
            case EasingType.EaseInSine: return 1f - Mathf.Cos((t * Mathf.PI) / 2f);
            case EasingType.EaseOutSine: return Mathf.Sin((t * Mathf.PI) / 2f);
            case EasingType.EaseInOutSine: return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
            case EasingType.Custom: return customCurve.Evaluate(t);
            default: return t;
        }
    }

    private void SetAlpha(Component target, float value)
    {
        if (target is CanvasGroup cg)
        {
            cg.alpha = value;
        }
        else if (target is Image img)
        {
            Color c = img.color;
            c.a = value;
            img.color = c;
        }
    }

    private float GetAlpha(Component target)
    {
        if (target is CanvasGroup cg)
        {
            return cg.alpha;
        }
        else if (target is Image img)
        {
            return img.color.a;
        }
        return 0f;
    }

    private IEnumerator AnimateMove(RectTransform rt, Vector2 startVal, Vector2 targetVal, float duration, float delay, TweenAnimation anim, bool mirrorEase)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float raw_t = (duration > 0) ? Mathf.Clamp01(elapsedTime / duration) : 1f;
            float eased_t = GetEasedValue(raw_t, anim.easingType, anim.customEaseCurve, mirrorEase);
            rt.anchoredPosition = Vector2.LerpUnclamped(startVal, targetVal, eased_t);
            yield return null;
        }
        if (anim.snapToEndState)
        {
            rt.anchoredPosition = targetVal;
        }
    }

    private IEnumerator AnimateMoveWorld(RectTransform rt, Vector3 startVal, Vector3 targetVal, float duration, float delay, TweenAnimation anim, bool mirrorEase)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float raw_t = (duration > 0) ? Mathf.Clamp01(elapsedTime / duration) : 1f;
            float eased_t = GetEasedValue(raw_t, anim.easingType, anim.customEaseCurve, mirrorEase);
            rt.position = Vector3.LerpUnclamped(startVal, targetVal, eased_t);
            yield return null;
        }
        if (anim.snapToEndState)
        {
            rt.position = targetVal;
        }
    }

    private IEnumerator AnimateMoveWorld_Follow(RectTransform rt, Vector3 startVal, RectTransform targetRT, float duration, float delay, TweenAnimation anim, bool mirrorEase)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        float elapsedTime = 0f;
        Vector3 initialFollowerPos = startVal;
        while (elapsedTime < duration)
        {
            if (targetRT == null) { yield break; }

            elapsedTime += Time.deltaTime;
            float raw_t = (duration > 0) ? Mathf.Clamp01(elapsedTime / duration) : 1f;
            float eased_t = GetEasedValue(raw_t, anim.easingType, anim.customEaseCurve, mirrorEase);

            Vector3 currentTargetPos = targetRT.position;

            Vector3 finalTargetPos = new Vector3(
                anim.ignoreX ? initialFollowerPos.x : currentTargetPos.x,
                anim.ignoreY ? initialFollowerPos.y : currentTargetPos.y,
                anim.ignoreZ ? initialFollowerPos.z : currentTargetPos.z
            );

            rt.position = Vector3.LerpUnclamped(initialFollowerPos, finalTargetPos, eased_t);
            yield return null;
        }

        if (anim.snapToEndState && targetRT != null)
        {
            Vector3 finalTargetPos = targetRT.position;
            if (anim.ignoreX) finalTargetPos.x = rt.position.x;
            if (anim.ignoreY) finalTargetPos.y = rt.position.y;
            if (anim.ignoreZ) finalTargetPos.z = rt.position.z;
            rt.position = finalTargetPos;
        }
    }

    private IEnumerator AnimateScale(RectTransform rt, Vector3 startVal, Vector3 targetVal, float duration, float delay, TweenAnimation anim, bool mirrorEase)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float raw_t = (duration > 0) ? Mathf.Clamp01(elapsedTime / duration) : 1f;
            float eased_t = GetEasedValue(raw_t, anim.easingType, anim.customEaseCurve, mirrorEase);
            rt.localScale = Vector3.LerpUnclamped(startVal, targetVal, eased_t);
            yield return null;
        }
        if (anim.snapToEndState)
        {
            rt.localScale = targetVal;
        }
    }

    private IEnumerator AnimateRotation(RectTransform rt, Vector3 startVal, Vector3 targetVal, float duration, float delay, TweenAnimation anim, bool mirrorEase)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float raw_t = (duration > 0) ? Mathf.Clamp01(elapsedTime / duration) : 1f;
            float eased_t = GetEasedValue(raw_t, anim.easingType, anim.customEaseCurve, mirrorEase);
            rt.localEulerAngles = Vector3.LerpUnclamped(startVal, targetVal, eased_t);
            yield return null;
        }
        if (anim.snapToEndState)
        {
            rt.localEulerAngles = targetVal;
        }
    }

    private IEnumerator AnimateAlpha(Component target, float startVal, float targetVal, float duration, float delay, TweenAnimation anim, bool mirrorEase)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float raw_t = (duration > 0) ? Mathf.Clamp01(elapsedTime / duration) : 1f;
            float eased_t = GetEasedValue(raw_t, anim.easingType, anim.customEaseCurve, mirrorEase);
            SetAlpha(target, Mathf.LerpUnclamped(startVal, targetVal, eased_t));
            yield return null;
        }
        if (anim.snapToEndState)
        {
            SetAlpha(target, targetVal);
        }
    }

    private IEnumerator AnimateCustomFloat(float startVal, float endVal, float duration, float delay, TweenAnimation anim, bool mirrorEase)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        float elapsedTime = 0f;
        anim.lastKnownFloat = startVal;
        anim.onUpdateFloat.Invoke(startVal);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float raw_t = (duration > 0) ? Mathf.Clamp01(elapsedTime / duration) : 1f;
            float eased_t = GetEasedValue(raw_t, anim.easingType, anim.customEaseCurve, mirrorEase);
            float currentValue = Mathf.LerpUnclamped(startVal, endVal, eased_t);
            anim.lastKnownFloat = currentValue;
            anim.onUpdateFloat.Invoke(currentValue);
            yield return null;
        }

        if (anim.snapToEndState)
        {
            anim.lastKnownFloat = endVal;
            anim.onUpdateFloat.Invoke(endVal);
        }
    }

    private IEnumerator AnimateCustomColor(Color startVal, Color endVal, float duration, float delay, TweenAnimation anim, bool mirrorEase)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        float elapsedTime = 0f;
        anim.lastKnownColor = startVal;
        anim.onUpdateColor.Invoke(startVal);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float raw_t = (duration > 0) ? Mathf.Clamp01(elapsedTime / duration) : 1f;
            float eased_t = GetEasedValue(raw_t, anim.easingType, anim.customEaseCurve, mirrorEase);
            Color currentColor = Color.LerpUnclamped(startVal, endVal, eased_t);
            anim.lastKnownColor = currentColor;
            anim.onUpdateColor.Invoke(currentColor);
            yield return null;
        }

        if (anim.snapToEndState)
        {
            anim.lastKnownColor = endVal;
            anim.onUpdateColor.Invoke(endVal);
        }
    }
}