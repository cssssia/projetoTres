using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Player { HOST, CLIENT, DRAW, DEFAULT };
[System.Serializable]
public class Trick
{
    [HideInInspector] public Player WhoStartedTrick { get; private set; }
    [HideInInspector] public Player WhoWonFirstTrick { get; set; }
    [HideInInspector] public Player LastTurnPlayer { get; set; }

    public List<Card> HostCardsPlayed;
    public List<Card> ClientCardsPlayed;

    [HideInInspector] public int HostTurnsWon;
    [HideInInspector] public int ClientTurnsWon;
    [HideInInspector] public int TurnsDraw;

    [HideInInspector] public int CurrentTrick;
    public List<Player> TurnsWonHistory;
    [HideInInspector] public List<Player> TurnsStartedHistory;
    [HideInInspector] public int TrickBetMultiplier;

    public Trick(Player p_whoStartsTrick)
    {
        WhoStartedTrick = p_whoStartsTrick;
        LastTurnPlayer = Player.DEFAULT;
        WhoWonFirstTrick = Player.DEFAULT;
        HostCardsPlayed = new();
        ClientCardsPlayed = new();

        HostTurnsWon = 0;
        ClientTurnsWon = 0;

        CurrentTrick = 0;

        TrickBetMultiplier = 1;

        TurnsWonHistory = new();
        TurnsStartedHistory = new()
        {
            p_whoStartsTrick
        };
    }

    public void CardPlayed(Card p_card, Player p_player, out bool p_goToNextTrick)
    {

        p_goToNextTrick = false;

        if (p_player is Player.HOST) HostCardsPlayed.Add(p_card);
        else ClientCardsPlayed.Add(p_card);

        if (HostCardsPlayed.Count > CurrentTrick && ClientCardsPlayed.Count > CurrentTrick)
        {
            p_goToNextTrick = true;
        }

        LastTurnPlayer = p_player;

    }

    public Player GetTurnWinner(int p_round)
    {
        if (HostCardsPlayed[p_round].cardValue > ClientCardsPlayed[p_round].cardValue) return Player.HOST;
        else if (HostCardsPlayed[p_round].cardValue < ClientCardsPlayed[p_round].cardValue) return Player.CLIENT;
        return Player.DRAW;
    }

    public Player GetCurrentTurnWinner()
    {
        if (HostCardsPlayed[CurrentTrick].cardValue > ClientCardsPlayed[CurrentTrick].cardValue)
        {
            CurrentTrick++;
            HostTurnsWon++;
            if (WhoWonFirstTrick == Player.DEFAULT) WhoWonFirstTrick = Player.HOST;
            return Player.HOST;
        }
        else if (HostCardsPlayed[CurrentTrick].cardValue < ClientCardsPlayed[CurrentTrick].cardValue)
        {
            CurrentTrick++;
            ClientTurnsWon++;
            if (WhoWonFirstTrick == Player.DEFAULT) WhoWonFirstTrick = Player.CLIENT;
            return Player.CLIENT;
        }

        CurrentTrick++;
        TurnsDraw++;
        return Player.DRAW;
    }

}
