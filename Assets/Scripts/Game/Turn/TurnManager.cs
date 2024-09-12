using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TurnManager : Singleton<TurnManager>
{
    public Match CurrentMatch;


    public void PlayCard(CardsScriptableObject.Card p_card, int p_playerID)
    {
        CurrentMatch.CardPlayed(p_card, (Player)p_playerID);
    }
   


}