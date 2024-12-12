using System;
using UnityEngine;
using UnityEngine.UI;

public class GameRulesUI : MonoBehaviour
{
    [SerializeField] private Button m_rulesBackButton;
    [SerializeField] private GameObject m_pauseUI;

	void Awake()
	{
		m_rulesBackButton.onClick.AddListener(() => {
            Hide();
			m_pauseUI.SetActive(true);
        });

		Hide();
	}

	private void Show()
	{
		gameObject.SetActive(true);
	}

	private void Hide()
    {
        gameObject.SetActive(false);
    }

}