using RatiadaAbsoluta;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerEyesManager : MonoBehaviour
{
    PlayerController m_playerController;
    [SerializeField] private Player m_player;
    private int m_currentCoveredEyes;
    [SerializeField] private List<PlayerEyeBehavior> m_eyesBehavior;
    //RoundManager RoundManager.Instance;

    private void Start()
    {
        m_currentCoveredEyes = 16;
        RoundManager.Instance.OnBetAsked += OnBet;

        PlayerController.OnPlayerSpawned += (player) =>
        {
            if (player == PlayerController.LocalInstance)
            {
                CountEyes(m_player, (m_player, 1, 1));
            }
        };
    }

    private void OnBet(object p_tripa, EventArgs p_args)
    {
        CountEyes(m_player, ((Player, int, int))p_tripa);
        //if (m_player is Player.HOST)
        //{
        //    if (GameManager.Instance.betState.Value is GameManager.BetState.ClientTurn)
        //    {
        //    }
        //}
        //else
        //{
        //    if (GameManager.Instance.betState.Value is GameManager.BetState.HostTurn)
        //    {
        //        CountEyes(Player.CLIENT);
        //    }
        //}
    }

    void CountEyes(Player p_player, (Player, int, int) p_tripa)
    {
        Player l_whoAsked = p_tripa.Item1;
        int l_valueAsked = p_tripa.Item2;
        int l_trickBetMultiplier = p_tripa.Item3;

        int l_nextEyes = 16 - (p_player == Player.HOST ? RoundManager.Instance.PointsClient.Value : RoundManager.Instance.PointsHost.Value);

        l_nextEyes -= l_trickBetMultiplier;

        if (l_whoAsked == p_player)
        {
            l_nextEyes -= (l_valueAsked - l_trickBetMultiplier);
            print("b " + l_nextEyes);
        }

        if (l_nextEyes < m_currentCoveredEyes)
        {
            Debug.Log("tira " + (m_currentCoveredEyes - l_nextEyes) + "olhos do " + m_player);
            StartCoroutine(AnimButtonRemoval(false, m_currentCoveredEyes, l_nextEyes));
        }
        else if (l_nextEyes > m_currentCoveredEyes)
        {
            Debug.Log("volta " + (m_currentCoveredEyes - l_nextEyes) + "olhos do " + m_player);
            StartCoroutine(AnimButtonRemoval(true, m_currentCoveredEyes, l_nextEyes));
        }
        else print("tudo normar no " + m_player);

        m_currentCoveredEyes = l_nextEyes;
    }

    IEnumerator AnimButtonRemoval(bool p_cover, int p_initialIndex, int p_finalIndex)
    {
        if ((PlayerController.LocalInstance.IsClientPlayer && m_player is Player.CLIENT)
            || (PlayerController.LocalInstance.IsHostPlayer && m_player is Player.HOST))
        {
            Debug.Log("anim screen");

            Debug.Log("anim exposition");
        }
        else
        {
            Debug.Log("intial index = " + p_initialIndex);
            Debug.Log("final index = " + p_finalIndex);

            if (p_cover)
            {
                for (int i = p_initialIndex; i < p_finalIndex; i++)
                {
                    m_eyesBehavior[i].SetCover(p_cover);
                }
            }
            else
            {
                for (int i = p_initialIndex -1; i >= p_finalIndex; i--)
                {
                    m_eyesBehavior[i].SetCover(p_cover);
                }
            }
        }

        yield return null;
    }

    [NaughtyAttributes.Button]
    public void GetEyesBehavior()
    {
        m_eyesBehavior = transform.GetComponentsInChildren<PlayerEyeBehavior>().ToList();
        m_eyesBehavior.OrderByDescending(e => e);
    }

    //public void
}
