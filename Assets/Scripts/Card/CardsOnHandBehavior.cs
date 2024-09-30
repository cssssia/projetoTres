using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardsOnHandBehavior : MonoBehaviour
{
    [SerializeField] private bool m_invertZPlayer;
    [SerializeField] private Vector3 m_idleScale;
    [SerializeField] private CardBehavior[] m_cardsBehavior;
    [SerializeField] private CardBehavior m_currentHoverCard;
    [SerializeField] private CardBehavior m_currentHoldingCard;
    [SerializeField] private Image m_throwCardTargetImage;

    [SerializeField] private float m_handWidth;
    [SerializeField] private int m_cardsQuantity = 3;

    [Header("Target")]
    [SerializeField] private Transform[] m_targets;
    [SerializeField] private CardTransform[] m_targetsTransform;
    [SerializeField] private int m_currentTargetIndex;
    PointerEventData m_pointerEventData;
    private void Start()
    {
        SetCardsIdlePosition(true);
        m_pointerEventData = new PointerEventData(EventSystem.current);
    }

    public void SetCardsIdlePosition(bool p_alsoSetPosition)
    {
        for (int i = 0; i < m_cardsBehavior.Length; i++)
        {
            bool l_useCard = m_cardsQuantity > i;
            m_cardsBehavior[i].gameObject.SetActive(l_useCard);
            if (!l_useCard) return;
        }

        if (m_cardsQuantity == 1)
        {
            m_cardsBehavior[0].transform.localPosition = Vector3.zero;
        }
        else if (m_cardsQuantity > 1)
        {
            for (int i = 0; i < m_cardsBehavior.Length; i++)
            {
                if (m_cardsQuantity < i) return;

                float l_axisPosition = (m_handWidth / (float)m_cardsQuantity) * i - m_handWidth / 2f;

                Vector3 l_newPos = new Vector3(l_axisPosition, 0, -0.01f * i);
                CardTransform l_transform = new(l_newPos, new Vector3(270f, 0, 0), m_idleScale);

                m_cardsBehavior[i].SetIdleTransform(l_transform, m_invertZPlayer);
                if (p_alsoSetPosition) m_cardsBehavior[i].transform.localPosition = l_newPos;
            }
        }
    }

    public bool CheckHoverObject(GameObject p_gameObject)
    {
        if (m_currentHoldingCard != null) return false;

        bool l_isCard = false;
        if (p_gameObject != null)
        {
            for (int i = 0; i < m_cardsBehavior.Length; i++)
            {
                if (m_cardsBehavior[i].gameObject == p_gameObject)
                {
                    if (m_cardsBehavior[i] != m_currentHoverCard && m_cardsBehavior[i].CurrentState is not CardAnimType.PLAY)
                    {
                        m_currentHoverCard = m_cardsBehavior[i];
                        m_cardsBehavior[i].HighlightCard();
                    }

                    l_isCard = true;
                }
            }
        }

        if (!l_isCard)
        {
            if (m_currentHoverCard != null)
            {
                m_currentHoverCard.HighlightOff();
                m_currentHoverCard = null;
            }
        }
        else
        {
            for (int i = 0; i < m_cardsBehavior.Length; i++)
            {
                if (m_cardsBehavior[i] != m_currentHoverCard) m_cardsBehavior[i].HighlightOff();
            }
        }

        return l_isCard;
    }

    public void UpdateMousePos(Vector3 p_mousePos)
    {
        if (m_currentHoldingCard != null)
        {
            m_currentHoldingCard.DragCard(p_mousePos);
        }
    }

    public bool CheckClickObject(GameObject p_gameObject)
    {
        bool l_isCard = false;
        if (p_gameObject != null)
        {
            for (int i = 0; i < m_cardsBehavior.Length; i++)
            {
                if (m_cardsBehavior[i].gameObject == p_gameObject)
                {
                    m_currentHoldingCard = m_cardsBehavior[i];
                    m_currentHoldingCard.StartDrag(Input.mousePosition);
                    m_throwCardTargetImage.gameObject.SetActive(true);

                    l_isCard = true;
                }
            }
        }

        return l_isCard;
    }

    List<RaycastResult> m_resultList;
    public void CheckClickUp()
    {
        if (m_currentHoldingCard != null)
        {
            m_pointerEventData.position = Input.mousePosition;

            if (m_resultList == null) m_resultList = new List<RaycastResult>();
            else m_resultList.Clear();

            EventSystem.current.RaycastAll(m_pointerEventData, m_resultList);

            bool l_playCard = false;
            for (int i = 0; i < m_resultList.Count; i++)
            {
                if (m_resultList[i].gameObject == m_throwCardTargetImage.gameObject)
                {
                    PlayCard(m_currentHoldingCard);
                    l_playCard = true;
                    m_throwCardTargetImage.gameObject.SetActive(false);
                    break;
                }
            }

            if (!l_playCard)
            {
                m_currentHoldingCard.EndDrag();
                m_currentHoldingCard = null;
            }

            m_throwCardTargetImage.gameObject.SetActive(false);
        }
    }

    private void PlayCard(CardBehavior cardBehavior)
    {
        cardBehavior.PlayCard(GetNextCardTarget(), delegate { print("finished anim"); });
        m_currentHoldingCard = null;
    }

    private CardTransform GetNextCardTarget()
    {
        if (m_targetsTransform.Length < m_targets.Length)
        {
            m_targetsTransform = new CardTransform[m_targets.Length];

            for (int i = 0; i < m_targetsTransform.Length; i++)
            {
                m_targetsTransform[i] = new
                    (m_targets[i].position,
                    m_targets[i].rotation.eulerAngles,
                    Vector3.one * 0.1f);
            }

        }

        return m_targetsTransform[m_currentTargetIndex];
    }
}
