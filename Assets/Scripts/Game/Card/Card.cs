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
    private CardBehavior m_behavior;
    public CardBehavior GetCardBehavior
    {
        get
        {
            if (m_behavior == null) m_behavior = gameObject.GetComponent<CardBehavior>();
            return m_behavior;

        }
    }
}

public enum Suit
{
    NONE,
    CENTIPEDE,
    RAT,
    CAT,
    OWL
}