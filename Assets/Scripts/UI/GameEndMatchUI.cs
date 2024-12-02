using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameEndMatchUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_results;
    [SerializeField] private Button m_mainMenuButton;

    void Awake()
    {
        m_mainMenuButton.onClick.AddListener(() => {
			NetworkManager.Singleton.Shutdown();
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneLoader.Scene.SCN_Menu.ToString());
        });
    }

	void Start()
	{
		MatchManager.Instance.MatchHasEnded.OnValueChanged += MatchHasEnded_OnValueChanged;

		Hide();
	}

	private void MatchHasEnded_OnValueChanged(bool p_previousValue, bool p_newValue)
	{
		if (p_newValue)
        {
            if (MatchManager.Instance.WonMatch.Value == (Player)PlayerController.LocalInstance.PlayerIndex)
                m_results.text = "you won";
            else
                m_results.text = "you lost";

			Show();
        }
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