using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Player { DEFAULT, HOST, CLIENT, DRAW };
public enum Truco { DEFAULT = 1, TRUCO, RETRUCO, VALEQUATRO }
[System.Serializable]
public class Trick
{
    [HideInInspector] public Player WhoStartedTrick { get; private set; }
    [HideInInspector] public Player WhoWonFirstTrick { get; set; }
    [HideInInspector] public Player LastTurnPlayer { get; set; }

    public List<CardsScriptableObject.Card> HostCardsPlayed;
    public List<CardsScriptableObject.Card> ClientCardsPlayed;

    [HideInInspector] public int HostTurnsWon;
    [HideInInspector] public int ClientTurnsWon;

    [HideInInspector] public int CurrentTrick;
    [HideInInspector] public List<Player> TurnsWonHistory;
    [HideInInspector] public List<Player> TurnsStartedHistory;
    [HideInInspector] public int TrickBetMultiplier;

    // public bool IsNoBet
    // {
    //     get { return TrickBetMultiplier == Truco.DEFAULT; }
    // }
    // public bool IsTruco
    // {
    //     get { return TrickBetMultiplier == Truco.TRUCO; }
    // }
    // public bool IsRetruco
    // {
    //     get { return TrickBetMultiplier == Truco.RETRUCO; }
    // }
    // public bool IsValeQuatro
    // {
    //     get { return TrickBetMultiplier == Truco.VALEQUATRO; }
    // }


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

        TurnsWonHistory = new();
        TurnsStartedHistory = new()
        {
            p_whoStartsTrick
        };
    }

    public void CardPlayed(CardsScriptableObject.Card p_card, Player p_player, out bool p_goToNextTrick)
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
        if (HostCardsPlayed[p_round].value > ClientCardsPlayed[p_round].value) return Player.HOST;
        else if (HostCardsPlayed[p_round].value < ClientCardsPlayed[p_round].value) return Player.CLIENT;
        return Player.DRAW;
    }

    public Player GetCurrentTurnWinner()
    {
        if (HostCardsPlayed[CurrentTrick].value > ClientCardsPlayed[CurrentTrick].value)
        {
            CurrentTrick++;
            HostTurnsWon++;
            if (WhoWonFirstTrick == Player.DEFAULT) WhoWonFirstTrick = Player.HOST;
            return Player.HOST;
        }
        else if (HostCardsPlayed[CurrentTrick].value < ClientCardsPlayed[CurrentTrick].value)
        {
            CurrentTrick++;
            ClientTurnsWon++;
            if (WhoWonFirstTrick == Player.DEFAULT) WhoWonFirstTrick = Player.CLIENT;
            return Player.CLIENT;
        }

        CurrentTrick++;
        return Player.DRAW;
    }

}
