using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Player { HOST, CLIENT,  DRAW};
public class Match
{
    public Player WhoStartedMatch { get; private set; }

    public List<CardsScriptableObject.Card> HostCardsPlayed;
    public List<CardsScriptableObject.Card> ClientCardsPlayed;

    public int RoundMatch;
    public List<Player> RoundsWonHistory;
    public List<Player> RoundsStartedHistory;

    public Match(Player p_whoStartsMatch)
    {
        WhoStartedMatch = p_whoStartsMatch;
        HostCardsPlayed = new();
        ClientCardsPlayed = new();
        RoundMatch = 0;

        RoundsWonHistory = new();
        RoundsStartedHistory = new()
        {
            p_whoStartsMatch
        };
    }

    public void CardPlayed(CardsScriptableObject.Card p_card, Player p_player)
    {
        if (p_player is Player.HOST) HostCardsPlayed.Add(p_card);
        else ClientCardsPlayed.Add(p_card);
    }

    public Player GetRoundWinner(int p_round)
    {
        if (HostCardsPlayed[p_round].value > ClientCardsPlayed[p_round].value) return Player.HOST;
        else if (HostCardsPlayed[p_round].value < ClientCardsPlayed[p_round].value) return Player.CLIENT;
        return Player.DRAW;
    }

    public Player GetCurrentRoundWinner()
    {
        if (HostCardsPlayed[RoundMatch].value > ClientCardsPlayed[RoundMatch].value) return Player.HOST;
        else if (HostCardsPlayed[RoundMatch].value < ClientCardsPlayed[RoundMatch].value) return Player.CLIENT;
        return Player.DRAW;
    }

}
