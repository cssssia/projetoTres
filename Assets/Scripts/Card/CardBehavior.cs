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

    public CardTransform(Vector3 p_position, Vector3 p_rotation)
    {
        Position = p_position;
        Rotation = p_rotation;
    }
    public CardTransform()
    {
    }

    public CardTransform(CardTransform p_cardTransform)
    {
        Position = p_cardTransform.Position;
        Rotation = p_cardTransform.Rotation;
    }
}

public enum CardAnimType { PLAY, IDLE, HOVER }

public class CardBehavior : MonoBehaviour
{
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

    public void SetIdleTransform(CardTransform p_cardTransform)
    {
        m_idleCardTranform = new(p_cardTransform);

        m_individualHighlightCardTranform = new();
        m_individualHighlightCardTranform.Position = m_idleCardTranform.Position + m_highlightCardTranform.Position;
        m_individualHighlightCardTranform.Rotation = m_idleCardTranform.Rotation + m_highlightCardTranform.Rotation;
    }

    public void ResetTransform()
    {
        transform.position = m_startPosition;
        transform.rotation = Quaternion.Euler(m_startRotation);
    }

    CardAnimConfig l_tempCardAnim;
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
            case CardAnimType.HOVER:
                l_tempCardAnim = m_hoverCardAnim;
                break;
        }

        StartCoroutine(IAnimateToPlace(p_cardTransform, l_tempCardAnim));
    }

    public void HighlightCard()
    {
        StartCoroutine(IAnimateToPlace(m_individualHighlightCardTranform, m_hoverCardAnim));
    }

    Vector3 l_tempPosition;
    Vector3 l_initialPosition;
    Vector3 l_tempRotation;
    Vector3 l_initialRotation;
    IEnumerator IAnimateToPlace(CardTransform p_cardTransform, CardAnimConfig p_animConfig)
    {
        l_initialPosition = p_animConfig.UseLocalPosition ? transform.localPosition : transform.position;
        l_initialRotation = p_animConfig.UseLocalPosition ? transform.localRotation.eulerAngles : transform.rotation.eulerAngles;

        for (float time = 0f; time < p_animConfig.AnimTime; time += Time.deltaTime)
        {
            l_tempRotation.x = Mathf.LerpAngle(l_initialRotation.x, p_cardTransform.Rotation.x, p_animConfig.RotationAnimCurve.Evaluate(time / p_animConfig.AnimTime));
            l_tempRotation.y = Mathf.LerpAngle(l_initialRotation.y, p_cardTransform.Rotation.y, p_animConfig.RotationAnimCurve.Evaluate(time / p_animConfig.AnimTime));
            l_tempRotation.z = Mathf.LerpAngle(l_initialRotation.z, p_cardTransform.Rotation.z, p_animConfig.RotationAnimCurve.Evaluate(time / p_animConfig.AnimTime));

            l_tempPosition = Vector3.LerpUnclamped(l_initialPosition, p_cardTransform.Position, p_animConfig.MoveAnimCurve.Evaluate(time / p_animConfig.AnimTime));
            l_tempPosition += Vector3.one * Mathf.Lerp(0, p_animConfig.CardYPump, p_animConfig.YPumpCurve.Evaluate(time / p_animConfig.AnimTime));

            if (p_animConfig.UseLocalPosition)
            {
                transform.localPosition = l_tempPosition;
                transform.localRotation = Quaternion.Euler(l_tempRotation);
            }
            else
            {
                transform.position = l_tempPosition;
                transform.rotation = Quaternion.Euler(l_tempRotation);
            }

            yield return null;

        }
    }
}
