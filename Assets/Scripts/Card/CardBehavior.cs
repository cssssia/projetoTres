using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CardTransform
{
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;

    public CardTransform() { }
    public CardTransform(Vector3 p_position, Vector3 p_rotation, Vector3 p_scale)
    {
        Position = p_position;
        Rotation = p_rotation;
        Scale = p_scale;
    }
    public CardTransform(CardTransform p_cardTransform)
    {
        Position = p_cardTransform.Position;
        Rotation = p_cardTransform.Rotation;
        Scale = p_cardTransform.Scale;
    }
}

public enum CardAnimType { PLAY, IDLE, HIGHLIGHT, DRAG, DEAL }

public class CardBehavior : MonoBehaviour
{
    private CardAnimType m_currentState = CardAnimType.IDLE;
    public CardAnimType CurrentState { get { return m_currentState; } }

    [Header("Card Transform")]
    [SerializeField] private CardTransform m_idleCardTranform;
    [SerializeField] private CardTransform m_highlightCardTranform;
    [SerializeField] private CardTransform m_individualHighlightCardTranform;

    [Header("Card AnimConfig")]
    [SerializeField] private CardAnimConfig m_playCardAnim;
    [SerializeField] private CardAnimConfig m_idleCardAnim;
    [SerializeField] private CardAnimConfig m_hoverCardAnim;
    [SerializeField] private CardAnimConfig m_dealCardAnim;
    [SerializeField] private Vector3 m_dragOffset;
    [SerializeField] private Transform m_dragAnchor;

    [Header("Card Data")]
    public Card card;
    public Item item;

    private Vector3 m_startPosition;
    private Vector3 m_startRotation;

    public Action OnDestroyAction;

    private IEnumerator Start()
    {
        yield return null;
        m_startPosition = transform.position;
        m_startRotation = transform.rotation.eulerAngles;
    }

    private void OnDestroy()
    {
        OnDestroyAction?.Invoke();
    }

    public void SetCardData(Card p_card)
    {
        card = p_card;
        item = null;
    }

    public void SetCardData(Item p_item)
    {
        card = null;
        item = p_item;
    }

    public void SetIdleTransform(CardTransform p_cardTransform, bool l_invertZ)
    {
        m_idleCardTranform = new(p_cardTransform);

        m_individualHighlightCardTranform = new();
        m_highlightCardTranform.Position.z *= l_invertZ ? -1 : 1;
        m_individualHighlightCardTranform.Position = m_idleCardTranform.Position + m_highlightCardTranform.Position;
        m_individualHighlightCardTranform.Rotation = m_idleCardTranform.Rotation + m_highlightCardTranform.Rotation;
        m_individualHighlightCardTranform.Scale = m_highlightCardTranform.Scale;
    }

    public void ResetTransform()
    {
        transform.position = m_startPosition;
        transform.rotation = Quaternion.Euler(m_startRotation);
    }

    CardAnimConfig l_tempCardAnim;
    Coroutine m_currentAnim;
    public Coroutine AnimateToPlace(CardTransform p_cardTransform, CardAnimType p_animType, Action<GameObject> p_action = null)
    {
        switch (p_animType)
        {
            case CardAnimType.PLAY:
                l_tempCardAnim = m_playCardAnim;
                break;
            case CardAnimType.IDLE:
                l_tempCardAnim = m_idleCardAnim;
                break;
            case CardAnimType.HIGHLIGHT:
                l_tempCardAnim = m_hoverCardAnim;
                break;
            case CardAnimType.DEAL:
                l_tempCardAnim = m_dealCardAnim;
                break;
        }

        if (m_currentAnim != null) StopCoroutine(m_currentAnim);

        m_currentAnim = StartCoroutine(IAnimateToPlace(p_cardTransform, l_tempCardAnim, p_animType, p_action));
        return m_currentAnim;
    }

    public void HighlightCard()
    {
        if (m_currentAnim != null) StopCoroutine(m_currentAnim);

        m_currentAnim = StartCoroutine(IAnimateToPlace(m_individualHighlightCardTranform, m_hoverCardAnim, CardAnimType.HIGHLIGHT));
    }

    public void HighlightOff()
    {
        if (m_currentState is CardAnimType.HIGHLIGHT)
            AnimateToPlace(m_idleCardTranform, CardAnimType.IDLE);
    }

