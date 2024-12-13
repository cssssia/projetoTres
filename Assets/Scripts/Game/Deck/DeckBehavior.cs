using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class DeckTransform
{
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;

    public DeckTransform() { }
    public DeckTransform(Vector3 p_position, Vector3 p_scale)
    {
        Position = p_position;
        Scale = p_scale;
    }
    public DeckTransform(DeckTransform p_deckTransform)
    {
        Position = p_deckTransform.Position;
        Scale = p_deckTransform.Scale;
    }
}

public enum DeckAnimType { IDLE, HIGHLIGHT }
public class DeckBehavior : MonoBehaviour
{
    private DeckAnimType m_currentState = DeckAnimType.IDLE;
    public DeckAnimType CurrentState { get { return m_currentState; } }
    [SerializeField] private DeckAnimConfig m_idleDeckAnim;
    [SerializeField] private DeckTransform m_idleDeckTranform;
    [Space]
    [SerializeField] private DeckAnimConfig m_hoverDeckAnim;
    [SerializeField] private DeckTransform m_highlightDeckTranform;
    public void GiveUp(Action<GameObject> p_onFinishAnim)
    {
        Debug.Log("[GAME] GiveUp");
        p_onFinishAnim?.Invoke(gameObject);
    }

    DeckAnimConfig l_tempCardAnim;
    Coroutine m_currentAnim;
    public Coroutine AnimateToPlace(DeckTransform p_cardTransform, DeckAnimType p_animType)
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

        m_currentAnim = StartCoroutine(IAnimateToPlace(p_cardTransform, l_tempCardAnim, p_animType));
        return m_currentAnim;
    }

    [NaughtyAttributes.Button]
    public void HighlightDeck()
    {
        if (m_currentAnim != null) StopCoroutine(m_currentAnim);

        m_currentAnim = StartCoroutine(IAnimateToPlace(m_highlightDeckTranform, m_hoverDeckAnim, DeckAnimType.HIGHLIGHT,
                                        ()=> { AudioManager.Instance.PlayOneShot(FMODEvents.Instance.HoverCard, transform.position); }));
       
    }

    [NaughtyAttributes.Button]
    public void HighlightOff()
    {
        if (m_currentState is DeckAnimType.HIGHLIGHT) AnimateToPlace(m_idleDeckTranform, DeckAnimType.IDLE);
    }

    public Vector3 playerPosition;
    Vector3 l_tempPosition, l_initialPosition, l_tempScale, l_initialScale;

    IEnumerator IAnimateToPlace(DeckTransform p_cardTransform, DeckAnimConfig p_animConfig,     
                            DeckAnimType p_cardState, Action p_onFinishAnim = null)
    {
        m_currentState = p_cardState;
        l_initialPosition = transform.localPosition;

        l_initialScale = transform.localScale;

        for (float time = 0f; time < p_animConfig.AnimTime; time += Time.deltaTime)
        {
            l_tempPosition = Vector3.LerpUnclamped(l_initialPosition, p_cardTransform.Position, p_animConfig.MoveAnimCurve.Evaluate(time / p_animConfig.AnimTime));
            l_tempScale = Vector3.LerpUnclamped(l_initialScale, p_cardTransform.Scale, p_animConfig.MoveAnimCurve.Evaluate(time / p_animConfig.AnimTime));

            transform.localPosition = l_tempPosition;
            transform.localScale = l_tempScale;

            yield return null;
        }

        l_tempPosition = Vector3.LerpUnclamped(l_initialPosition, p_cardTransform.Position, p_animConfig.MoveAnimCurve.Evaluate(1f));
        l_tempScale = Vector3.Lerp(l_initialScale, p_cardTransform.Scale, 1f);

        transform.localPosition = l_tempPosition;
        transform.localScale = l_tempScale;

        p_onFinishAnim?.Invoke();
        m_currentAnim = null;
    }

}