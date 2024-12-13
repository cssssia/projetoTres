using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BetTransform
{
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;

    public BetTransform() { }
    public BetTransform(Vector3 p_position, Vector3 p_rotation, Vector3 p_scale)
    {
        Position = p_position;
        Rotation = p_rotation;
        Scale = p_scale;
    }
    public BetTransform(BetTransform p_BetTransform)
    {
        Position = p_BetTransform.Position;
        Rotation = p_BetTransform.Rotation;
        Scale = p_BetTransform.Scale;
    }
}

public enum BetAnimType { PLAY, IDLE, HIGHLIGHT, DRAG }
public class BetBehavior : MonoBehaviour
{
    public int playerId;
    private BetAnimType m_currentState = BetAnimType.IDLE;

    [Header("Bet Transform")]
    [SerializeField] private BetTransform m_idleBetTranform;
    [SerializeField] private BetTransform m_highlightBetTranform;
    [SerializeField] private BetTransform m_individualHighlightBetTranform;

    [Header("Bet AnimConfig")]
    [SerializeField] private BetAnimConfig m_playBetAnim;
    [SerializeField] private BetAnimConfig m_idleBetAnim;
    [SerializeField] private BetAnimConfig m_hoverBetAnim;

    private Vector3 m_startPosition;
    private Vector3 m_startRotation;

    [SerializeField] private GameObject[] m_stackBets;
    [SerializeField] private Material stackBetsMaterial;
    [SerializeField] private Transform[] m_eyesPool;

    private void Start()
    {
        m_startPosition = transform.position;
        m_startRotation = transform.rotation.eulerAngles;
    }

    BetAnimConfig l_tempBetAnim;
    Coroutine m_currentAnim;
    public void AnimateToPlace(BetTransform p_betTransform, BetAnimType p_animType, bool p_isIncrease = false, Action<GameObject, bool> p_action = null)
    {
        switch (p_animType)
        {
            case BetAnimType.PLAY:
                l_tempBetAnim = m_playBetAnim;
                break;
            case BetAnimType.IDLE:
                l_tempBetAnim = m_idleBetAnim;
                break;
            case BetAnimType.HIGHLIGHT:
                l_tempBetAnim = m_hoverBetAnim;
                break;
        }

        if (m_currentAnim != null) StopCoroutine(m_currentAnim);

        m_currentAnim = StartCoroutine(IAnimateToPlace(p_betTransform, l_tempBetAnim, p_animType, p_isIncrease, p_action));

    }

    Vector3 l_tempPosition, l_initialPosition, l_tempRotation, l_initialRotation, l_tempScale, l_initialScale;
    IEnumerator IAnimateToPlace(BetTransform p_betTransform, BetAnimConfig p_animConfig, BetAnimType p_betState, bool p_isIncrease = false, Action<GameObject, bool> p_onFinishAnim = null)
    {
        m_currentState = p_betState;
        l_initialPosition = p_animConfig.UseLocalPosition ? transform.localPosition : transform.position;
        l_initialRotation = p_animConfig.UseLocalPosition ? transform.localRotation.eulerAngles : transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(l_initialRotation);
        l_initialScale = transform.localScale;

        for (float time = 0f; time < p_animConfig.AnimTime; time += Time.deltaTime)
        {
            float l_rotateTValue = p_animConfig.RotationAnimCurve.Evaluate(time / p_animConfig.AnimTime);
            l_tempRotation.x = Mathf.LerpAngle(l_initialRotation.x, p_betTransform.Rotation.x, l_rotateTValue);
            l_tempRotation.y = Mathf.LerpAngle(l_initialRotation.y, p_betTransform.Rotation.y, l_rotateTValue);
            l_tempRotation.z = Mathf.LerpAngle(l_initialRotation.z, p_betTransform.Rotation.z, l_rotateTValue);

            l_tempPosition = Vector3.LerpUnclamped(l_initialPosition, p_betTransform.Position, p_animConfig.MoveAnimCurve.Evaluate(time / p_animConfig.AnimTime));
            l_tempPosition.z += Mathf.Lerp(0, p_animConfig.betZPump, p_animConfig.ZPumpCurve.Evaluate(time / p_animConfig.AnimTime));

            //l_tempScale = Vector3.Lerp(l_initialScale, p_betTransform.Scale, l_rotateTValue);

            if (p_animConfig.UseLocalPosition)
            {
                transform.localPosition = l_tempPosition;
                transform.localRotation = Quaternion.Euler(l_tempRotation);
                //transform.localScale = l_tempScale;
            }
            else
            {
                transform.position = l_tempPosition;
                transform.rotation = Quaternion.Euler(l_tempRotation);
                //transform.localScale = l_tempScale;
            }

            yield return null;
        }

        p_onFinishAnim?.Invoke(gameObject, p_isIncrease);
        m_currentAnim = null;
    }

    bool m_dragging;
    private Vector3 l_startMousePos;
    public void StartDrag(Vector3 p_mousePos)
    {
        m_dragging = true;
        m_currentState = BetAnimType.DRAG;
    }

    private Vector3 l_tempDragPos;
    public void DragBet(Vector3 p_mousePosition, RaycastHit p_raycastHit)
    {
        if (!m_dragging) return;

        l_tempDragPos = p_raycastHit.point;

        transform.position = l_tempDragPos;
    }

    public void EndDrag()
    {
        m_dragging = false;
        AnimateToPlace(m_idleBetTranform, BetAnimType.IDLE);
    }

