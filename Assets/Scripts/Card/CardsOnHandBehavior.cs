using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsOnHandBehavior : MonoBehaviour
{
    [SerializeField] private CardBehavior[] m_cardsBehavior;

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
                CardTransform l_transform = new(l_newPos, new Vector3(270f, 0, 0));

                m_cardsBehavior[i].SetIdleTransform(l_transform);
                if (p_alsoSetPosition) m_cardsBehavior[i].transform.localPosition = l_newPos;
            }
        }
    }

    CardBehavior currentHighlitedCard;
    public bool CheckObject(GameObject p_gameObject)
    {
        for (int i = 0; i < m_cardsBehavior.Length; i++)
        {
            if (m_cardsBehavior[i].gameObject == p_gameObject)
            {
                m_cardsBehavior[i].HighlightCard();
                return true;
            }
        }

        return false;
    }
}
