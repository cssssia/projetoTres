using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BetOnHandBehavior : MonoBehaviour
{
    [SerializeField] private List<BetBehavior> m_betsBehavior;
    [SerializeField] private BetBehavior m_currentBet;
    public BetBehavior OtherBetBehavior;
    [SerializeField] private RectTransform m_acceptTargetRect;
    [SerializeField] private RectTransform m_increaseTargetRect;
    [SerializeField] private RectTransform m_initialBetTargetRect;
    [SerializeField] private BetTargetTag m_acceptBetTargetTag;
    [SerializeField] private BetTargetTag m_increaseBetTargetTag;
    [SerializeField] private BetTargetTag m_initialBetTargetTag;

    [Header("Drag")]
    [SerializeField] private LayerMask m_tableLayer;
    [SerializeField] private LayerMask m_betLayer;

    private HandItemAnimController m_handAnimController;
    public HandItemAnimController OtherHandAnimController;
    PlayerController m_player;
    public void OnPlayerSpawned(PlayerController p_player)
    {
        m_player = p_player;
        BetBehavior[] l_bets = FindObjectsOfType<BetBehavior>();
        BetTargetTag[] l_betTargetTag = FindObjectsOfType<BetTargetTag>();

        m_betsBehavior = new();

        foreach (BetBehavior bet in l_bets)
        {
            if (bet.playerId == m_player.PlayerIndex)
            {
                m_betsBehavior.Add(bet);
            }
            else
                OtherBetBehavior = bet;
        }

        foreach (BetTargetTag betTag in l_betTargetTag)
        {
            if (betTag.playerId == m_player.PlayerIndex)
            {
                if (betTag.IsAccept)
                {
                    betTag.text.text = Localization.Instance.Localize("Game.Accept");
                    m_acceptBetTargetTag = betTag;
                    m_acceptTargetRect = m_acceptBetTargetTag.targetRect;
                }
                else if (betTag.IsIncrease)
                {
                    betTag.text.text = Localization.Instance.Localize("Game.Increase");
                    m_increaseBetTargetTag = betTag;
                    m_increaseTargetRect = m_increaseBetTargetTag.targetRect;
                }
                else if (betTag.IsBet)
                {
                    betTag.text.text = Localization.Instance.Localize("Game.Bet");
                    m_initialBetTargetTag = betTag;
                    m_initialBetTargetRect = m_initialBetTargetTag.targetRect;
                }
                betTag.gameObject.SetActive(false);
            }
            else
                betTag.gameObject.SetActive(false);
        }

        m_player.OnChangedCanBet += OnOtherPlayerBet;
        FindHandAnimController(m_player);

    }
    void FindHandAnimController(PlayerController p_player)
    {
        var l_handAnimControllers = FindObjectsByType<HandItemAnimController>(default);

        for (int i = 0; i < l_handAnimControllers.Length; i++)
        {
            if (l_handAnimControllers[i].PlayerType == (Player)p_player.PlayerIndex)
            {
                m_handAnimController = l_handAnimControllers[i];
            }
            else
                OtherHandAnimController = l_handAnimControllers[i];
        }
    }
    public void OnOtherPlayerBet()
    {
        if (!m_player.CanBet || !RoundManager.Instance.BetHasStarted.Value) return;

        if (RoundManager.Instance.StopIncreaseBet.Value) SetBetObjects(true);
        else SetBetObjects(false);
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
            if (CanBet())
            {
                bool l_raycasted = Physics.Raycast(l_ray, out l_mousePosRaycastHit, 100f, m_betLayer);
                for (int i = 0; i < m_betsBehavior.Count; i++)
                {
                    if (!l_raycasted) m_betsBehavior[i].HighlightOff();
                    else if (m_betsBehavior[i].playerId == m_player.PlayerIndex
                                && l_mousePosRaycastHit.transform == m_betsBehavior[i].transform) m_betsBehavior[i].HighlightBetButton();
                }
            }
            else for (int i = 0; i < m_betsBehavior.Count; i++) m_betsBehavior[i].HighlightOff();
        }
    }

    bool CanBet()
    {
        if (!m_player.CanBet || (!RoundManager.Instance.BetHasStarted.Value && !m_player.CanPlay)) return false;
        else return true;
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
                    bool l_stopIncreaseBet = RoundManager.Instance.StopIncreaseBet.Value;
                    //Debug.Log("can play: " + m_player.CanPlay);
                    //Debug.Log("canBet: " + m_player.CanBet);
                    //Debug.Log("betHasStarted: " + RoundManager.Instance.BetHasStarted.Value);
                    //Debug.Log("stopIncreaseBet: " + l_stopIncreaseBet);

                    if (!CanBet())
                    {

                        continue;
                    }
                    m_currentBet = m_betsBehavior[i];

                    m_currentBet.StartDrag(Input.mousePosition);

                    if (RoundManager.Instance.BetHasStarted.Value && !l_stopIncreaseBet)
                    {
                        SetBetObjects(false);
                    }
                    else
                    {
                        SetBetObjects(true);
                    }

                    l_isBet = true;
                }
            }
        }

        return l_isBet;
    }

    public List<RaycastResult> m_resultList;
    public void CheckClickUp(bool p_canBet, Action<bool> p_actionOnStartAnimation, Action<GameObject, bool> p_actionOnEndAnimation)
    {
        bool l_bet = false;

        if (m_currentBet != null)
        {
            l_ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (p_canBet)
            {
                //if (!RoundManager.Instance.BetHasStarted.Value)
                //{
                //    Debug.Log("[GAME] BetHasStarted");
                //    Bet(true, m_currentBet, p_actionOnEndAnimation);
                //    l_bet = true;
                //}

                if (Physics.Raycast(l_ray, out l_mousePosRaycastHit, 100f, m_betLayer))
                {
                    if (l_mousePosRaycastHit.transform.gameObject == m_acceptTargetRect.gameObject
                        || (RoundManager.Instance.BetHasStarted.Value &&
                            l_mousePosRaycastHit.transform.gameObject == m_initialBetTargetRect.gameObject))
                    {
                        //www.youtube.com/results?search_query=anime+shoot+boobs+scene
                        
                        p_actionOnStartAnimation.Invoke(false);
                        Bet(false, m_currentBet, p_actionOnEndAnimation);
                        l_bet = true;

                        SetAllBetObjectsOff();
                    }
                    else if (l_mousePosRaycastHit.transform.gameObject == m_increaseTargetRect.gameObject
                             || (!RoundManager.Instance.BetHasStarted.Value &&
                                 l_mousePosRaycastHit.transform.gameObject == m_initialBetTargetRect.gameObject))
                    {
                        p_actionOnStartAnimation.Invoke(true);
                        Bet(true, m_currentBet, p_actionOnEndAnimation);
                        l_bet = true;

                        SetAllBetObjectsOff();
                    }
                }
            }

            if (!l_bet)
            {
                m_currentBet.EndDrag();
                m_currentBet = null;
                SetAllBetObjectsOff();
            }
        }

    }

    private void Bet(bool p_isIncrease, BetBehavior p_myBet, Action<GameObject, bool> p_action)
    {
        p_myBet.Bet(p_isIncrease, p_action, m_handAnimController);
        m_currentBet.EndDrag();
        m_currentBet = null;
    }

    private void SetBetObjects(bool p_start)
    {
        m_increaseTargetRect.gameObject.SetActive(!p_start);
        m_acceptTargetRect.gameObject.SetActive(!p_start);
        m_initialBetTargetRect.gameObject.SetActive(p_start);
    }

    private void SetAllBetObjectsOff(bool p_on = false)
    {
        m_increaseTargetRect.gameObject.SetActive(p_on);
        m_acceptTargetRect.gameObject.SetActive(p_on);
        m_initialBetTargetRect.gameObject.SetActive(p_on);
    }
}
