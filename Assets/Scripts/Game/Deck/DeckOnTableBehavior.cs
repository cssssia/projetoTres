using System;
using UnityEngine;

public class DeckOnTableBehavior : MonoBehaviour
{
    [SerializeField] private DeckBehavior m_deck;
    [SerializeField] private DeckBehavior m_currentDeck;

    public void OnPlayerSpawned()
    {
        m_deck = FindObjectOfType<DeckBehavior>();
    }

    public bool CheckClickObject(GameObject p_gameObject)
    {
        bool l_isDeck = false;

        if (p_gameObject != null && p_gameObject.CompareTag("Deck"))
        {
            m_currentDeck = p_gameObject.GetComponent<DeckBehavior>();
        }

        return l_isDeck;
    }

    public void CheckClickUp(Action<GameObject> p_actionOnEndAnimation)
    {
        bool l_deck = false;

        if (m_currentDeck != null)
        {
            GiveUp(p_actionOnEndAnimation);
            l_deck = true;
        }

        if (!l_deck)
        {
            m_currentDeck = null;
        }
    }

    private void GiveUp(Action<GameObject> p_action)
    {
        m_currentDeck.GiveUp(p_action);
    }

    public void CheckHoverObject(GameObject p_gameObject)
    {
        if (p_gameObject == m_deck.gameObject) m_deck.HighlightDeck();
        else m_deck.HighlightOff();
    }

}