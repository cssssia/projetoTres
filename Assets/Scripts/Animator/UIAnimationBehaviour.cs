using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class UIAnimation
{
    [Header("General")]
    public RectTransform uiElement;
    public bool useUnscaledTime = false;

    [Header("Fade")]
    public bool shouldFade;
    [ShowIf("shouldFade")] public bool setInitialFade;
    [ShowIf("shouldFade")] public bool setSetImageColor;
    [ShowIf("shouldFade")][AllowNesting][Range(0, 1)] public float startAlpha;
    [ShowIf("shouldFade")][AllowNesting][Range(0, 1)] public float endAlpha;
    [ShowIf("shouldFade")][AllowNesting] public float fadeAnimationTime;
    [ShowIf("shouldFade")][AllowNesting] public AnimationCurve fadeAnimationCurve;
    [ShowIf("shouldFade")][AllowNesting] public float fadeDelayTime;

    [Header("Move")]
    public bool shouldMove;
    public bool useLocalPosition;
    [ShowIf("shouldMove")] public bool setInitialPosition;
    [ShowIf("shouldMove")][EnableIf("useLocalPosition")][AllowNesting] public RectTransform parent;
    [ShowIf("shouldMove")][EnableIf("useLocalPosition")][AllowNesting] public Vector3 startLocalPosition;
    [ShowIf("shouldMove")][EnableIf("useLocalPosition")][AllowNesting] public Vector3 finalLocalPosition;
    [ShowIf("shouldMove")][DisableIf("useLocalPosition")][AllowNesting] public Vector3 startPosition;
    [ShowIf("shouldMove")][DisableIf("useLocalPosition")][AllowNesting] public Vector3 finalPosition;
    [ShowIf("shouldMove")][AllowNesting] public float moveAnimationTime;
    [ShowIf("shouldMove")][AllowNesting] public AnimationCurve moveAnimationCurve;
    [ShowIf("shouldMove")][AllowNesting] public float moveDelayTime;

    [Header("Scale")]
    [ShowIf("isEnabled")] public bool shouldScale;
    [ShowIf("shouldScale")] public bool setInitialScale;
    [ShowIf("shouldScale")][AllowNesting] public Vector3 startScale;
    [ShowIf("shouldScale")][AllowNesting] public Vector3 finalScale;
    [ShowIf("shouldScale")][AllowNesting] public float scaleAnimationTime;
    [ShowIf("shouldScale")][AllowNesting] public AnimationCurve scaleAnimationCurve;
    [ShowIf("shouldScale")][AllowNesting] public float scaleDelayTime;

    [Header("Set Size")]
    public bool shouldSetSize;
    [ShowIf("shouldSetSize")] public bool setInitialSize;
    [ShowIf("shouldSetSize")][AllowNesting] public Vector2 startSize;
    [ShowIf("shouldSetSize")][AllowNesting] public Vector2 finalSize;
    [ShowIf("shouldSetSize")][AllowNesting] public float sizeAnimationTime;
    [ShowIf("shouldSetSize")][AllowNesting] public AnimationCurve sizeAnimationCurve;
    [ShowIf("shouldSetSize")][AllowNesting] public float sizeDelayTime;

    [Header("Rotate")]
    public bool shouldRotate;
    public bool useLocalRotate;
    [ShowIf("shouldRotate")] public bool setInitialRotation;
    [ShowIf("shouldRotate")][EnableIf("useLocalRotate")][AllowNesting] public Vector3 startLocalRotate;
    [ShowIf("shouldRotate")][EnableIf("useLocalRotate")][AllowNesting] public Vector3 finalLocalRotate;
    [ShowIf("shouldRotate")][DisableIf("useLocalRotate")][AllowNesting] public Vector3 startRotate;
    [ShowIf("shouldRotate")][DisableIf("useLocalRotate")][AllowNesting] public Vector3 finalRotate;
    [ShowIf("shouldRotate")][AllowNesting] public float rotateAnimationTime;
    [ShowIf("shouldRotate")][AllowNesting] public AnimationCurve rotateAnimationCurve;
    [ShowIf("shouldRotate")][AllowNesting] public float rotateDelayTime;

    [Header("Shake")]
    public bool shouldShake;
    public bool useCorrectTimeShake;
    [ShowIf("shouldShake")][AllowNesting] public float maxShakeIntensity;
    [ShowIf("shouldShake")][AllowNesting] public float shakeAnimationTime;
    [ShowIf("shouldShake")][AllowNesting] public AnimationCurve shakeAnimationCurve;
    [ShowIf("shouldShake")][AllowNesting] public float shakeDelayTime;

    public Coroutine animCoroutine;

    bool l_usesCurve = false;
    float l_timeCounter = 0, l_initialFade = 0;
    CanvasGroup l_canvasGroup;
    Image l_image;
    Color l_tempColor;
    Vector3 l_initialPosition, l_initialScale, l_shakeOffset, l_currentPosition, l_initialRotation;
    Vector2 l_initialSize;
    public IEnumerator AnimateUIElement(UIAnimation p_uiAnimation)
    {
        if (p_uiAnimation.uiElement == null) yield break;

        bool l_isFadeIn;
        float l_fadeTimeCounter = 0,
              l_moveTimeCounter = 0,
              l_scaleTimeCounter = 0,
              l_rotateTimeCounter = 0,
              l_shakeTimeCounter = 0,
              l_sizeTimeCounter = 0;

        bool l_isFadeComplete = !p_uiAnimation.shouldFade;
        bool l_isMoveComplete = !p_uiAnimation.shouldMove;
        bool l_isScaleComplete = !p_uiAnimation.shouldScale;
        bool l_isRotateComplete = !p_uiAnimation.shouldRotate;
        bool l_isShakeComplete = !p_uiAnimation.shouldShake;
        bool l_isSizeComplete = !p_uiAnimation.shouldSetSize;

        l_timeCounter = 0;

        if (!setSetImageColor)
        {
            if (l_canvasGroup == null) l_canvasGroup = p_uiAnimation.uiElement.GetComponent<CanvasGroup>();
        }
        else
        {
            if (l_image == null) l_image = p_uiAnimation.uiElement.GetComponent<Image>();
        }

        if (p_uiAnimation.shouldFade)
        {
            if (!setSetImageColor)
            {
                if (l_canvasGroup != null
                && !setInitialFade) l_canvasGroup.alpha = p_uiAnimation.startAlpha;

                if (setInitialFade) l_initialFade = l_canvasGroup.alpha;
                else l_initialFade = p_uiAnimation.startAlpha;
            }
            else
            {
                l_tempColor = l_image.color;
                if (!setInitialFade)
                {
                    l_initialFade = p_uiAnimation.startAlpha;
                    l_tempColor.a = l_initialFade;
                    l_image.color = l_tempColor;
                }
            }
        }

        if (p_uiAnimation.shouldMove)
        {
            if (p_uiAnimation.useLocalPosition)
            {
                if (setInitialPosition) l_initialPosition = p_uiAnimation.uiElement.anchoredPosition;
                else l_initialPosition = p_uiAnimation.startLocalPosition;
            }
            else
            {
                if (setInitialPosition) l_initialPosition = p_uiAnimation.uiElement.transform.position;
                else l_initialPosition = p_uiAnimation.startPosition;
            }
        }

        if (p_uiAnimation.shouldShake)
        {
            if (!useLocalPosition) l_currentPosition = p_uiAnimation.uiElement.position;
            else l_currentPosition = p_uiAnimation.uiElement.anchoredPosition;
        }

        if (p_uiAnimation.shouldScale)
        {
            if (setInitialScale) l_initialScale = p_uiAnimation.uiElement.localScale;
            else l_initialScale = p_uiAnimation.startScale;
        }

        if (p_uiAnimation.shouldRotate)
        {
            if (setInitialRotation)
            {
                if (useLocalRotate) l_initialRotation = p_uiAnimation.uiElement.localEulerAngles;
                else l_initialRotation = p_uiAnimation.uiElement.transform.eulerAngles;
            }
            else
            {
                if (useLocalRotate) l_initialRotation = p_uiAnimation.startLocalRotate;
                else l_initialRotation = p_uiAnimation.startRotate;
            }
        }

        if (p_uiAnimation.shouldSetSize)
        {
            if (setInitialSize) l_initialSize = p_uiAnimation.uiElement.sizeDelta;
            else l_initialSize = p_uiAnimation.startSize;
        }

        while (!l_isFadeComplete || !l_isMoveComplete || !l_isScaleComplete || !l_isRotateComplete || !l_isShakeComplete || !l_isSizeComplete)
        {
            if (!l_isFadeComplete)
            {
                if (l_timeCounter >= p_uiAnimation.fadeDelayTime)
                {
                    l_isFadeIn = p_uiAnimation.startAlpha < p_uiAnimation.endAlpha;
                    l_fadeTimeCounter = l_timeCounter - p_uiAnimation.fadeDelayTime;
                    l_usesCurve = p_uiAnimation.fadeAnimationCurve.length > 0;

                    if (!setSetImageColor)
                    {
                        if (l_usesCurve)
                        {
                            l_canvasGroup.alpha = Mathf.LerpUnclamped(l_initialFade, p_uiAnimation.endAlpha, p_uiAnimation.fadeAnimationCurve.Evaluate(l_fadeTimeCounter / p_uiAnimation.fadeAnimationTime));
                        }
                        else
                        {
                            l_canvasGroup.alpha = Mathf.LerpUnclamped(l_initialFade, p_uiAnimation.endAlpha, l_fadeTimeCounter / p_uiAnimation.fadeAnimationTime);
                        }

                        if (l_fadeTimeCounter / p_uiAnimation.fadeAnimationTime >= 1)
                        {
                            l_canvasGroup.alpha = p_uiAnimation.endAlpha;

                            if (l_isFadeIn)
                            {
                                l_canvasGroup.interactable = true;
                                l_canvasGroup.blocksRaycasts = true;
                            }
                            else
                            {
                                l_canvasGroup.interactable = false;
                                l_canvasGroup.blocksRaycasts = false;
                            }

                            l_isFadeComplete = true;
                        }
                    }
                    else
                    {
                        if (l_usesCurve)
                        {
                            l_tempColor.a = Mathf.LerpUnclamped(l_initialFade, p_uiAnimation.endAlpha, p_uiAnimation.fadeAnimationCurve.Evaluate(l_fadeTimeCounter / p_uiAnimation.fadeAnimationTime));
                        }
                        else
                        {
                            l_tempColor.a = Mathf.LerpUnclamped(l_initialFade, p_uiAnimation.endAlpha, l_fadeTimeCounter / p_uiAnimation.fadeAnimationTime);
                        }

                        if (l_fadeTimeCounter / p_uiAnimation.fadeAnimationTime >= 1)
                        {
                            l_tempColor.a = p_uiAnimation.endAlpha;

                            l_isFadeComplete = true;
                        }

                        l_image.color = l_tempColor;
                    }
                }
            }
            if (!l_isMoveComplete)
            {
                if (l_timeCounter >= p_uiAnimation.moveDelayTime)
                {
                    //if (!l_isShakeComplete && l_timeCounter >= p_uiAnimation.shakeDelayTime)
                    //{
                    //    l_shakeOffset = p_uiAnimation.maxShakeIntensity * p_uiAnimation.shakeAnimationCurve.Evaluate(l_shakeTimeCounter / p_uiAnimation.shakeAnimationTime) * UnityEngine.Random.insideUnitSphere;
                    //    if (useCorrectTimeShake) l_shakeTimeCounter = l_timeCounter - p_uiAnimation.shakeDelayTime;
                    //    else l_shakeTimeCounter += l_timeCounter - p_uiAnimation.shakeDelayTime;

                    //    if (l_shakeTimeCounter / p_uiAnimation.shakeAnimationTime >= 1)
                    //    {
                    //        l_isShakeComplete = true;
                    //        l_shakeOffset = WonderUtil.Vector3Zero;
                    //    }
                    //}
                    //else l_shakeOffset = WonderUtil.Vector3Zero;

                    l_moveTimeCounter = l_timeCounter - p_uiAnimation.moveDelayTime;
                    l_usesCurve = p_uiAnimation.moveAnimationCurve.length > 0;

                    if (l_usesCurve)
                    {
                        if (p_uiAnimation.useLocalPosition)
                        {
                            l_currentPosition = Vector3.LerpUnclamped(l_initialPosition, p_uiAnimation.finalLocalPosition, p_uiAnimation.moveAnimationCurve.Evaluate(l_moveTimeCounter / p_uiAnimation.moveAnimationTime));
                            p_uiAnimation.uiElement.anchoredPosition = l_currentPosition;// + l_shakeOffset;
                        }
                        else
                        {
                            l_currentPosition = Camera.main.ViewportToScreenPoint(Vector3.LerpUnclamped(l_initialPosition, p_uiAnimation.finalPosition, p_uiAnimation.moveAnimationCurve.Evaluate(l_moveTimeCounter / p_uiAnimation.moveAnimationTime)));
                            p_uiAnimation.uiElement.transform.position = l_currentPosition;// + l_shakeOffset;
                        }
                    }
                    else
                    {
                        if (p_uiAnimation.useLocalPosition)
                        {
                            l_currentPosition = Vector3.LerpUnclamped(l_initialPosition, p_uiAnimation.finalLocalPosition, l_moveTimeCounter / p_uiAnimation.moveAnimationTime);
                            p_uiAnimation.uiElement.anchoredPosition = l_currentPosition;// + l_shakeOffset;
                        }
                        else
                        {
                            l_currentPosition = Camera.main.ViewportToScreenPoint(Vector3.LerpUnclamped(l_initialPosition, p_uiAnimation.finalPosition, l_moveTimeCounter / p_uiAnimation.moveAnimationTime));
                            p_uiAnimation.uiElement.transform.position = l_currentPosition;// + l_shakeOffset;
                        }
                    }

                    if (l_moveTimeCounter / p_uiAnimation.moveAnimationTime >= 1) l_isMoveComplete = true;
                }
            }
            if (!l_isShakeComplete && l_timeCounter >= p_uiAnimation.shakeDelayTime)
            {
                l_shakeOffset = p_uiAnimation.maxShakeIntensity * p_uiAnimation.shakeAnimationCurve.Evaluate(l_shakeTimeCounter / p_uiAnimation.shakeAnimationTime) * UnityEngine.Random.insideUnitSphere;
                if (useCorrectTimeShake) l_shakeTimeCounter = l_timeCounter - p_uiAnimation.shakeDelayTime;
                else l_shakeTimeCounter += l_timeCounter - p_uiAnimation.shakeDelayTime;

                if (l_shakeTimeCounter / p_uiAnimation.shakeAnimationTime >= 1f)
                {
                    l_isShakeComplete = true;
                    if (!useLocalPosition) p_uiAnimation.uiElement.position = l_currentPosition;
                    else p_uiAnimation.uiElement.anchoredPosition = l_currentPosition;
                }
                else
                {
                    if (!useLocalPosition) p_uiAnimation.uiElement.position = l_currentPosition + l_shakeOffset;
                    else p_uiAnimation.uiElement.anchoredPosition = l_currentPosition + l_shakeOffset;
                }

            }
            if (!l_isScaleComplete)
            {
                if (l_timeCounter >= p_uiAnimation.scaleDelayTime)
                {
                    l_scaleTimeCounter = l_timeCounter - p_uiAnimation.scaleDelayTime;
                    l_usesCurve = p_uiAnimation.scaleAnimationCurve.length > 0;

                    if (l_usesCurve)
                    {
                        p_uiAnimation.uiElement.localScale = Vector3.LerpUnclamped(l_initialScale, p_uiAnimation.finalScale, p_uiAnimation.scaleAnimationCurve.Evaluate(l_scaleTimeCounter / p_uiAnimation.scaleAnimationTime));
                    }
                    else
                    {
                        p_uiAnimation.uiElement.localScale = Vector3.LerpUnclamped(l_initialScale, p_uiAnimation.finalScale, l_scaleTimeCounter / p_uiAnimation.scaleAnimationTime);
                    }

                    if (l_scaleTimeCounter / p_uiAnimation.scaleAnimationTime >= 1) l_isScaleComplete = true;
                }
            }
            if (!l_isRotateComplete)
            {
                if (l_timeCounter >= p_uiAnimation.rotateDelayTime)
                {
                    l_rotateTimeCounter = l_timeCounter - p_uiAnimation.rotateDelayTime;
                    l_usesCurve = p_uiAnimation.rotateAnimationCurve.length > 0;

                    if (l_usesCurve)
                    {
                        if (p_uiAnimation.useLocalRotate) p_uiAnimation.uiElement.localEulerAngles = Vector3.LerpUnclamped(l_initialRotation, p_uiAnimation.finalLocalRotate, p_uiAnimation.rotateAnimationCurve.Evaluate(l_rotateTimeCounter / p_uiAnimation.rotateAnimationTime));
                        else p_uiAnimation.uiElement.transform.eulerAngles = Vector3.LerpUnclamped(l_initialRotation, p_uiAnimation.finalRotate, p_uiAnimation.rotateAnimationCurve.Evaluate(l_rotateTimeCounter / p_uiAnimation.rotateAnimationTime));
                    }
                    else
                    {
                        if (p_uiAnimation.useLocalRotate) p_uiAnimation.uiElement.localEulerAngles = Vector3.LerpUnclamped(l_initialRotation, p_uiAnimation.finalLocalRotate, l_rotateTimeCounter / p_uiAnimation.rotateAnimationTime);
                        else p_uiAnimation.uiElement.transform.eulerAngles = Vector3.LerpUnclamped(l_initialRotation, p_uiAnimation.finalRotate, l_rotateTimeCounter / p_uiAnimation.rotateAnimationTime);
                    }

                    if (l_rotateTimeCounter / p_uiAnimation.rotateAnimationTime >= 1) l_isRotateComplete = true;
                }
            }
            if (!l_isSizeComplete)
            {
                if (l_timeCounter >= p_uiAnimation.sizeDelayTime)
                {
                    l_sizeTimeCounter = l_timeCounter - p_uiAnimation.sizeDelayTime;
                    l_usesCurve = p_uiAnimation.sizeAnimationCurve.length > 0;

                    if (l_usesCurve) p_uiAnimation.uiElement.sizeDelta = Vector2.LerpUnclamped(l_initialSize, p_uiAnimation.finalSize, p_uiAnimation.sizeAnimationCurve.Evaluate(l_sizeTimeCounter / p_uiAnimation.sizeAnimationTime));
                    else p_uiAnimation.uiElement.sizeDelta = Vector2.LerpUnclamped(l_initialSize, p_uiAnimation.finalSize, l_sizeTimeCounter / p_uiAnimation.sizeAnimationTime);

                    if (l_sizeTimeCounter / p_uiAnimation.sizeAnimationTime >= 1) l_isSizeComplete = true;
                }
            }

            if (!useUnscaledTime) l_timeCounter += Time.deltaTime;
            else l_timeCounter += Time.unscaledDeltaTime;

            yield return null;
        }

        animCoroutine = null;
    }

    public void SetInitialTranforms(UIAnimation p_uiAnimation)
    {
        if (p_uiAnimation.uiElement == null) return;

        l_timeCounter = 0;

        if (l_canvasGroup == null) l_canvasGroup = p_uiAnimation.uiElement.GetComponent<CanvasGroup>();
        if (l_canvasGroup != null) l_canvasGroup.alpha = p_uiAnimation.startAlpha;


        if (p_uiAnimation.shouldFade)
        {
            if (setInitialFade) l_initialFade = l_canvasGroup.alpha;
            else l_initialFade = p_uiAnimation.startAlpha;

            l_canvasGroup.alpha = l_initialFade;
        }

        if (p_uiAnimation.shouldMove)
        {
            if (p_uiAnimation.useLocalPosition)
            {
                if (setInitialPosition) l_initialPosition = p_uiAnimation.uiElement.anchoredPosition;
                else l_initialPosition = p_uiAnimation.startLocalPosition;
            }
            else
            {
                if (setInitialPosition) l_initialPosition = p_uiAnimation.uiElement.transform.position;
                else l_initialPosition = p_uiAnimation.startPosition;
            }

            p_uiAnimation.uiElement.anchoredPosition = l_initialPosition;
        }

        if (p_uiAnimation.shouldScale)
        {
            if (setInitialScale) l_initialScale = p_uiAnimation.uiElement.localScale;
            else l_initialScale = p_uiAnimation.startScale;

            p_uiAnimation.uiElement.localScale = l_initialScale;
        }

        if (p_uiAnimation.shouldRotate)
        {
            if (setInitialRotation)
            {
                if (useLocalRotate) l_initialRotation = p_uiAnimation.uiElement.localEulerAngles;
                else l_initialRotation = p_uiAnimation.uiElement.transform.eulerAngles;
                p_uiAnimation.uiElement.localEulerAngles = l_initialRotation;
            }
            else
            {
                if (useLocalRotate) l_initialRotation = p_uiAnimation.startLocalRotate;
                else l_initialRotation = p_uiAnimation.startRotate;
                p_uiAnimation.uiElement.transform.eulerAngles = l_initialRotation;
            }
        }

    }

    public void SetFinalTransforms(UIAnimation p_uiAnimation)
    {
        if (p_uiAnimation.uiElement == null) return;

        l_timeCounter = 0;

        if (l_canvasGroup == null) l_canvasGroup = p_uiAnimation.uiElement.GetComponent<CanvasGroup>();
        if (l_canvasGroup != null) l_canvasGroup.alpha = p_uiAnimation.startAlpha;


        if (p_uiAnimation.shouldFade)
        {
            l_canvasGroup.alpha = p_uiAnimation.endAlpha;
        }

        if (p_uiAnimation.shouldMove)
        {
            if (p_uiAnimation.useLocalPosition)
            {
                l_initialPosition = p_uiAnimation.finalLocalPosition;
            }
            else
            {
                l_initialPosition = p_uiAnimation.finalPosition;
            }

            p_uiAnimation.uiElement.anchoredPosition = l_initialPosition;
        }

        if (p_uiAnimation.shouldScale)
        {
            p_uiAnimation.uiElement.localScale = p_uiAnimation.finalScale;
        }

        if (p_uiAnimation.shouldRotate)
        {
            if (p_uiAnimation.useLocalRotate)
            {
                p_uiAnimation.uiElement.localRotation = Quaternion.Euler(p_uiAnimation.finalLocalRotate);
            }
            else
            {
                p_uiAnimation.uiElement.rotation = Quaternion.Euler(p_uiAnimation.finalRotate);
            }
        }
    }
}

