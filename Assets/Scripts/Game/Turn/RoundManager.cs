using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoundManager : NetworkBehaviour
{
    public static RoundManager Instance;

    public CardsScriptableObject CardsSO;

    public NetworkVariable<bool> RoundHasStarted;

    public List<VictoryHistory> RoundWonHistory;
    public int VictoriesHost;
    public int VictoriesClient;

    public Trick CurrentTrick;
    // [HideInInspector] public bool HostTrickWon;
    // [HideInInspector] public bool ClientTrickWon;

    public NetworkVariable<bool> BetHasStarted;
    public NetworkVariable<bool> StopIncreaseBet;
    public NetworkVariable<int> BetAsked;

	public event EventHandler OnRoundWon;
    public event EventHandler OnTrickWon;
	public event EventHandler OnCardPlayed;
	public event EventHandler OnStartPlayingCard;
	public event EventHandler OnBet;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        if (!IsServer) return;

        RoundHasStarted.Value = false;
        BetHasStarted.Value = false;
        StopIncreaseBet.Value = false;
        BetAsked.Value = 1;
    }

    [ServerRpc (RequireOwnership = false)]
    public void StartRoundServerRpc(Player p_playerType)
    {
        Debug.Log("[GAME] " + p_playerType + " StartedTrick");
        CurrentTrick = new Trick(p_playerType);
        RoundHasStarted.Value = true;
    }

    [ServerRpc (RequireOwnership = false)]
    public void PlayCardServerRpc(int p_index, Player p_playerType, int p_targetIndex, NetworkObjectReference p_cardNetworkObjectReference)
    {
        Player l_wonRound = Player.DEFAULT;

        CurrentTrick.CardPlayed(CardsSO.deck[p_index], p_playerType, out bool p_goToNextTrick);

        if (p_goToNextTrick)
        {
            CurrentTrick.TurnsWonHistory.Add(CurrentTrick.GetCurrentTurnWinner());
            Debug.Log("[GAME] " + CurrentTrick.TurnsWonHistory[^1] + " Won Turn");
            OnTrickWon?.Invoke(CurrentTrick.TurnsWonHistory[^1], EventArgs.Empty);
        }

        if (CurrentTrick.CurrentTrick > 1)
        {
            if (CurrentTrick.HostTurnsWon > CurrentTrick.ClientTurnsWon) l_wonRound = Player.HOST;
            else if (CurrentTrick.HostTurnsWon < CurrentTrick.ClientTurnsWon) l_wonRound = Player.CLIENT;

            if (CurrentTrick.TurnsDraw == 1 && CurrentTrick.HostTurnsWon == CurrentTrick.ClientTurnsWon) //if there is one draw and host and client are both with 1 victory
            {
                if (CurrentTrick.WhoWonFirstTrick == Player.HOST) l_wonRound = Player.HOST;
                else if (CurrentTrick.WhoWonFirstTrick == Player.CLIENT) l_wonRound = Player.CLIENT;
            }
            else if (CurrentTrick.TurnsDraw >= 2) //three draws
            {
                if (CurrentTrick.WhoStartedTrick == Player.HOST) l_wonRound = Player.HOST;
                else if (CurrentTrick.WhoStartedTrick == Player.CLIENT) l_wonRound = Player.CLIENT;
            }

            AdjustVictoryServerRpc(l_wonRound);
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
        Debug.Log("[GAME] BetServerRpc");
        CurrentTrick.TrickBetMultiplier = BetAsked.Value;
        if (!BetHasStarted.Value) BetHasStarted.Value = true;
        if (p_increaseBet)
        {
            BetAsked.Value++;
            if (BetAsked.Value == 4) StopIncreaseBet.Value = true;
            Debug.Log("[GAME] Increase Bet: " + BetAsked.Value + " " + CurrentTrick.TrickBetMultiplier);
        }
        else
        {
            Debug.Log("[GAME] Accept Bet: " + BetAsked.Value + " " + CurrentTrick.TrickBetMultiplier);
            BetHasStarted.Value = false;
        }

        OnBet?.Invoke(p_increaseBet, EventArgs.Empty);
    }

    [ServerRpc (RequireOwnership = false)]
    public void GiveUpServerRpc(Player p_playerLost)
    {
        Debug.Log("[GAME] GiveUpServerRpc " + p_playerLost);

        AdjustVictoryServerRpc(p_playerLost == Player.HOST ? Player.CLIENT : Player.HOST);
    }

    [ServerRpc (RequireOwnership = false)]
    public void AdjustVictoryServerRpc(Player p_wonRound)
    {
        RoundWonHistory.Add(new VictoryHistory(p_wonRound, CurrentTrick.TrickBetMultiplier));

        YouWonClientRpc(p_wonRound);
    }

    [ClientRpc]
    public void YouWonClientRpc(Player p_wonRound)
    {
        OnRoundWon?.Invoke(p_wonRound, EventArgs.Empty);

        if (IsServer)
        {
            BetHasStarted.Value = false;
            StopIncreaseBet.Value = false;
            BetAsked.Value = 1;
        }
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

public struct VictoryHistory
{
    public Player player;
    public int roundValue;

    public  VictoryHistory(Player p_player, int p_roundValue)
    {
        player = p_player;
        roundValue = p_roundValue;
    }
}