using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CheckCards : MonoBehaviour
{
    [SerializeField] private CardsScriptableObject m_cardsSO;
    [SerializeField] private int m_maxValue;
    [SerializeField] private List<CardsScriptableObject.Card> m_deck;
    public bool notSpawned;

    void Start()
    {
        FindMaxValue();
        notSpawned = true;
    }

    void Update()
    {
        if (notSpawned)
        {
            SortByValue();
            SpawnCards();
        }
    }

    void FindMaxValue()
    {
        for (int i = 0; i < m_cardsSO.deck.Count; i++)
        {
            if (m_maxValue < m_cardsSO.deck[i].value)
                m_maxValue = m_cardsSO.deck[i].value;
        }
    }

    void SortByValue()
    {
        notSpawned = false;
        for (int j = 0; j <= m_maxValue; j++)
        {
            for (int i = 0; i < m_cardsSO.deck.Count; i++)
            {
                if (m_cardsSO.deck[i].value == j)
                    m_deck.Add(m_cardsSO.deck[i]);
            }
        }
    }

    void SpawnCards()
    {
        for (int i = 0; i < m_deck.Count; i++)
        {
            GameObject l_newCard = Instantiate(m_cardsSO.prefab, new Vector3(i, 0f, 0f), Quaternion.identity);
            l_newCard.name = m_deck[i].name;
            l_newCard.GetComponent<MeshRenderer>().material = m_deck[i].material;
        }
    }

}
