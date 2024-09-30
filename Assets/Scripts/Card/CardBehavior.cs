using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class CardTransform
{
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;

    public CardTransform(Vector3 p_position, Vector3 p_rotation, Vector3 p_scale)
    {
        Position = p_position;
        Rotation = p_rotation;
        Scale = p_scale;
    }
    public CardTransform()
    {
    }

    public CardTransform(CardTransform p_cardTransform)
    {
        Position = p_cardTransform.Position;
        Rotation = p_cardTransform.Rotation;
        Scale = p_cardTransform.Scale;
    }
}

public enum CardAnimType { PLAY, IDLE, HIGHLIGHT }

public class CardBehavior : MonoBehaviour
{
    private CardAnimType m_currentState = CardAnimType.IDLE;
    [SerializeField] private CardTransform m_idleCardTranform;
    [SerializeField] private CardTransform m_highlightCardTranform;
    [SerializeField] private CardTransform m_individualHighlightCardTranform;

    [SerializeField] private CardAnimConfig m_playCardAnim;
    [SerializeField] private CardAnimConfig m_idleCardAnim;
    [SerializeField] private CardAnimConfig m_hoverCardAnim;
    private Vector3 m_startPosition;
    private Vector3 m_startRotation;

    private void Start()
    {
        m_startPosition = transform.position;
        m_startRotation = transform.rotation.eulerAngles;
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
    public void AnimateToPlace(CardTransform p_cardTransform, CardAnimType p_animType)
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
        }

        if (m_currentAnim != null) StopCoroutine(m_currentAnim);

        m_currentAnim = StartCoroutine(IAnimateToPlace(p_cardTransform, l_tempCardAnim, p_animType));
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

    Vector3 l_tempPosition, l_initialPosition, l_tempRotation, l_initialRotation, l_tempScale, l_initialScale;
    IEnumerator IAnimateToPlace(CardTransform p_cardTransform, CardAnimConfig p_animConfig, CardAnimType p_cardState)
    {
        m_currentState = p_cardState;
        l_initialPosition = p_animConfig.UseLocalPosition ? transform.localPosition : transform.position;
        l_initialRotation = p_animConfig.UseLocalPosition ? transform.localRotation.eulerAngles : transform.rotation.eulerAngles;
        l_initialScale = transform.localScale;

        for (float time = 0f; time < p_animConfig.AnimTime; time += Time.deltaTime)
        {
            float l_rotateTValue = p_animConfig.RotationAnimCurve.Evaluate(time / p_animConfig.AnimTime);
            l_tempRotation.x = Mathf.LerpAngle(l_initialRotation.x, p_cardTransform.Rotation.x, l_rotateTValue);
            l_tempRotation.y = Mathf.LerpAngle(l_initialRotation.y, p_cardTransform.Rotation.y, l_rotateTValue);
            l_tempRotation.z = Mathf.LerpAngle(l_initialRotation.z, p_cardTransform.Rotation.z, l_rotateTValue);

            l_tempPosition = Vector3.LerpUnclamped(l_initialPosition, p_cardTransform.Position, p_animConfig.MoveAnimCurve.Evaluate(time / p_animConfig.AnimTime));
            l_tempPosition += Vector3.one * Mathf.Lerp(0, p_animConfig.CardYPump, p_animConfig.YPumpCurve.Evaluate(time / p_animConfig.AnimTime));

            l_tempScale = Vector3.Lerp(l_initialScale, p_cardTransform.Scale, l_rotateTValue);

            if (p_animConfig.UseLocalPosition)
            {
                transform.localPosition = l_tempPosition;
                transform.localRotation = Quaternion.Euler(l_tempRotation);
                transform.localScale = l_tempScale;
            }
            else
            {
                transform.position = l_tempPosition;
                transform.rotation = Quaternion.Euler(l_tempRotation);
                transform.localScale = l_tempScale;
            }

            yield return null;
        }

        m_currentAnim = null;
    }

    private Vector3 l_tempDragPos;
    public void DragCard(Vector3 p_mousePosition)
    {
        l_tempDragPos = Camera.main.ScreenToWorldPoint(p_mousePosition);
        l_tempDragPos.z = transform.position.z;
        transform.position = l_tempDragPos;
    }

    public void EndDrag()
    {
        AnimateToPlace(m_idleCardTranform, CardAnimType.IDLE);
    }
}
