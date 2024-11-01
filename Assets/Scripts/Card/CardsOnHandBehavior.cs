using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardsOnHandBehavior : MonoBehaviour
{
    [SerializeField] private bool m_invertZPlayer;
    [SerializeField] private List<CardBehavior> m_cardsBehavior;
    private CardBehavior m_currentHoverCard;
    private CardBehavior m_currentHoldingCard;
    [SerializeField] private Image m_throwCardTargetImage;
    [SerializeField] private CardThrowTargetTag m_throwCardThrowTargetTag;

    [Header("Idle")]
    [SerializeField] private Vector3 m_idleScale;
    [SerializeField] private float m_handWidth;
    [SerializeField] private int m_cardsQuantity = 3;

    [Header("Target")]
    [SerializeField] private List<Transform> m_targets;
    [SerializeField] private CardTransform[] m_targetsTransform;
    [SerializeField] private int m_currentTargetIndex;
    PointerEventData m_pointerEventData;
    private void Start()
    {
        //SetCardsIdlePosition(true);
        m_pointerEventData = new PointerEventData(EventSystem.current);
    }

    public void OnPlayerSpawned()
    {
        m_throwCardThrowTargetTag = FindObjectOfType<CardThrowTargetTag>();
        print(m_throwCardThrowTargetTag == null);
        m_throwCardTargetImage = m_throwCardThrowTargetTag.targetImage;
        m_throwCardTargetImage.gameObject.SetActive(false);

    }

    CardBehavior l_card;
    public void AddCardOnHand(NetworkObject p_cardNetworkObject, bool p_lastCard)
    {
        p_cardNetworkObject.TryGetComponent(out l_card);

        if (m_cardsBehavior == null) m_cardsBehavior = new();
        m_cardsBehavior.Add(l_card);

        if (p_lastCard)
        {
            SetCardsIdlePosition(true);
        }
    }

    public void GetCardsAnim(int p_cardIndex, Action p_eventToCallForEachCard)
    {
        m_cardsBehavior[p_cardIndex].AnimToIdlePos(
            delegate
            {
                p_eventToCallForEachCard.Invoke();
            });
    }

    [NaughtyAttributes.Button]
    public void DEBUG_SetCardsPOs()
    {
        SetCardsIdlePosition(true);
    }

    public void SetCardsIdlePosition(bool p_alsoSetPosition)
    {
        for (int i = 0; i < m_cardsBehavior.Count; i++)
        {
            bool l_useCard = m_cardsQuantity > i;
            m_cardsBehavior[i].gameObject.SetActive(l_useCard);
            if (!l_useCard) return;
        }

        if (m_cardsQuantity == 1)
        {
            if (p_alsoSetPosition) m_cardsBehavior[0].transform.localPosition = Vector3.zero;
        }
        else if (m_cardsQuantity > 1)
        {
            for (int i = 0; i < m_cardsBehavior.Count; i++)
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
            for (int i = 0; i < m_cardsBehavior.Count; i++)
            {
                if (m_cardsBehavior[i] != null && m_cardsBehavior[i].gameObject == p_gameObject)
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
            for (int i = 0; i < m_cardsBehavior.Count; i++)
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
            for (int i = 0; i < m_cardsBehavior.Count; i++)
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
    public void CheckClickUp(bool p_canPlay, Action<GameObject> p_actionOnEndAnimation)
    {
        if (m_currentHoldingCard != null)
        {
            m_pointerEventData.position = Input.mousePosition;

            if (m_resultList == null) m_resultList = new List<RaycastResult>();
            else m_resultList.Clear();

            EventSystem.current.RaycastAll(m_pointerEventData, m_resultList);

            bool l_playCard = false;

            if (p_canPlay)
            {
                for (int i = 0; i < m_resultList.Count; i++)
                {
                    if (m_resultList[i].gameObject == m_throwCardTargetImage.gameObject)
                    {
                        PlayCard(m_currentHoldingCard, p_actionOnEndAnimation);
                        l_playCard = true;
                        m_throwCardTargetImage.gameObject.SetActive(false);
                        break;
                    }
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

    public void ResetTargetIndex()
    {
        m_currentTargetIndex = 0;
    }

    public void AddTarget(Transform p_target, int p_targetIndex)
    {
        if (m_targets == null || m_targets.Count < 3)
        {
            m_targets = new List<Transform>();
            for (int i = 0; i < 3; i++) m_targets.Add(null);
        }

        m_targets[p_targetIndex] = p_target;
    }

    private void PlayCard(CardBehavior cardBehavior, Action<GameObject> p_action)
    {
        cardBehavior.PlayCard(GetNextCardTarget(), p_action);
        m_currentHoldingCard = null;
        m_cardsBehavior.Remove(cardBehavior);
        m_currentTargetIndex++;
    }

    private CardTransform GetNextCardTarget()
    {
        if (m_targetsTransform.Length < m_targets.Count)
        {
            m_targetsTransform = new CardTransform[m_targets.Count];

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


    /*
        ---- [TEST SECTION] ----
        ---- [TEST SECTION] ----
        ---- [TEST SECTION] ----
    */

    public void RemoveAllCardsFromHandBehavior()
    {
        m_cardsBehavior.Clear();
    }

}
