using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BetOnHandBehavior : MonoBehaviour
{
    [SerializeField] private List<BetBehavior> m_betsBehavior;
    [SerializeField] private BetBehavior m_currentBet;
    [SerializeField] private RectTransform m_acceptTargetRect;
    [SerializeField] private RectTransform m_increaseTargetRect;
    [SerializeField] private BetTargetTag m_acceptBetTargetTag;
    [SerializeField] private BetTargetTag m_increaseBetTargetTag;

    [Header("Drag")]
    [SerializeField] private LayerMask m_tableLayer;
    [SerializeField] private LayerMask m_betLayer;

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
            if (betTag.playerId == p_playerId)
            {
                if (betTag.IsAccept)
                {
                    m_acceptBetTargetTag = betTag;
                    m_acceptTargetRect = m_acceptBetTargetTag.targetRect;
                }
                else if (betTag.IsIncrease)
                {
                    m_increaseBetTargetTag = betTag;
                    m_increaseTargetRect = m_increaseBetTargetTag.targetRect;
                }
                betTag.gameObject.SetActive(false);
            }
            else
                betTag.gameObject.SetActive(false);
        }
    }

    RaycastHit l_mousePosRaycastHit; Ray l_ray;
    public void UpdateMousePos(Vector3 p_mousePos)
    {
        l_ray = Camera.main.ScreenPointToRay(p_mousePos);


        if (m_currentBet != null)
        {
            if (Physics.Raycast(l_ray, out l_mousePosRaycastHit, 100f, m_tableLayer))
            {
                m_currentBet.DragBet(p_mousePos, l_mousePosRaycastHit);
            }
        }
        else
        {
            bool l_raycasted = Physics.Raycast(l_ray, out l_mousePosRaycastHit, 100f, m_betLayer);
            for (int i = 0; i < m_betsBehavior.Count; i++)
            {
                if (!l_raycasted) m_betsBehavior[i].HighlightOff();
                else m_betsBehavior[i].HighlightBetButton();
            }

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

                    if (RoundManager.Instance.BetHasStarted.Value)
                    {
                        m_currentBet.StartDrag(Input.mousePosition);
                        m_acceptTargetRect.gameObject.SetActive(true);
                        if (!RoundManager.Instance.StopIncreaseBet.Value) m_increaseTargetRect.gameObject.SetActive(true);
                    }

                    l_isBet = true;
                }
            }
        }

        return l_isBet;
    }

    public List<RaycastResult> m_resultList;
    public void CheckClickUp(bool p_canBet, Action<GameObject, bool> p_actionOnEndAnimation)
    {
        bool l_bet = false;

        if (m_currentBet != null)
        {
            l_ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (p_canBet)
            {
                if (!RoundManager.Instance.BetHasStarted.Value)
                {
                    Debug.Log("[GAME] BetHasStarted");
                    Bet(true, m_currentBet, p_actionOnEndAnimation);
                    l_bet = true;
                }

                if (Physics.Raycast(l_ray, out l_mousePosRaycastHit, 100f, m_betLayer))
                {
                    if (l_mousePosRaycastHit.transform.gameObject == m_acceptTargetRect.gameObject)
                    {
                        Bet(false, m_currentBet, p_actionOnEndAnimation);
                        l_bet = true;
                        m_acceptTargetRect.gameObject.SetActive(false);
                        m_increaseTargetRect.gameObject.SetActive(false);
                    }
                    else if (l_mousePosRaycastHit.transform.gameObject == m_increaseTargetRect.gameObject)
                    {
                        Bet(true, m_currentBet, p_actionOnEndAnimation);
                        l_bet = true;
                        m_acceptTargetRect.gameObject.SetActive(false);
                        m_increaseTargetRect.gameObject.SetActive(false);
                    }
                }
            }

            if (!l_bet)
            {
                m_currentBet.EndDrag();
                m_currentBet = null;
                m_acceptTargetRect.gameObject.SetActive(false);
                m_increaseTargetRect.gameObject.SetActive(false);
            }
        }

    }

    private void Bet(bool p_isIncrease, BetBehavior p_myBet, Action<GameObject, bool> p_action)
    {
        p_myBet.Bet(p_isIncrease, p_action);
        m_currentBet.EndDrag();
        m_currentBet = null;
    }
}
