using UnityEngine;

[System.Serializable]
public class Card
{
    public int id;
    public int value;
    public Suit suit = Suit.NONE;
    public Player playerId = Player.DEFAULT;
    public GameObject gameObject;
    public bool isOnGame;
}

public enum Suit
{
    NONE,
    CENTIPEDE,
    RAT,
    CAT,
    OWL
}