using Unity.VisualScripting;
using UnityEngine;

public class TurnManager : Singleton<TurnManager>
{

    private int m_firstPlayerCardValue;
    private int m_secondPlayerCardValue;

    void Start()
    {
        m_firstPlayerCardValue = 0;
        m_secondPlayerCardValue = 0;
    }

    public void PlayCard(int p_value)
    {
        if (m_firstPlayerCardValue == 0)
            m_firstPlayerCardValue = p_value;
        else if (m_secondPlayerCardValue == 0)
        {
            m_secondPlayerCardValue = p_value;
            CheckWinner();
        }
    }

    private void CheckWinner()
    {
        if (m_firstPlayerCardValue > m_secondPlayerCardValue)
        {

        }
        else if (m_secondPlayerCardValue > m_firstPlayerCardValue)
        {

        }

        //check for empate later
    }

}