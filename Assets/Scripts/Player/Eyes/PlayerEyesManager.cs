using RatiadaAbsoluta;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerEyesManager : MonoBehaviour
{
    [SerializeField] private Player m_player;
    [SerializeField] private List<PlayerEyeBehavior> m_eyesBehavior;

    private void Start()
    {
        RoundManager.Instance.OnBet += OnBet;
    }

    private void OnBet(object p_increaseBet, EventArgs p_args)
    {
        if (m_player is Player.HOST)
        {
            if (GameManager.Instance.betState.Value is GameManager.BetState.ClientTurn)
            {
                CountEyes(Player.HOST);
            }
        }
        else
        {
            if (GameManager.Instance.betState.Value is GameManager.BetState.HostTurn)
            {
                CountEyes(Player.CLIENT);
            }
        }
    }

    void CountEyes(Player p_player)
    {
        int l_eyes = 1;
        for (int i = 0; i < RoundManager.Instance.RoundWonHistory.Count; i++)
        {
            if (RoundManager.Instance.RoundWonHistory[i].player != p_player)
                l_eyes += RoundManager.Instance.RoundWonHistory[i].roundValue;
        }

        Debug.Log(p_player + " number of eyes emiting: " + l_eyes);
    }

    [NaughtyAttributes.Button]
    public void GetEyesBehavior()
    {
        m_eyesBehavior = transform.GetComponentsInChildren<PlayerEyeBehavior>().ToList();
        m_eyesBehavior.Shuffle();
    }

    //public void
}