    public Vector3 Rotation;
    public Vector3 LocalRotation;
    private void Update()
    {
        Rotation = transform.rotation.eulerAngles;
        LocalRotation = transform.localRotation.eulerAngles;
    }

    public Coroutine AnimToIdlePos(CardAnimType p_animType = CardAnimType.IDLE, Action<GameObject> p_action = null)
    {
        Debug.Log($"Anim card {gameObject.name}");
        return AnimateToPlace(m_idleCardTranform, p_animType, p_action);
    }

    public Vector3 playerPosition;
    Vector3 l_tempPosition, l_initialPosition, l_tempScale, l_initialScale;
    Quaternion l_initialQuat, l_tempQuat, l_finalQuat;
    IEnumerator IAnimateToPlace(CardTransform p_cardTransform, CardAnimConfig p_animConfig, CardAnimType p_cardState, Action<GameObject> p_onFinishAnim = null)
    {
        m_currentState = p_cardState;
        l_initialPosition = p_animConfig.UseLocalPosition ? transform.localPosition : transform.position;
        l_initialQuat = p_animConfig.UseLocalPosition ? transform.localRotation : transform.rotation;
        l_tempQuat = l_initialQuat;
        l_finalQuat = Quaternion.Euler(p_cardTransform.Rotation);

        l_initialScale = transform.localScale;

        for (float time = 0f; time < p_animConfig.AnimTime; time += Time.deltaTime)
        {
            float l_rotateTValue = p_animConfig.RotationAnimCurve.Evaluate(time / p_animConfig.AnimTime);
            l_tempQuat = Quaternion.Lerp(l_initialQuat, l_finalQuat, l_rotateTValue);

            l_tempPosition = Vector3.LerpUnclamped(l_initialPosition, p_cardTransform.Position, p_animConfig.MoveAnimCurve.Evaluate(time / p_animConfig.AnimTime));
            l_tempPosition.y += Mathf.Lerp(0, p_animConfig.CardYPump, p_animConfig.YPumpCurve.Evaluate(time / p_animConfig.AnimTime));

            l_tempScale = Vector3.Lerp(l_initialScale, p_cardTransform.Scale, l_rotateTValue);

            if (p_animConfig.UseLocalPosition)
            {
                transform.localPosition = l_tempPosition;
                transform.localRotation = l_tempQuat;
                transform.localScale = l_tempScale;
            }
            else
            {
                transform.position = l_tempPosition;
                transform.rotation = l_tempQuat;
                transform.localScale = l_tempScale;
            }

            yield return null;
        }

        l_tempQuat = Quaternion.Lerp(l_initialQuat, l_finalQuat, 1f);
        l_tempPosition = Vector3.LerpUnclamped(l_initialPosition, p_cardTransform.Position, p_animConfig.MoveAnimCurve.Evaluate(1f));
        l_tempPosition.y += Mathf.Lerp(0, p_animConfig.CardYPump, p_animConfig.YPumpCurve.Evaluate(1f));
        l_tempScale = Vector3.Lerp(l_initialScale, p_cardTransform.Scale, 1f);

        if (p_animConfig.UseLocalPosition)
        {
            transform.localPosition = l_tempPosition;
            transform.localRotation = l_tempQuat;
            transform.localScale = l_tempScale;
        }
        else
        {
            transform.position = l_tempPosition;
            transform.rotation = l_tempQuat;
            transform.localScale = l_tempScale;
        }

        p_onFinishAnim?.Invoke(gameObject);
        m_currentAnim = null;
    }

    private Vector3 l_startMousePos;
    public void StartDrag(Vector3 p_mousePos)
    {
        transform.localScale = Vector3.one * 0.075f;

        l_startMousePos = p_mousePos - Camera.main.WorldToScreenPoint(transform.position);
    }


    private Vector3 l_tempDragPos;
    public void DragCard(Vector3 p_mousePosition)
    {
        m_currentState = CardAnimType.DRAG;

        l_tempDragPos = Camera.main.ScreenToWorldPoint(p_mousePosition - l_startMousePos);
        l_tempDragPos.z = transform.position.z;

        transform.position = l_tempDragPos;
    }

    public void EndDrag()
    {
        AnimateToPlace(m_idleCardTranform, CardAnimType.IDLE);
    }

    public void PlayCard(CardTransform p_targetTransform, Action<GameObject> p_onFinishAnim)
    {
        AnimateToPlace(p_targetTransform, CardAnimType.PLAY, p_onFinishAnim);
    }
}
