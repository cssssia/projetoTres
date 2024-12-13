using System;
using UnityEngine;

[Serializable]
public class DeckTransform
{
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;

    public DeckTransform() { }
    public DeckTransform(Vector3 p_position, Vector3 p_rotation, Vector3 p_scale)
    {
        Position = p_position;
        Rotation = p_rotation;
        Scale = p_scale;
    }
    public DeckTransform(DeckTransform p_deckTransform)
    {
        Position = p_deckTransform.Position;
        Rotation = p_deckTransform.Rotation;
        Scale = p_deckTransform.Scale;
    }
}

public enum DeckAnimType { IDLE, HIGHLIGHT }
public class DeckBehavior : MonoBehaviour
{
    private DeckAnimType m_currentState = DeckAnimType.IDLE;
    public DeckAnimType CurrentState { get { return m_currentState; } }
    [SerializeField] private DeckAnimConfig m_idleDeckAnim;
    [SerializeField] private DeckAnimConfig m_hoverDeckAnim;
    [SerializeField] private DeckTransform m_idleDeckTranform;
    [SerializeField] private DeckTransform m_individualHighlightDeckTranform;
    public void GiveUp(Action<GameObject> p_onFinishAnim)
    {
        Debug.Log("[GAME] GiveUp");
        p_onFinishAnim?.Invoke(gameObject);
    }

    DeckAnimConfig l_tempCardAnim;
    Coroutine m_currentAnim;
    public Coroutine AnimateToPlace(DeckTransform p_cardTransform, DeckAnimType p_animType, Action<GameObject> p_action = null)
    {
        switch (p_animType)
        {
            case DeckAnimType.IDLE:
                l_tempCardAnim = m_idleDeckAnim;
                break;
            case DeckAnimType.HIGHLIGHT:
                l_tempCardAnim = m_hoverDeckAnim;
                break;
        }

        if (m_currentAnim != null) StopCoroutine(m_currentAnim);

        m_currentAnim = StartCoroutine(IAnimateToPlace(p_cardTransform, l_tempCardAnim, p_animType, p_action));
        return m_currentAnim;
    }

    public void HighlightCard()
    {
        if (m_currentAnim != null) StopCoroutine(m_currentAnim);

        m_currentAnim = StartCoroutine(IAnimateToPlace(m_individualHighlightDeckTranform, m_hoverDeckAnim, DeckAnimType.HIGHLIGHT));
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.HoverCard, transform.position);
    }

    public void HighlightOff()
    {
        if (m_currentState is DeckAnimType.HIGHLIGHT)
            AnimateToPlace(m_idleDeckTranform, DeckAnimType.IDLE);
    }

    IEnumerator IAnimateToPlace(DeckTransform p_cardTransform, DeckAnimConfig p_animConfig, DeckAnimType p_cardState, Action<GameObject> p_onFinishAnim = null)
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

}