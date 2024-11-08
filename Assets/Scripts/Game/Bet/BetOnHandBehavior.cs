using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BetOnHandBehavior : MonoBehaviour
{
    [SerializeField] private List<BetBehavior> m_betsBehavior;
    [SerializeField] private BetBehavior m_currentBet;
    [SerializeField] private RectTransform m_acceptTargetRect;
    [SerializeField] private RectTransform m_increaseTargetRect;
    [SerializeField] private BetTargetTag m_acceptBetTargetTag;
    [SerializeField] private BetTargetTag m_increaseBetTargetTag;
    PointerEventData m_pointerEventData;

    void Start()
    {
        m_pointerEventData = new PointerEventData(EventSystem.current);
    }

    public void OnPlayerSpawned(int p_playerId)
    {
        BetBehavior[] l_bets = FindObjectsOfType<BetBehavior>();
        BetTargetTag[] l_betTargetTag = FindObjectsOfType<BetTargetTag>();

        m_betsBehavior = new();

        foreach (BetBehavior bet in l_bets)
        {
            if (bet.playerId == p_playerId)
            {
                m_betsBehavior.Add(bet);
            }
        }

        foreach (BetTargetTag betTag in l_betTargetTag)
        {
            if (betTag.IsAccept)
            {
                m_acceptBetTargetTag = betTag;
                m_acceptTargetRect = m_acceptBetTargetTag.targetRect;
                m_acceptTargetRect.gameObject.SetActive(false);
            }
            else if (betTag.IsIncrease)
            {
                m_increaseBetTargetTag = betTag;
                m_increaseTargetRect = m_increaseBetTargetTag.targetRect;
                m_increaseTargetRect.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateMousePos(Vector3 p_mousePos)
    {
        if (m_currentBet != null)
        {
            m_currentBet.DragBet(p_mousePos);
        }
    }

    public bool CheckClickObject(GameObject p_gameObject)
    {
        bool l_isBet = false;

        if (p_gameObject != null)
        {
            for (int i = 0; i < m_betsBehavior.Count; i++)
            {
                if (m_betsBehavior[i].gameObject == p_gameObject)
                {
                    m_currentBet = m_betsBehavior[i];
                    m_currentBet.StartDrag(Input.mousePosition);
                    m_acceptTargetRect.gameObject.SetActive(true);
                    m_increaseTargetRect.gameObject.SetActive(true);

                    l_isBet = true;
                }
            }
        }

        return l_isBet;
    }

    public List<RaycastResult> m_resultList;
    public void CheckClickUp(bool p_canBet, Action<GameObject, bool> p_actionOnEndAnimation)
    {

        if (m_currentBet != null)
        {
            m_pointerEventData.position = Input.mousePosition;

            if (m_resultList == null) m_resultList = new List<RaycastResult>();
            else m_resultList.Clear();

            EventSystem.current.RaycastAll(m_pointerEventData, m_resultList);

            bool l_bet = false;
            Debug.Log(p_canBet);

            if (p_canBet)
            {
                for (int i = 0; i < m_resultList.Count; i++)
                {

                Debug.Log(m_resultList[i].gameObject.name);
                    if (m_resultList[i].gameObject == m_acceptTargetRect.gameObject)
                    {
                        Bet(true, m_currentBet, p_actionOnEndAnimation);
                        l_bet = true;
                        m_acceptTargetRect.gameObject.SetActive(false);
                        m_increaseTargetRect.gameObject.SetActive(false);
                        break;
                    }
                    else if (m_resultList[i].gameObject == m_increaseTargetRect.gameObject)
                    {
                        Bet(false, m_currentBet, p_actionOnEndAnimation);
                        l_bet = true;
                        m_acceptTargetRect.gameObject.SetActive(false);
                        m_increaseTargetRect.gameObject.SetActive(false);
                        break;
                    }
                }
            }


            if (!l_bet)
            {
                m_currentBet.EndDrag();
                m_currentBet = null;
            }
        }

    }

    private void Bet(bool p_isIncrease, BetBehavior p_myBet, Action<GameObject, bool> p_action)
    {
        Debug.Log("bet");
        p_myBet.AnimateToPlace(p_isIncrease, p_action);
    }
}
