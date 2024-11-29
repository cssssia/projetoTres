using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class Card
{
    public string cardName;
    public int cardValue;
    public Suit cardSuit;
    public int cardIndexSO;
    public Player cardPlayer;
    public NetworkObjectReference cardNetworkObjectReference;
    public bool playedCard;

    public Card (string p_cardName, int p_cardValue, int p_cardIndexSO, NetworkObjectReference p_networkObject)
    {
        cardName = p_cardName;
        cardValue = p_cardValue;
        cardSuit = GetCardSuit();
        cardIndexSO = p_cardIndexSO;
        cardPlayer = Player.DEFAULT;
        cardNetworkObjectReference = p_networkObject;
        playedCard = false;
    }

    private Suit GetCardSuit()
    {
        string l_cardSuit = cardName[..1];

        return l_cardSuit switch
        {
            "H" => Suit.CENTIPEDE,
            "D" => Suit.RAT,
            "C" => Suit.CAT,
            "S" => Suit.OWL,
            _ => Suit.NONE,
        };
    }
}

public enum Suit
{
    CENTIPEDE,
    RAT,
    CAT,
    OWL,
    NONE
}