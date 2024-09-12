using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TurnManager : Singleton<TurnManager>
{
    public Match CurrentMatch;
    public int HostTurnsWonHistory;
    public int ClientTurnsWonHistory;

    public void StartMatch(Player p_playerType)
    {
        CurrentMatch = new Match(p_playerType);
    }

    public void PlayCard(CardsScriptableObject.Card p_card, Player p_playerType)
    {
        if (CurrentMatch.LastTurnPlayer == p_playerType)
        {
            Debug.Log("not your turn");
            return;
        }

        CurrentMatch.CardPlayed(p_card, p_playerType, out bool p_goToNextRound);

        if (p_goToNextRound)
            CurrentMatch.RoundsWonHistory.Add(CurrentMatch.GetCurrentRoundWinner());

        if (CurrentMatch.RoundMatch > 2)
        {
            if (CurrentMatch.HostRoundsWon > CurrentMatch.ClientRoundsWon) HostTurnsWonHistory++;
            else if (CurrentMatch.HostRoundsWon > CurrentMatch.ClientRoundsWon) ClientTurnsWonHistory++;
        }

    }

}