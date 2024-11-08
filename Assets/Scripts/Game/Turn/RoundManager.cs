using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class RoundManager : NetworkBehaviour
{
    public static RoundManager Instance;

    public CardsScriptableObject CardsSO;

    public List<Player> MatchWonHistory;
    public NetworkVariable<bool> RoundHasStarted;
    public NetworkVariable<int> WhoStartedRound;

    public NetworkVariable<int> VictoriesHost;
    public NetworkVariable<int> VictoriesClient;

    public Trick CurrentTrick;
    [HideInInspector] public bool HostTrickWon;
    [HideInInspector] public bool ClientTrickWon;

    public NetworkVariable<bool> BetHasStarted;

	public event EventHandler OnRoundWon;
    public event EventHandler OnTrickWon;
	public event EventHandler OnCardPlayed;
	public event EventHandler OnStartPlayingCard;
	public event EventHandler OnBet;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        HostTrickWon = false;
        ClientTrickWon = false;

        BetHasStarted.Value = false;
        RoundHasStarted.Value = false;
        WhoStartedRound.Value = 0;
        VictoriesHost.Value = 0;
        VictoriesClient.Value = 0;
    }

    [ServerRpc (RequireOwnership = false)]
    public void StartRoundServerRpc(Player p_playerType)
    {
        Debug.Log("[GAME] " + p_playerType + " StartedRound");
        CurrentTrick = new Trick(p_playerType);
        RoundHasStarted.Value = true;
        WhoStartedRound.Value = (int)p_playerType;
    }

    int draw;

    [ServerRpc (RequireOwnership = false)]
    public void PlayCardServerRpc(int p_index, Player p_playerType, int p_targetIndex, NetworkObjectReference p_cardNetworkObjectReference)
    {
        CurrentTrick.CardPlayed(CardsSO.deck[p_index], p_playerType, out bool p_goToNextTrick);

        if (p_goToNextTrick)
        {
            CurrentTrick.TurnsWonHistory.Add(CurrentTrick.GetCurrentTurnWinner());
            Debug.Log("[GAME] " + CurrentTrick.GetTurnWinner(CurrentTrick.CurrentTrick - 1) + " Won Round");
            OnTrickWon?.Invoke(CurrentTrick.GetTurnWinner(CurrentTrick.CurrentTrick - 1), EventArgs.Empty);
            if (CurrentTrick.GetTurnWinner(CurrentTrick.CurrentTrick - 1) == Player.DRAW)
                draw++;
        }

        if (CurrentTrick.CurrentTrick > 1)
        {
            if (CurrentTrick.HostTurnsWon > CurrentTrick.ClientTurnsWon) HostTrickWon = true;
            else if (CurrentTrick.HostTurnsWon < CurrentTrick.ClientTurnsWon) ClientTrickWon = true;

            if (draw == 1 && !HostTrickWon && !ClientTrickWon) //if there is one draw and host and client are both with 1 victory
            {
                if (CurrentTrick.WhoWonFirstTrick == Player.HOST) HostTrickWon = true;
                else if (CurrentTrick.WhoWonFirstTrick == Player.CLIENT) ClientTrickWon = true;
            }
            else if (draw >= 2) //three draws
            {
                if (CurrentTrick.WhoStartedTrick == Player.HOST) HostTrickWon = true;
                else if (CurrentTrick.WhoStartedTrick == Player.CLIENT) ClientTrickWon = true;
            }

            if (HostTrickWon)
            {
                MatchWonHistory.Add(Player.HOST);
                VictoriesHost.Value += CurrentTrick.TrickBetMultiplier;
            }
            else if (ClientTrickWon)
            {
                MatchWonHistory.Add(Player.CLIENT);
                VictoriesClient.Value += CurrentTrick.TrickBetMultiplier;
            }

            YouWonClientRpc(HostTrickWon, ClientTrickWon);
        }

        CustomSender l_customSender = new();
        l_customSender.playerType = (int)p_playerType;
        l_customSender.targetIndex = p_targetIndex;
        l_customSender.cardNO = p_cardNetworkObjectReference;
        l_customSender.cardIndex = p_index;

        OnCardPlayed?.Invoke(l_customSender, EventArgs.Empty);

    }

    [ServerRpc (RequireOwnership = false)]
    public void BetServerRpc(bool p_increaseBet)
    {
        if (p_increaseBet)
        {
            BetHasStarted.Value = true;
            CurrentTrick.TrickBetMultiplier++;
        }
        else BetHasStarted.Value = false;

        OnBet?.Invoke(CurrentTrick.TrickBetMultiplier == 4, EventArgs.Empty);
    }

    [ClientRpc]
    public void YouWonClientRpc(bool p_HostTrickWon, bool p_ClientTrickWon)
    {
        if (p_HostTrickWon) OnRoundWon?.Invoke(0, EventArgs.Empty);
        else if (p_ClientTrickWon) OnRoundWon?.Invoke(1, EventArgs.Empty);

        HostTrickWon = false;
        ClientTrickWon = false;
    }

    [ServerRpc (RequireOwnership = false)]
    public void OnStartAnimServerRpc(int p_playerIndex, int p_targetIndex, int p_cardIndex, NetworkObjectReference p_cardNetworkObjectReference)
    {
        OnStartPlayingCard?.Invoke(new CustomSender(p_playerIndex, p_targetIndex, p_cardNetworkObjectReference, p_cardIndex), EventArgs.Empty);
    }
}

public struct CustomSender
{
    public int playerType;
    public int targetIndex;
    public NetworkObjectReference cardNO;
    public int cardIndex;

    public CustomSender(int p_playerType, int p_targetIndex, NetworkObjectReference p_cardNO, int p_cardIndex)
    {
        playerType = p_playerType;
        targetIndex = p_targetIndex;
        cardNO = p_cardNO;
        cardIndex = p_cardIndex;
    }
}