using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetBehavior : MonoBehaviour
{
    [SerializeField] private BetTag m_myBet;

    public void OnPlayerSpawned(int p_playerId)
    {
        BetTag[] l_bets = FindObjectsOfType<BetTag>();

        foreach (BetTag betTag in l_bets)
        {
            if (betTag.playerId == p_playerId)
                m_myBet = betTag;
        }
    }

    bool m_amBetting;
    public bool CheckClickObject(GameObject p_gameObject)
    {
        bool l_isBet = false;
        m_amBetting = false;

        if (p_gameObject != null && m_myBet.gameObject == p_gameObject)
        {
            m_amBetting = l_isBet = true;
        }

        return l_isBet;
    }

    public void CheckClickUp(bool p_canPlay, Action<GameObject> p_actionOnEndAnimation)
    {
        bool l_bet = false;

        if (m_amBetting && p_canPlay)
        {
            Bet();
            l_bet = true;
        }

        if (!l_bet)
        {
            m_amBetting = false;
        }
    }

    private void Bet()
    {
        Debug.Log("bet");
    }
}