    public void Bet(bool p_isIncrease, Action<GameObject, bool> p_onFinishAnim, HandItemAnimController p_animController, bool p_isOwner = true)
    {
        StartCoroutine(AnimBet(p_isIncrease, p_onFinishAnim, p_animController, p_isOwner));
    }

    private IEnumerator AnimBet(bool p_isIncrease, Action<GameObject, bool> p_onFinishAnim, HandItemAnimController p_animController, bool p_isOwner)
    {
        p_animController.betHandAnimator.OnDeliveredButton += AddButtonOStack;

        bool l_waiting = true;

        StartCoroutine(p_animController.betHandAnimator.GetEyebutton(() => { l_waiting = false; }, p_isIncrease, p_isOwner));

        while (l_waiting) yield return null;

        p_onFinishAnim?.Invoke(gameObject, p_isIncrease);
    }

    CardsManager m_cardsManager;

    private void AddButtonOStack()
    {
        if (m_cardsManager == null)
        {
            m_cardsManager = CardsManager.Instance;
            m_cardsManager.ReturnEyes += ReturnButtons;
        }

        for (int i = 0; i < m_stackBets.Length; i++)
        {
            if (!m_stackBets[i].activeInHierarchy)
            {
                m_stackBets[i].SetActive(true);
                break;
            }
        }
    }

    public void HighlightBetButton()
    {
        if (m_currentAnim != null) StopCoroutine(m_currentAnim);

        //AudioManager.Instance.PlayOneShot(FMODEvents.Instance.HoverCard, transform.position);
        m_currentAnim = StartCoroutine(IAnimateToPlace(m_individualHighlightBetTranform, m_hoverBetAnim, BetAnimType.HIGHLIGHT));
    }

    public void HighlightOff()
    {
        if (m_currentState is BetAnimType.HIGHLIGHT)
            AnimateToPlace(m_idleBetTranform, BetAnimType.IDLE);
    }

    public void ReturnButtons(object p_ovject, EventArgs p_args)
    {
        Debug.Log("return eye");
        int id = (int)p_ovject;

        if (id == playerId)
        {
            StartCoroutine(AnimButtonsToPlayer());
        }
        else
        {
            DestroyButtons();
        }
    }

    [Space]
    public Vector3 endRotation;
    public List<Vector3> endPosition;
    private List<Vector3> startPosition;
    private List<Vector3> startRotation;
    public CardAnimConfig returnEyeAnimConfig;
    private IEnumerator AnimButtonsToPlayer()
    {
        startPosition = new();
        startRotation = new();
        for (int i = 0; i < m_stackBets.Length; i++)
        {
            if (m_stackBets[i].activeInHierarchy)
            {
                startPosition.Add(m_stackBets[i].transform.localPosition);
                startRotation.Add(m_stackBets[i].transform.rotation.eulerAngles);

                m_eyesPool[i].transform.localPosition = startPosition[^1];
                m_eyesPool[i].transform.rotation = Quaternion.Euler(startRotation[^1]);
                m_eyesPool[i].gameObject.SetActive(true);
                m_stackBets[i].SetActive(false);
            }
            else m_eyesPool[i].gameObject.SetActive(false);
        }


        float l_time = 0f;
        while (l_time < returnEyeAnimConfig.AnimTime)
        {
            float l_timeFraction = l_time / returnEyeAnimConfig.AnimTime;
            for (int i = 0; i < m_eyesPool.Length; i++)
            {
                if (!m_eyesPool[i].gameObject.activeInHierarchy) continue;

                l_tempPosition = Vector3.LerpUnclamped(startPosition[i], endPosition[i],
                                        returnEyeAnimConfig.MoveAnimCurve.Evaluate(l_timeFraction));
                l_tempPosition.y += returnEyeAnimConfig.CardYPump * returnEyeAnimConfig.YPumpCurve.Evaluate(l_timeFraction);

                m_eyesPool[i].localPosition = l_tempPosition;
                m_eyesPool[i].rotation = Quaternion.Euler(Vector3.LerpUnclamped(startRotation[i], endRotation,
                    returnEyeAnimConfig.RotationAnimCurve.Evaluate(l_timeFraction)));
                //print("start " + startRotation[i]);
                //print("cur " + m_eyesPool[i].rotation);
                //print("final " + endRotation);
            }

            yield return null;
            l_time += Time.deltaTime;
        }

        for (int i = 0; i < m_eyesPool.Length; i++) m_eyesPool[i].gameObject.SetActive(false);
    }

    [Header("desdtroy anim")]
    public AnimationCurve destroyStackAnimCurve;
    public float destroyStackAnimTime;
    public float initDestroyStack = 0f;
    public float finalDestroyStack = 45f;
    public void DestroyButtons()
    {
        StartCoroutine(IDestroyStack());
    }

    IEnumerator IDestroyStack()
    {
        float l_time = 0f;
        while (l_time < destroyStackAnimTime)
        {
            stackBetsMaterial.SetFloat("_Custom_hide",
                Mathf.Lerp(initDestroyStack, finalDestroyStack, destroyStackAnimCurve.Evaluate(l_time / destroyStackAnimTime)));

            yield return null;
            l_time += Time.deltaTime;
        }

        for (int i = 0; i < m_stackBets.Length; i++) m_stackBets[i].SetActive(false);

        stackBetsMaterial.SetFloat("_Custom_hide", 0f);
    }
}