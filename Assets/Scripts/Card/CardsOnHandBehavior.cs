using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private void Start()
    {
        SetCardsIdlePosition(true);
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
        bool l_isCard = false;
        if (p_gameObject != null)
        {
            for (int i = 0; i < m_cardsBehavior.Length; i++)
            {
                if (m_cardsBehavior[i].gameObject == p_gameObject)
                {
                    if (m_cardsBehavior[i] != m_currentHoverCard)
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
        if(m_currentHoldingCard != null)
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
                    m_throwCardTargetImage.gameObject.SetActive(true);

                    l_isCard = true;
                }
            }
        }

        return l_isCard;
    }


    public void CheckClickUp()
    {
        if (m_currentHoldingCard != null)
        {
            //Ray l_ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //RaycastHit[] l_hit = Physics.RaycastAll(l_ray, out )

            m_currentHoldingCard.EndDrag();
            m_currentHoldingCard = null;
        }
    }
}
