using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Player { DEFAULT, HOST, CLIENT, DRAW};
[System.Serializable]
public class Match
{
    public Player WhoStartedMatch { get; private set; }
    public Player LastTurnPlayer { get; set; }

    public List<CardsScriptableObject.Card> HostCardsPlayed;
    public List<CardsScriptableObject.Card> ClientCardsPlayed;

    public int HostRoundsWon;
    public int ClientRoundsWon;

    public int RoundMatch;
    public List<Player> RoundsWonHistory;
    public List<Player> RoundsStartedHistory;

    public Match(Player p_whoStartsMatch)
    {
        WhoStartedMatch = p_whoStartsMatch;
        LastTurnPlayer = Player.DEFAULT;
        HostCardsPlayed = new();
        ClientCardsPlayed = new();

        HostRoundsWon = 0;
        ClientRoundsWon = 0;

        RoundMatch = 0;

        RoundsWonHistory = new();
        RoundsStartedHistory = new()
        {
            p_whoStartsMatch
        };
    }

    public void CardPlayed(CardsScriptableObject.Card p_card, Player p_player, out bool p_goToNextRound)
    {

        p_goToNextRound = false;

        if (p_player is Player.HOST) HostCardsPlayed.Add(p_card);
        else ClientCardsPlayed.Add(p_card);

        if (HostCardsPlayed.Count > RoundMatch && ClientCardsPlayed.Count > RoundMatch)
        {
            p_goToNextRound = true;
        }

        LastTurnPlayer = p_player;

    }

    public Player GetRoundWinner(int p_round)
    {
        if (HostCardsPlayed[p_round].value > ClientCardsPlayed[p_round].value) return Player.HOST;
        else if (HostCardsPlayed[p_round].value < ClientCardsPlayed[p_round].value) return Player.CLIENT;
        return Player.DRAW;
    }

    public Player GetCurrentRoundWinner()
    {
        if (HostCardsPlayed[RoundMatch].value > ClientCardsPlayed[RoundMatch].value)
        {
            RoundMatch++;
            HostRoundsWon++;
            return Player.HOST;
        }
        else if (HostCardsPlayed[RoundMatch].value < ClientCardsPlayed[RoundMatch].value)
        {
            RoundMatch++;
            ClientRoundsWon++;
            return Player.CLIENT;
        }

        RoundMatch++;
        return Player.DRAW;
    }

}
