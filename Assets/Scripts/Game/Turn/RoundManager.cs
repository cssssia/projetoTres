using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class RoundManager : NetworkBehaviour
{
    public static RoundManager Instance;

    public CardsScriptableObject CardsSO;

    [SerializeField] public NetworkList<int> RoundWonHistory;
    [SerializeField] public NetworkVariable<bool> RoundHasStarted;
    [SerializeField] public NetworkVariable<int> WhoStartedRound;

    public Trick CurrentTrick;
    [HideInInspector] public bool HostTrickWon;
    [HideInInspector] public bool ClientTrickWon;

	public event EventHandler OnRoundWon;
    public event EventHandler OnTrickWon;
	public event EventHandler OnCardPlayed;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        HostTrickWon = false;
        ClientTrickWon = false;

        RoundHasStarted.Value = false;
        WhoStartedRound.Value = 0;

        RoundWonHistory = new NetworkList<int>();
    }

    [ServerRpc (RequireOwnership = false)]
    public void StartMatchServerRpc(Player p_playerType)
    {
        Debug.Log(p_playerType + " StartedMatch");
        CurrentTrick = new Trick(p_playerType);
        RoundHasStarted.Value = true;
        WhoStartedRound.Value = (int)p_playerType;
    }

    [ServerRpc (RequireOwnership = false)]
    public void PlayCardServerRpc(int p_index, Player p_playerType)
    {

        CurrentTrick.CardPlayed(CardsSO.deck[p_index], p_playerType, out bool p_goToNextTrick);

        if (p_goToNextTrick)
        {
            CurrentTrick.TurnsWonHistory.Add(CurrentTrick.GetCurrentTurnWinner());
            Debug.Log(CurrentTrick.GetTurnWinner(CurrentTrick.CurrentTrick - 1) + " Won Round");
            OnTrickWon?.Invoke(CurrentTrick.GetTurnWinner(CurrentTrick.CurrentTrick - 1), EventArgs.Empty);
        }

        if (CurrentTrick.CurrentTrick > 1)
        {
            if (CurrentTrick.HostTurnsWon > CurrentTrick.ClientTurnsWon) HostTrickWon = true;
            else if (CurrentTrick.HostTurnsWon < CurrentTrick.ClientTurnsWon) ClientTrickWon = true;

            RoundWonHistory.Add((int)CurrentTrick.GetTurnWinner(CurrentTrick.CurrentTrick - 1));
            YouWonClientRpc(HostTrickWon, ClientTrickWon);
        }

        OnCardPlayed?.Invoke(p_playerType, EventArgs.Empty);

    }

    [ClientRpc]
    public void YouWonClientRpc(bool p_HostTrickWon, bool p_ClientTrickWon)
    {
        if (p_HostTrickWon) OnRoundWon?.Invoke(0, EventArgs.Empty);
        else if (p_ClientTrickWon) OnRoundWon?.Invoke(1, EventArgs.Empty);

        HostTrickWon = false;
        ClientTrickWon = false;
    }

}