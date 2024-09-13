using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;

    public Match CurrentMatch;
    public int HostTurnsWonHistory;
    public int ClientTurnsWonHistory;

    public CardsScriptableObject CardsSO;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    [ServerRpc (RequireOwnership = false)]
    public void StartMatchServerRpc(Player p_playerType)
    {
        Debug.Log("StartMatchServerRpc");
        CurrentMatch = new Match(p_playerType);
    }

    [ServerRpc (RequireOwnership = false)]
    public void PlayCardServerRpc(int p_index, Player p_playerType)
    {

        Debug.Log("PlayCardServerRpc");

        if (CurrentMatch.LastTurnPlayer == p_playerType)
        {
            Debug.Log("not your turn");
            return;
        }

        CurrentMatch.CardPlayed(CardsSO.deck[p_index], p_playerType, out bool p_goToNextRound);

        if (p_goToNextRound)
            CurrentMatch.RoundsWonHistory.Add(CurrentMatch.GetCurrentRoundWinner());

        if (CurrentMatch.RoundMatch > 2)
        {
            if (CurrentMatch.HostRoundsWon > CurrentMatch.ClientRoundsWon) HostTurnsWonHistory++;
            else if (CurrentMatch.HostRoundsWon > CurrentMatch.ClientRoundsWon) ClientTurnsWonHistory++;
        }

    }

}