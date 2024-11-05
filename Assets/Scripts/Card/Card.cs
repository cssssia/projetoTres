using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class Card
{
    [SerializeField] public string cardName;
    [SerializeField] public int cardValue;
    [SerializeField] public int cardIndexSO;
    [SerializeField] public int cardPlayer;
    [SerializeField] public NetworkObject cardNetworkObject;
    public bool playedCard;
}