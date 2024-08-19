using System;
using UnityEngine;

public class PauseMultiplayerUI : MonoBehaviour
{
	void Start()
	{
		GameManager.Instance.OnMultiplayerGamePaused += GameManager_OnMultiplayerGamePaused;
		GameManager.Instance.OnMultiplayerGameUnpaused += GameManager_OnMultiplayerGameUnpaused;

		Hide();
	}

	private void GameManager_OnMultiplayerGamePaused(object p_sender, EventArgs e)
	{
		Show();
	}

	private void GameManager_OnMultiplayerGameUnpaused(object p_sender, EventArgs e)
	{
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