public enum UIAnimationType { ENTRY, LEAVE }

public class UIAnimationBehaviour : MonoBehaviour
{
    public bool enableAnimationsOnStart;
    public bool entryAnimationsOnEnable;
    public List<UIAnimation> entryUiAnimationsList;
    public List<UIAnimation> leaveUiAnimationsList;

    [Header("Actions")]
    public UnityEvent<bool> OnEntryAnimationsStarted;
    public UnityEvent<bool> OnEntryAnimationsFinished;
    public UnityEvent<bool> OnLeaveAnimationsStarted;
    public UnityEvent<bool> OnLeaveAnimationsFinished;

    [Header("Debug")]
    public RectTransform m_debugRectTransform;

    public bool Animating { get; private set; }

    private void Start()
    {
        if (enableAnimationsOnStart) PlayAnimations(UIAnimationType.ENTRY);
    }

    private void OnEnable()
    {
        if (entryAnimationsOnEnable) PlayAnimations(UIAnimationType.ENTRY);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void PlayLeaveAnimations()
    {
        PlayAnimations(UIAnimationType.LEAVE);
    }

    public void PlayEnteryAnimations()
    {
        PlayAnimations(UIAnimationType.ENTRY);
    }

    Coroutine l_allAnimCoroutine;
    public Coroutine PlayAnimations(UIAnimationType p_uiAnimationType, Action p_action = null)
    {
        if (gameObject.activeInHierarchy == false) return null;

        Animating = true;

        if (l_allAnimCoroutine != null) StopCoroutine(l_allAnimCoroutine);

        if (p_uiAnimationType.Equals(UIAnimationType.ENTRY))
        {
            for (int i = 0; i < entryUiAnimationsList.Count; i++)
            {
                if (entryUiAnimationsList[i].animCoroutine != null) StopCoroutine(entryUiAnimationsList[i].animCoroutine);
                entryUiAnimationsList[i].animCoroutine = StartCoroutine(entryUiAnimationsList[i].AnimateUIElement(entryUiAnimationsList[i]));
            }
        }
        else
        {
            for (int i = 0; i < leaveUiAnimationsList.Count; i++)
            {
                if (leaveUiAnimationsList[i].animCoroutine != null) StopCoroutine(leaveUiAnimationsList[i].animCoroutine);
                leaveUiAnimationsList[i].animCoroutine = StartCoroutine(leaveUiAnimationsList[i].AnimateUIElement(leaveUiAnimationsList[i]));
            }
        }

        l_allAnimCoroutine = StartCoroutine(WaitAnims(p_uiAnimationType, p_action));

        return l_allAnimCoroutine;
    }

    IEnumerator WaitAnims(UIAnimationType p_uiAnimationType, Action p_action)
    {
        if (p_uiAnimationType == UIAnimationType.ENTRY) OnEntryAnimationsStarted?.Invoke(true);
        else if (p_uiAnimationType == UIAnimationType.LEAVE) OnLeaveAnimationsStarted?.Invoke(true);

        yield return null;

        bool l_allFinished = true;

        do
        {
            l_allFinished = true;

            if (p_uiAnimationType.Equals(UIAnimationType.ENTRY))
            {
                for (int i = 0; i < entryUiAnimationsList.Count; i++)
                {
                    if (entryUiAnimationsList[i].animCoroutine != null)
                    {
                        l_allFinished = false;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < leaveUiAnimationsList.Count; i++)
                {
                    if (leaveUiAnimationsList[i].animCoroutine != null)
                    {
                        l_allFinished = false;
                        break;
                    }
                }
            }

            yield return null;
        }
        while (!l_allFinished);

        if (p_uiAnimationType == UIAnimationType.ENTRY) OnEntryAnimationsFinished?.Invoke(true);
        else if (p_uiAnimationType == UIAnimationType.LEAVE) OnLeaveAnimationsFinished?.Invoke(true);

        if (p_action != null) p_action?.Invoke();
        Animating = false;
    }

    public void SetInitialTransforms()
    {
        for (int i = 0; i < entryUiAnimationsList.Count; i++)
        {
            entryUiAnimationsList[i].SetFinalTransforms(entryUiAnimationsList[i]);
        }
    }

    public void SetFinalTransforms()
    {
        for (int i = 0; i < leaveUiAnimationsList.Count; i++)
        {
            leaveUiAnimationsList[i].SetFinalTransforms(leaveUiAnimationsList[i]);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UIAnimationBehaviour))]
public class UIAnimationsManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        UIAnimationBehaviour script = (UIAnimationBehaviour)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Debug position"))
        {
            Debug.Log($"Element position " + Camera.main.ScreenToViewportPoint(script.m_debugRectTransform.transform.position));
            Debug.Log($"Element localposition " + script.m_debugRectTransform.anchoredPosition);
        }

        if (GUILayout.Button("Initial positions")) script.SetInitialTransforms();
        if (GUILayout.Button("Final positions")) script.SetFinalTransforms();

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Debug entry animation"))
            {
                script.PlayAnimations(UIAnimationType.ENTRY);
            }
            else if (GUILayout.Button("Debug leave animation"))
            {
                script.PlayAnimations(UIAnimationType.LEAVE);
            }
        }
    }
}
#endif