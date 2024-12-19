using RatiadaAbsoluta;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerEyesManager : NetworkBehaviour
{
    [SerializeField] private PlayerController m_playerController;
    [SerializeField] private Player m_player;
    private int m_currentCoveredEyes;
    [SerializeField] private List<PlayerEyeBehavior> m_eyesBehavior;
    //RoundManager RoundManager.Instance;

    public override void OnNetworkSpawn()
    {
        //Debug.Log($"PLAYERS ESYES + - IsClient: {IsClient}, IsHost: {IsHost}, IsServer: {IsServer}, IsOwner: {IsOwner}");
        m_currentCoveredEyes = 16;

        if (IsServer)
        {
            RoundManager.Instance.OnBet += OnBet;
            CardsManager.Instance.OnRoundWon += OnRoundWon;
            AnimButtonRemovalServerRpc(m_player, m_currentCoveredEyes, Player.DEFAULT);
        }

    }

    private void OnArrivedPlayer(bool p_arrived)
    {
        //Debug.Log("OnArrivedPlayer " + m_player + IsOwner);
        AnimFakeButtonRemovalServerRpc(m_player, m_currentCoveredEyes, m_player, p_arrived);

        if (!p_arrived)
        {
            //anim exposure
            //if (IsOwner)
            //{
            //Debug.Log($"PLAYERS ESYES + - IsClient: {IsClient}, IsHost: {IsHost}, IsServer: {IsServer}, IsOwner: {IsOwner}");
            //Debug.Log("anima exposição");

            //    CameraController.Instance.SetExposure()
            //}
        }
    }

    public void OnPlayerSpawned(PlayerController p_playerController)
    {
        //m_playerController = p_playerController;
        m_player = (Player)p_playerController.PlayerIndex;

        var l_handAnimControllers = FindObjectsByType<HandItemAnimController>(default);

        for (int i = 0; i < l_handAnimControllers.Length; i++)
        {
            if (m_player == l_handAnimControllers[i].PlayerType)
            {
                //Debug.Log("listening " + m_player + IsOwner);
                l_handAnimControllers[i].betHandAnimator.OnArrivedPlayer += OnArrivedPlayer;
            }
        }
    }

    private void OnBet(object sender, EventArgs e)
    {
        //Debug.Log("on bet");
        Player l_player = (((bool, Player))sender).Item2;
        AnimButtonRemovalServerRpc(m_player, m_currentCoveredEyes, l_player);
    }

    private void OnRoundWon(object sender, EventArgs e)
    {
        AnimButtonRemovalServerRpc(m_player, m_currentCoveredEyes, Player.DEFAULT);
    }

    [ServerRpc(RequireOwnership = false)]
    void AnimButtonRemovalServerRpc(Player p_player, int p_currentCoveredEyes, Player p_whoAsked)
    {
        //Debug.Log($"AnimButtonRemovalServerRpc + - IsClient: {IsClient}, IsHost: {IsHost}, IsServer: {IsServer}, IsOwner: {IsOwner}");

        //Debug.Log("pts client: " + RoundManager.Instance.PointsClient.Value);
        //Debug.Log("pts host: " + RoundManager.Instance.PointsHost.Value);

        int l_nextEyes = 16 - (p_player == Player.HOST ? RoundManager.Instance.PointsClient.Value : RoundManager.Instance.PointsHost.Value);

        l_nextEyes -= RoundManager.Instance.TrickBetMultiplier.Value;

        if (p_whoAsked == p_player)
        {
            l_nextEyes -= RoundManager.Instance.BetAsked.Value - RoundManager.Instance.TrickBetMultiplier.Value;
            //print("next eyes " + l_nextEyes);
        }

        AnimButtonRemovalClientRpc(p_player, l_nextEyes, p_currentCoveredEyes);
    }

    [ServerRpc(RequireOwnership = false)]
    void AnimFakeButtonRemovalServerRpc(Player p_player, int p_currentCoveredEyes, Player p_whoAsked, bool p_arrived)
    {
        //Debug.Log($"AnimButtonRemovalServerRpc + - IsClient: {IsClient}, IsHost: {IsHost}, IsServer: {IsServer}, IsOwner: {IsOwner}");

        //Debug.Log("pts client: " + RoundManager.Instance.PointsClient.Value);
        //Debug.Log("pts host: " + RoundManager.Instance.PointsHost.Value);

        if (p_arrived) return;

        int l_nextEyes = 16 - (p_player == Player.HOST ? RoundManager.Instance.PointsClient.Value : RoundManager.Instance.PointsHost.Value);

        l_nextEyes -= RoundManager.Instance.TrickBetMultiplier.Value;

        if (p_whoAsked == p_player)
        {
            l_nextEyes -= RoundManager.Instance.BetAsked.Value - RoundManager.Instance.TrickBetMultiplier.Value + 1;
            //print("next eyes " + l_nextEyes);
        }

        AnimButtonRemovalClientRpc(p_player, l_nextEyes, p_currentCoveredEyes);
    }

    [ClientRpc]
    void AnimButtonRemovalClientRpc(Player p_player, int p_nextEyes, int p_currentCoveredEyes)
    {
        if (p_player == m_player && IsOwner)
        {
            CameraController.Instance.SetExposure(1f - (((float)p_nextEyes) / 15f));
        }

        if (p_nextEyes < p_currentCoveredEyes)
        {
            //Debug.Log("tira " + (p_currentCoveredEyes - p_nextEyes) + "olhos do " + p_player);
            AnimButtonRemoval(p_player, false, p_currentCoveredEyes, p_nextEyes);
        }
        else if (p_nextEyes > p_currentCoveredEyes)
        {
            //Debug.Log("volta " + (p_currentCoveredEyes - p_nextEyes) + "olhos do " + p_player);
            AnimButtonRemoval(p_player, true, p_currentCoveredEyes, p_nextEyes);
        }
        //else print("tudo normar no " + p_player);

        m_currentCoveredEyes = p_nextEyes;
    }

    void AnimButtonRemoval(Player p_player, bool p_cover, int p_initialIndex, int p_finalIndex)
    {
        StartCoroutine(IAnimButtonRemoval(p_player, p_cover, p_initialIndex, p_finalIndex));
    }

    IEnumerator IAnimButtonRemoval(Player p_player, bool p_cover, int p_initialIndex, int p_finalIndex)
    {


        if (IsOwner)
        {
            //Debug.Log("anim MY screen");
            //Debug.Log("anim MY exposition");
        }

        //Debug.Log("intial index = " + p_initialIndex);
        //Debug.Log("final index = " + p_finalIndex);

        if (p_cover)
        {
            for (int i = p_initialIndex; i < p_finalIndex; i++)
            {
                m_eyesBehavior[i].SetCover(p_cover);
            }
        }
        else
        {
            for (int i = p_initialIndex - 1; i >= p_finalIndex; i--)
            {
                m_eyesBehavior[i].SetCover(p_cover);
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
