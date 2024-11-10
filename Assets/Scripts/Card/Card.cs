using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class Card
{
    public string cardName;
    public int cardValue;
    public int cardIndexSO;
    public Player cardPlayer;
    public NetworkObjectReference cardNetworkObjectReference;
    public bool playedCard;

    public Card (string p_cardName, int p_cardValue, int p_cardIndexSO, NetworkObjectReference p_networkObject)
    {
        cardName = p_cardName;
        cardValue = p_cardValue;
        cardIndexSO = p_cardIndexSO;
        cardPlayer = Player.DEFAULT;
        cardNetworkObjectReference = p_networkObject;
        playedCard = false;
    }
}