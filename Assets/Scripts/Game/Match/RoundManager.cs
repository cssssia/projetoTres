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
    public NetworkVariable<int> PointsHost;
    public NetworkVariable<int> PointsClient;

    public Trick CurrentTrick;

    public NetworkVariable<bool> BetHasStarted;
    public NetworkVariable<bool> StopIncreaseBet;
    public NetworkVariable<int> BetAsked;

	public event EventHandler OnRoundWon;
    public event EventHandler OnAddItemToPlayer;
    public event EventHandler OnTrickWon;
	public event EventHandler OnCardPlayed;
	public event EventHandler OnItemUsed;
    public event EventHandler OnStartPlayingCard;
    public event EventHandler OnAnimItemUsed;
    public event EventHandler OnBet;
    public event EventHandler OnEndedDealing;
    public event EventHandler OnEndedDealingItem;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        if (!IsServer) return;

        RoundWonHistory = new List<VictoryHistory>();
        RoundHasStarted.Value = false;
        BetHasStarted.Value = false;
        StopIncreaseBet.Value = false;
        BetAsked.Value = 1;
        PointsHost.Value = 0;
        PointsClient.Value = 0;
    }

    [ServerRpc (RequireOwnership = false)]
    public void StartRoundServerRpc(Player p_playerType)
    {
        Debug.Log("[GAME] " + p_playerType + " StartedTrick");
        CurrentTrick = new Trick(p_playerType);
        RoundHasStarted.Value = true;
    }

    [ServerRpc (RequireOwnership = false)]
    public void PlayCardServerRpc(int p_cardIndex, Player p_playerType, int p_targetIndex)
    {
        Debug.Log("[GAME] Play Card " + CardsManager.Instance.GetCardByIndex(p_cardIndex).cardName);

        Player l_wonRound = Player.DEFAULT;

        CurrentTrick.CardPlayed(CardsManager.Instance.GetCardByIndex(p_cardIndex), p_playerType, out bool p_goToNextTrick);

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

        }

        CardsManager.Instance.SetPlayedCardUsableDeckClientRpc(p_cardIndex, true);
        OnCardPlayed?.Invoke(p_cardIndex, EventArgs.Empty);

        if (l_wonRound != Player.DEFAULT) AdjustVictoryServerRpc(l_wonRound);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayItemCardServerRpc(int p_itemIndex)
    {
        Debug.Log("PlayItemCardServerRpc");
        //CurrentTrick.CardPlayed(CardsManager.Instance.GetCardByIndex(p_cardIndex), p_playerType, out bool p_goToNextTrick);
        CardsManager.Instance.UseItemServerRpc(p_itemIndex);
        OnItemUsed?.Invoke(p_itemIndex, EventArgs.Empty);
    }

    [ServerRpc (RequireOwnership = false)]
    public void BetServerRpc(bool p_increaseBet)
    {
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
        Debug.Log("[GAME] " + p_playerLost + " Gave Up");

        AdjustVictoryServerRpc(p_playerLost == Player.HOST ? Player.CLIENT : Player.HOST);
    }

    [ServerRpc (RequireOwnership = false)]
    public void AdjustVictoryServerRpc(Player p_wonRound)
    {
        VictoryHistory l_victoryHistory = new VictoryHistory(p_wonRound, CurrentTrick.TrickBetMultiplier);

        RoundWonHistory.Add(l_victoryHistory);

        if (p_wonRound == Player.HOST) PointsHost.Value += CurrentTrick.TrickBetMultiplier;
        else if (p_wonRound == Player.CLIENT) PointsClient.Value += CurrentTrick.TrickBetMultiplier;

        YouWonClientRpc(p_wonRound);
    }

    [ClientRpc]
    public void YouWonClientRpc(Player p_wonRound)
    {
        if (IsServer)
        {
            BetHasStarted.Value = false;
            StopIncreaseBet.Value = false;
            BetAsked.Value = 1;
        }

        OnRoundWon?.Invoke(p_wonRound, EventArgs.Empty);
    }

    [ServerRpc (RequireOwnership = false)]
    public void OnStartPlayingCardAnimServerRpc(int p_playerIndex,bool p_isItem, int p_targetIndex, int p_cardIndex, NetworkObjectReference p_cardNetworkObjectReference)
    {
        OnStartPlayingCard?.Invoke((new CustomSender(p_playerIndex, p_targetIndex, p_cardNetworkObjectReference, p_cardIndex), p_isItem), EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership =false)]
    public void OnUseItemServerRpc(int p_player, int p_itemID)
    {
        Debug.Log("OnUseItemServerRpc");
        OnAnimItemUsed?.Invoke((p_player, p_itemID), EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnEndedDealingCardsServerRpc(int p_playerID)
    {
        OnEndedDealing?.Invoke(p_playerID, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership =false)]
    public void OnAddItemServerRpc(int p_playerID, NetworkObjectReference p_cardNetworkObjectReference, ItemType p_itemType)
    {
        OnAddItemToPlayer.Invoke(new CustomSender(p_playerID, p_cardNetworkObjectReference, p_itemType), EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnEndedDealingItemServerRpc(int p_playerID)
    {
        OnEndedDealingItem?.Invoke(p_playerID, EventArgs.Empty);
    }

}

public struct CustomSender
{
    public int playerType;
    public int targetIndex;
    public NetworkObjectReference cardNO;
    public int cardIndex;
    public ItemType itemType;

    public CustomSender(int p_playerType, int p_targetIndex, NetworkObjectReference p_cardNO, int p_cardIndex)
    {
        playerType = p_playerType;
        targetIndex = p_targetIndex;
        cardNO = p_cardNO;
        cardIndex = p_cardIndex;
        itemType = ItemType.NONE;
    }

    public CustomSender(int p_playerType, NetworkObjectReference p_cardNO, ItemType p_itemType)
    {
        playerType = p_playerType;
        cardNO = p_cardNO;
        itemType = p_itemType;
        targetIndex = -1;
        cardIndex = -1;
    }
}

[Serializable]
public class VictoryHistory
{
    public Player player;
    public int roundValue;

    public  VictoryHistory(Player p_player, int p_roundValue)
    {
        player = p_player;
        roundValue = p_roundValue;
    }
}