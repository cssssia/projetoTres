using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ObjectAnimation
{
    [Header("General")]
    public GameObject element;
    public bool useUnscaledTime = false;

    [Header("Fade")]
    public bool shouldFade;
    [ShowIf("shouldFade")] public bool setInitialFade;
    [ShowIf("shouldFade")][AllowNesting][Range(0, 1)] public float startAlpha;
    [ShowIf("shouldFade")][AllowNesting][Range(0, 1)] public float endAlpha;
    [ShowIf("shouldFade")][AllowNesting] public float fadeAnimationTime;
    [ShowIf("shouldFade")][AllowNesting] public AnimationCurve fadeAnimationCurve;
    [ShowIf("shouldFade")][AllowNesting] public float fadeDelayTime;

    [Header("Move")]
    public bool shouldMove;
    public bool useLocalPosition;
    [ShowIf("shouldMove")] public bool setInitialPosition;
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

    [Header("Rotate")]
    public bool shouldRotate;
    public bool useLocalRotate;
    [ShowIf("shouldRotate")][EnableIf("useLocalRotate")][AllowNesting] public Vector3 startLocalRotate;
    [ShowIf("shouldRotate")][EnableIf("useLocalRotate")][AllowNesting] public Vector3 finalLocalRotate;
    [ShowIf("shouldRotate")][DisableIf("useLocalRotate")][AllowNesting] public Vector3 startRotate;
    [ShowIf("shouldRotate")][DisableIf("useLocalRotate")][AllowNesting] public Vector3 finalRotate;
    [ShowIf("shouldRotate")][AllowNesting] public float rotateAnimationTime;
    [ShowIf("shouldRotate")][AllowNesting] public AnimationCurve rotateAnimationCurve;
    [ShowIf("shouldRotate")][AllowNesting] public float rotateDelayTime;

    public Coroutine animCoroutine;

    bool l_usesCurve = false;
    float l_timeCounter = 0, l_initialFade = 0;
    SpriteRenderer l_sprite;
    Vector3 l_initialPosition, l_initialScale;
    Color l_tempColor;
    public IEnumerator AnimateUIElement(ObjectAnimation p_animation)
    {
        if (p_animation.element == null) yield break;

        bool l_isFadeIn;
        float l_fadeTimeCounter = 0,
              l_moveTimeCounter = 0,
              l_scaleTimeCounter = 0,
              l_rotateTimeCounter = 0;

        bool l_isFadeComplete = !p_animation.shouldFade;
        bool l_isMoveComplete = !p_animation.shouldMove;
        bool l_isScaleComplete = !p_animation.shouldScale;
        bool l_isRotateComplete = !p_animation.shouldRotate;

        l_timeCounter = 0;

        if (p_animation.shouldFade
            && l_sprite == null) l_sprite = p_animation.element.GetComponent<SpriteRenderer>();
        if (l_sprite != null) l_tempColor = l_sprite.color;

        if (p_animation.shouldFade)
        {
            if (l_sprite != null
                && !setInitialFade) l_tempColor.a = p_animation.startAlpha;

            if (setInitialFade) l_initialFade = l_tempColor.a;
            else l_initialFade = p_animation.startAlpha;
        }

        if (p_animation.shouldMove)
        {
            if (p_animation.useLocalPosition)
            {
                if (setInitialPosition) l_initialPosition = p_animation.element.transform.localPosition;
                else l_initialPosition = p_animation.startLocalPosition;
            }
            else
            {
                if (setInitialPosition) l_initialPosition = p_animation.element.transform.position;
                else l_initialPosition = p_animation.startPosition;
            }
        }

        if (p_animation.shouldScale)
        {
            if (setInitialScale) l_initialScale = p_animation.element.transform.localScale;
            else l_initialScale = p_animation.startScale;
        }

        while (!l_isFadeComplete || !l_isMoveComplete || !l_isScaleComplete || !l_isRotateComplete)
        {
            if (!l_isFadeComplete)
            {
                if (l_timeCounter >= p_animation.fadeDelayTime)
                {
                    l_isFadeIn = p_animation.startAlpha < p_animation.endAlpha;
                    l_fadeTimeCounter = l_timeCounter - p_animation.fadeDelayTime;
                    l_usesCurve = p_animation.fadeAnimationCurve.length > 0;

                    if (l_usesCurve)
                    {
                        l_tempColor.a = p_animation.fadeAnimationCurve.Evaluate(Mathf.LerpUnclamped(l_initialFade, p_animation.endAlpha, l_fadeTimeCounter / p_animation.fadeAnimationTime));
                    }
                    else
                    {
                        l_tempColor.a = Mathf.LerpUnclamped(l_initialFade, p_animation.endAlpha, l_fadeTimeCounter / p_animation.fadeAnimationTime);
                    }

                    l_sprite.color = l_tempColor;

                    if (l_fadeTimeCounter / p_animation.fadeAnimationTime >= 1)
                    {
                        l_tempColor.a = p_animation.endAlpha;
                        l_sprite.color = l_tempColor; 

                        l_isFadeComplete = true;
                    }
                }
            }

            if (!l_isMoveComplete)
            {
                if (l_timeCounter >= p_animation.moveDelayTime)
                {
                    l_moveTimeCounter = l_timeCounter - p_animation.moveDelayTime;
                    l_usesCurve = p_animation.moveAnimationCurve.length > 0;

                    if (l_usesCurve)
                    {
                        if (p_animation.useLocalPosition) p_animation.element.transform.localPosition = Vector3.LerpUnclamped(l_initialPosition, p_animation.finalLocalPosition, p_animation.moveAnimationCurve.Evaluate(l_moveTimeCounter / p_animation.moveAnimationTime));
                        else p_animation.element.transform.position = Camera.main.ViewportToScreenPoint(Vector3.LerpUnclamped(l_initialPosition, p_animation.finalPosition, p_animation.moveAnimationCurve.Evaluate(l_moveTimeCounter / p_animation.moveAnimationTime)));
                    }
                    else
                    {
                        if (p_animation.useLocalPosition) p_animation.element.transform.localPosition = Vector3.LerpUnclamped(l_initialPosition, p_animation.finalLocalPosition, l_moveTimeCounter / p_animation.moveAnimationTime);
                        else p_animation.element.transform.position = Camera.main.ViewportToScreenPoint(Vector3.LerpUnclamped(l_initialPosition, p_animation.finalPosition, l_moveTimeCounter / p_animation.moveAnimationTime));
                    }

                    if (l_moveTimeCounter / p_animation.moveAnimationTime >= 1) l_isMoveComplete = true;
                }
            }

            if (!l_isScaleComplete)
            {
                if (l_timeCounter >= p_animation.scaleDelayTime)
                {
                    l_scaleTimeCounter = l_timeCounter - p_animation.scaleDelayTime;
                    l_usesCurve = p_animation.scaleAnimationCurve.length > 0;

                    if (l_usesCurve)
                    {
                        p_animation.element.transform.localScale = Vector3.LerpUnclamped(l_initialScale, p_animation.finalScale, p_animation.scaleAnimationCurve.Evaluate(l_scaleTimeCounter / p_animation.scaleAnimationTime));
                    }
                    else
                    {
                        p_animation.element.transform.localScale = Vector3.LerpUnclamped(l_initialScale, p_animation.finalScale, l_scaleTimeCounter / p_animation.scaleAnimationTime);
                    }

                    if (l_scaleTimeCounter / p_animation.scaleAnimationTime >= 1) l_isScaleComplete = true;
                }
            }

            if (!l_isRotateComplete)
            {
                if (l_timeCounter >= p_animation.rotateDelayTime)
                {
                    l_rotateTimeCounter = l_timeCounter - p_animation.rotateDelayTime;
                    l_usesCurve = p_animation.rotateAnimationCurve.length > 0;

                    if (l_usesCurve)
                    {
                        if (p_animation.useLocalRotate) p_animation.element.transform.localEulerAngles = Vector3.LerpUnclamped(p_animation.startLocalRotate, p_animation.finalLocalRotate, p_animation.rotateAnimationCurve.Evaluate(l_rotateTimeCounter / p_animation.rotateAnimationTime));
                        else p_animation.element.transform.eulerAngles = Vector3.LerpUnclamped(p_animation.startRotate, p_animation.finalRotate, p_animation.rotateAnimationCurve.Evaluate(l_rotateTimeCounter / p_animation.rotateAnimationTime));
                    }
                    else
                    {
                        if (p_animation.useLocalRotate) p_animation.element.transform.localEulerAngles = Vector3.LerpUnclamped(p_animation.startLocalRotate, p_animation.finalLocalRotate, l_rotateTimeCounter / p_animation.rotateAnimationTime);
                        else p_animation.element.transform.eulerAngles = Vector3.LerpUnclamped(p_animation.startRotate, p_animation.finalRotate, l_rotateTimeCounter / p_animation.rotateAnimationTime);
                    }

                    if (l_rotateTimeCounter / p_animation.rotateAnimationTime >= 1) l_isRotateComplete = true;
                }
            }

            if(!useUnscaledTime) l_timeCounter += Time.deltaTime;
            else l_timeCounter += Time.unscaledDeltaTime;

            yield return null;
        }

        animCoroutine = null;
    }

    public void SetInitialTranforms(ObjectAnimation p_animation)
    {
        if (p_animation.element == null) return;

        l_timeCounter = 0;

        if (p_animation.shouldFade
            && l_sprite == null) l_sprite = p_animation.element.GetComponent<SpriteRenderer>();
        if (l_sprite != null)
        {
            l_tempColor = l_sprite.color;
            l_tempColor.a = p_animation.startAlpha;
        }

        if (p_animation.shouldFade)
        {
            if (setInitialFade) l_initialFade = l_tempColor.a;
            else l_initialFade = p_animation.startAlpha;

            l_tempColor.a = l_initialFade;
            l_sprite.color = l_tempColor;
        }

        if (p_animation.shouldMove)
        {
            if (p_animation.useLocalPosition)
            {
                if (setInitialPosition) l_initialPosition = p_animation.element.transform.localPosition;
                else l_initialPosition = p_animation.startLocalPosition;

                p_animation.element.transform.localPosition = l_initialPosition;
            }
            else
            {
                if (setInitialPosition) l_initialPosition = p_animation.element.transform.position;
                else l_initialPosition = p_animation.startPosition;

                p_animation.element.transform.position = l_initialPosition;
            }

        }

        if (p_animation.shouldScale)
        {
            if (setInitialScale) l_initialScale = p_animation.element.transform.localScale;
            else l_initialScale = p_animation.startScale;

            p_animation.element.transform.localScale = l_initialScale;
        }

        if (p_animation.shouldRotate)
        {
            if (p_animation.useLocalRotate)
            {
                p_animation.element.transform.localRotation = Quaternion.Euler(p_animation.startLocalRotate);
            }
            else
            {
                p_animation.element.transform.rotation = Quaternion.Euler(p_animation.startRotate);
            }
        }
    }

    public void SetFinalTransforms(ObjectAnimation p_animation)
    {
        if (p_animation.element == null) return;

        l_timeCounter = 0;

        if (p_animation.shouldFade
            && l_sprite == null) l_sprite = p_animation.element.GetComponent<SpriteRenderer>();
        if (l_sprite != null)
        {
            l_tempColor = l_sprite.color;
            l_tempColor.a = p_animation.startAlpha;
        }


        if (p_animation.shouldFade)
        {
            l_tempColor.a = p_animation.endAlpha;
            l_sprite.color = l_tempColor;
        }

        if (p_animation.shouldMove)
        {
            if (p_animation.useLocalPosition)
            {
                l_initialPosition = p_animation.finalLocalPosition;
                p_animation.element.transform.localPosition = l_initialPosition;
            }
            else
            {
                l_initialPosition = p_animation.finalPosition;
                p_animation.element.transform.position = l_initialPosition;
            }

        }

        if (p_animation.shouldScale)
        {
            p_animation.element.transform.localScale = p_animation.finalScale;
        }

        if (p_animation.shouldRotate)
        {
            if (p_animation.useLocalRotate)
            {
                p_animation.element.transform.localRotation = Quaternion.Euler(p_animation.finalLocalRotate); 
            }
            else
            {
                p_animation.element.transform.rotation = Quaternion.Euler(p_animation.finalRotate);
            }
        }
    }
}

//public enum UIAnimationType { ENTRY, LEAVE }

public class ObjectAnimationBehaviour : MonoBehaviour
{
    public bool enableAnimationsOnStart;
    public bool entryAnimationsOnEnable;
    public List<ObjectAnimation> entryUiAnimationsList;
    public List<ObjectAnimation> leaveUiAnimationsList;

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
[CustomEditor(typeof(ObjectAnimationBehaviour))]
public class ObjectAnimationBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ObjectAnimationBehaviour script = (ObjectAnimationBehaviour)target;

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