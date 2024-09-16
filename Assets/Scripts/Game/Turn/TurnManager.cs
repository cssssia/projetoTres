using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;

    public CardsScriptableObject CardsSO;

    [SerializeField] public NetworkList<int> MatchWonHistory;
    [SerializeField] public NetworkVariable<bool> MatchHasStarted;

    public Match CurrentMatch;
    [HideInInspector] public bool HostMatchWon;
    [HideInInspector] public bool ClientMatchWon;

	public event EventHandler OnMatchWon;
	public event EventHandler OnCardPlayed;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        HostMatchWon = false;
        ClientMatchWon = false;

        MatchHasStarted.Value = false;

        MatchWonHistory = new NetworkList<int>();
    }

    [ServerRpc (RequireOwnership = false)]
    public void StartMatchServerRpc(Player p_playerType)
    {
        //if (MatchWonHistory.Count > 0 && (Player)MatchWonHistory[MatchWonHistory.Count - 1] == p_playerType) { Debug.Log("not your turn"); return; }

        Debug.Log(p_playerType + " StartedMatch");
        CurrentMatch = new Match(p_playerType);
        MatchHasStarted.Value = true;
    }

    [ServerRpc (RequireOwnership = false)]
    public void PlayCardServerRpc(int p_index, Player p_playerType)
    {
        //if (CurrentMatch.LastTurnPlayer == p_playerType) { Debug.Log("not your turn"); return; }

        CurrentMatch.CardPlayed(CardsSO.deck[p_index], p_playerType, out bool p_goToNextRound);

        if (p_goToNextRound)
        {
            CurrentMatch.RoundsWonHistory.Add(CurrentMatch.GetCurrentRoundWinner());
            Debug.Log(CurrentMatch.GetRoundWinner(CurrentMatch.RoundMatch - 1) + " Won Round");
        }

        if (CurrentMatch.RoundMatch > 1)
        {
            if (CurrentMatch.HostRoundsWon > CurrentMatch.ClientRoundsWon) HostMatchWon = true;
            else if (CurrentMatch.HostRoundsWon < CurrentMatch.ClientRoundsWon) ClientMatchWon = true;

            MatchWonHistory.Add((int)CurrentMatch.GetRoundWinner(CurrentMatch.RoundMatch - 1));
            //CardsManager.Instance.SpawnNewPlayCardsServerRpc();
            YouWonClientRpc(HostMatchWon, ClientMatchWon);
        }

        OnCardPlayed?.Invoke(p_playerType, EventArgs.Empty);

    }

    [ClientRpc]
    public void YouWonClientRpc(bool p_hostMatchWon, bool p_clientMatchWon)
    {
        if (p_hostMatchWon) OnMatchWon?.Invoke(0, EventArgs.Empty);
        else if (p_clientMatchWon) OnMatchWon?.Invoke(1, EventArgs.Empty);

        HostMatchWon = false;
        ClientMatchWon = false;
    }

}