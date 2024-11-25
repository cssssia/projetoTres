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

        if (p_gameObject != null)
        {
            if (m_deck == p_gameObject)
            {
                m_currentDeck = p_gameObject.GetComponent<DeckBehavior>();

                l_isDeck = true;
            }
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
}